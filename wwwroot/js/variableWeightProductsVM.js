
var VariableWeightProductsVM = Backbone.Model.extend({

    url: "/api/variableWeightProducts",

    //Model: VariableWeightProductVM,
    productsUnitsEnum: ["Kg", "L"],

    parse: function(data) {
	var that = this;

	if (data && data.VariableWeighOrdersViewModel) {
	    _.forEach(data.VariableWeighOrdersViewModel, function(vwOrdersVM) {
		_.forEach(vwOrdersVM.VariableWeighProductsViewModel, function(vwProductVM) {
		    vwProductVM.ProductUnit = that.productsUnitsEnum[vwProductVM.ProductUnit];
		});
	    });
	 }
	return data;
    },

    getVWProductVM: function(orderNumber, productId) {
	var productVM;
	_.forEach(this.get("VariableWeighOrdersViewModel"), function (vwOrderVM) {
	    if (vwOrderVM.OrderNumber == orderNumber) {
		_.forEach(vwOrderVM.VariableWeighProductsViewModel, function (vwProductVM) {
		    if (vwProductVM.ProductId == productId) {
			productVM = vwProductVM;
		    }
		});
	    }
	});
	return productVM;
    }
});
