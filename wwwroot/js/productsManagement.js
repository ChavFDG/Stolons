
var setupProductPreviewTooltip = function() {
    $(".productPreview").each(function() {
	var publicProductTemplate = _.template($("#publicProductTemplate").html());
	var productId = $(this).attr("data-productId");
	var productStockModel = window.ProductsModel.findWhere({ProductId: productId});

	$(this).tooltip(
	    {
		track: true,
		classes: {
		    "ui-tooltip": "productPreviewTooltip",
		    "ui-tooltip-content": "productPreviewTooltipContent"
		},
		hide: false,
		show: false,
		content: publicProductTemplate(
		    {
			product: productStockModel.get("Product").toJSON(),
			productModel: productStockModel.get("Product"),
			productStock:  productStockModel.toJSON(),
			productStockModel:  productStockModel
		    })
	    });
    });
};

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
//Setup preview tooltips once models are avaible
ProductsModel.on('sync', setupProductPreviewTooltip);

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

window.StockMgtViewModal = new StockMgtViewModal({ model: window.ProductsModel });
