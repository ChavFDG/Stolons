
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

    //return the list of unique products for this producerBill by grouping billentries by products
    getProductStocks: function() {
	var productsIdx = {};

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    productsIdx[billEntry.ProductStock.get("Id")] = billEntry.ProductStock;
	});
	return _.values(productsIdx);
    },

    getProductStock: function(productStockId) {
	var productStock;

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    if (billEntry.ProductStock.get("Id") == productStockId) {
		productStock = billEntry.ProductStock;
		return false;
	    }
	});
	return productStock;
    },

    //return the list of consumers for this producerBill by grouping billentries by consumers
    getConsumers: function() {
	var consumersIdx = {};

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    consumersIdx[billEntry.ConsumerBill.AdherentStolon.Id] = billEntry.ConsumerBill.AdherentStolon;
	});
	return _.values(consumersIdx);
    },

    //Return consumer bill entry for a given product or nil
    getBillEntry: function(consumerId, productStockId) {
	var billEntry = null;

	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.ProductStock.get('Id') == productStockId && entry.ConsumerBill.AdherentStolon.Id == consumerId) {
		billEntry = entry;
		return false;
	    }
	});
	return billEntry;
    },

    //Get the total quantity for this productStock
    getProductStockTotal: function(productStockId) {
	var totalQty = 0;

	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.ProductStock.get('Id') == productStockId) {
		totalQty += parseInt(entry.Quantity);
	    }
	});
	return totalQty;
    },

    getProductStockTotalQuantityString: function(productStockId) {
	var totalQty = this.getProductStockTotal(productStockId);
	var productStock = this.getProductStock(productStockId);

	return this.getQuantityString(productStock.get("Product").toJSON(), totalQty);
    },

    getBillEntryQuantityString: function(billEntryId) {
	var billEntry;
	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.Id == billEntryId) {
		billEntry = entry;
		return false;
	    }
	});
	var product = billEntry.ProductStock.get('Product').toJSON();
	return this.getQuantityString(product, billEntry.Quantity);
    },

    getQuantityString: function(product, quantity) {
	if (product.Type == 1) {
	    if (quantity > 1) {
		return  quantity + " pièce";
	    } else {
		return  quantity + " pièces";
	    }
	} else {
	    var qty = quantity * product.QuantityStep;

	    if (product.ProductUnit == 0) {
		var strUnit = " g";
		if (qty >= 1000) {
		    qty /= 1000;
		    strUnit = " Kg";
		}
		return qty + strUnit;
	    } else if (product.ProductUnit == 1) {
		var strUnit = " mL";
		if (qty >= 1000) {
		    qty /= 1000;
		    strUnit = " L";
		}
		return qty + strUnit;
	    }
	}
    },

    getProductStockQuantityString: function(billEntryId) {
	
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
