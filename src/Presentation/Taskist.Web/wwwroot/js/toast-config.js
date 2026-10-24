(function () {
    "use strict";

    if (!window.toastr) return;

    var defaults = {
        closeButton: true,
        newestOnTop: true,
        progressBar: true,
        preventDuplicates: true,
        positionClass: "toast-top-right",
        showDuration: 200,
        hideDuration: 300,
        timeOut: 5000,
        extendedTimeOut: 2000,
        showMethod: "fadeIn",
        hideMethod: "fadeOut",
        tapToDismiss: false
    };

    var byType = {
        success: { title: "Success", timeOut: 3000 },
        info: { title: "Info", timeOut: 5000 },
        warning: { title: "Warning", timeOut: 6000 },
        error: { title: "Error", timeOut: 8000 }
    };

    var MAX_VISIBLE = 4;

    toastr.options = $.extend({}, defaults);

    function trimStack() {
        var $toasts = $("#toast-container > .toast");
        if ($toasts.length >= MAX_VISIBLE) {
            $toasts.slice(MAX_VISIBLE - 1).each(function () { toastr.clear($(this), { force: true }); });
        }
    }

    Object.keys(byType).forEach(function (type) {
        var original = toastr[type];
        toastr[type] = function (message, title, overrides) {
            trimStack();
            var options = $.extend({}, defaults, { timeOut: byType[type].timeOut }, overrides || {});
            return original.call(toastr, message, title || byType[type].title, options);
        };
    });
})();
