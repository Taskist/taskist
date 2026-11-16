(function () {
    "use strict";

    function escapeHtml(value) {
        return $("<div>").text(String(value == null ? "" : value)).html();
    }

    function gitLabError(message) {
        JSManager.showSwal(message || "Request to the Git server failed.", "error");
    }

    function cleanupSync($panel) {
        $panel.find(".branch-cleanup__group").each(function () {
            var $picks = $(this).find(".branch-cleanup__pick:enabled");
            var checked = $picks.filter(":checked").length;
            $(this).find(".branch-cleanup__all").prop({ checked: checked > 0 && checked === $picks.length, indeterminate: checked > 0 && checked < $picks.length });
        });
        var total = $panel.find(".branch-cleanup__pick:checked").length;
        $panel.find(".branch-cleanup__selected").text(total);
        $panel.find(".branch-cleanup__delete").prop("disabled", total === 0);
    }

    $(document).on("click", ".js-branch-panel", function () {
        var $btn = $(this);
        if ($btn.prop("disabled")) return;

        var $icon = $btn.find("i").first();
        var iconClass = $icon.attr("class");
        var $body = $("#pdOffCanvasBody");
        $btn.prop("disabled", true);
        $icon.attr("class", "fas fa-spinner fa-spin me-1");

        $("#pdOffCanvasBodyTitle").text($btn.data("title"));
        $body.html("<div class='branch-cleanup__loading'><span class='spinner-border spinner-border-sm' role='status'></span><span>" + escapeHtml($btn.data("loading")) + "</span></div>");
        bootstrap.Offcanvas.getOrCreateInstance(document.getElementById("pdOffCanvas")).show();

        $.get($btn.data("url"))
            .done(function (html) { $body.html(html); })
            .fail(function (xhr) {
                $body.html("<div class='alert alert-danger py-2 small'>" + escapeHtml("Request failed (" + xhr.status + ").") + "</div>");
            })
            .always(function () {
                $btn.prop("disabled", false);
                $icon.attr("class", iconClass);
            });
    });

    $(document).on("click", ".unlinked-branch-link", function () {
        var $row = $(this).closest(".unlinked-branch-row");
        var $panel = $row.closest(".unlinked-branches");
        var name = $row.data("name");

        $.confirm({
            title: escapeHtml(name),
            icon: "fas fa-link",
            content: "<label class='form-label small'>" + escapeHtml($panel.data("prompt")) + "</label>" +
                "<input type='text' class='form-control form-control-sm unlinked-task-ids' placeholder='e.g. 2520, 2531' />" +
                "<div class='alert alert-danger py-2 px-3 small mt-2 mb-0 branch-dialog-error d-none' role='alert'></div>",
            buttons: {
                link: {
                    text: "Link",
                    btnClass: "btn-blue",
                    action: function () {
                        var dialog = this;
                        var $error = dialog.$content.find(".branch-dialog-error").addClass("d-none").empty();
                        var ids = (dialog.$content.find(".unlinked-task-ids").val() || "").replace(/#/g, "").split(/[\s,;]+/).filter(Boolean).join(",");
                        if (!ids) return false;
                        $.ajax({
                            type: "POST",
                            url: $panel.data("link-url"),
                            data: JSManager.addAntiForgeryToken({ ids: ids, name: name }),
                            success: function (r) {
                                if (r && r.status === 2000) {
                                    dialog.close();
                                    JSManager.showSuccess(r.message);
                                    $row.slideUp(150, function () { $row.remove(); });
                                } else {
                                    $error.html("<i class='fas fa-circle-exclamation me-1'></i>" + escapeHtml((r && r.message) || "Request to the Git server failed.")).removeClass("d-none");
                                }
                            },
                            error: function (xhr) {
                                $error.html("<i class='fas fa-circle-exclamation me-1'></i>" + escapeHtml("Request failed (" + xhr.status + ").")).removeClass("d-none");
                            }
                        });
                        return false;
                    }
                },
                cancel: { text: "Cancel" }
            }
        });
    });

    $(document).on("change", ".branch-cleanup__all", function () {
        $(this).closest(".branch-cleanup__group").find(".branch-cleanup__pick:enabled").prop("checked", this.checked);
        cleanupSync($(this).closest(".branch-cleanup"));
    });

    $(document).on("change", ".branch-cleanup__pick", function () {
        cleanupSync($(this).closest(".branch-cleanup"));
    });

    $(document).on("click", ".branch-cleanup__open", function (e) {
        e.stopPropagation();
    });

    $(document).on("click", ".branch-cleanup__delete", function () {
        var $btn = $(this);
        var $panel = $btn.closest(".branch-cleanup");
        var $picked = $panel.find(".branch-cleanup__pick:checked");
        var ids = $picked.map(function () { return this.value; }).get();
        if (!ids.length) return;

        Swal.fire({
            text: String($panel.data("confirm")).replace("{0}", ids.length),
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#808080",
            confirmButtonText: $panel.data("yes"),
            cancelButtonText: $panel.data("no"),
            allowOutsideClick: false
        }).then(function (result) {
            if (!result.isConfirmed) return;
            $btn.prop("disabled", true).find("i").attr("class", "fas fa-spinner fa-spin me-1");
            $.ajax({
                type: "POST",
                url: $panel.data("delete-url"),
                data: JSManager.addAntiForgeryToken({ ids: ids.join(",") }),
                success: function (r) {
                    var failures = (r && r.failures) || [];
                    if (r && (r.deleted > 0 || r.alreadyGone > 0)) {
                        $picked.each(function () {
                            var name = $(this).closest(".branch-cleanup__row").find(".branch-cleanup__name").text();
                            var failed = failures.some(function (f) { return f.indexOf(name + ":") === 0; });
                            if (!failed) $(this).closest(".branch-cleanup__row").slideUp(150, function () { $(this).remove(); cleanupSync($panel); });
                        });
                    }
                    var html = "<div>" + escapeHtml(r ? r.message : "") + "</div>";
                    if (failures.length)
                        html += "<ul class='text-start small mt-2 mb-0'>" + failures.map(function (f) { return "<li>" + escapeHtml(f) + "</li>"; }).join("") + "</ul>";
                    Swal.fire({ html: html, icon: failures.length ? (r.deleted || r.alreadyGone ? "warning" : "error") : "success" });
                },
                error: function (xhr) { gitLabError("Request failed (" + xhr.status + ")."); },
                complete: function () {
                    $btn.find("i").attr("class", "fas fa-trash-can me-1");
                    cleanupSync($panel);
                }
            });
        });
    });
})();
