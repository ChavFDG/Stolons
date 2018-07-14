// Write your Javascript code.
    
$(document).ready(function () {

    // DEPRECATED: tooltips should use the one from bootstrap.
    // the code bellow is kept for background compatibility
    $('.warning-container')
        .hover(function () {
            $(this).children('.warning-informations').show()
        })
        .mouseout(function () {
            $(this).children('.warning-informations').hide()
        })


    $('[data-toggle="tooltip"]').tooltip({ container: 'body', html: true })
});
