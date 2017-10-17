
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

    getProductStocks: function() {
	var productsIdx = {};

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    productsIdx[billEntry.ProductStock.get("Id")] = billEntry.ProductStock;
	});
	return _.values(productsIdx);
    },

    getConsumers: function() {
	var consumersIdx = {};

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    consumersIdx[billEntry.ConsumerBill.AdherentStolon.Id] = billEntry.ConsumerBill.AdherentStolon;
	});
	return _.values(consumersIdx);
    },

    //Get consumer bill entry for a given product or nil
    getBillEntry: function(consumerId, productStockId) {
	var billEntry = null;

	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.ProductStock.Id == productStockId && entry.ConsumerBill.AdherentStolon.Id == consumerId) {
		billEntry = entry;
		return false;
	    }
	});
	return billEntry;
    },

    getProductStockTotal: function(productStockId) {
	var totalQty = 0;

	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.ProductStock.Id == productStockId) {
		totalQty += parseInt(entry.Quantity);
	    }
	});
	return totalQty;
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
