
ProductTypesModel = Backbone.Collection.extend({
    url: "/api/ProductTypes",

    initialize: function () {
        this.fetch();
    }
});

ManageProductView = Backbone.View.extend(
    {

        el: "body",

        events: {
            "change #SellType": "sellTypeChanged",
	    "submit #productForm": "onSubmit"
        },

        initialize: function () {
            this.sellTypeChanged();
        },

        sellTypeChanged: function () {
            var sellType = $("#SellType").val();

	    if (this.subView) {
		this.subView.unbind();
		this.subView.undelegateEvents();
	    }
            if (sellType == 0 || sellType == 2) {
		this.subView = new SellTypeWeightView({parent: this});
            } else if (sellType == 1) {
		this.subView = new SellTypePieceView({parent: this});
            } else if (sellType == 3) {
		this.subView = new SellTypeVariableWeightView({parent: this});
	    }
        },

	validateWeightPrice: function() {
	    var price = parseInt($("#price").val());

	    if ($("#HideWeightPrice").is(':checked') && $("#HideWeightPrice").is(':visible')) {
		return true;
	    }
	    if ($("#price").is(':visible')) {
		if (_.isNaN(price) || price < 0.01) {
		    $("#price-error-container").toggleClass("hidden", false);
		    return false;
		} else {
		    $("#price-error-container").toggleClass("hidden", true);
		    return true;
		}
	    } else {
		$("#price-error-container").toggleClass("hidden", true);
		return true;
	    }
	    return true;
	},

	onSubmit: function(event) {
	    return this.subView.validate();
	}
    }
);

SellTypeWeightView = Backbone.View.extend({

    el: "#productForm",

    events: {
	"keyup #quantityStep": "updatePriceField",
	"keyup #price": "updatePriceField"
    },

    initialize: function(opts) {
	this.parent = opts.parent;
	$("#unitPrice").attr("readonly", true);
	$("#unitPriceContainer").toggleClass("hidden", false);
	$("#weight-price-container").toggleClass("hidden", false);
	$("#quantityStep").attr("readonly", false);
        $("#productWeightUnit").toggleClass("hidden", false);
        $("#productQtyStep").toggleClass("hidden", false);
        $("#productAvgWeight").toggleClass("hidden", false);
        $("#hideWeightPriceContainer").toggleClass("hidden", true);
	$("#minProductWeightContainer").toggleClass("hidden", true);
	$("#maxProductWeightContainer").toggleClass("hidden", true);
	$("#minPrice").toggleClass("hidden", true);
	$("#maxPrice").toggleClass("hidden", true);
	$("#meanPriceContainer").toggleClass("hidden", true);
	this.updatePriceField();
    },

    updatePriceField: function () {
        $("#price").val($("#price").val().replace('.', ','));

	if (!this.parent.validateWeightPrice()) {
	    return false;
	}
        var sellType = $("#SellType").val();
	var price = parseFloat($("#price").val().replace(',', '.'));

        if (sellType == 1) {
            $("#unitPrice").removeAttr("readonly");
        } else {
            $("#unitPrice").attr("readonly", true);
            var qtyStep = parseFloat($("#quantityStep").val());
            if (_.isNumber(price) && qtyStep) {
                var unitPrice = (price * qtyStep / 1000);
                if (unitPrice != 'NaN') {
                    var tempUnitPrice = unitPrice.toString().replace('.', ',');
                    $("#unitPrice").val(tempUnitPrice);
                }
            }
        }
    },

    validate: function() {
	return this.parent.validateWeightPrice();
    }
});

SellTypePieceView = Backbone.View.extend({

    el: "#productForm",

    events: {
	"change #HideWeightPrice": "hideWeightPrice"
    },

    initialize: function(opts) {
	this.parent = opts.parent;
	console.log("Sell type: piece");
	//Vente à la pièce, on desactive tout ce qui concerne le poids
	$("#unitPrice").attr("readonly", false);
	$("#unitPriceContainer").toggleClass("hidden", false);
	$("#quantityStep").attr("readonly", true);
        $("#productWeightUnit").toggleClass("hidden", true);
        $("#productQtyStep").toggleClass("hidden", true);
        $("#productAvgWeight").toggleClass("hidden", true);
        $("#hideWeightPriceContainer").toggleClass("hidden", false);
	$("#minProductWeightContainer").toggleClass("hidden", true);
	$("#maxProductWeightContainer").toggleClass("hidden", true);
	$("#minPrice").toggleClass("hidden", true);
	$("#maxPrice").toggleClass("hidden", true);
	$("#meanPriceContainer").toggleClass("hidden", true);

	//Hide automatically weightPrice on load if price == 0
	if (parseInt($("#price").val()) == 0) {
	    $("#HideWeightPrice").prop('checked', true);
	    this.hideWeightPrice();
	}
    },

    hideWeightPrice: function () {
        var selected = $("#HideWeightPrice").is(':checked');

        if (selected) {
	    $("#price").val(0);
	    $("#price-error-container").toggleClass("hidden", true);
	    $("#weight-price-container").toggleClass("hidden", true);
        } else {
	    console.log("show weight price");
	    $("#weight-price-container").toggleClass("hidden", false);
	}
	this.parent.validateWeightPrice();
    },

    validate: function() {
	return this.parent.validateWeightPrice();
    }
});

SellTypeVariableWeightView = Backbone.View.extend({

    el: "#productForm",

    events: {
	"input #minWeight": "updateVariablePrices",
	"input #maxWeight": "updateVariablePrices",
	"input #price": "updateVariablePrices",
    },

    initialize: function(opts) {
	this.parent = opts.parent;
	$("#unitPriceContainer").toggleClass("hidden", true);
	$("#weight-price-container").toggleClass("hidden", false);
	$("#unitPrice").attr("readonly", true);
	$("#quantityStep").attr("readonly", true);
	$("#minProductWeightContainer").toggleClass("hidden", false);
	$("#maxProductWeightContainer").toggleClass("hidden", false);
	$("#hideWeightPriceContainer").toggleClass("hidden", true);
	$("#minPrice").toggleClass("hidden", false);
	$("#maxPrice").toggleClass("hidden", false);
	$("#productQtyStep").toggleClass("hidden", true);
	$("#meanPriceContainer").toggleClass("hidden", false);
	this.updateVariablePrices();
    },

    updateVariablePrices: function() {
	var weightPrice = parseFloat($("#price").val().replace(",", "."));
	var minWeight = parseFloat($("#minWeight").val().replace(",", "."));
	var maxWeight = parseFloat($("#maxWeight").val().replace(",", "."));
	var meanWeight = ((maxWeight + minWeight) / 2);
	var meanPrice = meanWeight * weightPrice;

	if (this.validate()) {
	    $("#quantityStep").val(meanWeight * 1000);
	    $("#unitPrice").val(meanPrice.toFixed(2).toString().replace(".", ",")); //Just to be sure
	    $("#meanPrice").val(meanPrice.toFixed(2).toString().replace(".", ","));
	    $("#minimumPrice").val((minWeight * weightPrice).toFixed(2));
	    $("#maximumPrice").val((maxWeight * weightPrice).toFixed(2));
	    $("#price").val($("#price").val().replace('.', ','));
	    $("#minWeight").val($("#minWeight").val().replace('.', ','));
	    $("#maxWeight").val($("#maxWeight").val().replace('.', ','));
	}
    },

    validate: function() {
	var weightPrice = parseFloat($("#price").val().replace(",", "."));
	var minWeight = parseFloat($("#minWeight").val().replace(",", "."));
	var maxWeight = parseFloat($("#maxWeight").val().replace(",", "."));

	var valid = true;
	if (valid && (minWeight < 0.01 || _.isNaN(minWeight))) {
	    valid = false;
	}
	if (valid && (maxWeight < 0.01 || _.isNaN(maxWeight))) {
	    valid = false;
	}
	if (valid && maxWeight < minWeight) {
	    valid = false;
	}
	$("#minmax-weight-error-container").toggleClass("hidden", valid);
	return valid && this.parent.validateWeightPrice();
    }
});

ProductTypesView = Backbone.View.extend({

    el: "#famillySelect",

    template: _.template($("#familiesTemplate").html()),

    initialize: function (args) {
        this.model = args.model;
        this.listenTo(this.model, 'sync change', this.render);
        this.selectedFamily = "Tous";
    },

    onOptionSelected: function (selectedData) {
        this.selectedFamily = selectedData.params.data.id || "Tous";
    },

    selectElemTemplate: function (elem) {
        if (!elem.id) {
            return elem.text;
        }
        var dataImage = $(elem.element).data("image");
        if (!dataImage) {
            return elem.text;
        } else {
            return $('<span class="select-option"><img src="/' + dataImage + '" />' + $(elem.element).text() + '</span>');
        }
    },

    render: function () {
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

$(function () {

    var productTypesModel = new ProductTypesModel();

    var view = new ProductTypesView({ model: productTypesModel });

    var manageProductView = new ManageProductView({});

    $("#UploadFile1").change(function () {
        readURL(this);
    });

    var input = document.getElementById('image');
    input.onclick = function () {
        this.value = null;
    };

    input.onchange = function () {
        resizeImageToSpecificWidth(600, 'MainPictureHeavy');
        resizeImageToSpecificWidth(155, 'MainPictureLight');
    };
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

function resizeImageToSpecificWidth(width, type) {
    var input = document.getElementById('image');

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (event) {
            var img = new Image();
            img.onload = function () {
                var oc = document.createElement('canvas'), octx = oc.getContext('2d');
                oc.width = img.width;
                oc.height = img.height;
                octx.drawImage(img, 0, 0);
                while (oc.width * 0.5 > width) {
                    oc.width *= 0.5;
                    oc.height *= 0.5;
                    octx.drawImage(oc, 0, 0, oc.width, oc.height);
                }
                oc.width = width;
                oc.height = oc.width * img.height / img.width;
                octx.drawImage(img, 0, 0, oc.width, oc.height);

                if (type == 'MainPictureLight') {
                    document.getElementById('image1Preview').src = oc.toDataURL();
                }

                $('input[name=' + type + ']').val(oc.toDataURL());

            };
            img.src = event.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}
