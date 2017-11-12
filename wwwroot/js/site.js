// Write your Javascript code.

// auto adjust basket position fixed depending on scroll
document.addEventListener("DOMContentLoaded", function () { // line equivalent to $(document).ready(() => {})
    function updateBasketPosition() {
        if (window.screen.width >= 990) {
            if (window.pageYOffset >= 300) {
                document.getElementById('tmpBasket').style.position = 'fixed'
                document.getElementById('tmpBasket').style.top = '20px'
            } else {
                document.getElementById('tmpBasket').style.position = 'static'
            }
        } else {
            // the line bellow is usefull to allow css rules to apply under 990px event if the js changed it
            // js will add 'style' attribute to html that will overrides css rules by specificity
            document.getElementById('tmpBasket').style.position = 'inherit'
        }
    }
    window.onscroll = updateBasketPosition
    window.onresize = updateBasketPosition
}
