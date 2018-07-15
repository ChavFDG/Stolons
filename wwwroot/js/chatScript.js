function addMessage(message, activeAdherentStolon) {

    var template = _.template($("#chatMessageTemplate").html());
    var html = template((
        {
            message: message,
            activeAdherentStolon: activeAdherentStolon
        }
    ));
    $(html).insertAfter('div.comment:last');
}
