
//Tab management
$(function () {
    var tab = localStorage.getItem("WeekBasketTab");
    switch (tab) {
        case "consumersPickUp":
            $('a[name="consumersPickUp"]').tab('show');
            break;
        case "producersDelivery":
            $('a[name="producersDelivery"]').tab('show');
            break;
        case "producersToPay":
            $('a[name="producersToPay"]').tab('show');
            break;
        default:
            $('a[name="consumersPickUp"]').tab('show');
    }
    $("ul.nav a").on('click', onTabClick);
});

function onTabClick(e) {
    var tab = $(e.target);

    var tabName = tab.attr("name");
    setTab(tabName);
    return true;
}

function setTab(tab) {
    localStorage.setItem("WeekBasketTab", tab);
}
//End Tab management


function addProducerBillToPay(bill, activeAdherentStolon) {

    var template = _.template($("#producerBillToPayTemplate").html());
    var html = template((
        {
            bill: bill,
            activeAdherentStolon: activeAdherentStolon
        }
    ));
    $('#producersToPayTable > tbody:last-child').append(html);
}

$(document).ready(function () {

    $.ajax({
        type: "POST",
        url: "GetBillsToPay",
        dataType: 'json',
        success: function (data) {
            $.each(data.Bills, function (i, bill) {
                addProducerBillToPay(bill, data.ActiveAdherentStolon);
            });
        },
    });

    window.deleteWeekBasket = function (billId) {
        bootbox.confirm(
            "Annuler le panier entrainera la modification en conséquent des factures producteurs concernées excepté si ces derniers ont déjà livré leurs produits. Dans ce cas leurs produits leurs seront dus<br><br>Confirmer vous l'annulation du panier ?",
            function (result) {
                if (result) {
                    bootbox.prompt(
                        {
                            value: "",
                            title: "Entrer la raison de l'annulation du panier: ",
                            inputType: "textarea",
                            callback: function (result) {
                                $.blockUI();
                                if (result !== null) {
                                    $.blockUI();
                                    $.ajax(
                                        {
                                            type: "POST",
                                            url: "CancelConsumerBill",
                                            data:
                                            {
                                                billId: billId,
                                                reason: result
                                            },
                                            dataType: 'json',
                                            success: function (data) {
                                                $.unblockUI();
                                                location.reload();
                                                /* Trop d'information à reset alors on refresh la page
                                                 if (data === true) {
                                                    $("#" + billId).remove();
                                                }*/
                                            }
                                        });
                                }
                            }
                        });
                }
            }
        );
    };


    window.payConsumerBill = function (billId, payementMode) {
        bootbox.confirm(
            "Confirmer la récupération et le payement de la facture",
            function (result) {
                if (result) {
                    $.blockUI();
                    $.ajax({
                        type: "POST",
                        url: "UpdateConsumerBill",
                        data: {
                            billId: billId,
                            mode: payementMode
                        },
                        dataType: 'json',
                        success: function (data) {
                            $.unblockUI();
                            if (data === true) {
                                $("#" + billId).remove();
                            }
                        }
                    });
                }
            }
        );
    };

    window.validateProdDelivery = function (billId, state) {
        bootbox.confirm(
            "Confirmer la livraison de la commande",
            function (result) {
                if (result) {
                    $.blockUI();
                    $.ajax({
                        type: "POST",
                        url: "UpdateProducerBill",
                        data: {
                            billId: billId,
                            state: state
                        },
                        dataType: 'json',
                        success: function (data) {
                            $.unblockUI();
                            if (data.Error === false) {
                                $("#" + billId).remove();
                                addProducerBillToPay(data.Bill, data.ActiveAdherentStolon)
                            }
                        }
                    });
                }
            }
        );
    };

    window.validateProdPayement = function (billId, state) {
        bootbox.confirm(
            "Confirmer le payement de la facture",
            function (result) {
                if (result) {
                    $.blockUI();
                    $.ajax({
                        type: "POST",
                        url: "UpdateProducerBill",
                        data: {
                            billId: billId,
                            state: state
                        },
                        dataType: 'json',
                        success: function (data) {
                            $.unblockUI();
                            if (data.Error === false) {
                                $("#" + billId).remove();
                            }
                        }
                    });
                }
            }
        );
    };
});