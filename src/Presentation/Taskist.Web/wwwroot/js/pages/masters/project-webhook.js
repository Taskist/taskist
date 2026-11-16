/* Project → Repositories → Webhook: register the webhook through the Git API, or rotate the secret for manual setup.
   A new secret is returned once and never shown again. */
(function () {
    "use strict";

    setTimeout(function () { $(".repository-webhook").each(init); }, 0);

    function init() {
        var $root = $(this);
        if ($root.data("bound")) return;
        $root.data("bound", true);

        function post(url, onSuccess, $btn) {
            $btn.prop("disabled", true);
            $.ajax({
                type: "POST",
                url: url,
                data: JSManager.addAntiForgeryToken({ id: $root.data("repository-id") }),
                success: function (r) {
                    if (r && r.status === 2000) onSuccess(r);
                    else JSManager.showSwal((r && r.message) || "Request failed.", "error");
                },
                error: function (xhr) { JSManager.showSwal("Request failed (" + xhr.status + ").", "error"); },
                complete: function () { $btn.prop("disabled", false); }
            });
        }

        $root.on("click", ".repository-webhook__register", function () {
            post($root.data("register-url"), function (r) { JSManager.showSuccess(r.message); }, $(this));
        });

        $root.on("click", ".repository-webhook__regenerate", function () {
            var $btn = $(this);
            JSManager.confirm($root.data("regenerate-confirm"), function () {
                post($root.data("regenerate-url"), function (r) {
                    $root.find(".repository-webhook__secret-value").val(r.secret);
                    $root.find(".repository-webhook__secret").removeClass("d-none");
                }, $btn);
            });
        });
    }
})();
