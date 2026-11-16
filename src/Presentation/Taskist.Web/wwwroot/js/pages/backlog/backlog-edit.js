(function () {
    "use strict";

    var $cfg = $("#backlog-edit-config");
    if (!$cfg.length) return;

    var cfg = {
        id: parseInt($cfg.data("id"), 10),
        saveUrl: $cfg.data("save-url"),
        saveAllUrl: $cfg.data("save-all-url"),
        deleteUrl: $cfg.data("delete-url"),
        indexUrl: $cfg.data("index-url"),
        closingCheckUrl: $cfg.data("closing-check-url"),
        closingUrl: $cfg.data("closing-url"),
        closingTitle: $cfg.data("closing-title") || "Closing Comment"
    };

    window.BacklogEdit = {
        dirty: new Set(),
        collectionsDirty: false,
        customDirty: {},

        markDirty: function (name) {
            if (!name) return;
            this.dirty.add(name);
            this.arm();
        },

        markCollectionsDirty: function () {
            this.collectionsDirty = true;
            this.arm();
        },

        markCustomDirty: function (reference, value) {
            if (reference === undefined || reference === null) return;
            this.customDirty[reference] = value;
            this.arm();
        },

        arm: function () {
            $("#btnSaveBacklog").prop("disabled", false);
            JSManager.NavGuard.markDirty();
        },

        hasChanges: function () {
            return this.dirty.size > 0 || this.collectionsDirty || Object.keys(this.customDirty).length > 0;
        },

        clear: function () {
            this.dirty.clear();
            this.collectionsDirty = false;
            this.customDirty = {};
            $("#btnSaveBacklog").prop("disabled", true);
            JSManager.NavGuard.clear();
            $(document).trigger("backlog:saved");
        },

        postChanges: function (names, reload, onSaved) {
            var self = this;

            if (window.tinymce) tinymce.triggerSave();

            var data = { id: cfg.id };
            names.forEach(function (n, i) {
                data["changes[" + i + "].Name"] = n;
                data["changes[" + i + "].Value"] = ($('[name="' + n + '"]').val() || "").toString();
            });

            (($(".backlog-tags").val()) || []).forEach(function (v, i) { data["tags[" + i + "]"] = v; });

            var refs = Object.keys(self.customDirty);
            refs.forEach(function (ref, i) {
                data["customFields[" + i + "].Reference"] = ref;
                data["customFields[" + i + "].Value"] = self.customDirty[ref];
            });

            $.ajax({
                type: "POST",
                url: cfg.saveAllUrl,
                data: JSManager.addAntiForgeryToken(data),
                traditional: false,
                dataType: "json",
                beforeSend: function () { if (JSManager.blockUI) JSManager.blockUI(); },
                success: function (r) {
                    if (r.status === 2000) {
                        self.clear();
                        if (onSaved) { onSaved(); return; }
                        JSManager.showSuccess(r.message);
                        if (reload) window.location.reload();
                    } else {
                        JSManager.showError(r.message);
                    }
                },
                error: function () { JSManager.showError("Save failed."); },
                complete: function () { if (JSManager.unblockUI) JSManager.unblockUI(); }
            });
        },

        save: function () {
            if (!this.hasChanges()) return;
            if (!$("#EditForm").valid()) return;

            var self = this;
            var names = Array.from(this.dirty);

            if (this.dirty.has("StatusId")) {
                var statusVal = $('[name="StatusId"]').val();
                var others = names.filter(function (n) { return n !== "StatusId"; });
                var checkAndClose = function () {
                    var resp = JSManager.ajaxGetWithResponse(cfg.closingCheckUrl, { id: cfg.id, taskType: $("#TaskTypeId").val(), status: statusVal });
                    if (resp && resp.Valid) {
                        self.markDirty("StatusId");
                        JSManager.openForm(cfg.closingUrl + "?id=" + cfg.id + "&status=" + statusVal, cfg.closingTitle, "col-md-6");
                        return;
                    }
                    self.dirty.add("StatusId");
                    self.postChanges(["StatusId"], true);
                };

                self.dirty.delete("StatusId");
                if (others.length || self.collectionsDirty || Object.keys(self.customDirty).length)
                    self.postChanges(others, false, checkAndClose);
                else
                    checkAndClose();
                return;
            }

            this.postChanges(names, true);
        }
    };

    $(function () {
        $(".ajax-update, .ajax-update-module").on("change", function () {
            window.BacklogEdit.markDirty($(this).attr("Name"));
        });

        $(".backlog-tags").on("change", function () {
            window.BacklogEdit.markCollectionsDirty();
        });

        $("#btnSaveBacklog").on("click", function () { window.BacklogEdit.save(); });

        $("#btnDeleteBacklog").on("click", function () {
            JSManager.confirm("Are you sure you want to delete this task? This action cannot be undone.", function () {
                var dto = JSManager.addAntiForgeryToken({ id: cfg.id });
                JSManager.ajaxPost(cfg.deleteUrl, dto, undefined, undefined, function (r) {
                    if (r.status === 2000) {
                        window.BacklogEdit.clear();
                        JSManager.showSuccess(r.message);
                        window.location.href = cfg.indexUrl;
                    } else {
                        JSManager.showError(r.message);
                    }
                });
            });
        });

        $("#btnMoveToQuickCapture").on("click", function () {
            var $btn = $(this);
            var escape = function (s) { return $("<div>").text(String(s || "")).html(); };

            $.confirm({
                title: $btn.data("title"),
                icon: "fas fa-inbox",
                columnClass: "medium",
                content: "<p>" + escape($btn.data("message")) + "</p>" +
                    '<textarea class="form-control form-control-sm move-qc-reason" rows="2" maxlength="250" placeholder="' +
                    escape($btn.data("reason-placeholder")) + '"></textarea>' +
                    '<div class="move-qc-check mt-3"></div>',
                onContentReady: function () {
                    var dialog = this;
                    var $box = dialog.$content.find(".move-qc-check");
                    dialog.buttons.move.disable();
                    $box.html('<span class="text-muted small"><i class="fas fa-spinner fa-spin me-1"></i>' + escape($btn.data("checking-text")) + "</span>");

                    $.ajax({
                        type: "POST",
                        url: $btn.data("preview-url"),
                        data: JSManager.addAntiForgeryToken({ ids: String($btn.data("task-id")), includeSubTasks: false }),
                        success: function (r) {
                            if (!r || r.status !== 2000) {
                                $box.html('<div class="text-danger small">' + escape(r ? r.message : "Failed") + "</div>");
                                return;
                            }
                            var skipped = r.skipped || [];
                            if (skipped.length) {
                                $box.html('<div class="alert alert-danger py-2 px-3 small mb-0">' + escape(skipped[0].reason) + "</div>");
                                return;
                            }
                            var staying = r.staying || [];
                            $box.html(staying.length
                                ? '<div class="alert alert-info py-2 px-3 small mb-0"><i class="fas fa-circle-info me-1"></i>' +
                                  escape(String($btn.data("staying-text")).replace("{0}", staying.map(function (s) { return "#" + s.id; }).join(", "))) + "</div>"
                                : "");
                            dialog.buttons.move.enable();
                        },
                        error: function () { $box.html('<div class="text-danger small">Failed</div>'); }
                    });
                },
                buttons: {
                    move: {
                        text: $btn.data("move-label"),
                        btnClass: "btn-blue",
                        action: function () {
                            var reason = $.trim(this.$content.find(".move-qc-reason").val() || "");
                            var dto = JSManager.addAntiForgeryToken({ reason: reason });
                            JSManager.ajaxPost($btn.data("url"), dto, undefined, undefined, function (r) {
                                if (r && r.status === 2000) {
                                    window.BacklogEdit.clear();
                                    JSManager.showSuccess(r.message);
                                    window.location.href = $btn.data("redirect-url");
                                } else {
                                    JSManager.showError(r ? r.message : "Failed");
                                }
                            });
                        }
                    },
                    cancel: { text: "Cancel" }
                }
            });
        });

        var owner = $cfg.data("owner") || "";
        var titleLabel = $cfg.data("title-label") || $cfg.data("title");
        var titleHtml = '<span class="bl-head-label">' + titleLabel + '</span>'
            + '<button type="button" class="bl-head-id" id="btnCopyTaskId" title="Copy link to this task">'
            + '<i class="fas fa-hashtag bl-head-id__hash"></i>'
            + '<span class="bl-head-id__num">' + cfg.id + '</span>'
            + '<i class="far fa-copy bl-head-id__copy"></i>'
            + '</button>';
        if (owner) {
            titleHtml += '<span class="bl-head-sep" aria-hidden="true"></span>'
                + '<span title="Owner" class="bl-head-owner">'
                + '<i class="fa fa-user bl-head-owner__icon"></i>' + owner + '</span>';
        }
        JSManager.setPageTitle(titleHtml);

        $(document).off("click.copyTaskId").on("click.copyTaskId", "#btnCopyTaskId", function () {
            var link = window.location.origin + window.location.pathname;
            var done = function () {
                if (JSManager.showSuccess) JSManager.showSuccess("Task #" + cfg.id + " link copied");
                var $b = $("#btnCopyTaskId").addClass("is-copied");
                setTimeout(function () { $b.removeClass("is-copied"); }, 1200);
            };
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(link).then(done, done);
            } else {
                var $t = $("<textarea>").val(link).appendTo("body").select();
                try { document.execCommand("copy"); } catch (e) { }
                $t.remove();
                done();
            }
        });

        var buttons = "";
        buttons += "<a href=\"#taskActivityCard\" data-activity-open=\"history\" title=\"" + $cfg.data("history-label") + "\" class=\"btn btn-sm btn-primary text-white me-2\"><i class=\"fa-solid fa-history\"></i></a>";
        buttons += "<a href=\"#taskActivityCard\" data-activity-open=\"comments\" title=\"" + $cfg.data("comments-label") + "\" class=\"btn btn-sm btn-success text-white me-2\"><i class=\"fa-solid fa-comments\"></i></a>";
        JSManager.setPageButtons(buttons);

        // Allow inline style + our card markup through Bootstrap's tooltip sanitizer
        // (needed so the status-card header keeps its status color).
        var tipAllowList = Object.assign({}, bootstrap.Tooltip.Default.allowList);
        ['div', 'span', 'i', 'b'].forEach(function (tag) {
            tipAllowList[tag] = (tipAllowList[tag] || []).concat(['class', 'style']);
        });

        var tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        [...tooltipTriggerList].map(function (el) {
            return new bootstrap.Tooltip(el, { allowList: tipAllowList });
        });
    });
})();
