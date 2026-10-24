(function () {
    "use strict";

    $(function () {
        var $git = $(".select2-gitproject");
        if (!$git.length) return;

        $git.each(function () {
            var $this = $(this);
            $this.wrap('<div class="position-relative"></div>').select2({
                placeholder: "Search GitLab projects...",
                allowClear: true,
                width: "100%",
                minimumInputLength: 0,
                dropdownParent: $this.parent(),
                ajax: {
                    url: $this.data("url"),
                    dataType: "json",
                    delay: 250,
                    data: function (params) {
                        return { q: params.term };
                    },
                    processResults: function (data) {
                        if (data && data.error) {
                            if ($this.data("last-error") !== data.error) {
                                $this.data("last-error", data.error);
                                $this.select2("close");
                                JSManager.showSwal(data.error, "error");
                            }
                            return { results: [] };
                        }
                        $this.removeData("last-error");
                        return {
                            results: (data || []).map(function (item) {
                                return { id: item.Value, text: item.Text };
                            })
                        };
                    },
                    cache: true
                }
            });
        });
    });
})();

(function () {
    "use strict";

    $(document).on("click", ".copy-webhook-url", function () {
        var $btn = $(this);
        var $input = $btn.closest(".input-group").find(".gitlab-webhook-url");
        var text = $input.val();
        var done = function () { JSManager.showSuccess($btn.data("copied") || "Copied"); };

        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(text).then(done);
        } else {
            $input.trigger("select");
            document.execCommand("copy");
            done();
        }
    });
})();

(function () {
    "use strict";

    var timer = null;
    var request = 0;

    function escapeHtml(value) {
        return String(value == null ? "" : value)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }

    function refresh($root) {
        var token = ++request;
        $.ajax({
            type: "GET",
            url: $root.data("preview-url"),
            data: {
                format: $root.find(".project-release-format").val(),
                fullIncrement: $root.find(".project-release-increment").val()
            },
            success: function (r) {
                if (token !== request || !r) return;
                var $preview = $root.find(".project-release-preview");
                if (!r.valid) {
                    $preview.html('<span class="text-danger">' + escapeHtml(r.message) + "</span>");
                    return;
                }
                $preview.html(escapeHtml($root.data("full-text")) + " <code>" + escapeHtml(r.full) + "</code> · " +
                    escapeHtml($root.data("patch-text")) + " <code>" + escapeHtml(r.patch) + "</code>");
            }
        });
    }

    $(function () {
        var $root = $(".project-release-version");
        if (!$root.length) return;

        refresh($root);
        $root.on("input", ".project-release-format", function () {
            clearTimeout(timer);
            timer = setTimeout(function () { refresh($root); }, 400);
        }).on("change", ".project-release-increment", function () {
            refresh($root);
        });
    });
})();
