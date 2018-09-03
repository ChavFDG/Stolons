function getNewMessages(lastMessageDate) {
    if (lastMessageDate === null) {
        lastMessageDate = $('div.comment').last().attr('data-date');
    }

    $.ajax({
        type: "POST",
        data: { date: lastMessageDate },
        url: "Chat/GetNewMessages",
        dataType: 'json',
        success: function (data) {
            $.each(data.Messages, function (i, message) {
                addMessage(message, data.ActiveAdherentStolon);
            });
        },
    });
}

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

$(document).ready(function () {
    getNewMessages("1900-01-01")
    $("div.chat").animate({ scrollTop: 50000000 }, 1000);

    window.removeMessage = function (id) {
        bootbox.confirm(
            "Êtes-vous sûr de vouloir supprimer ce message ?",
            function (result) {
                if (result) {
                    $.blockUI();
                    $.ajax({
                        type: "POST",
                        url: "Chat/RemoveMessage",
                        data: { id: id },
                        dataType: 'json',
                        success: function (data) {
                            $.unblockUI();
                            if (data === true)
                                $(("#" + id)).remove();
                        }
                    });
                }

            }
        );
    };

    window.editMessage = function (id) {
        var content = $(("#" + id + " .messageContent")).html();
        bootbox.prompt(
            {
                value: $.trim(content),
                title: "Edition du message : ",
                inputType: "textarea",
                callback: function (result) {
                    if (result !== null) {
                        $.blockUI();
                        $.ajax({
                            type: "POST",
                            url: "Chat/EditMessage",
                            data: {
                                id: id,
                                content : result
                            },
                            dataType: 'json',
                            success: function (data) {
                                $.unblockUI();
                                if (data === true)
                                    $(("#" + id + " .messageContent")).html(result);
                            }
                        });
                    }
                }
            }
        );
    };


    $('a#add-comment').on('click', function () {
        var $this = $(this);

        var comment = $('textarea[name=comment]').val();
        $('textarea[name=comment]').removeClass('error');

        if (comment !== '') {
            $(this).addClass('hidden');
            $('span#loading').removeClass('hidden');

            var lastMessageDate = $('div.comment').last().attr('data-date');
            $('textarea[name=comment]').val('');
            $.ajax({
                type: "POST",
                url: "Chat/AddMessage",
                data: { message: comment, date: lastMessageDate },
                dataType: 'json',
                success: function (data) {
                    $this.removeClass('hidden');
                    $('span#loading').addClass('hidden');
                    $.each(data.Messages, function (i, message) {
                        addMessage(message, data.ActiveAdherentStolon);
                    });

                    $("div.chat").animate({ scrollTop: 50000000 }, 1000);
                },
            });
        } else {
            $('textarea[name=comment]').addClass('error');
        }
        return false;
    });

    window.setInterval(function () {
        getNewMessages();
    }, 30000);
});
