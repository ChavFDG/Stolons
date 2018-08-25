
var ProductsManagementView = Backbone.View.extend({

    el: "#productsManagement",

    events: {
	"click .openStockMgtModal": "openStockMgtModal",
	"mouseenter .setupProductPreview": "productPreview"
    },

    initialize: function() {
	this.tooltipSetup = {};
	this.productStocksModels = {};
	this.stockMgtModal = new StockMgtViewModal();
	this.publicProductTemplate = _.template($("#publicProductTemplate").html());
    },

    getProductStockModel: function(productStockId) {
	var that = this;
	var def = $.Deferred();

	if (!this.productStocksModels[productStockId]) {
	    this.productStocksModels[productStockId] = new ProductStockModel({Id: productStockId});
	    this.productStocksModels[productStockId].fetch().done(function() {
		def.resolve(that.productStocksModels[productStockId]);
	    });
	} else {
	    def.resolve(this.productStocksModels[productStockId]);
	}
	return def.promise();
    },

    openStockMgtModal: function(event) {
	var that = this;
	var productStockId = $(event.currentTarget).data("product-stock-id");

	this.getProductStockModel(productStockId).done(function(productStock) {
	    that.stockMgtModal.open(productStock);
	});
	event.preventDefault();
	return false;
    },

    productPreview: function(ev) {
	var that = this;
	var productStockId = $(ev.currentTarget).data("product-stock-id");

	this.getProductStockModel(productStockId).done(function(productStockModel) {
	    $(ev.currentTarget).tooltip(
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
	    that.tooltipSetup[productStockId] = true;
	    $(ev.currentTarget).removeClass("setupProductPreview");
	    //Force a new hover event after tooltip setup to show it immediately
	    $(ev.currentTarget).trigger("mouseenter");
	});
	ev.preventDefault();
	return false;
    }
});

var StockMgtViewModal = Backbone.View.extend({

    el: "#stockMgt",

    events: {
        "change #WeekStock": "validateWeekStock",
        "change #RemainingStock": "validateRemainingStock",
        "click #saveStocks": "saveStocks",
        "click #cancelEditStocks": "closeModal",
	"click #cancel": "closeModal"
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

        if (this.currentProductStock.get("Product").get("Type") != 1) {
            if (Math.abs(remainingStock * 1000) % this.currentProductStock.get("Product").get("QuantityStep") != 0) {
                this.validation.remainingStockError = "Le stock doit être divisible par le palier de vente.";
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
	    this.currentProductStock.set({"WeekStock": this.currentProductStock.get("WeekStock") + diffQty });
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
	if (this.currentProductStock.get("AdherentStolon").Stolon.Mode == 0 || this.currentProductStock.get("Product").get("StockManagement") == 1) {
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

//Init
$(function() {
    new ProductsManagementView();
});
