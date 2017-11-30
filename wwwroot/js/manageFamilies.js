
ProductTypeModel = Backbone.Model.extend({

    idAttribute: "Id"
    
});

ProductTypesModel = Backbone.Collection.extend({

    model: ProductTypeModel,

    url: "/api/ProductTypes",

    initialize: function() {
	this.fetch();
    },

    renameCategory: function(category, newName) {
	var promise = $.ajax({
	    url: "/ProductsManagement/RenameCategory",
	    type: 'POST',
	    data: {
		categoryId: category.id,
		newCategoryName: newName
	    }
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    renameFamily: function(family, newName) {
	var promise = $.ajax({
	    url: "/ProductsManagement/RenameFamily",
	    type: 'POST',
	    data: {
		familyId: family.id,
		newFamilyName: newName
	    }
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    createCategory: function(categoryName) {
	var promise = $.ajax({
	    url: "/ProductsManagement/CreateCategory",
	    type: 'POST',
	    data: {
		categoryName: categoryName
	    }
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    createFamily: function(categoryId, familyName) {
	var promise = $.ajax({
	    url: "/ProductsManagement/CreateFamily",
	    type: 'POST',
	    data: {
		categoryId: categoryId,
		familyName: familyName
	    }
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    updateCategoryPicture: function(categoryId, pictureFormData) {
	pictureFormData.append("categoryId", categoryId);
	var promise = $.ajax({
	    url: "/ProductsManagement/UpdateCategoryPicture",
	    type: 'POST',
	    data: pictureFormData,
	    cache: false,
	    contentType: false,
	    processData: false
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    updateFamilyPicture: function(familyId, pictureFormData) {
	pictureFormData.append("familyId", familyId);
	var promise = $.ajax({
	    url: "/ProductsManagement/UpdateFamilyPicture",
	    type: 'POST',
	    data: pictureFormData,
	    cache: false,
	    contentType: false,
	    processData: false
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    deleteCategory: function(category) {
	var promise = $.ajax({
	    url: "/ProductsManagement/DeleteCategory",
	    type: 'POST',
	    data: {
		categoryId: category.Id
	    }
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    deleteFamily: function(family) {
	var promise = $.ajax({
	    url: "/ProductsManagement/DeleteFamily",
	    type: 'POST',
	    data: {
		familyId: family.Id
	    }
	});
	var self = this;
	promise.then(function() {
	    self.fetch();
	});
    },

    parse: function(data) {
	_.forEach(data, function(category, idx) {
	    data[idx].id = category.Id;
	    data[idx].text = category.Name;
	    data[idx].icon = category.Image ? "/" + category.Image : "/images/productFamilies/default.jpg";
	    data[idx].children = category.ProductFamilly;
	    //data[idx].state = {"opened": true};
	    data[idx].category = true;
	    _.forEach(data[idx].children, function(family, fIdx) {
		data[idx].children[fIdx].id = family.Id;
		data[idx].children[fIdx].text = family.FamillyName;
		data[idx].children[fIdx].icon = family.Image ? "/" + family.Image : "/images/productFamilies/default.jpg";
		data[idx].children[fIdx].category = false;
	    });
	});
	if (!_.isEmpty(this.models)) {
	    this.reset(data);
	}
	return data;
    }

});

MainView = Backbone.View.extend({

    el: "#manageCategories",

    events: {
	"click #addCategory": "addCategory",
	"click #addFamily": "addFamily",
	"click #rename": "rename",
	"click #updatePicture": "chosePicture",
	"click #delete": "deleteNode",
	"click #cancelDelete": "cancelDelete",
	"click #confirmDelete": "confirmDelete",
	"change #picture": "pictureChanged"
    },

    initialize: function(args) {
	this.model = args.model;
	this.model.once('sync', this.render, this);
	this.model.on('reset', this.reloadTreeData, this);
    },

    render: function() {
	this.selected = null;
	$("#tree").jstree({
	    "core": {
		"data": this.model.toJSON(),
		"check_callback": function() {
		    return true;
		},
		"themes" : {
		    "variant" : "responsive"
		}
	    },
	    "plugins" : [ "wholerow" ]
	    // "dnd": {
	    // 	"is_draggable": function() {
	    // 	    console.log(arguments);
		    
	    // 	    return true;
	    // 	},
	    // 	"check_while_dragging": true
	    // }
	});
	this.instance = $("#tree").jstree(true);
	this.registerEventsHandlers();
    },

    reloadTreeData: function() {
	this.instance.settings.core.data = this.model.toJSON();
	this.instance.refresh();
    },
    
    registerEventsHandlers: function() {
	$("#tree").on('changed.jstree', _.bind(this.nodeSelected, this));
	$("#tree").on('rename_node.jstree', _.bind(this.nodeRenamed, this));
	$("#tree").on('create_node.jstree', _.bind(this.nodeCreated, this));
    },

    addCategory: function() {
	var self = this;
	//var newNode = this.instance.create_node(null, "Catégorie");
	this.model.createCategory("Catégorie");
	//this.instance.edit(newNode);
    },

    addFamily: function() {
	this.instance.open_node(this.selectedNode);
	//var newNode = this.instance.create_node(this.selectedNode, "Famille");
	this.model.createFamily(this.selectedNode.original.id, "Famille");
	//this.instance.edit(newNode);
    },

    rename: function() {
	if (this.instance && this.selectedNode) {
	    this.instance.edit(this.selectedNode);
	} else {
	    console.log("No selected node to edit");
	}
    },

    chosePicture: function() {
	$("#picture").trigger('click');
    },

    pictureChanged: function(event) {
	console.log("image changed", arguments);
	var formData = new FormData($("#pictureForm")[0]);
	if (this.selectedNode.original.category) {
	    this.model.updateCategoryPicture(this.selectedNode.original.Id, formData);
	} else {
	    this.model.updateFamilyPicture(this.selectedNode.original.Id, formData);
	}
    },

    deleteNode: function() {
	$("#deleteConfirmation").removeClass("hidden");
    },

    cancelDelete: function() {
	$("#deleteConfirmation").addClass("hidden");
    },

    confirmDelete: function() {
	if (this.selectedNode && this.selectedNode.original.category) {
	    this.model.deleteCategory(this.selectedNode.original);
	} else {
	    this.model.deleteFamily(this.selectedNode.original);
	}
	$("#deleteConfirmation").addClass("hidden");
    },

    nodeSelected: function(event, data) {
	var node = data.node;

	if (data.action === "select_node") {
	    this.selectedNode = node;
	} else {
	    this.selectedNode = null;
	}
	if (this.selectedNode) {
	    if (this.selectedNode.original && this.selectedNode.original.CanBeRemoved === true) {
		$("#delete").attr("disabled", false);
	    } else {
		$("#delete").attr("disabled", true);
	    }
	    $("#rename").attr("disabled", false);
	    $("#updatePicture").attr("disabled", false);
	} else {
	    $("#delete").attr("disabled", true);
	    $("#rename").attr("disabled", true);
	    $("#updatePicture").attr("disabled", true);
	}
	$("#deleteConfirmation").addClass("hidden");
	if (this.selectedNode && this.selectedNode.original.category) {
	    $("#addFamily").attr("disabled", false);
	} else {
	    $("#addFamily").attr("disabled", true);
	}
    },

    nodeRenamed: function(event, data) {
	if (data.node.original.category) {
	    this.model.renameCategory(data.node.original, data.text);
	} else {
	    this.model.renameFamily(data.node.original, data.text);
	}
    },

    nodeCreated: function(event, data) {
	var parentNode;
	if (data.node.parent !== "#") {
	    parentNode = this.instance.get_node(data.node.parent);
	}
	console.log(parentNode);
	if (!parentNode) {
	    this.model.createCategory(data.node.text);
	} else {
	    this.model.createFamily(parentNode, data.node.text);
	}
    }

});

$(function() {

    var productTypesModel = new ProductTypesModel();

    //var categoriesView = new CategoriesView({model: productTypesModel});

    var mainView = new MainView({model: productTypesModel});

});
