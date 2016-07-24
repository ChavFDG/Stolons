
window.PublicProducers = {};

PublicProducers = window.PublicProducers;

ProducerModel = Backbone.Model.extend(
    {
	default: {},

	idAttribute: "Id"
    }
);

ProducersModel = Backbone.Collection.extend(
    {
	url: "/api/producers",

	model: ProducerModel,

	initialize: function() {
	    this.fetch();
	}
    }
);

ProducerViewModal = Backbone.View.extend({

    el: "#producerModal",

    template: _.template($("#producerModalTemplate").html()),

    initialize: function() {
    },

    open: function(producerId) {
	this.currentProducer = PublicProducers.ProducersModel.get(producerId);
	this.renderModal();
    },

    onClose: function() {
	this.currentProducer = null;
	this.$el.off('hide.bs.modal');
	this.$el.empty();
    },

    render: function() {
	this.$el.html(this.template({producer: this.currentProducer.toJSON()}));
    },

    renderModal: function() {
	this.render();
	this.$el.modal({keyboard: true, show: true});
	this.$el.on('hide.bs.modal', _.bind(this.onClose, this));
    }
});

function initMap(producersModel) {

    var privasCenterCoodinates = [44.7177685, 4.596498399999973];

    var markers = {};

    var map = L.map('map').setView(privasCenterCoodinates, 11);

    var mapLink = '<a href="http://openstreetmap.org">OpenStreetMap</a>';

    L.tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: 'Map data &copy; '+ mapLink , maxZoom: 24}).addTo(map);

    var producerIcon = L.icon({
	iconUrl: '/images/mapMarker.png',
	iconSize:     [38, 95], // size of the icon
	shadowSize:   [50, 64], // size of the shadow
	iconAnchor:   [22, 94], // point of the icon which will correspond to marker's location
	shadowAnchor: [4, 62],  // the same for the shadow
	popupAnchor:  [-3, -76] // point from which the popup should open relative to the iconAnchor
    });

    //Adding producers markers to the map
    producersModel.forEach(function(producer) {
	var popup = L.popup().setContent("<b>" + producer.get("CompanyName") + "</b><br />Cliquer pour voir les détails.");

	var marker = L.marker([producer.get("Latitude"), producer.get("Longitude")], {icon: producerIcon});
	marker.bindPopup(popup);
	marker.on("click", function() {
	    ProducerModalView.open(producer.get("Id"));
	    return false;
	});
	marker.addTo(map);
	markers[producer.get("Id")] = marker;
    });

    PublicProducers.selectProducer = function(producerId) {
	var producer = PublicProducers.ProducersModel.get(producerId);

	if (producer.get("Latitude") != 0.0 && producer.get("Longitude") != 0.0) {
	    //Center the producer on the map
	    map.panTo([producer.get("Latitude"), producer.get("Longitude")]);
	    markers[producerId].openPopup();
	}
    };

}

$(function() {

    window.ProducerModalView = new ProducerViewModal();

    PublicProducers.ProducersModel = new ProducersModel();

    PublicProducers.ProducersModel.on("sync", function() {
	initMap(PublicProducers.ProducersModel);
    });

});
