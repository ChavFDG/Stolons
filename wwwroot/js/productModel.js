
ProductModel = Backbone.Model.extend({

    //Products id key is 'Id'
    idAttribute: "Id",

    defaults: {},

    getPictureUrl: function (type) {
        var pictures = this.get("Pictures");
        if (_.isEmpty(pictures) || _.isEmpty(pictures[0])) {
            //Default image
            return "/images/panier.jpg";
        }
        if (type === 'light') {
            return "/" + this.get("LightPath") + "/" + pictures[0];
        }
        return "/" + this.get("HeavyPath") + "/" + pictures[0];
    }
});

ProductStockModel = Backbone.Model.extend({

    //Products id key is 'Id'
    idAttribute: "Id",

    defaults: {},

    urlRoot: "/api/productStock",

    unitsEnum: ["Kg", "L"],

    /*
      return the html string according to this product's sell type
     */
    getSellTypeString: function () {
        if (this.get("Product").get("Type") === 0) {
            return "Au poids";
        } else if (this.get("Product").get("Type") === 1) {
            return "À la pièce";
        } else if (this.get("Product").get("Type") === 2) {
            return "Emballé";
        } else if (this.get("Product").get("Type") === 3) {
            return "Poids variable";
        }
    },

    getStockUnitString: function () {
        if (this.get("Product").get("Type") == 1 || this.get("Product").get("Type") == 3) {
            return "Pièces";
        }
        var productUnit = this.get("Product").get("ProductUnit");
        return this.unitsEnum[productUnit];
    },

    getUnitPriceString: function () {
        var sellType = this.get("Product").get("Type");
        //A la piece
        if (sellType === 1) {
            return this.get("Product").get("UnitPrice") + " € l'unité";
        } else if (sellType === 0 || sellType === 2) { //Poids/emballé
            var weightStepPrice = parseFloat(this.get("Product").get("WeightPrice"));
            weightStepPrice = weightStepPrice * (parseFloat(this.get("Product").get("QuantityStep")) / 1000);
            return weightStepPrice.toFixed(2) + " € pour " + this.get("Product").get("QuantityStepString");
        } else if (sellType === 3) { //Poids variable
            var txt = "<small>De " + this.get("Product").get("MinimumWeight") + " " + this.unitsEnum[this.get("Product").get("ProductUnit")] + " (" + this.get("Product").get("MinimumPrice") + "€)";
            txt += " à " + this.get("Product").get("MaximumWeight") + " " + this.unitsEnum[this.get("Product").get("ProductUnit")] + " (" + this.get("Product").get("MaximumPrice") + "€)</small>";
            return txt;
        }
    },

    getVolumePriceString: function () {
        var productUnit = this.get("Product").get("ProductUnit");

        if (this.get("Product").get("Type") === 3) {
            var tooltipText = "Produit vendu au poids variable. Le prix moyen est à titre informatif. Le poids ainsi que le prix définitif vous seront communiqués lors de la récupération des produits";
            return "<label>≈ " + this.get("Product").get("UnitPrice") + " € (" + this.get("Product").get("WeightPrice") + "€/" + this.unitsEnum[productUnit] + ") <i data-toggle=\"tooltip\" title=\"" + tooltipText + "\" style=\"display: inline;\" class=\"far fa-question-circle\"></i></label>";
        } else {
            if (this.get("Product").get("WeightPrice") === 0 || this.get("Product").get("QuantityStep") === 1000) {
                return "";
            }
            return "(" + this.get("Product").get("WeightPrice") + " € / " + this.unitsEnum[productUnit] + ")";
        }
    },

    getSellStepString: function () {
        //Piece et poids variable
        if (this.get("Product").get("Type") === 1 || this.get("Product").get("Type") === 3) {
            return " Pièce(s)";
        }
        var productUnit = this.get("Product").get("ProductUnit");
    },

    getStockManagementString: function () {
        switch (this.get("Product").get("StockManagement")) {
            case 0:
                return "A la semaine";
            case 1:
                return "Stock fixe";
            case 2:
                return "Illimité";
            default:
                return "";
        }
    },

    //retourne la string correctement formattee pour le poids donne en grammes
    prettyPrintQuantity: function (weight) {
        if (weight >= 1000) {
            return (weight / 1000) + " " + largeunit;
        }
        return weight + " " + productUnit;
    },

    //Stock restant en fonction du type de vente et de la quantity step
    getRemainingQuantityStock: function () {
        // Stock illimité
        if (this.get("Product").get("StockManagement") === 2) {
            return Infinity;
        }
        if (this.get("Product").get("Type") === 1 || this.get("Product").get("Type") === 3) {
            return this.get("RemainingStock");
        } else {
            return (this.get("RemainingStock") * this.get("Product").get("QuantityStep")) / 1000;
        }
    },

    //Quantity already ordered by consumers this week
    getWeekQuantityOrdered: function () {
        if (this.get("Product").get("StockManagement") === 2) {
            return 0; //NA
        }
        var diff = this.get("WeekStock") - this.get("RemainingStock");
        if (this.get("Product").get("Type") === 1 || this.get("Product").get("Type") === 3) {
            return diff;
        } else {
            return (diff * this.get("Product").get("QuantityStep")) / 1000;
        }
    },

    //Stock restant en fonction du type de vente et de la quantity step
    getWeekQuantityStock: function () {
        if (this.get("Product").get("Type") === 1 || this.get("Product").get("Type") === 3) {
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

//Actually ProductStocks...
ProductsModel = Backbone.Collection.extend(
    {
        defaults: [],

        model: ProductStockModel,

        url: "/api/Products",

        initialize: function () {
            this.fetch();
        },

        getProductsForFamily: function (family) {
            var products = [];
            this.forEach(function (productModel) {
                var productFamilly = productModel.get("Product").get("Familly");
                if (!_.isEmpty(productFamilly)) {
                    if (productFamilly.FamillyName === family) {
                        products.push(productModel);
                    }
                }
            });
            return products;
        },

        getProductsForType: function (typeName) {
            var products = [];
            this.forEach(function (productModel) {
                var familly = productModel.get("Product").get("Familly");
                if (!_.isEmpty(familly)) {
                    if (familly.Type.Name === typeName) {
                        products.push(productModel);
                    }
                }
            });
            return products;
        }
    }
);

ProducerProductStockCollection = Backbone.Collection.extend(
    {
        defaults: [],

        model: ProductStockModel,

        url: function () {
            return "/api/publicProducerProducts?producerStolonId=" + this.producerId;
        },

        //producerId is the adherentStolonId
        initialize: function (producerId) {
            this.producerId = producerId;
        },

        getProductsForFamily: function (family) {
            var products = [];
            this.forEach(function (productModel) {
                var productFamilly = productModel.get("Product").get("Familly");
                if (!_.isEmpty(productFamilly)) {
                    if (productFamilly.FamillyName === family) {
                        products.push(productModel);
                    }
                }
            });
            return products;
        },

        getProductsForType: function (typeName) {
            var products = [];
            this.forEach(function (productModel) {
                var familly = productModel.get("Product").get("Familly");
                if (!_.isEmpty(familly)) {
                    if (familly.Type.Name === typeName) {
                        products.push(productModel);
                    }
                }
            });
            return products;
        }
    }
);

window.ProductsCollection = ProductsModel;
window.ProductStockModel = ProductStockModel;
