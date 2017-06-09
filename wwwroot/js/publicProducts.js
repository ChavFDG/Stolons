
window.PublicProducts = {};

PublicProducts = window.PublicProducts;

ProductTypesModel = Backbone.Collection.extend({

    idAttribute: "Id",

    url: "/api/ProductTypes",

    initialize: function() {
	this.fetch();
    },

    parse: function(data) {
	_.forEach(data, function(item, idx) {
	    data[idx].id = item.Id;
	});
	return data;
    }
});

ProductsModel = Backbone.Collection.extend(
    {
	defaults: [],

	model: ProductModel,

	idAttribute: "Id",

	url: "/api/publicProducts",

	initialize: function() {
	    this.fetch();
	},

	// parse: function(data) {
	//     _.forEach(data, function(item, idx) {
	// 	data[idx].id = item.Id;
	//     });
	//     return data;
	// }
    }
);

function initModels() {
    PublicProducts.ProductTypesModel = new ProductTypesModel();
    PublicProducts.ProductsModel = new ProductsModel();
}

/* ---------------------------  Bellow Views definitions -------------------- */

ProducerViewModal = Backbone.View.extend({

    el: "#producerModal",

    template: _.template($("#producerModalTemplate").html()),

    initialize: function() {
    },

    open: function(productId) {
	this.currentProducer = PublicProducts.ProductsModel.get(productId).get("Producer");
	this.renderModal();
    },

    onClose: function() {
	this.currentProducer = null;
	this.$el.off('hide.bs.modal');
	this.$el.empty();
    },

    render: function() {
	this.$el.html(this.template({producer: this.currentProducer}));
    },

    renderModal: function() {
	this.render();
	this.$el.modal({keyboard: true, show: true});
	this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
    }
});

ProductModalView = Backbone.View.extend({

    el: "#productModal",

    template: _.template($("#productModalTemplate").html()),

    open: function(productId) {
	this.currentProduct = PublicProducts.ProductsModel.get(productId);
	this.renderModal();
    },

    onClose: function() {
	this.currentProduct = null;
	this.$el.off('hide.bs.modal');
	this.$el.empty();
    },

    render: function() {
	this.$el.html(this.template({product: this.currentProduct.toJSON(), productModel: this.currentProduct}));
    },

    renderModal: function() {
	this.render();
	this.$el.modal({keyboard: true, show: true});
	this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
    }
});


FiltersView = Backbone.View.extend({

    el: "#filters",

    template: _.template($("#filtersTemplate").html()),

    initialize: function(args) {
	this.model = args.model;
	this.productsModel = args.productsModel;
	this.model.on('sync', this.initTreeData, this);
	this.selectedFamily = "Tous";
    },

    initTreeData: function() {
	this.treeData = this.model.toJSON();
	data = this.treeData;
	_.forEach(data, function(category, idx) {
	    data[idx].id = category.Id;
	    data[idx].text = category.Name;
	    data[idx].icon = category.Image ? "/" + category.Image : "/images/productFamilies/default.jpg";
	    data[idx].children = category.ProductFamilly;
	    data[idx].category = true;
	    _.forEach(data[idx].children, function(family, fIdx) {
		data[idx].children[fIdx].id = family.Id;
		data[idx].children[fIdx].text = family.FamillyName;
		data[idx].children[fIdx].icon = family.Image ? "/" + family.Image : "/images/productFamilies/default.jpg";
		data[idx].children[fIdx].category = false;
	    });
	});
	this.treeData = [{
	    id: "Tous",
	    text: "Tous",
	    icon: false,
	    children: this.treeData,
	    category: true,
	    state: {
		opened: true,
		selected: false
	    }
	}];
	this.render();
    },

    famillyMatch: function(product) {
	if (!this.selectedNode) {
	    return true;
	} else {
	    if (this.selectedNode.category) {
		if (this.selectedNode.id === "Tous") {
		    return true
		}
		if (!product.Familly || !product.Familly.Type) {
		    return false;
		}
		return product.Familly.Type && product.Familly.Type.Name == this.selectedNode.text;
	    } else {
		if (!product.Familly) {
		    return false;
		}
		return product.Familly.FamillyName === this.selectedNode.text;
	    }
	}
    },

    productNameMatch: function(product, searchTerm) {
	return _.isEmpty(searchTerm) || product.Name.toLowerCase().indexOf(searchTerm) != -1;
    },

    productDescMatch: function(product, searchTerm) {
	if (_.isEmpty(product.Description)) {
	    return false;
	}
	return _.isEmpty(searchTerm) || product.Description.toLowerCase().indexOf(searchTerm) != -1;
    },

    filterProducts: function() {
	var searchTerm = this.$("#search").val() || "";
	searchTerm = searchTerm.toLowerCase();
	var nbMatch = 0;
	this.productsModel.forEach(function(productModel) {
	    var product = productModel.toJSON();

	    if (this.famillyMatch(product) && (this.productNameMatch(product, searchTerm) || this.productDescMatch(product, searchTerm))) {
		$("#product-" + product.Id).removeClass("hidden");
		nbMatch++;
	    } else {
		$("#product-" + product.Id).removeClass("hidden").addClass("hidden");
	    }
	}, this);
	if (nbMatch === 0) {
	    $("#emptyProducts").removeClass("hidden");
	} else {
	    $("#emptyProducts").addClass("hidden");
	}
    },

    render: function() {
	this.$el.html(this.template({ productTypes: this.model.toJSON() }));
	$("#tree").jstree({
	    "core": {
		"data": this.treeData,
		"themes" : {
		    "variant" : "responsive"
		}
	    }
	});
	this.instance = $("#tree").jstree(true);
	this.registerEventsHandlers();
	this.$('#search').on("input", _.bind(function() {
	    this.filterProducts();
	}, this));
    },

    registerEventsHandlers: function() {
	$("#tree").on('changed.jstree', _.bind(this.nodeSelected, this));
    },

    nodeSelected: function(event, data) {
	this.selectedNode = data.node && data.node.original;
	this.filterProducts();
    }
});

ProductView = Backbone.View.extend(
    {
	template: _.template($("#productTemplate").html()),

	initialize: function(args) {
	    this.productId = args.productId;
	    this.el = args.el;
	    this.model = args.model;
	    this.model.on("sync change", this.render, this);
	},

	render: function() {
	    this.$el.html(this.template({product: this.model.toJSON(), productModel: this.model}));
	}
    }
);

ProductsView = Backbone.View.extend(
    {
	el: "#products",

	template: _.template($("#productsTemplate").html()),

	initialize: function(args) {
	    this.model = args.model;
	    this.model.on("sync", this.render, this);
	    this.views = {};
	},

	render: function() {
	    this.$el.html(this.template({products: this.model.models}));
	    this.model.forEach(function(productModel) {
		var productView = new ProductView({
		    el: "#product-" + productModel.get("Id"),
		    model: productModel
		});
		this.views[productModel.get("Id")] = productView;
		productView.render();
	    }, this);
	}
    }
);

function initViews() {

    //make the modal view global
    window.ProductModalView = new ProductModalView();
    window.ProducerModalView = new ProducerViewModal();

    PublicProducts.FiltersView = new FiltersView({model: PublicProducts.ProductTypesModel, productsModel: PublicProducts.ProductsModel});
    PublicProducts.ProductsView = new ProductsView({model: PublicProducts.ProductsModel});
}

$(function() {
    initModels();
    initViews();
});
