
ProductTypesModel = Backbone.Collection.extend({
    url: "/api/ProductTypes",

    initialize: function() {
	this.fetch();
    }
});

//Juste pour la gestion de quelques evenements sur la vue globale
ManageProductView = Backbone.View.extend(
    {

	el: "body",

	events: {
	    "change #SellType": "sellTypeChanged",
	    "input #Product_QuantityStep": "updatePriceField",
	    "input #price": "updatePriceField",
	    "change #hideVolumePrice": "toggleVolumePriceField"
	},

	initialize: function() {
	    this.sellTypeChanged();
	},

	sellTypeChanged: function() {
	    var sellType = $("#SellType").val();

	    if (sellType == 1) {
		//Vente à la pièce, on desactive tout ce qui concerne le poids
		$("#productWeightUnit").addClass("hidden");
		$("#productQtyStep").addClass("hidden");
		$("#productAvgWeight").addClass("hidden");
		$("#pieceHideVolumePrice").removeClass("hidden");
	    } else {
		$("#productWeightUnit").removeClass("hidden");
		$("#productQtyStep").removeClass("hidden");
		$("#productAvgWeight").removeClass("hidden");
		$("#pieceHideVolumePrice").addClass("hidden");
	    }
	    this.updatePriceField();
	    this.updateVolumePriceField();
	},

	updatePriceField: function() {
	    $("#price").val($("#price").val().replace(',', '.'));
	    var sellType = $("#SellType").val();

	    if (sellType == 1) {
		$("#unitPrice").removeAttr("readonly");
	    } else {
		$("#unitPrice").attr("readonly", true);
		var price = $("#price").val();
		var qtyStep = $("#Product_QuantityStep").val();
		if (price && qtyStep) {
		    $("#unitPrice").val(price * qtyStep / 1000);
		    $("#unitPrice").attr("value", price * qtyStep / 1000);
		}
	    }
	},

	updateVolumePriceField: function() {
	    var selected = $("#hideVolumePrice").is(':checked');
	    var sellType = $("#SellType").val();
	    var price = $("#price").val();

	    if (!selected && sellType == 1 && price == 0) {
		$("#hideVolumePrice").prop("checked", true);
		$("#price").attr("readonly", true);
	    } else {
		$("#hideVolumePrice").prop("checked", false);
		$("#price").removeAttr("readonly");
	    }
	},

	toggleVolumePriceField: function() {
	    var sellType = $("#SellType").val();
	    var selected = $("#hideVolumePrice").is(':checked');

	    if (selected) {
		$("#price").attr("readonly", true);
		$("#price").val(0);
	    } else if (sellType != 1) {
		$("#price").removeAttr("readonly");
	    } else {
		$("#price").removeAttr("readonly");
	    }
	}
    }
);

ProductTypesView = Backbone.View.extend({

    el: "#famillySelect",

    template: _.template($("#familiesTemplate").html()),

    initialize: function(args) {
	this.model = args.model;
	this.listenTo(this.model, 'sync change', this.render);
	this.selectedFamily = "Tous";
    },

    onOptionSelected: function(selectedData) {
	this.selectedFamily = selectedData.params.data.id || "Tous";
    },

    selectElemTemplate: function(elem) {
	if (!elem.id) {
	    return elem.text;
	}
	var dataImage = $(elem.element).data("image");
	if (!dataImage) {
	    return elem.text;
	} else {
	    return $('<span class="select-option"><img src="/' + dataImage +'" />' + $(elem.element).text() + '</span>');
	}
    },

    render: function() {
	var currentFamilly = $("#productFamilly").text() || "Tous";
	currentFamilly = currentFamilly.trim();
	this.$el.html(this.template({ currentFamilly: currentFamilly, productTypes: this.model.toJSON() }));
	this.$('#familiesDropDown').select2({
	    minimumResultsForSearch: Infinity,
	    templateResult: this.selectElemTemplate,
	    templateSelection: this.selectElemTemplate
	});
	this.$('#familiesDropDown').on("select2:select", _.bind(this.onOptionSelected, this));
    }
});

$(function() {

    var productTypesModel = new ProductTypesModel();

    var view = new ProductTypesView({model: productTypesModel});

    var manageProductView = new ManageProductView({});

    $("#UploadFile1").change(function(){
	readURL(this);
    });

});


function readURL(input) {
    if (input.files && input.files[0]) {
	var reader = new FileReader();

	reader.onload = function (e) {
	    $('#image1Preview').attr('src', e.target.result);
	}

	reader.readAsDataURL(input.files[0]);
    }
}
