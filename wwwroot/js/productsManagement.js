
var ProductsStockCollection = Backbone.Collection.extend(
    {
        defaults: [],

        model: ProductStockModel,

        url: "/api/producerProducts",

        initialize: function () {
            this.fetch();
        },

        parse: function (data) {
            var productsStock = [];

            _.forEach(data, function (productStockVm) {
                var productStock = productStockVm.ProductStock;
                productsStock["OrderedQuantityString"] = productStockVm.OrderedQuantityString;
                productsStock.push(productStock);
            });
            return productsStock;
        }
    }
);

window.ProductsModel = new ProductsStockCollection();

var StockMgtViewModal = Backbone.View.extend({

    el: "#stockMgt",

    events: {
        "change #WeekStock": "validateWeekStock",
        "change #RemainingStock": "validateRemainingStock",
        "click #saveStocks": "saveStocks",
        "click #cancelEditStocks": "closeModal"
    },

    template: _.template($("#stockMgtTemplate").html()),

    initialize: function () {
        this.validation = {};
    },

    open: function (productStockId) {
        this.currentProductStock = ProductsModel.get(productStockId);
        this.renderModal();
        if (this.currentProductStock.get("AdherentStolon").Stolon.Mode == 0 || this.currentProductStock.get("Product").get("StockManagement") == 1) {
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

    validateWeekStock: function () {
        var weekStock = Math.abs(parseFloat($("#WeekStock").val()));

        if (this.currentProductStock.get("Product").get("Type") != 1) {
            if ((weekStock * 1000) % this.currentProductStock.get("Product").get("QuantityStep") != 0) {
                this.validation.weekStockError = "Le stock doit être divisible par le palier de vente (" + this.currentProductStock.get("Product").get("QuantityStepString") + ").";
                this.render();
                return;
            }
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

        if (this.currentProductStock.get("Product").get("Type") != 1) {
            if ((remainingStock * 1000) % this.currentProductStock.get("Product").get("QuantityStep") != 0) {
                this.validation.remainingStockError = "Le stock doit être divisible par le palier de vente.";
                this.render();
                return;
            }
        } else {
            if (!this.isInt(remainingStock)) {
                this.validation.remainingStockError = "Le nombre de pièces doit être un nombre entier.";
                console.log("Le nombre de pièces doit être un nombre entier.");
                this.render();
                return;
            }
        }
	this.currentProductStock.set({ "RemainingStock": remainingStock });
        this.validation.remainingStockError = "";
        this.render();
    },

    saveStocks: function () {
        if ($("#saveStocks").attr("disabled") == "disabled") {
            return false;
        }
        var self = this;
        var responseHandler = function (responseText) {
	    if (responseText.startsWith("Erreur")) {
		self.validation.remainingStockError = responseText;
		self.render();
	    } else {
		location.reload();
	    }
	};
        var promise;
	if (this.currentProductStock.get("AdherentStolon").Stolon.Mode == 0 || this.currentProductStock.get("Product").get("StockManagement") == 1) {
	    console.log("current reminaing stock = " + this.currentProductStock.get("RemainingStock"));
	    console.log("init reminaing stock = " + this.initRemainingStock);
	    var diffStock = this.currentProductStock.get("RemainingStock") - this.initRemainingStock;
	    //var diffStock = this.initRemainingStock - this.currentProductStock.get("RemainingStock");
	    console.log("diff stock = " + diffStock);
            promise = $.ajax({
                url: "/ProductsManagement/ChangeCurrentStock",
                type: 'POST',
                data: {
                    id: self.currentProductStock.get("Id"),
		    stockDiff: diffStock
                }
            });
        } else {
	    var diffStock = this.initWeekStock - this.currentProductStock.get("WeekStock");
	    console.log("diff stock = " + diffStock);
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
                //productModel: new ProductModel(this.currentProductStock.get("Product")),
                productStock: this.currentProductStock,
                validation: this.validation
            }
        ));
    },

    renderModal: function () {
        this.render();
        this.$el.modal({ keyboard: true, show: true });
        this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
    }
});

window.StockMgtViewModal = new StockMgtViewModal({ model: window.ProductsModel });
