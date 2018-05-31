
ProductTypesModel = Backbone.Collection.extend({
    url: "/api/ProductTypes",

    initialize: function () {
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
            //"input #price": "updatePriceField",
	    "change #price": "updatePriceField",
            "change #HideWeightPrice": "hideWeightPrice",
	    "submit #productForm": "onSubmit"
        },

        initialize: function () {
            this.sellTypeChanged();

	    //Trigger form validation at page load
	    // $("[name='product-form']").valid({
	    // 	ignore: ":hidden, .skip",
	    // 	debug: true
	    // });
        },

        sellTypeChanged: function () {
            var sellType = $("#SellType").val();

            if (sellType == 1) {
                //Vente à la pièce, on desactive tout ce qui concerne le poids
                $("#productWeightUnit").toggleClass("hidden", true);
                $("#productQtyStep").toggleClass("hidden", true);
                $("#productAvgWeight").toggleClass("hidden", true);
                $("#pieceHideWeightPrice").toggleClass("hidden", false);

		//Hide automatically weightPrice on load if price == 0
		if (parseInt($("#price").val()) == 0) {
		    $("#HideWeightPrice").prop('checked', true);
		    this.hideWeightPrice();
		}
            } else {
                $("#productWeightUnit").toggleClass("hidden", false);
                $("#productQtyStep").toggleClass("hidden", false);
                $("#productAvgWeight").toggleClass("hidden", false);
                $("#pieceHideWeightPrice").toggleClass("hidden", true);
            }
            this.updatePriceField();
            this.updateVolumePriceField();
        },

        updatePriceField: function () {
            $("#price").val($("#price").val().replace('.', ','));
	    if (!this.validateWeightPrice()) {
		return false;
	    }
            var sellType = $("#SellType").val();
	    var price = $("#price").val();

            if (sellType == 1) {
                $("#unitPrice").removeAttr("readonly");
            } else {
                $("#unitPrice").attr("readonly", true);
                var qtyStep = $("#Product_QuantityStep").val();
                if (price && qtyStep) {
                    var tempPrice = price.replace(',', '.');
                    var unitPrice = (tempPrice * qtyStep / 1000);
                    if (unitPrice != 'NaN') {
                        var tempUnitPrice = unitPrice.toString().replace('.', ',');
                        $("#unitPrice").val(tempUnitPrice);
                    }
                }
            }
        },

	validateWeightPrice: function() {
	    var price = parseInt($("#price").val());
	    
	    if ($("#HideWeightPrice").is(':checked')) {
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
	
        updateVolumePriceField: function () {
            var selected = $("#HideWeightPrice").is(':checked');
            var sellType = $("#SellType").val();
            var price = $("#price").val();
        },

        hideWeightPrice: function () {
            var selected = $("#HideWeightPrice").is(':checked');

            if (selected) {
		$("#price").val(0);
		$("#weight-price-container").toggleClass("hidden", true);
		$("#price-error-container").toggleClass("hidden", true);
            } else if ($("#SellType").val() == 1) {
		$("#weight-price-container").toggleClass("hidden", false);
	    }
	    this.validateWeightPrice();
        },

	onSubmit: function(event) {
	    if (!this.validateWeightPrice($("#price").val())) {
		event.preventDefault();
		return false;
	    } else {
		console.log("price ok");
	    }
	    return true;
	}
    }
);

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
