/* Backlog form (_Form partial) - dropzone upload, TinyMCE editors, document thumbnails.
   Reads server values from #backlog-form-config. Loaded by Create/Edit. */

(function () {
    "use strict";

    var $cfg = $("#backlog-form-config");
    if (!$cfg.length) return;

    var cfg = $cfg.data();
    var backlogId = parseInt(cfg.backlogId, 10) || 0;

    function renderDocuments() {
        // on Create the task isn't saved yet (Id = 0), so there are no stored
        // documents to fetch - Documents(0) would hit an access check and fail
        if (backlogId > 0) {
            JSManager.renderPartial(cfg.documentsUrl, { id: backlogId }, 'thumbnail-zone');
        }
    }
    window.renderDocuments = renderDocuments; // Documents partial refreshes via this after delete

    $(function () {
        Dropzone.autoDiscover = false;
        var docs = [];

        $("div#dropzone-container").dropzone({
            url: cfg.uploadUrl,
            autoProcessQueue: true,
            maxFilesize: 2,
            maxFiles: 5,
            addRemoveLinks: true,
            acceptedFiles: ".png,.jpg,.jpeg,.gif,.bmp,.webp,.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.csv,.zip",
            init: function () {
                this.on("addedfile", function (file) {
                    var progressElement = file.previewElement.querySelector(".dz-progress");
                    if (progressElement) progressElement.remove();
                });

                this.on("sending", function (file, xhr, formData) {
                    formData.append("reference", backlogId);
                    // the global AutoValidateAntiforgeryToken filter requires a token on this POST
                    var token = $('input[name="__RequestVerificationToken"]').val();
                    if (token) formData.append("__RequestVerificationToken", token);
                });

                this.on("success", function (file, response) {
                    file.serverFilePath = response.token;
                    if (backlogId > 0) {
                        this.removeFile(file);
                        renderDocuments();
                    } else {
                        docs.push(response.token);
                        $('#Token').val(JSON.stringify(docs));
                    }
                });

                this.on("maxfilesexceeded", function (file) {
                    this.removeFile(file);
                    alert("You can only upload up to 5 files.");
                });
            }
        });

        // rich-text editors; sync content back to the textarea on change so the
        // form POST picks it up. Content saves with the whole form.
        var richTextConfig = {
            plugins: 'lists link image table code',
            toolbar: 'undo redo | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent',
            height: 250,
            menubar: false,
            setup: function (editor) {
                editor.on('change keyup', function () { editor.save(); });
            }
        };

        tinymce.init(Object.assign({ selector: '#Description' }, richTextConfig));
        tinymce.init(Object.assign({ selector: '#DeveloperNotes' }, richTextConfig));
        tinymce.init(Object.assign({ selector: '#QualityNotes' }, richTextConfig));

        renderDocuments();

        // collapsible section chevron rotation
        $('#attachSection').on('show.bs.collapse hide.bs.collapse', function (e) {
            $('[data-bs-target="#attachSection"]').toggleClass('collapsed', e.type === 'hide');
        });
    });
})();
