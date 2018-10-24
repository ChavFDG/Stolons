// Write your Javascript code.

$(document).ready(function () {

    bootbox.setLocale("fr");

    $('[data-toggle="tooltip"]').tooltip({ container: 'body', html: true });

    // when you hover a toggle show its dropdown menu
    $('.auto-dropdown').hover(function() {
	$(this).find('.dropdown-menu').stop(true, true).delay(0).fadeIn(400);
    }, function() {
	$(this).find('.dropdown-menu').stop(true, true).delay(200).fadeOut(300);
    });

    
});
