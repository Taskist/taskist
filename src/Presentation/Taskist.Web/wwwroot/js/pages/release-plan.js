var ReleasePlan = (function () {
    "use strict";

    var TOOLTIP_SELECTOR = ".release-projects[data-bs-toggle='tooltip']";

    function escapeHtml(value) {
        return String(value == null ? "" : value)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }

    function renderProjects(row) {
        var names = (row && row.ProjectNames) || [];
        if (names.length === 0) return '<span class="text-muted">' + escapeHtml(row ? row.Project : "") + "</span>";
        if (names.length === 1) return escapeHtml(names[0]);

        var tip = '<div class="release-projects-tip"><strong>' + names.length + " projects</strong>" +
            names.map(function (n) { return "<div>" + escapeHtml(n) + "</div>"; }).join("") + "</div>";

        return '<span class="release-projects" data-bs-toggle="tooltip" data-bs-html="true" data-bs-placement="top"' +
            ' data-bs-title="' + escapeHtml(tip) + '">' + escapeHtml(names[0]) +
            ' <span class="badge bg-secondary">+' + (names.length - 1) + "</span></span>";
    }

    function percent(completed, total) {
        return total > 0 ? Math.round(completed * 100 / total) : 0;
    }

    function renderProgress(row, url) {
        row._backlogUrl = url;
        var link = '<a href="' + escapeHtml(url) + '" target="_blank">' + row.CompletedBacklog + "/" + row.TotalBacklog + "</a>";
        var breakdown = (row && row.ProjectProgress) || [];
        if (breakdown.length < 2) return link;

        return link + ' <a href="#" class="release-progress-toggle ms-1" title="Progress by project">' +
            '<i class="fas fa-chevron-down"></i></a>';
    }

    function renderBreakdown(row) {
        var rows = (row.ProjectProgress || []).map(function (p) {
            var pct = percent(p.Completed, p.Total);
            var bar = pct === 100 ? "bg-success" : pct >= 50 ? "bg-primary" : "bg-warning";
            return '<div class="release-progress-row">' +
                '<a class="release-progress-name" href="' + escapeHtml(row._backlogUrl + "&project=" + p.ProjectId) + '" target="_blank"' +
                ' title="' + escapeHtml("Open " + p.Project + " tasks in this release") + '">' + escapeHtml(p.Project) + "</a>" +
                '<div class="progress release-progress-bar"><div class="progress-bar ' + bar + '" style="width:' + pct + '%"></div></div>' +
                '<span class="release-progress-count">' + p.Completed + "/" + p.Total + " (" + pct + "%)</span>" +
                "</div>";
        }).join("");

        return '<div class="release-progress-breakdown">' + rows + "</div>";
    }

    function postMismatchAction(url, data) {
        $.ajax({
            type: "POST",
            url: url,
            data: JSManager.addAntiForgeryToken(data),
            traditional: true,
            success: function (r) {
                if (r && r.status === 2000) {
                    JSManager.showSuccess(r.message);
                    setTimeout(function () { window.location.reload(); }, 800);
                } else {
                    JSManager.showError(r ? r.message : "Failed");
                }
            },
            error: function () { JSManager.showError("Failed"); }
        });
    }

    function resubmit($form) {
        $form.data("carry-checked", true);
        $form.find("button[type='submit']").first().trigger("click");
    }

    function promptCarryOver($form, info) {
        var options = (info.targets || []).map(function (t) {
            return '<option value="' + escapeHtml(t.value) + '">' + escapeHtml(t.text) + "</option>";
        }).join("");

        var content = "<p>" + escapeHtml(String($form.data("carry-msg")).replace("{0}", info.count)) + "</p>" +
            (options
                ? '<select class="form-select form-select-sm release-carry-target"><option value="">' +
                  escapeHtml($form.data("carry-select")) + "</option>" + options + "</select>"
                : '<p class="text-muted small mb-0">' + escapeHtml($form.data("carry-none")) + "</p>");

        var buttons = {};
        if (options) {
            buttons.move = {
                text: $form.data("carry-move"),
                btnClass: "btn-blue",
                action: function () {
                    var target = this.$content.find(".release-carry-target").val();
                    if (!target) { JSManager.showError($form.data("carry-select")); return false; }
                    $form.find("[name='ConfirmOpenTasks']").val("true");
                    $form.find("[name='CarryOverReleaseId']").val(target);
                    resubmit($form);
                }
            };
        }
        buttons.anyway = {
            text: $form.data("carry-anyway"),
            btnClass: "btn-orange",
            action: function () {
                $form.find("[name='ConfirmOpenTasks']").val("true");
                $form.find("[name='CarryOverReleaseId']").val("");
                resubmit($form);
            }
        };
        buttons.cancel = { text: "Cancel" };

        $.confirm({ title: $form.data("carry-title"), content: content, icon: "fa fa-triangle-exclamation", columnClass: "medium", buttons: buttons });
    }

    $(document)
        .on("click", ".release-progress-toggle", function (e) {
            e.preventDefault();
            var $tr = $(this).closest("tr");
            var row = $("#ReleaseGrid").DataTable().row($tr);
            if (!row || !row.data()) return;

            if (row.child.isShown()) {
                row.child.hide();
                $(this).find("i").removeClass("fa-chevron-up").addClass("fa-chevron-down");
            } else {
                row.child(renderBreakdown(row.data())).show();
                $(this).find("i").removeClass("fa-chevron-down").addClass("fa-chevron-up");
            }
        })
        .on("click", ".release-edit-form button[type='submit']", function (e) {
            var $form = $(this).closest("form");
            if ($form.data("carry-checked")) {
                $form.data("carry-checked", false);
                return;
            }

            var status = String($form.find("[name='Status']").val());
            var released = String($form.data("released-status"));
            if (status !== released || String($form.data("original-status")) === released) return;

            e.preventDefault();
            var info = JSManager.ajaxGetWithResponse($form.data("open-tasks-url"), {});
            if (!info || !info.count) {
                resubmit($form);
                return;
            }
            promptCarryOver($form, info);
        })
        .on("preDraw.dt", "#ReleaseGrid", function () {
            $(this).find(TOOLTIP_SELECTOR).each(function () {
                var tip = bootstrap.Tooltip.getInstance(this);
                if (tip) tip.dispose();
            });
        })
        .on("draw.dt", "#ReleaseGrid", function () {
            $(this).find(TOOLTIP_SELECTOR).each(function () {
                bootstrap.Tooltip.getOrCreateInstance(this, { delay: { show: 150, hide: 0 }, container: "body", customClass: "release-projects-tooltip" });
            });
        })
        .on("click", ".release-mismatch-toggle", function (e) {
            e.preventDefault();
            var $group = $(this).closest(".release-mismatch-group");
            var $tasks = $group.find(".release-mismatch-tasks");
            var opening = $tasks.hasClass("d-none");
            var $text = $(this).find(".release-mismatch-toggle-text");

            $tasks.toggleClass("d-none", !opening);
            $text.text(opening ? $text.data("hide") : $text.data("show"));
            $(this).find("i").toggleClass("fa-chevron-down", !opening).toggleClass("fa-chevron-up", opening);
        })
        .on("change", ".release-mismatch-row", function () {
            var $group = $(this).closest(".release-mismatch-group");
            var checked = $group.find(".release-mismatch-row:checked").length;
            var $label = $group.find(".release-mismatch-clear-label");
            $label.text(checked > 0 ? String($label.data("some")).replace("{0}", checked) : $label.data("all"));
        })
        .on("click", ".release-mismatch-add", function () {
            var $panel = $(this).closest(".release-mismatch");
            var $group = $(this).closest(".release-mismatch-group");
            var message = String($panel.data("confirm-add"))
                .replace("{0}", $group.data("project"))
                .replace("{1}", $group.data("release"));

            JSManager.confirm(escapeHtml(message), function () {
                postMismatchAction($panel.data("add-url"), { releaseId: $group.data("release-id"), projectId: $group.data("project-id") });
            });
        })
        .on("click", ".release-mismatch-clear", function () {
            var $panel = $(this).closest(".release-mismatch");
            var $group = $(this).closest(".release-mismatch-group");
            var $checked = $group.find(".release-mismatch-row:checked");
            var $rows = $checked.length ? $checked : $group.find(".release-mismatch-row");
            var ids = $rows.map(function () { return this.value; }).get();
            if (!ids.length) return;

            JSManager.confirm(escapeHtml(String($panel.data("confirm-clear")).replace("{0}", ids.length)), function () {
                postMismatchAction($panel.data("clear-url"), { ids: ids });
            });
        })
        .on("change", "input[name='AllProjects']", function () {
            var allProjects = $("input[name='AllProjects']:checked").val() === "true";
            $(this).closest("form").find(".release-project-picker").toggleClass("d-none", allProjects);
        })
        .on("input", ".release-version-input", function () {
            var $form = $(this).closest("form");
            setVersionAuto($form, false);
            scheduleCheck($form);
        })
        .on("blur", ".release-version-input", function () {
            checkVersion($(this).closest("form"));
        })
        .on("click", ".release-version-auto", function () {
            var $form = $(this).closest("form");
            setVersionAuto($form, true);
            refreshVersion($form);
        })
        .on("change", "select[name='ReleaseType']", function () {
            var $form = $(this).closest("form");
            var $base = $form.find(".release-patch-base");
            $base.prop("hidden", String($(this).val()) !== String($base.data("patch-type")));
        })
        .on("change", "select[name='ReleaseType'], input[name='AllProjects'], select[name='SelectedProjectIds'], select[name='BaseReleaseId']", function () {
            var $form = $(this).closest("form");
            refreshVersion($form, this.name === "BaseReleaseId");
            if (this.name !== "ReleaseType" && this.name !== "BaseReleaseId") scheduleCheck($form);
        });

    function isAuto($form) {
        return $form.find("[name='VersionAuto']").val() === "true";
    }

    function scopeData($form) {
        return {
            allProjects: $form.find("input[name='AllProjects']:checked").val() !== "false",
            projectIds: $form.find("select[name='SelectedProjectIds']").val() || []
        };
    }

    function setVersionAuto($form, auto) {
        var $group = $form.find(".release-version");
        if (!$group.length) return;

        $form.find("[name='VersionAuto']").val(auto ? "true" : "false");
        $group.toggleClass("is-auto", auto);
        $form.find(".release-version-mode").text($group.data(auto ? "auto-text" : "manual-text"));
        if (auto) showWarning($form, null, null);
    }

    function refreshVersion($form, keepBases) {
        var $group = $form.find(".release-version");
        if (!$group.length) return;

        var token = ($group.data("request") || 0) + 1;
        $group.data("request", token);

        var data = scopeData($form);
        data.releaseType = $form.find("[name='ReleaseType']").val();
        data.baseReleaseId = $form.find("select[name='BaseReleaseId']").val() || 0;

        $.ajax({
            type: "GET",
            url: $group.data("next-url"),
            traditional: true,
            data: data,
            success: function (r) {
                if ($group.data("request") !== token || !r) return;
                if (!keepBases) updateBases($form, r.bases || []);
                $form.find(".release-version-format").text(r.format);
                $form.find(".release-version-scope").text($group.data(r.projectFormat ? "project-text" : "default-text"));
                if (isAuto($form)) $form.find(".release-version-input").val(r.version);
            }
        });
    }

    function updateBases($form, bases) {
        var $select = $form.find("select[name='BaseReleaseId']");
        if (!$select.length) return;

        var current = String($select.val() || "");
        $select.find("option").filter(function () { return this.value !== ""; }).remove();
        bases.forEach(function (b) { $select.append(new Option(b.text, b.value)); });

        var keep = bases.some(function (b) { return String(b.value) === current; });
        $select.val(keep ? current : "").trigger("change.select2");
    }

    function scheduleCheck($form) {
        clearTimeout($form.data("check-timer"));
        $form.data("check-timer", setTimeout(function () { checkVersion($form); }, 400));
    }

    function checkVersion($form) {
        var $field = $form.find(".release-version-field");
        if (!$field.length) return;
        clearTimeout($form.data("check-timer"));

        if (isAuto($form)) {
            showWarning($form, null, null);
            return;
        }

        var name = $.trim($form.find(".release-version-input").val());
        if (!name) {
            showWarning($form, null, null);
            return;
        }

        var token = ($field.data("request") || 0) + 1;
        $field.data("request", token);

        var data = scopeData($form);
        data.name = name;
        data.id = $form.find("input[name='Id']").val() || 0;

        $.ajax({
            type: "GET",
            url: $field.data("check-url"),
            traditional: true,
            data: data,
            success: function (r) {
                if ($field.data("request") !== token || !r || isAuto($form)) return;
                if (r.exists) showWarning($form, $field.data("exists-text"), "text-danger");
                else if (!r.matchesFormat) showWarning($form, String($field.data("mismatch-text")).replace("{0}", r.format), "text-warning-emphasis");
                else showWarning($form, null, null);
            }
        });
    }

    function showWarning($form, text, css) {
        var $warning = $form.find(".release-version-warning");
        $warning.removeClass("text-danger text-warning-emphasis").prop("hidden", !text);
        if (text) $warning.addClass(css).html('<i class="fas fa-triangle-exclamation me-1"></i>' + escapeHtml(text));
    }

    return { renderProjects: renderProjects, renderProgress: renderProgress };
})();
