/* User edit page: an administrator removes a user's linked single sign-on account. The user can sign in with
   single sign-on again afterwards; if their e-mail still matches, the account is linked again. */
(function () {
    "use strict";

    $(document).on("click", ".js-remove-signin", function () {
        var $btn = $(this);
        var $root = $("#userSignIns");

        JSManager.confirm($root.data("remove-confirm"), function () {
            $btn.prop("disabled", true);
            $.ajax({
                type: "POST",
                url: $root.data("remove-url"),
                data: JSManager.addAntiForgeryToken({ userId: $root.data("user-id"), id: $btn.data("id") }),
                success: function (r) {
                    if (r && r.status === 2000) {
                        JSManager.showSuccess(r.message);
                        setTimeout(function () { window.location.reload(); }, 700);
                    } else {
                        JSManager.showSwal((r && r.message) || "Could not remove this sign-in.", "error");
                    }
                },
                error: function (xhr) { JSManager.showSwal("Request failed (" + xhr.status + ").", "error"); },
                complete: function () { $btn.prop("disabled", false); }
            });
        });
    });
})();
