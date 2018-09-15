
window.BillsManagement = {}

BillsManagement = window.BillsManagement;

$(function () {
    BillsManagement.CorrectionView = Backbone.View.extend({

        el: "#correctionModal",

        template: _.template($("#correctionModalTemplate").html()),

        events: {
            "click .minus": "decrement",
            "click .plus": "increment",
            "click .decrementTotal": "decrementProductTotal",
            "click .incrementTotal": "incrementProductTotal",
            "keyup #correction-reason": "reasonChanged",
            "click #validateCorrection": "save"
        },

        initialize: function (model) {
            this.model = model;
            this.render();
        },

	doubleScroll: function(element) {
	    var scrollbar = document.createElement('div');
	    scrollbar.appendChild(document.createElement('div'));
	    scrollbar.style.overflow = 'auto';
	    scrollbar.style.overflowY = 'hidden';
	    scrollbar.firstChild.style.width = element.scrollWidth+'px';
	    scrollbar.firstChild.style.paddingTop = '1px';
	    scrollbar.firstChild.appendChild(document.createTextNode('\xA0'));
	    scrollbar.onscroll = function() {
		element.scrollLeft = scrollbar.scrollLeft;
	    };
	    element.onscroll = function() {
		scrollbar.scrollLeft = element.scrollLeft;
	    };
	    element.parentNode.insertBefore(scrollbar, element);
	},

        render: function () {
            this.$el.html(this.template({
                view: this,
                producerBill: this.model
            }));
	    //this.doubleScroll(document.getElementById('correction-table'));
            this.validate();
        },

        open: function () {
            this.$el.modal({ keyboard: true, show: true });
            this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
            //Initialize changed billEntries
            this.billEntries = {};
        },

        onClose: function () {
            this.$el.off('hide.bs.modal');
            this.$el.empty();
            this.model.off("sync change", this.render, this);
        },

        decrement: function (event) {
            var billEntryId = $(event.currentTarget).data("bill-entry-id");
            var billEntry = this.model.getBillEntryById(billEntryId);

            if (billEntry) {
                this.billEntries[billEntryId] = billEntry;
                billEntry.Quantity -= 1;
                if (billEntry.Quantity <= 0) {
                    billEntry.Quantity = 0;
                }
                this.render();
            }
            event.preventDefault();
            return false;
        },

        increment: function (event) {
            var billEntryId = $(event.currentTarget).data("bill-entry-id");
            var billEntry = this.model.getBillEntryById(billEntryId);

            if (billEntry) {
                this.billEntries[billEntryId] = billEntry;
                billEntry.Quantity += 1;
                this.render();
            }
            event.preventDefault();
            return false;
        },

        decrementProductTotal: function (event) {
            var productStockId = $(event.currentTarget).data("product-stock-id");

            if (productStockId) {
                this.model.productStocksTotals[productStockId] -= 1;
                if (this.model.productStocksTotals[productStockId] <= 0) {
                    this.model.productStocksTotals[productStockId] = 0;
                }
                this.render();
            }
            event.preventDefault();
            return false;
        },

        incrementProductTotal: function (event) {
            var productStockId = $(event.currentTarget).data("product-stock-id");

            if (productStockId) {
                this.model.productStocksTotals[productStockId] += 1;
                this.render();
            }
            event.preventDefault();
            return false;
        },

        validate: function () {
            var that = this;
            this.valid = true;

            // validates that the total product quantity is equal to the sum of the quantities in billentries.
            _.each(this.model.getProductStocks(), function (productStock) {
		if (that.model.isAssignedVariableWeigh(productStock) == true) {
		    return;
		}
                var productStockId = productStock.get("Id");
                var billEntries = that.model.getBillEntriesForProductStock(productStockId);
                var totalEntriesQty = 0;
                _.forEach(billEntries, function (entry) {
                    totalEntriesQty += entry.Quantity;
                });
                if (totalEntriesQty != that.model.productStocksTotals[productStockId]) {
                    that.valid = false;
                    $("#product-col-" + productStockId).toggleClass("correction-col-error", true);
                }
            });
            this.reason = $("#correction-reason").val();
            this.valid = this.valid && !_.isEmpty(this.reason);
            if (this.valid) {
                $("#validateCorrection").removeAttr("disabled");
            } else {
                $("#validateCorrection").attr("disabled", "");
            }
        },

        reasonChanged: function (event) {
            this.reason = $("#correction-reason").val();
            this.validate();
        },

        //Send modified billEntries quantities to serveur
        save: function (event) {
            var that = this;
            if ($(event.currentTarget).attr("disabled")) {
                return false;
            }
            var data = {
                "NewQuantities": [],
                "ProducerBillId": that.model.get("BillId")
            };
            this.reason = $("#correction-reason").val();
            if (_.isEmpty(this.reason)) {
                $("#correction-reason").toggleClass("error", true);
                return false;
            }
            _.each(this.billEntries, function (billEntry, billEntryId) {
                data.NewQuantities.push({ "BillId": billEntryId, "Quantity": billEntry.Quantity });
            });
	    $.blockUI();
            data["Reason"] = this.reason;
            if (_.isEmpty(data.NewQuantities)) {
                location.reload();
            } else {
                //Disabling button while waiting for the request to finish
                $("#validateCorrection").attr("disabled", "disabled");
                $.ajax({
                    url: "/WeekBasketManagement/UpdateBillCorrection",
                    type: 'POST',
                    data: data
                }).then(function (success) {
		    //success is a boolean sent as response from server
		    $.unblockUI();
                    if (success !== true) {
                        that.saveErrors = "Erreur lors de la sauvegarde."
                        that.render();
                    } else {
                        location.reload();
                    }
                });
            }
            event.preventDefault();
            return false;
        }
    });
});

$(function () {
    var openCorrectionModal = function (billId) {
	producerBillModel = new ProducerBillModel(billId);
	producerBillModel.fetchDeferred.done(function () {
            CorrectionView = new BillsManagement.CorrectionView(producerBillModel);
            CorrectionView.open();
	});
    };

    $("a.open-correction-modal").each(function(idx, elem) {
	var billId = $(elem).data("bill-id");
	$(elem).click(function() {
	    openCorrectionModal(billId);
	});
    });
});
