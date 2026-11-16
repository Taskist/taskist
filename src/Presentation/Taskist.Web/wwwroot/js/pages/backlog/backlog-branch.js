/* Git branch actions (create / link / unlink) for GitHub and GitLab, shared by the task page and the Backlog grid.
   Errors raised while a dialog is open are shown inside that dialog (with the Git server's own message);
   errors with no dialog open fall back to a SweetAlert. Branch creation uses the clicker's own
   Git account, so a "connect your account" panel is shown when they have not connected yet. */
var BacklogBranch = (function () {
    "use strict";

    var PREFIXES = ["feature", "bugfix", "task", "hotfix"];

    function escapeHtml(value) {
        return $("<div>").text(String(value == null ? "" : value)).html();
    }

    function gitLabError(message) {
        JSManager.showSwal(message || "Request to the Git server failed.", "error");
    }

    function connectLink() {
        var url = $("[data-connect-url]").first().data("connect-url");
        var label = $("[data-connect-url]").first().data("connect-label") || "Connect my Git account";
        return url ? "<a href='" + escapeHtml(url) + "' class='btn btn-sm btn-primary mt-2'><i class='fas fa-plug me-1'></i>" + escapeHtml(label) + "</a>" : "";
    }

    function repositorySelect(repositories, selected) {
        if (!repositories || repositories.length < 2) return "";
        return "<label class='form-label small mb-1'>Repository</label><select class='form-select form-select-sm branch-repo mb-2'>" +
            repositories.map(function (r) {
                return "<option value='" + escapeHtml(r.id) + "'" + (String(r.id) === String(selected) ? " selected" : "") + ">" + escapeHtml(r.name) + "</option>";
            }).join("") + "</select>";
    }

    function showDialogError($content, message, needsConnection) {
        var $box = $content.find(".branch-dialog-error");
        $box.html("<i class='fas fa-circle-exclamation me-1'></i>" + escapeHtml(message || "Request to the Git server failed.") +
            (needsConnection ? "<div>" + connectLink() + "</div>" : "")).removeClass("d-none");
        $box[0].scrollIntoView({ block: "nearest" });
    }

    function post(url, data, onSuccess, $btn, $dialogContent) {
        if ($btn) $btn.prop("disabled", true);
        if ($dialogContent) $dialogContent.find(".branch-dialog-error").addClass("d-none").empty();

        var fail = function (message, needsConnection) {
            if ($dialogContent && $dialogContent.closest("body").length) showDialogError($dialogContent, message, needsConnection);
            else gitLabError(message);
        };

        $.ajax({
            type: "POST",
            url: url,
            data: JSManager.addAntiForgeryToken(data),
            success: function (r) {
                if (r && r.status === 2000) onSuccess(r);
                else fail(r ? r.message : null, r && r.needsConnection);
            },
            error: function (xhr) { fail("Request failed (" + xhr.status + ")."); },
            complete: function () { if ($btn) $btn.prop("disabled", false); }
        });
    }

    function withPrefix(name, prefix) {
        var rest = name.indexOf("/") >= 0 ? name.substring(name.indexOf("/") + 1) : name;
        return prefix + "/" + rest;
    }

    function skippedHtml(skipped, editUrl) {
        if (!skipped || !skipped.length) return "";
        return "<div class='alert alert-warning py-2 px-3 small mb-2'><div class='fw-semibold mb-1'>" + skipped.length + " task(s) will not be linked:</div><ul class='mb-0 ps-3'>" +
            skipped.map(function (s) {
                return "<li><a href='" + escapeHtml(editUrl + "/" + s.id) + "' target='_blank'>#" + escapeHtml(s.id) + "</a> " + escapeHtml(s.reason) + "</li>";
            }).join("") + "</ul></div>";
    }

    function busy($btn, on) {
        if (!$btn || !$btn.length) return;
        if (on) {
            $btn.data("busy-html", $btn.html()).prop("disabled", true).addClass("disabled");
            var $icon = $btn.find("i").first();
            if ($icon.length) $icon.attr("class", "fas fa-spinner fa-spin me-1");
            else $btn.prepend("<i class='fas fa-spinner fa-spin me-1'></i>");
        } else {
            var html = $btn.data("busy-html");
            if (html !== undefined) $btn.html(html);
            $btn.prop("disabled", false).removeClass("disabled");
        }
    }

    function openCreate(cfg) {
        var $trigger = $(cfg.trigger);
        if ($trigger.prop("disabled")) return;
        busy($trigger, true);
        $.get(cfg.contextUrl, { ids: cfg.ids.join(","), repositoryId: cfg.repositoryId || null })
            .done(function (ctx) {
                if (!ctx || ctx.status !== 2000) { gitLabError(ctx ? ctx.message : null); return; }
                if (!ctx.canCreate) { gitLabError(cfg.labels.disabled); return; }

                if (ctx.needsConnection) {
                    $.alert({
                        title: cfg.labels.createTitle,
                        icon: "fas fa-code-branch",
                        columnClass: "medium",
                        content: "<div class='branch-connect'>" +
                            "<div class='branch-connect__text'><i class='fas fa-user-lock me-1'></i>" + escapeHtml(ctx.connectionMessage) + "</div>" +
                            "<div class='small text-muted mt-2'>" + escapeHtml(cfg.labels.connectWhy || "") + "</div>" +
                            connectLink() + "</div>",
                        buttons: { close: { text: "Close" } }
                    });
                    return;
                }

                var options = (ctx.baseBranches || []).map(function (b) {
                    return "<option value='" + escapeHtml(b) + "'" + (b === ctx.defaultBranch ? " selected" : "") + ">" + escapeHtml(b) + (b === ctx.defaultBranch ? " (default)" : "") + "</option>";
                }).join("");

                var prefixButtons = PREFIXES.map(function (p) {
                    return "<button type='button' class='btn btn-sm btn-outline-secondary branch-prefix' data-prefix='" + p + "'>" + p + "/</button>";
                }).join(" ");

                var existing = ctx.existing || [];
                var existingIds = existing.map(function (s) { return s.id; });
                var onlyExisting = !(ctx.eligible || []).length && existing.length > 0;
                var existingHtml = existing.length
                    ? "<div class='branch-existing" + (onlyExisting ? " branch-existing--all" : "") + "'>" +
                        "<div class='fw-semibold mb-1'><i class='fas fa-code-branch me-1'></i>" + escapeHtml(onlyExisting ? cfg.labels.existingAll : cfg.labels.existingSome) + "</div>" +
                        "<ul class='mb-2 ps-3'>" + existing.map(function (s) {
                            return "<li><a href='" + escapeHtml(cfg.editUrl + "/" + s.id) + "' target='_blank'>#" + escapeHtml(s.id) + "</a> " + escapeHtml(s.reason) + "</li>";
                        }).join("") + "</ul>" +
                        "<div class='form-check small mb-0'><input class='form-check-input branch-include-existing' type='checkbox' id='brIncludeExisting' />" +
                        "<label class='form-check-label' for='brIncludeExisting'>" + escapeHtml(cfg.labels.includeExisting) + "</label></div></div>"
                    : "";
                var targetIds = function ($c) {
                    var ids = (ctx.eligible || []).slice();
                    if ($c.find(".branch-include-existing").is(":checked")) ids = ids.concat(existingIds);
                    return ids;
                };

                var content =
                    repositorySelect(ctx.repositories, ctx.repositoryId) +
                    skippedHtml(ctx.skipped, cfg.editUrl) + existingHtml +
                    ((ctx.eligible || []).length ? "<p class='small text-muted mb-2'>" + escapeHtml(String(cfg.labels.linkInfo).replace("{0}", (ctx.eligible || []).map(function (i) { return "#" + i; }).join(", "))) + "</p>" : "") +
                    "<label class='form-label small mb-1'>" + escapeHtml(cfg.labels.name) + "</label>" +
                    "<input type='text' class='form-control form-control-sm branch-name' value='" + escapeHtml(ctx.suggestedName) + "' maxlength='255' />" +
                    "<div class='mt-1 mb-2'>" + prefixButtons + "</div>" +
                    "<label class='form-label small mb-1'>" + escapeHtml(cfg.labels.base) + "</label>" +
                    "<select class='form-select form-select-sm branch-base'>" + options + "</select>" +
                    "<div class='alert alert-danger py-2 px-3 small mt-3 mb-0 branch-dialog-error d-none' role='alert'></div>";

                $.confirm({
                    title: cfg.labels.createTitle,
                    icon: "fas fa-code-branch",
                    columnClass: "medium",
                    content: content,
                    onContentReady: function () {
                        var dialog = this;
                        var $c = dialog.$content;
                        $c.find(".branch-repo").on("change", function () {
                            dialog.close();
                            openCreate($.extend({}, cfg, { repositoryId: $(this).val() }));
                        });
                        $c.find(".branch-prefix").on("click", function () {
                            var $input = $c.find(".branch-name");
                            $input.val(withPrefix($input.val(), $(this).data("prefix")));
                        });
                        if (onlyExisting) {
                            dialog.buttons.create.disable();
                            $c.find(".branch-include-existing").on("change", function () {
                                if (this.checked) dialog.buttons.create.enable(); else dialog.buttons.create.disable();
                            });
                        }
                    },
                    buttons: {
                        create: {
                            text: cfg.labels.create,
                            btnClass: "btn-blue",
                            action: function () {
                                var dialog = this;
                                var name = $.trim(dialog.$content.find(".branch-name").val() || "");
                                var base = dialog.$content.find(".branch-base").val();
                                if (!name) { showDialogError(dialog.$content, cfg.labels.nameRequired); return false; }

                                var ids = targetIds(dialog.$content);
                                if (!ids.length) { showDialogError(dialog.$content, cfg.labels.existingAll); return false; }
                                post(cfg.createUrl, { ids: ids.join(","), name: name, baseBranch: base, repositoryId: ctx.repositoryId, includeExisting: dialog.$content.find(".branch-include-existing").is(":checked") }, function (r) {
                                    dialog.close();
                                    JSManager.showSuccess(r.message);
                                    copyCheckout(r.branch);
                                    if (cfg.onDone) cfg.onDone(r);
                                }, dialog.$$create, dialog.$content);
                                return false;
                            }
                        },
                        cancel: { text: "Cancel" }
                    }
                });
            })
            .fail(function (xhr) { gitLabError("Request failed (" + xhr.status + ")."); })
            .always(function () { busy($trigger, false); });
    }

    function openLink(cfg) {
        var content =
            "<div class='branch-repo-holder'></div>" +
            "<input type='text' class='form-control form-control-sm branch-search' placeholder='" + escapeHtml(cfg.labels.searchPlaceholder) + "' />" +
            "<div class='branch-results mt-2'></div>" +
            "<div class='alert alert-danger py-2 px-3 small mt-2 mb-0 branch-dialog-error d-none' role='alert'></div>";

        $.confirm({
            title: cfg.labels.linkTitle,
            icon: "fas fa-link",
            columnClass: "medium",
            content: content,
            onContentReady: function () {
                var dialog = this;
                var $c = dialog.$content;
                var timer = null;
                var repositoryId = null;

                function search() {
                    var term = $.trim($c.find(".branch-search").val() || "");
                    $c.find(".branch-results").html("<span class='text-muted small'><i class='fas fa-spinner fa-spin me-1'></i></span>");
                    $.get(cfg.searchUrl, { id: cfg.taskId, term: term, repositoryId: repositoryId })
                        .done(function (r) {
                            if (!r || r.status !== 2000) { $c.find(".branch-results").empty(); showDialogError($c, r ? r.message : null); return; }
                            if (r.repositories && r.repositories.length > 1 && !$c.find(".branch-repo").length) {
                                repositoryId = r.repositoryId;
                                $c.find(".branch-repo-holder").html(repositorySelect(r.repositories, r.repositoryId));
                                $c.find(".branch-repo").on("change", function () { repositoryId = $(this).val(); search(); });
                            }
                            var list = r.branches || [];
                            $c.find(".branch-results").html(list.length
                                ? "<div class='list-group list-group-flush branch-result-list'>" + list.map(function (b) {
                                    return "<button type='button' class='list-group-item list-group-item-action py-1 small branch-pick' data-name='" + escapeHtml(b) + "'><i class='fas fa-code-branch me-1 text-muted'></i>" + escapeHtml(b) + "</button>";
                                }).join("") + "</div>"
                                : "<span class='text-muted small'>" + escapeHtml(cfg.labels.noResults) + "</span>");
                        })
                        .fail(function (xhr) { showDialogError($c, "Request failed (" + xhr.status + ")."); });
                }

                $c.find(".branch-search").on("input", function () {
                    clearTimeout(timer);
                    timer = setTimeout(search, 300);
                }).trigger("focus");

                $c.on("click", ".branch-pick", function () {
                    post(cfg.linkUrl, { ids: cfg.taskId, name: $(this).data("name"), repositoryId: repositoryId }, function (r) {
                        dialog.close();
                        JSManager.showSuccess(r.message);
                        if (cfg.onDone) cfg.onDone(r);
                    }, null, $c);
                });

                search();
            },
            buttons: { cancel: { text: "Cancel" } }
        });
    }

    function unlink(cfg) {
        JSManager.confirm(escapeHtml(String(cfg.labels.unlinkConfirm).replace("{0}", cfg.branchName)), function () {
            post(cfg.unlinkUrl, { id: cfg.taskId, branchId: cfg.branchId }, function (r) {
                JSManager.showSuccess(r.message);
                if (cfg.onDone) cfg.onDone(r);
            });
        });
    }

    function linkByName(cfg) {
        post(cfg.linkUrl, { ids: cfg.taskId, name: cfg.name }, function (r) {
            JSManager.showSuccess(r.message);
            if (cfg.onDone) cfg.onDone(r);
        }, cfg.$btn);
    }

    function openCreateMergeRequest(cfg) {
        var L = cfg.labels;
        var $trigger = $(cfg.trigger);
        if ($trigger.prop("disabled")) return;
        busy($trigger, true);
        $.get(cfg.contextUrl, { id: cfg.taskId, branchId: cfg.branchId })
            .done(function (ctx) {
                if (!ctx || ctx.status !== 2000) { gitLabError(ctx ? ctx.message : null); return; }

                if (ctx.existingIid) {
                    $.alert({
                        title: L.title, icon: "fas fa-code-pull-request", columnClass: "medium",
                        content: "<div class='alert alert-info mb-0'>" + escapeHtml(String(L.existing).replace("{0}", ctx.existingIid)) +
                            (ctx.existingUrl ? " <a href='" + escapeHtml(ctx.existingUrl) + "' target='_blank' rel='noopener'>" + escapeHtml(ctx.refPrefix || "!") + escapeHtml(ctx.existingIid) + "</a>" : "") + "</div>",
                        buttons: { close: { text: "Close" } }
                    });
                    return;
                }

                if (ctx.needsConnection) {
                    $.alert({
                        title: L.title, icon: "fas fa-code-pull-request", columnClass: "medium",
                        content: "<div class='branch-connect'><div class='branch-connect__text'><i class='fas fa-user-lock me-1'></i>" +
                            escapeHtml(ctx.connectionMessage) + "</div>" + connectLink() + "</div>",
                        buttons: { close: { text: "Close" } }
                    });
                    return;
                }

                var options = (ctx.targetBranches || []).map(function (b) {
                    return "<option value='" + escapeHtml(b) + "'" + (b === ctx.defaultTarget ? " selected" : "") + ">" + escapeHtml(b) + "</option>";
                }).join("");
                var moves = ctx.openedStatus
                    ? "<div class='mr-dialog__note'><i class='fas fa-arrow-right-arrow-left me-1'></i>" +
                      escapeHtml(String(L.moves).replace("{0}", (ctx.taskIds || []).map(function (i) { return "#" + i; }).join(", ")).replace("{1}", ctx.openedStatus)) + "</div>"
                    : "";

                var content =
                    "<label class='form-label small mb-1'>" + escapeHtml(L.fieldTitle) + "</label>" +
                    "<input type='text' class='form-control form-control-sm mr-title' maxlength='250' value='" + escapeHtml(ctx.title) + "' />" +
                    "<div class='mr-dialog__branches'>" +
                        "<div><span class='mr-dialog__label'>" + escapeHtml(L.from) + "</span><code>" + escapeHtml(ctx.sourceBranch) + "</code></div>" +
                        "<i class='fas fa-arrow-right-long text-muted'></i>" +
                        "<div class='flex-grow-1'><span class='mr-dialog__label'>" + escapeHtml(L.into) + "</span><select class='form-select form-select-sm mr-target'>" + options + "</select></div>" +
                    "</div>" +
                    "<label class='form-label small mb-1'>" + escapeHtml(L.description) + "</label>" +
                    "<textarea class='form-control form-control-sm mr-description' rows='6'>" + escapeHtml(ctx.description) + "</textarea>" +
                    "<div class='mr-dialog__meta'><i class='fas fa-user-check me-1'></i>" + escapeHtml(L.reviewer) + ": <strong>" + escapeHtml(ctx.reviewer || L.noReviewer) + "</strong></div>" +
                    "<div class='form-check form-check-inline small mt-2'><input class='form-check-input mr-draft' type='checkbox' id='mrDraft' /><label class='form-check-label' for='mrDraft'>" + escapeHtml(L.draft) + "</label></div>" +
                    "<div class='form-check form-check-inline small mt-2'><input class='form-check-input mr-remove' type='checkbox' id='mrRemove' checked /><label class='form-check-label' for='mrRemove'>" + escapeHtml(L.remove) + "</label></div>" +
                    moves +
                    "<div class='alert alert-danger py-2 px-3 small mt-3 mb-0 branch-dialog-error d-none' role='alert'></div>";

                $.confirm({
                    title: L.title,
                    icon: "fas fa-code-pull-request",
                    columnClass: "col-md-8",
                    content: content,
                    onContentReady: function () {
                        var $c = this.$content;
                        $c.find(".mr-draft").on("change", function () { $c.find(".mr-dialog__note").toggleClass("is-muted", this.checked); });
                    },
                    buttons: {
                        create: {
                            text: L.create,
                            btnClass: "btn-blue",
                            action: function () {
                                var dialog = this;
                                var $c = dialog.$content;
                                var title = $.trim($c.find(".mr-title").val() || "");
                                if (!title) { showDialogError($c, L.titleRequired); return false; }

                                post(cfg.createUrl, {
                                    id: cfg.taskId,
                                    branchId: cfg.branchId,
                                    title: title,
                                    targetBranch: $c.find(".mr-target").val(),
                                    description: $c.find(".mr-description").val(),
                                    draft: $c.find(".mr-draft").is(":checked"),
                                    removeSourceBranch: $c.find(".mr-remove").is(":checked")
                                }, function (r) {
                                    dialog.close();
                                    JSManager.showSuccess(r.message);
                                    if (cfg.onDone) cfg.onDone(r);
                                }, dialog.$$create, $c);
                                return false;
                            }
                        },
                        cancel: { text: "Cancel" }
                    }
                });
            })
            .fail(function (xhr) { gitLabError("Request failed (" + xhr.status + ")."); })
            .always(function () { busy($trigger, false); });
    }

    function copyCheckout(branch) {
        if (!branch || !navigator.clipboard) return;
        navigator.clipboard.writeText("git fetch origin; git checkout '" + branch + "'").catch(function () { });
    }

    $(document).on("click", "#btnBulkCreateBranch", function () {
        var $btn = $(this);
        var d = $btn.data();
        var ids = (typeof selectedRows !== "undefined") ? Array.from(selectedRows) : [];
        if (!ids.length) return;

        openCreate({
            ids: ids,
            trigger: $btn,
            contextUrl: d.contextUrl,
            createUrl: d.createUrl,
            editUrl: d.editUrl,
            labels: {
                createTitle: d.labelCreateTitle, create: d.labelCreate, name: d.labelName, base: d.labelBase,
                linkInfo: d.labelLinkInfo, nameRequired: d.labelNameRequired, disabled: d.labelDisabled, connectWhy: d.labelConnectWhy,
                existingSome: d.labelExistingSome, existingAll: d.labelExistingAll, includeExisting: d.labelIncludeExisting
            },
            onDone: function () {
                if (typeof resetSelection === "function") resetSelection();
                if (window.BacklogRefresh) BacklogRefresh.refresh();
            }
        });
    });

    return { openCreate: openCreate, openLink: openLink, unlink: unlink, linkByName: linkByName, openCreateMergeRequest: openCreateMergeRequest, gitLabError: gitLabError };
})();
