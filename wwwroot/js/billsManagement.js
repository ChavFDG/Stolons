
BillsManagement = {}

BillsManagement.CorrectionView = Backbone.View.extend({

    el: "#correctionModal",

    template: _.template($("#correctionModalTemplate").html()),

    events: {
	"click .minus": "decrement",
	"click .plus": "increment",
	"click .decrementTotal": "decrementProductTotal",
	"click .incrementTotal": "incrementProductTotal",
	"click #validateCorrection": "validate"
    },

    initialize: function(model) {
	this.model = model;
	this.render();
    },

    render: function() {
	this.$el.html(this.template({
	    view: this,
	    producerBill: this.model,
	    
	}));
    },

    open: function() {
	this.$el.modal({ keyboard: true, show: true });
        this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
	//Initialize changed billEntries
	this.billEntries = {};
    },

    onClose: function() {
	this.$el.off('hide.bs.modal');
        this.$el.empty();
        this.model.off("sync change", this.render, this);
    },

    decrement: function(event) {
	var billEntryId = $(event.currentTarget).data("bill-entry-id");
	if (billEntryId) {
	    
	    event.preventDefault();
	    return false;
	}
    },

    increment: function(event) {
	var billEntryId = $(event.currentTarget).data("bill-entry-id");
	if (billEntryId) {
	    console.log("Increment, bill entry id = " + billEntryId);
	    event.preventDefault();
	    return false;
	}
    },

    decrementProductTotal: function(event) {
	var productStockId = $(event.currentTarget).data("product-stock-id");
	if (productStockId) {
	    console.log("decrement product total:" + productStockId);
	    event.preventDefault();
	    return false;
	}
    },

    incrementProductTotal: function(event) {
	var productStockId = $(event.currentTarget).data("product-stock-id");
	if (productStockId) {
	    console.log("increment product total:" + productStockId);
	    event.preventDefault();
	    return false;
	}
    }
});
