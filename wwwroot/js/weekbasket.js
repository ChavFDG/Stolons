window.WeekBasket = {};

WeekBasket = window.WeekBasket;

WeekBasket.roundPrice = function (decimal) {
    return Math.round(decimal * 100) / 100;
};

ProductTypeModel = Backbone.Model.extend({

    defaults: {},

    idAttribute: "Id",

    parse: function (data) {
        data["ProductFamilly"] = _.sortBy(data["ProductFamilly"], "FamillyName");
        return data;
    }
});

ProductTypesModel = Backbone.Collection.extend({

    url: "/api/ProductTypes",

    model: ProductTypeModel,

    initialize: function () {
        this.fetch();
    },

    comparator: "Name"
});

BillEntryModel = Backbone.Model.extend({

    idAttribute: "Id"

});

TmpWeekBasketModel = Backbone.Model.extend(
    {
        url: "/api/TmpWeekBasket",

        idAttribute: "Id",

        isEmpty: function () {
            return _.isEmpty(this.get("BillEntries"));
        },

        canPurchase: function () {
            //TODO move this elsewhere and handle the unconnected case
            //If there is no id, it means the user is not authenticated, so he can't purchase
            return !_.isEmpty(this.get("Id"));
        },

        getTotal: function () {
            var total = 0;
            _.forEach(this.get("BillEntries"), function (billEntry) {
                total += billEntry.Price;
            });
            return total;
        },

        getProductEntry: function (productId) {
            var productEntry;
            _.forEach(this.get("BillEntries"), function (billEntry) {
                if (billEntry.ProductStock.Id == productId) {
                    productEntry = billEntry;
                    return false;
                }
            });
            return productEntry;
        },

        addProductToBasket: function (productStockId) {
            var self = this;
            return $.ajax({
                url: '/api/addtobasket',
                data: {
                    productStockId: productStockId,
                    weekBasketId: self.get("Id")
                },
                type: 'post',
                success: function (response) {
                    if (response) {
                        self.set(response);
                    }
                }
            });
        },

        incrementProduct: function (productStockId) {
            var self = this;
            return $.ajax({
                url: '/api/incrementProduct',
                data: {
                    productStockId: productStockId,
                    weekBasketId: self.get("Id")
                },
                type: 'post',
                success: function (response) {
                    if (response) {
                        self.set(response);
                    }
                }
            });
        },

        decrementProduct: function (productStockId) {
            var self = this;
            return $.ajax({
                url: '/api/decrementProduct',
                data: {
                    productStockId: productStockId,
                    weekBasketId: self.get("Id")
                },
                type: 'post',
                success: function (response) {
                    if (response) {
                        self.set(response);
                    }
                }
            });
        },

        removeBillEntry: function (productStockId) {
            var self = this;
            return $.ajax({
                url: '/api/removeBillEntry',
                data: {
                    productStockId: productStockId,
                    weekBasketId: self.get("Id")
                },
                type: 'post',
                success: function (response) {
                    if (response) {
                        self.set(response);
                    }
                }
            });
        },

        resetBasket: function () {
            var self = this;
            return $.ajax({
                url: '/api/resetBasket',
                data: {
                    weekBasketId: self.get("Id")
                },
                type: 'post',
                success: function (response) {
                    self.set(response);
                }
            });
        }
    });

ValidatedWeekBasketModel = Backbone.Model.extend({

    url: "/api/validatedWeekBasket",

    initialize: function () {
        this.fetch();
    },

    getTotal: function () {
        var total = 0;
        _.forEach(this.get("BillEntries"), function (billEntry) {
            total += billEntry.Price;
        });
        return total;
    },

    //return true if the basket was validated this week
    exists: function () {
        return !_.isEmpty(this.get("BillEntries"));
    },

    isEmpty: function () {
        return _.isEmpty(this.get("BillEntries"));
    },

    getProductEntry: function (productId) {
        var productEntry;
        _.forEach(this.get("BillEntries"), function (billEntry) {
            if (billEntry.ProductStock.Id == productId) {
                productEntry = billEntry;
                return false;
            }
        });
        return productEntry;
    }
});

/* ---------------------------  Bellow Views definitions -------------------- */


ProductModalView = Backbone.View.extend({

    el: "#productModal",

    template: _.template($("#productModalTemplate").html()),

    initialize: function () {
    },

    open: function (productId) {
        this.currentProduct = WeekBasket.ProductsModel.get(productId);
        WeekBasket.TmpWeekBasketModel.on("sync change", this.render, this);
        this.renderModal();
    },

    onClose: function () {
        this.currentProduct = null;
        this.$el.off('hide.bs.modal');
        this.$el.empty();
        WeekBasket.TmpWeekBasketModel.off("sync change", this.render, this);
    },

    render: function () {
        this.$el.html(this.template({
            product: this.currentProduct.get("Product").toJSON(),
            productModel: this.currentProduct.get("Product"),
            productStock: this.currentProduct.toJSON(),
            productStockModel: this.currentProduct
        }));
    },

    renderModal: function () {
        this.render();
        this.$el.modal({ keyboard: true, show: true });
        this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
    }
});


ProducerModalView = Backbone.View.extend({

    el: "#producerModal",

    template: _.template($("#producerModalTemplate").html()),

    open: function (productId) {
        this.renderModal(WeekBasket.ProductsModel.get(productId).get("Product").get("Producer"));
    },

    onClose: function () {
        this.currentProduct = null;
        this.$el.off('hide.bs.modal');
        this.$el.empty();
    },

    renderModal: function (producer) {
        this.$el.html(this.template({ producer: producer }));
        this.$el.modal({ keyboard: true, show: true });
        this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
    }
});

ProductView = Backbone.View.extend(
    {
        template: _.template($("#productTemplate").html()),

        initialize: function (args) {
            this.productId = args.productId;
            this.el = args.el;
            this.model = args.model;
            this.model.on("sync change", this.render, this);
        },

        render: function () {
            this.$el.html(this.template({
                productStock: this.model.toJSON(),
                productStockModel: this.model,
                product: this.model.get("Product").toJSON(),
                productModel: this.model.get("Product")
            }));
	    //Force tooltips
	    $('[data-toggle="tooltip"]').tooltip({ container: 'body', html: true });
        }
    }
);

ProductActionView = Backbone.View.extend(
    {
        template: _.template($("#productActionTemplate").html()),

        events: {
            "click .minus": "decrement",
            "click .plus": "increment",
            "click .addProductBtn": "addToBasket"
        },

        initialize: function (args) {
            this.el = args.el;
            this.productId = args.productId;
            this.model = args.model;
            this.model.on("sync change", this.basketChanged, this);
        },

        basketChanged: function () {
            this.billEntry = this.model.getProductEntry(this.productId);
            this.render();
        },

        canIncrement: function () {
            if (!this.billEntry) {
                return false;
            }
            //Infinite stock
            if (this.billEntry.ProductStock.Product.StockManagement == 2) {
                return true;
            }
            var validatedBillEntry = WeekBasket.ValidatedWeekBasketModel.getProductEntry(this.productId);
            var validatedQty = (validatedBillEntry && validatedBillEntry.Quantity) || 0;
            var diffQty = this.billEntry.Quantity - validatedQty;
            var stepStock = this.billEntry.ProductStock.RemainingStock;

            return diffQty < stepStock;
        },

        canAddToBasket: function () {
            var validatedBillEntry = WeekBasket.ValidatedWeekBasketModel.getProductEntry(this.productId);
            var validatedQty = (validatedBillEntry && validatedBillEntry.Quantity) || 0;
            var productModel = WeekBasket.ProductsModel.get(this.productId);

            if (!this.billEntry) {
                //Infinite stock
                if (productModel.get("Product").get("StockManagement") == 2) {
                    return true;
                }
                if (productModel.get("RemainingStock") + validatedQty > 0) {
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        },

        addToBasket: function () {
            this.$(".productQuantityChanger").addClass("hidden");
            this.$(".productQuantityLoading").removeClass("hidden");
            WeekBasket.TmpWeekBasketModel.addProductToBasket(this.productId).then(_.bind(function () {
                this.$(".productQuantityChanger").removeClass("hidden");
                this.$(".productQuantityLoading").addClass("hidden");
                $("#basket__wrapper").animate({ scrollTop: $("#basket__wrapper").height() }, 200)
            }, this));
            return false;
        },

        increment: function () {
            this.$(".productQuantityChanger").addClass("hidden");
            this.$(".productQuantityLoading").removeClass("hidden");
            WeekBasket.TmpWeekBasketModel.incrementProduct(this.productId).then(_.bind(function () {
                this.$(".productQuantityLoading").addClass("hidden");
                this.$(".productQuantityChanger").removeClass("hidden");
            }, this));
            return false;
        },

        decrement: function () {
            this.$(".productQuantityChanger").addClass("hidden");
            this.$(".productQuantityLoading").removeClass("hidden");
            WeekBasket.TmpWeekBasketModel.decrementProduct(this.productId).then(_.bind(function () {
                this.$(".productQuantityLoading").addClass("hidden");
                this.$(".productQuantityChanger").removeClass("hidden");
            }, this));
            return false;
        },

        render: function () {
            var that = this;
            this.$el.html(that.template({
                billEntry: that.billEntry,
                canAddToBasket: _.bind(that.canAddToBasket, that),
                canIncrement: _.bind(that.canIncrement, that)
            }));
        }
    }
);

ProductsView = Backbone.View.extend(
    {
        el: "#products",

        template: _.template($("#productsTemplate").html()),

        initialize: function (args) {
            this.model = args.model;
            this.model.on("sync", this.render, this);
            this.tmpBasketModel = args.tmpBasketModel;
            this.views = {};
        },

        //TODO here group by categories
        render: function () {
            this.$el.html(this.template({ products: this.model, productTypes: WeekBasket.ProductTypesModel.toJSON() }));
            this.model.forEach(function (productStockModel) {
                var productView = new ProductView({
                    el: "#product-" + productStockModel.get("Id"),
                    model: productStockModel
                });
                this.views[productStockModel.get("Id")] = productView;
                productView.render();
                var productActionView = new ProductActionView(
                    {
                        model: this.tmpBasketModel,
                        productId: productStockModel.get("Id"),
                        el: "#product-" + productStockModel.get("Id") + " .pr_actions"
                    });
                productActionView.render();
                //Hide loading
                $('#loading').toggleClass('hidden', true);
                $('#topButton').toggleClass('hidden', false);
                $('#filtersPanelGroup').toggleClass('hidden', false);

            }, this);
        }
    }
);

TmpWeekBasketView = Backbone.View.extend(
    {
        el: "#tmpBasket",

        template: _.template($("#tmpWeekBasketTemplate").html()),

        initialize: function (args) {
            this.model = args.model;
            this.model.on("sync change", this.render, this);
            this.validatedBasketModel = args.validatedBasketModel;
        },

        render: function () {
            this.$el.html(this.template({ tmpBasketModel: this.model, tmpBasket: this.model.toJSON(), validatedBasketModel: this.validatedBasketModel }));
        }
    }
);

ValidatedWeekBasketView = Backbone.View.extend(
    {
        el: "#validatedBasket",

        template: _.template($("#validatedWeekBasketTemplate").html()),

        events: {
            "click .validatedBasketCollapse": "toggleCollapse"
        },

        initialize: function (args) {
            this.model = args.model;
            this.model.on("sync change", this.render, this);
            this.tmpBasketModel = args.tmpBasketModel;
            this.tmpBasketModel.on("sync change", this.render, this);
        },

        toggleCollapse: function () {
            if (this.$("#collapsible").hasClass("in")) {
                this.collapse(true);
            } else {
                this.collapse(false);
            }
        },

        collapse: function (hide) {
            if (hide) {
                this.$("#collapsible").collapse("hide");
                this.$(".glyphicon-collapse-up").addClass("hidden");
                this.$(".glyphicon-collapse-down").removeClass("hidden");
            } else {
                this.$("#collapsible").collapse("show");
                this.$(".glyphicon-collapse-up").removeClass("hidden");
                this.$(".glyphicon-collapse-down").addClass("hidden");
            }
        },

        render: function () {
            this.$el.html(this.template({
                tmpBasket: this.tmpBasketModel.toJSON(),
                validatedBasketModel: this.model,
                validatedBasket: this.model.toJSON()
            }));
        }
    }
);

var initModels = function () {
    var def = $.Deferred();
    WeekBasket.ProductTypesModel = new ProductTypesModel();
    WeekBasket.ProductsModel = new ProductsModel();
    WeekBasket.ValidatedWeekBasketModel = new ValidatedWeekBasketModel();
    WeekBasket.TmpWeekBasketModel = new TmpWeekBasketModel();
    WeekBasket.ProductsModel.on("sync", function () {
        WeekBasket.TmpWeekBasketModel.fetch();
    }, this);
};

var initViews = function () {

    window.ProductModalView = new ProductModalView();

    window.ProducerModalView = new ProducerModalView();

    //WeekBasket.FiltersView = new FiltersView({ model: WeekBasket.ProductTypesModel, productsModel: WeekBasket.ProductsModel });

    WeekBasket.ValidatedWeekBasketView = new ValidatedWeekBasketView(
        {
            model: WeekBasket.ValidatedWeekBasketModel,
            tmpBasketModel: WeekBasket.TmpWeekBasketModel
        });

    WeekBasket.ProductsView = new ProductsView({ model: WeekBasket.ProductsModel, tmpBasketModel: WeekBasket.TmpWeekBasketModel });

    WeekBasket.TmpWeekBasketView = new TmpWeekBasketView({
        model: WeekBasket.TmpWeekBasketModel,
        validatedBasketModel: WeekBasket.ValidatedWeekBasketModel,
        validatedBasketView: WeekBasket.ValidatedWeekBasketView
    });
};

var bootstrapWeekBasket = function () {

    initModels();
    initViews();

};

$(function () {
    bootstrapWeekBasket();
});
