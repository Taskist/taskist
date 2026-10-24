/* Notification channels master: send a test message with the saved settings. */
(function () {
    "use strict";

    $(document).on("click", ".js-channel-test", function () {
        var $btn = $(this);
        $btn.prop("disabled", true);
        $.ajax({
            type: "POST",
            url: $btn.data("url"),
            data: JSManager.addAntiForgeryToken({}),
            success: function (r) {
                if (r && r.status === 2000) JSManager.showSuccess(r.message);
                else JSManager.showSwal((r && r.message) || "Request failed.", "error");
            },
            error: function (xhr) { JSManager.showSwal("Request failed (" + xhr.status + ").", "error"); },
            complete: function () { $btn.prop("disabled", false); }
        });
    });
})();
