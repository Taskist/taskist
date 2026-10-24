(function () {
    "use strict";

    $(function () {
        var previewModal;

        function showPreview(html) {
            document.getElementById("emailPreviewFrame").srcdoc = html;
            if (!previewModal) {
                previewModal = new bootstrap.Modal(document.getElementById("emailPreviewModal"));
            }
            previewModal.show();
        }

        $(document).on("click", "#btnPreviewEmail", function () {
            var url = $(this).data("url");
            var data = JSManager.addAntiForgeryToken({
                name: $("#Name").val(),
                emailBody: $("#EmailBody").val()
            });

            $.ajax({ type: "POST", url: url, data: data, dataType: "html", success: showPreview });
        });
    });
})();
