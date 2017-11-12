// Write your Javascript code.
    
$(document).ready(function () {
    $('.warning-container')
        .hover(function () {
            $(this).children('.warning-informations').show()
        })
        .mouseout(function () {
            $(this).children('.warning-informations').hide()
        })
});
