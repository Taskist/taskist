/* Git connections master: one-click re-encryption of stored secrets with the current Data Protection key ring. */
(function () {
    "use strict";

    $(document).on("click", ".js-git-reencrypt", function () {
        var $btn = $(this);
        JSManager.confirm($btn.data("confirm"), function () {
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
    });
})();
