
var VariableWeightProductVM = Backbone.Model.extend({

    initialize: function() {
	console.log("init VW product VM");
    }
});

var VariableWeightProductsVM = Backbone.Collection.extend({

    url: "/api/variableWeightProducts",

    Model: VariableWeightProductVM,

    parse: function(data) {
	if (data && data.VariableWeighProductsViewModel) {
	    return data.VariableWeighProductsViewModel;
	}
    }
});
