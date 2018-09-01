// auto adjust basket position fixed depending on scroll
document.addEventListener("DOMContentLoaded", function () { // line equivalent to $(document).ready(() => {})
    var hasJustSwitched = false
    function updateBasketPosition() {
        var tmpBasket = document.getElementById('basket__wrapper')
	if (!tmpBasket) {
	    return;
	}
        if (window.screen.width >= 990) {
            if (window.pageYOffset >= 300) {
                tmpBasket.classList.add('fixed')
                if (!hasJustSwitched) {
                    hasJustSwitched = true
                    $(tmpBasket).animate({ scrollTop: $(tmpBasket).height() }, 200)
                    console.log('scrolled')
                }
            } else {
                tmpBasket.classList.remove('fixed')
                hasJustSwitched = false
            }
        } else {
            // the line bellow is usefull to allow css rules to apply under 990px event if the js changed it
            // js will add 'style' attribute to html that will overrides css rules by specificity
            tmpBasket.style.position = 'inherit'
        }
    }
    window.onscroll = updateBasketPosition
    window.onresize = updateBasketPosition
})
