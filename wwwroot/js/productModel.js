
ProductModel = Backbone.Model.extend({

    //Products id key is 'Id'
    idAttribute: "Id",

    defaults: {},

    getPictureUrl: function(type) {
	var pictures = this.get("Pictures");
	if (_.isEmpty(pictures) || _.isEmpty(pictures[0])) {
	    return "/images/panier.jpg";
	}
	if (type == 'light') {
	    return this.get("LightPath") + "\\" + pictures[0];
	}
	return this.get("HeavyPath")+ "\\" + pictures[0];
    }
});

ProductStockModel = Backbone.Model.extend({

    //Products id key is 'Id'
    idAttribute: "Id",

    defaults: {},

    unitsEnum: ["Kg", "L"],

    /*
      return the html string according to this product's sell type
     */
    getSellTypeString: function() {
	if (this.get("Product").get("Type") == 0) {
	    return "Au poids";
	} else if (this.get("Product").get("Type") == 1) {
	    return "À la pièce";
	} else if (this.get("Product").get("Type") == 2) {
	    return  "Emballé";
	} else {
	    return "Erreur";
	}
    },

    getStockUnitString: function() {
	if (this.get("Product").get("Type") == 1) {
	    return "Pièces";
	}
	var productUnit = this.get("Product").get("ProductUnit");
	return this.unitsEnum[productUnit];
    },

    getUnitPriceString: function() {
	if (this.get("Product").get("Type") == 1) {
	    return this.get("Product").get("UnitPrice") + " € l'unité";
	}
	var weightStepPrice = parseFloat(this.get("Product").get("WeightPrice"));
	weightStepPrice = weightStepPrice * (parseFloat(this.get("Product").get("QuantityStep")) / 1000);
	return weightStepPrice.toFixed(2) + " € pour " + this.get("Product").get("QuantityStepString");
    },

    getVolumePriceString: function() {
	var productUnit = this.get("Product").get("ProductUnit");
	if (this.get("Product").get("WeightPrice") === 0 || this.get("Product").get("QuantityStep") === 1000) {
	    return "";
	}
	return "(" + this.get("Product").get("WeightPrice") + " € / " +  this.unitsEnum[productUnit] + ")";
    },

    getSellStepString: function() {
	if (this.get("Product").get("Type") == 1) {
	    return " Pièce(s)";
	}
	var productUnit = this.get("Product").get("ProductUnit"); 
    },

    //retourne la string correctement formattee pour le poids donne en grammes
    prettyPrintQuantity: function(weight) {	
	if (weight >= 1000) {
	    return (weight / 1000) + " " + largeunit;
	}
	return weight + " " + productUnit;
    },

    //Stock restant en fonction du type de vente et de la quantity step
    getRemainingQuantityStock: function() {
	if (this.get("Product").get("Type") == 1) {
	    return this.get("RemainingStock");
	} else {
	    return (this.get("RemainingStock") * this.get("Product").get("QuantityStep")) / 1000;
	}
    },

    //Stock restant en fonction du type de vente et de la quanity step
    getWeekQuantityStock: function() {
	if (this.get("Product").get("Type") == 1) {
	    return this.get("WeekStock");
	} else {
	    return (this.get("WeekStock") * this.get("Product").get("QuantityStep")) / 1000;
	}
    },
    
    //Si tu as besoin de faire des changements sur les données sur le JSON avant de les rentrer dans le model
    parse: function (data) {
        data.Product = new ProductModel(data.Product);
        return data;
    }
});

window.ProductStockModel = ProductStockModel;
