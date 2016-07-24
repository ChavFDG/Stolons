
ProductModel = Backbone.Model.extend({

    //Products id key is 'Id'
    idAttribute: "Id",

    defaults: {},

    unitsEnum: ["Kg", "L"],

    getPictureUrl: function() {
	var pictures = this.get("Pictures");
	if (_.isEmpty(pictures) || _.isEmpty(pictures[0])) {
	    return "/images/panier.jpg";
	}
	return pictures[0];
    },

    /*
      return the html string according to this product's sell type
     */
    getSellTypeString: function() {
	if (this.get("Type") == 0) {
	    return "Au poids";
	} else if (this.get("Type") == 1) {
	    return "À la pièce";
	} else if (this.get("Type") == 2) {
	    return  "Emballé";
	} else {
	    return "Error";
	}
    },

    getStockUnitString: function() {
	if (this.get("Type") == 1) {
	    return "Pièces";
	}
	var productUnit = this.get("ProductUnit");
	return this.unitsEnum[productUnit];
    },

    getUnitPriceString: function() {
	if (this.get("Type") == 1) {
	    return this.get("UnitPrice") + " € l'unité";
	}
	return this.get("UnitPrice") + " € pour " + this.get("QuantityStepString");
    },

    getVolumePriceString: function() {
	var productUnit = this.get("ProductUnit");
	if (this.get("Price") === 0 || this.get("QuantityStep") === 1000) {
	    return "";
	}
	return "(" + this.get("Price") + " € / " +  this.unitsEnum[productUnit] + ")";
    },

    getSellStepString: function() {
	if (this.get("Type") == 1) {
	    return " Pièce(s)";
	}
	var productUnit = this.get("ProductUnit"); 
    },

    //retourne la string correctement formattee pour le poids donne en grammes
    prettyPrintQuantity: function(weight) {
	
	if (weight >= 1000) {
	    return (weight / 1000) + " " + largeunit;
	}
	return weight + " " + productUnit;
    }
});
