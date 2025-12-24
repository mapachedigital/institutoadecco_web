jQuery(function () {
    //Make the header fixed after the first scroll
    window.addEventListener('scroll', function () {
        if (window.scrollY > 200) {
            document.getElementById('navbar-top').classList.add('fixed-top');
            // add padding top to show content behind navbar
            navbar_height = document.querySelector('.navbar').offsetHeight;
            document.body.style.paddingTop = navbar_height + 'px';
        } else {
            document.getElementById('navbar-top').classList.remove('fixed-top');
            // remove padding top from body
            document.body.style.paddingTop = '0';
        }
    });
});
