(function () {
    "use strict";

    $(function () {
        var previewModal;

        $("#btnPreviewTeams").on("click", function () {
            var url = $(this).data("url");
            $.ajax({
                type: "POST",
                url: url,
                data: JSManager.addAntiForgeryToken({}),
                dataType: "html",
                success: function (html) {
                    document.getElementById("teamsPreviewFrame").srcdoc = html;
                    if (!previewModal) {
                        previewModal = new bootstrap.Modal(document.getElementById("teamsPreviewModal"));
                    }
                    previewModal.show();
                }
            });
        });

        var $btn = $("#btnTestTeams");
        if (!$btn.length) return;

        $btn.on("click", function () {
            var url = $btn.data("url");
            var webhookUrl = $("#Hr_TeamsWebhookUrl").val();
            var original = $btn.html();

            $btn.prop("disabled", true).html('<i class="fas fa-spinner fa-spin"></i>');

            JSManager.ajaxPost(url, { webhookUrl: webhookUrl }, undefined, undefined, function (response) {
                if (response.status === 2000) {
                    JSManager.showSuccess(response.message);
                } else {
                    JSManager.showError(response.message);
                }
                $btn.prop("disabled", false).html(original);
            });
        });
    });
})();
