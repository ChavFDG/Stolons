
BillsManagement = {}

BillsManagement.CorrectionView = Backbone.View.extend({

    el: "#correctionModal",

    template: _.template($("#correctionModalTemplate").html()),

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
    },

    onClose: function() {
	this.$el.off('hide.bs.modal');
        this.$el.empty();
        this.model.off("sync change", this.render, this);
    }
});
