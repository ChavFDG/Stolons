
window.PublicStolons = {};

var StolonModel = Backbone.Model.extend(
    {
	default: {},

	idAttribute: "Id"
    }
);

var StolonCollection = Backbone.Collection.extend(
    {
	url: "/api/stolons",

	model: StolonModel,

	initialize: function() {
	    this.fetch();
	},

	//Get the center of stolons coordinates for init map view
        getCenterCoordinates: function () {
            var count = 0;
            var latitude = 0;
            var longitude = 0;

            this.forEach(function (stolon) {
                if (stolon.get("Latitude") != 0.0 && stolon.get("Longitude") != 0.0) {
                    latitude += stolon.get("Latitude");
                    longitude += stolon.get("Longitude");
                    ++count;
                }
            });
            return [latitude / count, longitude / count];
        }
    }
);

var StolonsView = Backbone.View.extend(
    {

	el: "#stolonDetails",

	template: _.template($("#stolonDetailsTemplate").html()),

	initialize: function(model) {
	    this.model = model;
	},

	selectStolon: function(stolonId) {
	    var stolon = this.model.get(stolonId);
	    this.render(stolon);
	},

	render: function(stolon) {
	    $(this.el).toggleClass("hidden", false);
	    this.$el.html(this.template({stolon: stolon.toJSON()}));
	},

	closeDetails: function() {
	    this.$el.toggleClass("hidden", true);
	},

	initMap: function() {
	    var that = this;
            var centerCoordinates = this.model.getCenterCoordinates();
            this.markers = {};
            var bounds = [];
            var map = L.map('stolons-map').setView(centerCoordinates, 6);
            var mapLink = '<a href="http://openstreetmap.org">OpenStreetMap</a>';

            L.tileLayer('https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager_labels_under/{z}/{x}/{y}{r}.png', { attribution: 'Map data &copy; ' + mapLink, maxZoom: 24, subdomains: 'abcd' }).addTo(map);

            //Adding producers markers to the map
            this.model.forEach(function (stolon) {
		if (stolon.get("Latitude") == 0 || stolon.get("Longitude") == 0) {
                    return;
		}
		var bound = [stolon.get("Latitude"), stolon.get("Longitude")];
		var stolonIcon = L.icon({
                    iconUrl: stolon.get("LogoFilePath"),
                    iconSize: [30, 30], // size of the shadow
                    iconAnchor: [14, 26],  // the same for the shadow
                    shadowUrl: '/images/map_marker.png',
                    shadowSize: [68, 63], // size of the icon
                    shadowAnchor: [33, 31], // point of the icon which will correspond to marker's location
                    className: "mapMarker"
		});

		var marker = L.marker(bound, { icon: stolonIcon });
		marker.on("click", function () {
                    that.selectStolon(stolon.get("Id"));
                    return false;
		});
		marker.addTo(map).bindTooltip(stolon.get("Label"));
		that.markers[stolon.get("Id")] = marker;
		bounds.push(bound);
            });

	    if (bounds.length > 1) {
		//Recentrage automatique de la carte pour voir tous les points
		map.fitBounds(bounds);
	    }

            this.map = map;
	}
    }
);

$(function () {

    window.PublicStolons.StolonCollection = new StolonCollection();
    window.PublicStolons.StolonCollection.on("sync", function() {
	window.StolonsView = new StolonsView(window.PublicStolons.StolonCollection);
	window.StolonsView.initMap();
    });
});
