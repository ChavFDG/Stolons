
var ProductsManagementView = Backbone.View.extend({

    el: "#productsManagement",

    events: {
        "click .openStockMgtModal": "openStockMgtModal",
        "mouseenter .setupProductPreview": "productPreview"
    },

    initialize: function () {
        this.tooltipSetup = {};
        this.productStocksModels = {};
        this.stockMgtModal = new StockMgtViewModal();
        this.publicProductTemplate = _.template($("#publicProductTemplate").html());
    },

    getProductStockModel: function (productStockId) {
        var that = this;
        var def = $.Deferred();

        if (!this.productStocksModels[productStockId]) {
            this.productStocksModels[productStockId] = new ProductStockModel({ Id: productStockId });
            this.productStocksModels[productStockId].fetch().done(function () {
                def.resolve(that.productStocksModels[productStockId]);
            });
        } else {
            def.resolve(this.productStocksModels[productStockId]);
        }
        return def.promise();
    },

    openStockMgtModal: function (event) {
        var that = this;
        var productStockId = $(event.currentTarget).data("product-stock-id");

	$.blockUI();
        this.getProductStockModel(productStockId).done(function (productStock) {
            that.stockMgtModal.open(productStock);
        });
        event.preventDefault();
        return false;
    },

    productPreview: function (ev) {
        var that = this;
        var productStockId = $(ev.currentTarget).data("product-stock-id");

        this.getProductStockModel(productStockId).done(function (productStockModel) {
            if ($(ev.currentTarget).uitooltip("instance")) {
                $(ev.currentTarget).uitooltip("destroy");
                $(ev.currentTarget).attr("title", "");//Fck this`
            }
            if (!$(ev.currentTarget).uitooltip("instance")) {
                $(ev.currentTarget).uitooltip(
                    {
                        track: true,
                        classes: {
                            "ui-tooltip": "productPreviewTooltip",
                            "ui-tooltip-content": "productPreviewTooltipContent"
                        },
                        show: true,
                        content: that.publicProductTemplate(
                            {
                                product: productStockModel.get("Product").toJSON(),
                                productModel: productStockModel.get("Product"),
                                productStock: productStockModel.toJSON(),
                                productStockModel: productStockModel
                            })
                    }
                );
            }
            $(ev.currentTarget).uitooltip("enable");
            $(ev.currentTarget).uitooltip("open");
        });
    }
});

var StockMgtViewModal = Backbone.View.extend({

    el: "#stockMgt",

    events: {
        "change #WeekStock": "validateWeekStock",
        "change #RemainingStock": "validateRemainingStock",
        "click #saveStocks": "saveStocks",
        "click #cancelEditStocks": "closeModal",
        "click #cancel": "closeModal",
        "click #weekStockStepUp": "weekStockStepUp",
        "click #weekStockStepDown": "weekStockStepDown",
        "click #remainingStockStepUp": "remainingStockStepUp",
        "click #remainingStockStepDown": "remainingStockStepDown",
    },

    template: _.template($("#stockMgtTemplate").html()),

    initialize: function () {
        this.validation = {};
    },

    open: function (productStockModel) {
        // if ($(btn).is(":disabled")) {
        //     return false;
        // }
        // Working copy
        //this.currentProductStock = ProductsModel.get(productStockId).clone();
        this.currentProductStock = productStockModel.clone();//ProductsModel.get(productStockId).clone();
        this.renderModal();
        if (this.currentProductStock.get("AdherentStolon").Stolon.Mode === 0 || this.currentProductStock.get("Product").get("StockManagement") == 1) {
            this.validateRemainingStock();
        } else {
            this.validateWeekStock();
        }
        this.initWeekStock = this.currentProductStock.get("WeekStock");
        this.initRemainingStock = this.currentProductStock.get("RemainingStock");
    },

    isInt: function (n) {
        return n % 1 === 0;
    },

    weekStockStepUp: function () {
        var newStock;
        if (this.currentProductStock.get("Product").get("Type") === 1 || this.currentProductStock.get("Product").get("Type") == 3) {
            newStock = parseInt(this.currentProductStock.get("WeekStock")) + 1;
        } else {
            newStock = (parseFloat(this.currentProductStock.get("WeekStock") + 1) * (this.currentProductStock.get("Product").get("QuantityStep") / 1000)).toFixed(4);
        }
        $("#WeekStock").val(newStock);
        $("#WeekStock").trigger("change");
        return false;
    },

    weekStockStepDown: function () {
        var newStock;
        if (this.currentProductStock.get("Product").get("Type") === 1 || this.currentProductStock.get("Product").get("Type") == 3) {
            newStock = parseInt(this.currentProductStock.get("WeekStock")) - 1;
        } else {
            newStock = (parseFloat(this.currentProductStock.get("WeekStock") - 1) * (this.currentProductStock.get("Product").get("QuantityStep") / 1000)).toFixed(4);
        }
        $("#WeekStock").val(newStock);
        $("#WeekStock").trigger("change");
        return false;
    },

    remainingStockStepUp: function () {
        var newStock;
        if (this.currentProductStock.get("Product").get("Type") === 1 || this.currentProductStock.get("Product").get("Type") == 3) {
            newStock = parseInt(this.currentProductStock.get("RemainingStock")) + 1;
        } else {
            newStock = (parseFloat(this.currentProductStock.get("RemainingStock") + 1) * (this.currentProductStock.get("Product").get("QuantityStep") / 1000)).toFixed(4);
        }
        $("#RemainingStock").val(newStock);
        $("#RemainingStock").trigger("change");
        return false;
    },

    remainingStockStepDown: function () {
        var newStock;
        if (this.currentProductStock.get("Product").get("Type") === 1 || this.currentProductStock.get("Product").get("Type") == 3) {
            newStock = parseInt(this.currentProductStock.get("RemainingStock")) - 1;
        } else {
            newStock = (parseFloat(this.currentProductStock.get("RemainingStock") - 1) * (this.currentProductStock.get("Product").get("QuantityStep") / 1000)).toFixed(4);
        }
        $("#RemainingStock").val(newStock);
        $("#RemainingStock").trigger("change");
        return false;
    },

    validateWeekStock: function () {
        var weekStock = Math.abs(parseFloat($("#WeekStock").val()));

        if (this.currentProductStock.get("Product").get("Type") !== 1 && this.currentProductStock.get("Product").get("Type") != 3) {
            if ((Math.abs(weekStock * 1000) % this.currentProductStock.get("Product").get("QuantityStep")) != 0) {
                this.validation.weekStockError = "Le stock doit être divisible par le palier de vente (" + this.currentProductStock.get("Product").get("QuantityStepString") + ").";
                this.render();
                return;
            }
            weekStock = ((weekStock * 1000) / parseInt(this.currentProductStock.get("Product").get("QuantityStep")));
        } else {
            if (!this.isInt(weekStock)) {
                this.validation.weekStockError = "Le nombre de pièces doit être un nombre entier.";
                this.render();
                return;
            }
        }
        this.currentProductStock.set({ "WeekStock": weekStock });
        this.validation.weekStockError = "";
        this.render();
    },

    validateRemainingStock: function () {
        var remainingStock = Math.abs(parseFloat($("#RemainingStock").val()));

        if (this.currentProductStock.get("Product").get("Type") !== 1 && this.currentProductStock.get("Product").get("Type") != 3) {
            if (Math.abs(remainingStock * 1000) % this.currentProductStock.get("Product").get("QuantityStep") != 0) {
                this.validation.remainingStockError = "Le stock doit être divisible par le palier de vente (" + this.currentProductStock.get("Product").get("QuantityStepString") + ").";
                this.render();
                return;
            }
            remainingStock = ((remainingStock * 1000) / parseInt(this.currentProductStock.get("Product").get("QuantityStep")));
        } else {
            if (!this.isInt(remainingStock)) {
                this.validation.remainingStockError = "Le nombre de pièces doit être un nombre entier.";
                this.render();
                return;
            }
        }
        if (this.currentProductStock.get("Product").get("StockManagement") === 0) {
            var diffQty = remainingStock - this.currentProductStock.get("RemainingStock");
            this.currentProductStock.set({ "WeekStock": this.currentProductStock.get("WeekStock") + diffQty });
        }
        this.currentProductStock.set({ "RemainingStock": remainingStock });
        this.validation.remainingStockError = "";
        this.render();
    },

    saveStocks: function () {
        if ($("#saveStocks").attr("disabled") === "disabled") {
            return false;
        }
        var self = this;
        var changeWeekStock;
        var responseHandler = function (response) {
            if (response.Error !== false) {
                var appendText = "<br />Les stocks ont peut-être été mis à jours pendant l'édition. Veuillez rafraichir la page.<br />";
                if (changeWeekStock) {
                    self.validation.weekStockError = response.Message + appendText;
                } else {
                    self.validation.remainingStockError = response.Message + appendText;
                }
                self.render();
            } else {
                location.reload();
            }
        };
        var promise;
        if (this.currentProductStock.get("AdherentStolon").Stolon.Mode === 0 || this.currentProductStock.get("Product").get("StockManagement") == 1) {
            changeWeekStock = false;
            var diffStock = this.currentProductStock.get("RemainingStock") - this.initRemainingStock;
            promise = $.ajax({
                url: "/ProductsManagement/ChangeCurrentStock",
                type: 'POST',
                data: {
                    id: self.currentProductStock.get("Id"),
                    stockDiff: diffStock
                }
            });
        } else {
            changeWeekStock = true;
            var diffStock = this.currentProductStock.get("WeekStock") - this.initWeekStock;
            promise = $.ajax({
                url: "/ProductsManagement/ChangeStock",
                type: 'POST',
                data: {
                    id: self.currentProductStock.get("Id"),
                    stockDiff: diffStock
                }
            });
        }
        promise.then(responseHandler);
        return false;
    },

    closeModal: function () {
        this.$el.modal('hide');
    },

    onClose: function () {
        this.currentProductStock = null;
        this.$el.off('hide.bs.modal');
        this.$el.empty();
    },

    render: function () {
        this.$el.html(this.template(
            {
                productStock: this.currentProductStock,
                validation: this.validation
            }
        ));
    },

    renderModal: function () {
        this.render();
        this.$el.modal({ keyboard: true, show: true });
        this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
	$.unblockUI();
    }
});

var VariableWeightsProductsManagementView = Backbone.View.extend({

    el: "#variableWeightProducts",

    template: _.template($("#variableWeightProductsTemplate").html()),

    events: {
        "click #saveVW": "saveOrderVariableWeighs",
        "keyup input.vw-input": "onVWChange"
    },

    initialize: function () {
        if (!this.variableWeightOrdersVM) {
            this.variableWeightOrdersVM = new VariableWeightProductsVM();
            this.fetchDeferred = this.variableWeightOrdersVM.fetch();
        }
        this.errors = {};
    },

    render: function () {
        this.$el.html(this.template(
            {
                vwVM: this.variableWeightOrdersVM
            }
        ));
    },

    renderModal: function () {
        var that = this;

	$.blockUI();
        this.fetchDeferred.done(function () {
            that.render();
            that.$el.modal({ keyboard: true, show: true });
            that.$el.on('hide.bs.modal', _.bind(that.onClose, that));
	    $.unblockUI();
        });
    },

    onClose: function () {
        this.$el.off('hide.bs.modal');
        this.$el.empty();
    },

    validateVWInput: function (inputElem) {
        var newVal = parseFloat(inputElem.val().replace(",", "."));
        var productId = inputElem.data("product-id");
        var orderNumber = inputElem.data("order-number");
        var vwProductVM = this.variableWeightOrdersVM.getVWProductVM(orderNumber, productId);
        var valid = true;

        if (!_.isNumber(newVal) || _.isNaN(newVal) || newVal < vwProductVM.MinimumWeight || newVal > vwProductVM.MaximumWeight) {
            valid = false;
        }
        inputElem.toggleClass("error", !valid);
        return valid;
    },

    onVWChange: function (ev) {
        var that = this;
        var inputElem = $(ev.currentTarget);
        var orderNumber = inputElem.data("order-number");
        var productId = inputElem.data("product-id");
        var billEntryId = inputElem.data("bill-entry-id");
        var consumerIdx = inputElem.data("consumer-idx");

        var orderVM;
        inputElem.val(inputElem.val().replace(".", ","));
        _.forEach(this.variableWeightOrdersVM.get("VariableWeighOrdersViewModel"), function (vwOrderVM) {
            if (vwOrderVM.OrderNumber === orderNumber) {
                _.forEach(vwOrderVM.VariableWeighProductsViewModel, function (vwProductVM) {
                    if (vwProductVM.ProductId === productId) {
                        _.forEach(vwProductVM.ConsumersAssignedWeighs, function (assignedW, idx) {
                            if (assignedW.BillEntryId === billEntryId && idx === consumerIdx) {
                                that.validateVWInput(inputElem);
                                assignedW.AssignedWeigh = parseFloat(inputElem.val().replace(",", "."));
                            }
                        });
                    }
                });
            }
        });
        return true;
    },

    validateVWStolonOrder: function (orderNumber) {
        var that = this;
        var valid = true;

        $("input.vw-input").each(function (idx, jqElem) {
            var inputElem = $(jqElem);
            var elemOrderNumber = inputElem.data("order-number");
            if (elemOrderNumber !== orderNumber) {
                return;
            }
            if (!that.validateVWInput(inputElem)) {
                valid = false;
            }
        });
        return valid;
    },

    toFrenchDecimal: function(vwOrder) {
	_.forEach(vwOrder.VariableWeighProductsViewModel, function(VWproductVM) {
	    _.forEach(VWproductVM.ConsumersAssignedWeighs, function(assignedWeight) {
		assignedWeight.AssignedWeigh = assignedWeight.AssignedWeigh.toString().replace(".", ",");
	    });
	});
    },

    saveOrderVariableWeighs: function (ev) {
        var that = this;
        var buttonElem = $(ev.currentTarget);
        var orderNumber = buttonElem.data("order-number");

        var vwOrder;
        if (!this.validateVWStolonOrder(orderNumber)) {
            return false;
        }
        _.forEach(this.variableWeightOrdersVM.get("VariableWeighOrdersViewModel"), function (vwOrderVM) {
            if (vwOrderVM.OrderNumber === orderNumber) {
                vwOrder = vwOrderVM;
            }
        });
        if (!vwOrder) {
            return false;
        }
	this.toFrenchDecimal(vwOrder);
        $.blockUI();
        var promise = $.ajax({
            url: "/api/variableWeightProducts",
            type: 'POST',
            data: {
                "variableWeighOrderViewModel": vwOrder
            }
        });
        promise.always(function (j, s, res) {
            $.unblockUI();
            if (res.status !== 200) {
                $("#server-error").toggleClass("hidden", false);
            } else {
                window.location.reload();
            }
        });
    }

});

//Init
$(function () {
    new ProductsManagementView();
    var vwView = new VariableWeightsProductsManagementView();
    $("#enterVariableWeights").click(_.bind(vwView.renderModal, vwView));
});
