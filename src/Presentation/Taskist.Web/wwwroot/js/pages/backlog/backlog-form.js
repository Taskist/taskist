(function () {
    "use strict";

    var $cfg = $("#backlog-form-config");
    if (!$cfg.length) return;

    var cfg = {
        id: parseInt($cfg.data("id"), 10) || 0,
        parentId: $cfg.data("parent-id"),
        parentTitle: $cfg.data("parent-title"),
        canEdit: $cfg.data("can-edit") === true || $cfg.data("can-edit") === "true",
        projectId: $cfg.data("project-id"),
        mergeRequestId: parseInt($cfg.data("merge-request-id"), 10) || 0,
        parentTasksUrl: $cfg.data("parent-tasks-url"),
        uploadUrl: $cfg.data("upload-url"),
        uploadAccept: $cfg.data("upload-accept"),
        uploadMaxMb: parseInt($cfg.data("upload-max-mb"), 10) || 2,
        documentsUrl: $cfg.data("documents-url"),
        mergeRequestUrl: $cfg.data("merge-request-url"),
        modulesUrl: $cfg.data("modules-url"),
        projectOptionsUrl: $cfg.data("project-options-url"),
        subModulesUrl: $cfg.data("sub-modules-url"),
        availabilityUrl: $cfg.data("availability-url"),
        reloadUrl: $cfg.data("reload-url"),
        bugTaskTypeIds: String($cfg.data("bug-task-type-ids") || "").split(",").filter(Boolean),
        similarUrl: $cfg.data("similar-url")
    };

    var mz = null;

    function renderDocuments() {
        JSManager.renderPartial(cfg.documentsUrl, { id: cfg.id }, "thumbnail-zone");
    }

    function markEditorDirty(name) {
        if (cfg.id > 0 && cfg.canEdit && window.BacklogEdit)
            window.BacklogEdit.markDirty(name);
    }

    function initEditor(selector, name, height, grow) {
        var sizing = grow
            ? { min_height: grow.min, max_height: grow.max, autoresize_bottom_margin: 16 }
            : { height: height || 250 };
        tinymce.init($.extend({
            selector: selector,
            content_style: "body{font-family:system-ui,-apple-system,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;font-size:14px;}",
            browser_spellcheck: true,
            paste_data_images: false,
            plugins: grow ? "lists link image table code autoresize" : "lists link image table code",
            toolbar: "undo redo | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | table tabledelete | tableprops tablerowprops tablecellprops | tableinsertrowbefore tableinsertrowafter tabledeleterow | tableinsertcolbefore tableinsertcolafter tabledeletecol",
            menubar: false,
            setup: function (editor) {
                editor.on("init keyup change SetContent", function () {
                    $(document).trigger("backlog:editor-input", [name]);
                });

                if (cfg.id > 0 && cfg.canEdit) {
                    editor.on("keyup change", function () {
                        markEditorDirty(name);
                    });
                }

                editor.on("Paste", function (e) {
                    var clipboardItems = (e.clipboardData || window.clipboardData).items;
                    for (var i = 0; i < clipboardItems.length; i++) {
                        var item = clipboardItems[i];
                        if (item.kind === "file" && item.type.startsWith("image/")) {
                            var file = item.getAsFile();
                            if (file) {
                                e.preventDefault();
                                if (mz) mz.addFile(file);
                            }
                        }
                    }
                });
            }
        }, sizing));
    }

    $(function () {
        Dropzone.autoDiscover = false;
        var docs = [];

        var option = new Option(cfg.parentTitle, cfg.parentId, true, true);
        $("#ParentId").append(option).trigger("change");

        $("#ParentId").select2({
            placeholder: "Search tasks...",
            minimumInputLength: 3,
            ajax: {
                url: cfg.parentTasksUrl,
                dataType: "json",
                delay: 250,
                data: function (params) {
                    return { q: params.term, id: cfg.id };
                },
                processResults: function (data) {
                    return {
                        results: data.map(function (item) {
                            return { id: item.Value, text: item.Text };
                        })
                    };
                },
                cache: true
            }
        });

        $(".backlog-tags").each(function () {
            var $this = $(this);
            $this.wrap('<div class="position-relative"></div>').select2({
                tags: true,
                tokenSeparators: [","],
                placeholder: $this.data("placeholder") || "Add tags...",
                width: "100%",
                dropdownParent: $this.parent()
            });
        });

        $("#FormBacklog").on("submit", function (e) {
            if (!$(this).valid()) {
                e.preventDefault();
                return false;
            }

            var btn = $(".ladda-button")[0];
            var btn2 = $(".ladda-button")[1];
            if (btn) Ladda.create(btn).start();
            if (btn2) Ladda.create(btn2).start();
        });

        $("#ModuleId").change(function () {
            JSManager.bindSelect("SubModuleId", cfg.subModulesUrl, { module: $(this).val() });
        });

        $(document).on("keydown", function (e) {
            var key = (e.key || "").toLowerCase();
            if (!(e.ctrlKey || e.metaKey) || key !== "s") return;
            e.preventDefault();
            e.stopPropagation();
            if (window.BacklogEdit)
                $("#btnSaveBacklog").trigger("click");
            else
                $("#FormBacklog .ladda-button").first().trigger("click");
        });

        function rebuildProjectSelect(id, items, fallbackValue) {
            var $sel = $("#" + id);
            if (!$sel.length) return null;

            var current = $sel.val();
            var $placeholder = $sel.find("option").filter(function () { return this.value === "-1"; }).first().clone();

            $sel.empty();
            if ($placeholder.length) $sel.append($placeholder);
            (items || []).forEach(function (item) { $sel.append(new Option(item.text, item.value)); });

            var isValid = $sel.find("option").filter(function () { return this.value === current; }).length > 0;
            var next = isValid ? current : (fallbackValue || ($placeholder.length ? "-1" : ($sel.find("option").first().val() || "")));

            $sel.val(next);
            if (next !== current) {
                $sel.trigger("change");
                return $sel.closest(".mb-3, .form-group").find("label").first().text().trim() || id;
            }
            $sel.trigger("change.select2");
            return null;
        }

        function rescopeProjectFields(projectId) {
            if (!cfg.projectOptionsUrl) return;
            $.getJSON(cfg.projectOptionsUrl, { project: projectId, statusId: $("#StatusId").val() || 0 }, function (r) {
                if (!r || r.error) {
                    JSManager.showError(r && r.error ? r.error : "Could not load options for the selected project.");
                    return;
                }

                var changed = [
                    rebuildProjectSelect("ReleaseId", r.releases),
                    rebuildProjectSelect("AssigneeId", r.assignees),
                    rebuildProjectSelect("TesterId", r.testers),
                    rebuildProjectSelect("StatusId", r.statuses, r.suggestedStatusId)
                ].filter(Boolean);

                if (changed.length)
                    JSManager.showWarning("Not available in the new project, so reset: " + changed.join(", ") + ". Review before saving.");
            });
        }

        $(".backlog-project").on("change", function () {
            var projectId = $(this).val();
            if (window.BacklogEdit) {
                // Edit: re-scope module/sub-module to the new project and require re-selection.
                JSManager.bindSelect("ModuleId", cfg.modulesUrl, { project: projectId });
                $("#SubModuleId").empty().append('<option value="">Select</option>').trigger("change");
                window.BacklogEdit.markDirty("ProjectId");
                window.BacklogEdit.markDirty("ModuleId");
                window.BacklogEdit.markDirty("SubModuleId");
                rescopeProjectFields(projectId);
            } else {
                var form = document.getElementById("FormBacklog");
                form.action = cfg.reloadUrl;
                form.submit();
            }
        });

        function loadAvailability(fieldId) {
            var $box = $('.wp-avail[data-avail-for="' + fieldId + '"]');
            if (!$box.length || !cfg.availabilityUrl) return;
            var empId = $("#" + fieldId).val();
            if (!empId || empId === "0" || empId === "-1") { $box.empty(); return; }
            $.getJSON(cfg.availabilityUrl, { userId: empId, dueDate: $("#DueDate").val() || "" })
                .done(function (r) {
                    $box.html((r.items || []).map(function (it) {
                        return '<span class="wp-avail__chip wp-avail__' + it.level + '"><i class="' + it.icon + ' me-1"></i>' + it.text + '</span>';
                    }).join(""));
                });
        }

        $("#AssigneeId").on("change", function () { loadAvailability("AssigneeId"); });
        $("#TesterId").on("change", function () { loadAvailability("TesterId"); });
        $("#DueDate").on("change changeDate", function () { loadAvailability("AssigneeId"); loadAvailability("TesterId"); });

        loadAvailability("AssigneeId");
        loadAvailability("TesterId");

        $("div#dropzone-container").dropzone({
            url: cfg.uploadUrl,
            headers: { RequestVerificationToken: JSManager.getAntiForgeryToken() },
            autoProcessQueue: true,
            maxFilesize: cfg.uploadMaxMb,
            maxFiles: 5,
            addRemoveLinks: true,
            acceptedFiles: cfg.uploadAccept,
            init: function () {
                mz = this;
                this.on("addedfile", function (file) {
                    var progressElement = file.previewElement.querySelector(".dz-progress");
                    if (progressElement) progressElement.remove();
                });

                this.on("sending", function (file, xhr, formData) {
                    formData.append("reference", cfg.id);
                });

                this.on("success", function (file, response) {
                    file.serverFilePath = response.token;
                    if (cfg.id > 0) {
                        this.removeFile(file);
                        renderDocuments();
                    } else {
                        docs.push(response.token);
                        $("#Token").val(JSON.stringify(docs));
                    }
                });

                this.on("maxfilesexceeded", function (file) {
                    this.removeFile(file);
                    alert("You can only upload up to 5 files.");
                });

                document.addEventListener("paste", function (event) {
                    var items = (event.clipboardData || event.originalEvent.clipboardData).items;
                    for (var i = 0; i < items.length; i++) {
                        var item = items[i];
                        if (item.kind === "file" && item.type.startsWith("image/")) {
                            mz.addFile(item.getAsFile());
                        }
                    }
                });
            }
        });

        initEditor("#Description", "Description", null, { min: 320, max: 640 });
        initEditor("#RootCauseAnalysis", "RootCauseAnalysis", 220);
        initEditor("#ResolutionDetails", "ResolutionDetails", 220);

        if (cfg.id === 0 && cfg.similarUrl) {
            var $sim = $("#similar-tasks");
            var simTimer = null;
            var simAll = false;

            function simEsc(s) {
                return $("<div>").text(s == null ? "" : s).html();
            }

            function renderSimilar(items) {
                if (!items || !items.length) {
                    $sim.addClass("d-none").empty();
                    return;
                }

                var hasExact = items.some(function (m) { return m.exact; });
                var header = (hasExact
                    ? '<i class="fas fa-triangle-exclamation me-1"></i>A task with this exact title already exists:'
                    : '<i class="fas fa-circle-info me-1"></i>Similar tasks already exist:');

                var scopeLink = ' <a href="javascript:void(0)" class="similar-scope-toggle">'
                    + (simAll ? "current project only" : "search all my projects") + "</a>";

                var list = items.map(function (m) {
                    var meta = [m.project, m.taskType, m.status].filter(Boolean).map(simEsc).join(" · ");
                    return '<li class="similar-item' + (m.exact ? " similar-item--exact" : "") + '">'
                        + '<a href="' + m.url + '" target="_blank">#' + m.id + " " + simEsc(m.title) + "</a>"
                        + (meta ? ' <small class="text-muted">' + meta + "</small>" : "")
                        + "</li>";
                }).join("");

                $sim.removeClass("d-none")
                    .toggleClass("similar-tasks--exact", hasExact)
                    .html('<div class="similar-head">' + header + scopeLink + "</div><ul class=\"similar-list\">" + list + "</ul>");
            }

            function checkSimilar() {
                var title = ($("#Title").val() || "").trim();
                if (title.length < 3) {
                    $sim.addClass("d-none").empty();
                    return;
                }

                $.getJSON(cfg.similarUrl, { project: $("#ProjectId").val() || 0, title: title, all: simAll })
                    .done(renderSimilar)
                    .fail(function () { $sim.addClass("d-none").empty(); });
            }

            $("#Title").on("input", function () {
                if (simTimer) clearTimeout(simTimer);
                simTimer = setTimeout(checkSimilar, 400);
            });

            $(document).on("click", ".similar-scope-toggle", function () {
                simAll = !simAll;
                checkSimilar();
            });

            $(".backlog-project").on("change", function () { checkSimilar(); });
        }

        renderDocuments();
    });
})();
