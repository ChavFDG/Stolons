
window.PublicProducers = {};

PublicProducers = window.PublicProducers;

ProducerModel = Backbone.Model.extend(
    {
	default: {},

	idAttribute: "Id"
    }
);

ProducersCollection = Backbone.Collection.extend(
    {
	url: "/api/producers",

	model: ProducerModel,

	initialize: function() {
	    this.fetch();
	},

	//Get the center of producers coordinates for init map view
	getCenterCoordinates: function() {
	    var count = 0;
	    var latitude = 0;
	    var longitude = 0;

	    this.forEach(function(producer) {
		if (producer.get("Latitude") != 0.0 && producer.get("Longitude") != 0.0) {
		    latitude += producer.get("Latitude");
		    longitude += producer.get("Longitude");
		    ++count;
		}
	    });
	    return [latitude / count, longitude / count];
	}
    }
);

ProducerViewModal = Backbone.View.extend({

    el: "#producerModal",

    template: _.template($("#producerModalTemplate").html()),

    initialize: function() {
    },

    open: function(producerId) {
	    this.currentProducer = PublicProducers.ProducersCollection.get(producerId);
	    this.renderModal();
    },

    onClose: function() {
	this.currentProducer = null;
        this.$el.off('hidden.bs.modal');
        this.close();
    },

    render: function() {
	    this.$el.html(this.template({producer: this.currentProducer.toJSON()}));
    },

    renderModal: function() {
	    this.render();
	    this.$el.modal();
	    this.$el.on('hidden.bs.modal', _.bind(this.onClose, this));
    }
});

function initMap(producersModel) {
    
    var centerCoordinates = PublicProducers.ProducersCollection.getCenterCoordinates();

    var markers = {};

    var map = L.map('map').setView(centerCoordinates, 9);

    var mapLink = '<a href="http://openstreetmap.org">OpenStreetMap</a>';

    L.tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: 'Map data &copy; '+ mapLink , maxZoom: 24}).addTo(map);

    var bounds = [];

    //Adding producers markers to the map
    producersModel.forEach(function(producer) {
	if (producer.get("Latitude") == 0 || producer.get("Longitude") == 0) {
	    return;
	}
	var bound = [producer.get("Latitude"), producer.get("Longitude")];
	var producerIcon = L.icon({
	    iconUrl: producer.get("AvatarFilePath"),
	    iconSize: [48, 45], // size of the shadow
	    iconAnchor: [24, 62],  // the same for the shadow
	    shadowUrl: '/images/map_marker.png',
	    shadowSize: [50, 62], // size of the icon
	    shadowAnchor: [25, 62], // point of the icon which will correspond to marker's location
	    className: "mapMarker"
	});

	var marker = L.marker(bound, {icon: producerIcon});
	marker.on("click", function() {
	    return false;
	});
	marker.addTo(map).bindTooltip(producer.get("CompanyName"));
	markers[producer.get("Id")] = marker;
	bounds.push(bound);
    });

    //Recentrage automatique de la carte pour voir tous les points
    map.fitBounds(bounds);

    PublicProducers.selectProducer = function(producerId) {
	var producer = PublicProducers.ProducersModel.get(producerId);

	if (producer.get("Latitude") != 0.0 && producer.get("Longitude") != 0.0) {
	    //Center the producer on the map
	    map.panTo([producer.get("Latitude"), producer.get("Longitude")]);
	}
    };
}

$(function() {

    window.ProducerModalView = new ProducerViewModal();

    PublicProducers.ProducersCollection = new ProducersCollection();
    PublicProducers.ProducersCollection.on("sync", function() {
        initMap(PublicProducers.ProducersCollection);
    });

});
