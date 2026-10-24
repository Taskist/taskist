/* Empty/error state actions (no jQuery, so it also works on the minimal error layout):
   .js-history-back goes back (or home when there is no history), .js-reload reloads the page. */
(function () {
    "use strict";

    document.addEventListener("click", function (e) {
        var back = e.target.closest ? e.target.closest(".js-history-back") : null;
        if (back) {
            e.preventDefault();
            if (window.history.length > 1) window.history.back();
            else window.location.href = document.baseURI || "/";
            return;
        }
        var reload = e.target.closest ? e.target.closest(".js-reload") : null;
        if (reload) {
            e.preventDefault();
            window.location.reload();
        }
    });
})();
