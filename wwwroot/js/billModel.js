
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
	this.fetchDeferred = this.fetch();
    },

    //return the list of unique products for this producerBill by grouping billentries by products
    getProductStocks: function() {
	var productsIdx = {};

	_.forEach(this.get("BillEntries"), function(billEntry) {
	    productsIdx[billEntry.ProductStock.get("Id")] = billEntry.ProductStock;
	});
	return _.sortBy(_.values(productsIdx), function(productStock) {
	    return productStock.get("Product").get("Name");
	});
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
	return _.sortBy(_.values(consumersIdx), function(consumer) {
	    return consumer.LocalId;
	});
    },

    getBillEntryById: function(billEntryId) {
	var billEntry;

	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.Id == billEntryId) {
		billEntry = entry;
		return false;
	    }
	});
	return billEntry;
    },

    //Return bill entries for a given product or empty
    //The list contains several bill entries only in the case of variable weight products
    getBillEntries: function(consumerId, productStockId) {
	var billEntries = [];

	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.ProductStock.get('Id') == productStockId && entry.ConsumerBill.AdherentStolon.Id == consumerId) {
		billEntries.push(entry);
	    }
	});
	return billEntries;
    },

    getBillEntriesForProductStock: function(productStockId) {
	var billEntries = [];

	_.forEach(this.get("BillEntries"), function(entry) {
	    if (entry.ProductStock.get('Id') == productStockId) {
		billEntries.push(entry);
		return false;
	    }
	});
	return billEntries;
    },

    //Get the total quantity for this productStock
    getProductStockTotal: function(productStockId) {
	var totalQty = 0;

	if (typeof this.productStocksTotals[productStockId] == "undefined") {
	    //Working with cloned object here because bill entries can be modified
	    _.forEach(this.get("ClonedBillEntries"), function(entry) {
		if (entry.ProductStock.get('Id') == productStockId) {
		    if (entry.ProductStock.get("Product").get("Type") === 3 && entry.IsAssignedVariableWeigh) {
			totalQty += 1;
		    } else {
			totalQty += parseInt(entry.Quantity);
		    }
		}
	    });
	    this.productStocksTotals[productStockId] = totalQty;
	} else {
	    totalQty = this.productStocksTotals[productStockId];
	}
	return totalQty;
    },

    getProductStockTotalQuantityString: function(productStockId) {
	var totalQty = this.getProductStockTotal(productStockId);
	var productStock = this.getProductStock(productStockId);

	return this.getQuantityString(productStock.get("Product").toJSON(), totalQty);
    },

    getVariableWeightProductQuantityString: function(billEntry) {
	var nbConsumerBillEntrys = 0;

	console.log("getVariableWeightProductQuantityString", billEntry);
	_.forEach(this.get("ClonedBillEntries"), function(entry) {
	    if (entry.ProductStock.get('Id') == billEntry.ProductStock.get("Id")
		&& entry.ConsumerBill.AdherentStolon.Id == billEntry.ConsumerBill.AdherentStolon.Id
		&& billEntry.Quantity !== 0) {
		nbConsumerBillEntrys += 1;
	    }
	});
	return nbConsumerBillEntrys;
    },

    getBillEntryQuantityString: function(billEntryId) {
	var billEntry = this.getBillEntryById(billEntryId);
	var product = billEntry.ProductStock.get('Product').toJSON();

	//Hack Produit poids variable
	if (billEntry.ProductStock.get("Product").get("Type") == 3 && billEntry.IsAssignedVariableWeigh) {
	    var qty = this.getVariableWeightProductQuantityString(billEntry);
	    return this.getQuantityString(product, qty);
	} else {
	    return this.getQuantityString(product, billEntry.Quantity);
	}
    },

    getQuantityString: function (product, quantity) {
	    if (product.Type == 1 || product.Type == 3) {
	        if (quantity > 1) {
		    return  quantity + " pièces";
	        } else {
		    return  quantity + " pièce";
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

    isAssignedVariableWeigh: function(productStock) {
	var allAssigned = true;
	var billEntries = this.getBillEntriesForProductStock(productStock.get("Id"));

	_.forEach(billEntries, function(billEntry) {
	    if (billEntry.IsAssignedVariableWeigh !== true) {
		allAssigned = false;
		return false;
	    }
	});
	return allAssigned;
    },

    parse: function(data) {
	if (data && data.BillEntries) {
	    data.ClonedBillEntries = [];
	    _.forEach(data.BillEntries, function(billEntry) {
		//Because init of ProductStockModel like this doesn't call 'parse'
		billEntry.ProductStock.Product = new ProductModel(billEntry.ProductStock.Product);
		billEntry.ProductStock = new ProductStockModel(billEntry.ProductStock);
		//Save a working copy of bill entries for Total
		data.ClonedBillEntries.push(_.clone(billEntry));
	    });
	}
	this.productStocksTotals = {};
	return data;
    }
});
