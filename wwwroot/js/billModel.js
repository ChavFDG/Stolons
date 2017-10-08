
ProducerBillModel = Backbone.Model.extend({

    idAttribute: "Id",

    defaults: {
	Id: 0
    },

    url: function() {
	return "/api/producerBill?billId=" + this.billId;
    },

    initialize: function(billId) {
	this.billId = billId;
	this.fetch();
    },

    getProducts: function() {
	var products = [];

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    products.push(billEntry.ProductStock);
	});
	return products;
    },

    getConsumers: function() {
	var consumers = [];

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    consumers.push(billEntry.ConsumerBill);
	});
	return consumers;
    },

    parse: function(data) {
	if (data && data.BillEntries) {
	    _.forEach(data.BillEntries, function(billEntry) {
		//Because init of ProductStockModel like this doesn't call 'parse'
		billEntry.ProductStock.Product = new ProductModel(billEntry.ProductStock.Product);
		billEntry.ProductStock = new ProductStockModel(billEntry.ProductStock);
	    });
	}
	return data;
    }
});
