(function($) {

  /**
   * Copyright 2012, Digital Fusion
   * Licensed under the MIT license.
   * http://teamdf.com/jquery-plugins/license/
   *
   * @author Sam Sehnert
   * @desc A small plugin that checks whether elements are within
   *     the user visible viewport of a web browser.
   *     only accounts for vertical position, not horizontal.
   */

  $.fn.visible = function(partial) {

      var $t            = $(this),
          $w            = $(window),
          viewTop       = $w.scrollTop(),
          viewBottom    = viewTop + $w.height(),
          _top          = $t.offset().top,
          _bottom       = _top + $t.height(),
          compareTop    = partial === true ? _bottom : _top,
          compareBottom = partial === true ? _top : _bottom;

    return ((compareBottom <= viewBottom) && (compareTop >= viewTop));

  };

})(jQuery);

jQuery(document).ready(function() {
    // Add a nice cute effect when the user scrolls
    // https://css-tricks.com/slide-in-as-you-scroll-down-boxes/
    var win = jQuery(window);
    var allMods = jQuery(".module");

    allMods.each(function(i, el) {
        var el = jQuery(el);
        if (el.visible(true)) {
            el.addClass("already-visible");
        }
    });

    win.scroll(function(event) {
        allMods.each(function(i, el) {
            var el = jQuery(el);
            if (el.visible(true)) {
                el.addClass("come-in");
            }
        });
    });
}); // End of ready

var win = jQuery(window);
var allMods = jQuery(".scroll-animation");

allMods.each(function(i, el) {
    var el = jQuery(el);
    if (el.visible(true)) {
        el.addClass("already-visible");
    }
});

win.scroll(function(event) {
    allMods.each(function(i, el) {
        var el = jQuery(el);
        if (el.visible(true)) {
            el.addClass("come-in");
        }
    });
});
