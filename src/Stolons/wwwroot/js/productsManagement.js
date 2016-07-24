
var CurrentModeModel = Backbone.Model.extend(
    {
	default: {mode: 0},
	/*
	  0: commandes,
	  1: livraisons et stocks
	 */

	url: "/api/currentMode",

	initialize: function() {
	    this.fetch();
	},

	parse: function(data) {
	    return {mode: data};
	}
    }
);

window.CurrentModeModel = new CurrentModeModel();

var ProductsCollection = Backbone.Collection.extend(
    {
	defaults: [],

	model: ProductModel,

	url: "/api/producerProducts",

	initialize: function() {
	    this.fetch();
	},

	parse: function(data) {
	    var products = [];

	    _.forEach(data, function(productVm) {
		var product = productVm.Product;
		product["OrderedQuantityString"] = productVm.OrderedQuantityString;
		products.push(product);
	    });
	    return products;
	}
    }
);

window.ProductsModel = new ProductsCollection();

var StockMgtViewModal = Backbone.View.extend({

    el: "#stockMgt",

    events: {
	"change #WeekStock": "validateWeekStock",
	"change #RemainingStock": "validateRemainingStock",
	"click #saveStocks" : "saveStocks",
	"click #cancelEditStocks": "closeModal"
    },

    template: _.template($("#stockMgtTemplate").html()),

    initialize: function() {
	this.validation= {};
    },

    open: function(productId) {
	this.currentProduct = ProductsModel.get(productId);
	console.log(this.currentProduct);
	this.renderModal();
	if (window.CurrentModeModel.get("mode") == 0) {
	    this.validateRemainingStock();
	} else {
	    this.validateWeekStock();
	}
    },

    isInt: function(n) {
	return n % 1 === 0;
    },

    validateWeekStock: function() {
	var weekStock = Math.abs(parseFloat($("#WeekStock").val()));
	this.currentProduct.set({WeekStock: weekStock});

	if (this.currentProduct.get("Type") != 1) {
	    console.log("qtyStp = " + this.currentProduct.get("QuantityStep"));
	    if ((weekStock * 1000) % this.currentProduct.get("QuantityStep") != 0) {
		this.validation.weekStockError = "Le stock doit être divisible par le palier de vente (" + this.currentProduct.get("QuantityStepString") + ").";
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
	this.validation.weekStockError = "";
	this.render();
    },

    validateRemainingStock: function() {
	var remainingStock = Math.
	    abs(parseFloat($("#RemainingStock").val()));
	this.currentProduct.set({RemainingStock: remainingStock});

	if (this.currentProduct.get("Type") != 1) {
	    console.log("qtyStp = " + this.currentProduct.get("QuantityStep"));
	    if ((remainingStock * 1000) % this.currentProduct.get("QuantityStep") != 0) {
		this.validation.remainingStockError = "Le stock doit être divisible par le palier de vente.";
 		this.render();
		return;
	    }
	} else {
	    if (!this.isInt(remainingStock)) {
		this.validation.remainingStockError = "Le nombre de pièces doit être un nombre entier.";
		this.render();
		return;
	    }
	}
	this.validation.remainingStockError = "";
	this.render();
    },

    saveStocks: function() {
	if ($("#saveStocks").attr("disabled") == "disabled") {
	    return false;
	}
	var self = this;
	var responseHandler = function(xhr) {
	    location.reload();
	};

	var promise;
	if (window.CurrentModeModel.get("mode") == 0) {
	    promise = $.ajax({
		url: "/ProductsManagement/ChangeCurrentStock",
		type: 'POST',
		data: {
		    id: self.currentProduct.get("Id"),
		    newStock: self.currentProduct.get("RemainingStock"),
		}
	    });
	} else {
	    promise = $.ajax({
		url: "/ProductsManagement/ChangeStock",
		type: 'POST',
		data: {
		    id: self.currentProduct.get("Id"),
		    newStock: self.currentProduct.get("WeekStock"),
		}
	    });
	}
	promise.then(responseHandler);
	return false;
    },

    closeModal: function() {
	this.$el.modal('hide');
    },

    onClose: function() {
	this.currentProduct = null;
	this.$el.off('hide.bs.modal');
	this.$el.empty();
    },

    render: function() {
	this.$el.html(this.template(
	    {
		currentMode: window.CurrentModeModel.toJSON(),
		productModel: this.currentProduct,
		product: this.currentProduct.toJSON(),
		validation: this.validation
	    }
	));
    },

    renderModal: function() {
	this.render();
	this.$el.modal({keyboard: true, show: true});
	this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
    }
});

window.StockMgtViewModal = new StockMgtViewModal({model: window.ProductsModel});
