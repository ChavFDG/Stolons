
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
        url: function() {
	    return "/api/producers?stolonId=" + this.stolonId
	},

        model: ProducerModel,

        initialize: function (stolonId) {
	    this.stolonId = stolonId;
            this.fetch();
        },

        getFromAdherentId: function (id) {
            var producer;
            this.forEach(function (p) {
                if (p.get("Adherent").Id == id) {
                    producer = p;
                    return false;
                }
            });
            return producer;
        },

        //Get the center of producers coordinates for init map view
        getCenterCoordinates: function () {
            var count = 0;
            var latitude = 0;
            var longitude = 0;

            this.forEach(function (producer) {
                if (producer.get("Adherent").Latitude != 0.0 && producer.get("Adherent").Longitude != 0.0) {
                    latitude += producer.get("Adherent").Latitude;
                    longitude += producer.get("Adherent").Longitude;
                    ++count;
                }
            });
            return [latitude / count, longitude / count];
        }
    }
);

var ProducerDetailsView = Backbone.View.extend({

    el: "#producerDetails",

    template: _.template($("#producerDetailsTemplate").html()),

    initialize: function () {
        this.producerProducts = {};
    },

    render: function (producer) {
        var that = this;
        var deferred = $.Deferred();
        deferred.resolve();

        $('#producerDetails').toggleClass('hidden', false);
        $('#map').toggleClass('producersMapFull', false);
        $('#map').toggleClass('producersMapHalf', true);

        $('html, body').animate({
            scrollTop: $("#producerDetailsAnchor").offset().top
        }, 1000);

        deferred.done(function () {
            that.$el.html(that.template({ producer: producer.toJSON() }));
        });
    },

    initMap: function (producersModel) {
        var that = this;
        var centerCoordinates = PublicProducers.ProducersCollection.getCenterCoordinates();
        this.markers = {};
        var bounds = [];
        var map = L.map('map').setView(centerCoordinates, 9);
        var mapLink = '<a href="http://openstreetmap.org">OpenStreetMap</a>';

        L.tileLayer('https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager_labels_under/{z}/{x}/{y}{r}.png', { attribution: 'Map data &copy; ' + mapLink, maxZoom: 24, subdomains: 'abcd' }).addTo(map);

        //Adding producers markers to the map
        producersModel.forEach(function (producer) {
            if (producer.get("Adherent").Latitude == 0 || producer.get("Adherent").Longitude == 0) {
                return;
            }
            var bound = [producer.get("Adherent").Latitude, producer.get("Adherent").Longitude];
            var producerIcon = L.icon({
                iconUrl: producer.get("Adherent").AvatarFilePath,
                iconSize: [30, 30], // size of the shadow
                iconAnchor: [14, 26],  // the same for the shadow
                shadowUrl: '/images/map_marker.png',
                shadowSize: [68, 63], // size of the icon
                shadowAnchor: [33, 31], // point of the icon which will correspond to marker's location
                className: "mapMarker"
            });

            var marker = L.marker(bound, { icon: producerIcon });
            marker.on("click", function () {
                that.selectProducer(producer.get("Id"));
                return false;
            });
            marker.addTo(map).bindTooltip(producer.get("Adherent").CompanyName);
            that.markers[producer.get("Id")] = marker;
            bounds.push(bound);
        });

        //Recentrage automatique de la carte pour voir tous les points
        map.fitBounds(bounds);

        this.map = map;
    },

    selectProducer: function (producerId) {
        var producer = PublicProducers.ProducersCollection.get(producerId);

        if (producer.get("Adherent").Latitude != 0.0 && producer.get("Adherent").Longitude != 0.0) {
            //Center the producer on the map
            this.map.panTo([producer.get("Adherent").Latitude, producer.get("Adherent").Longitude]);
            _.each(_.values(this.markers), function (marker) {
                marker.closeTooltip();
            });
            this.markers[producerId].openTooltip();
        }
        this.render(producer);
    }
});

$(function () {
    var stolonId = $("#stolon-id").val();

    PublicProducers.ProducersCollection = new ProducersCollection(stolonId);
    PublicProducers.ProducersCollection.on("sync", function () {
        window.ProducerDetailsView = new ProducerDetailsView();
        window.ProducerDetailsView.initMap(PublicProducers.ProducersCollection);


        var hash = window.location.hash.substr(1);
        if (!_.isEmpty(hash)) {
            var producer = PublicProducers.ProducersCollection.getFromAdherentId(hash);
            if (producer) {
                window.ProducerDetailsView.selectProducer(producer.get("Id"));
            }
        }
    });

});
