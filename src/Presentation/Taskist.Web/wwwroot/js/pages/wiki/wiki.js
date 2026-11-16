(function () {
    "use strict";

    var $cfg = $("#wiki-config");
    if (!$cfg.length) return;

    var cfg = {
        projectId: parseInt($cfg.data("project-id"), 10) || 0,
        canAdd: $cfg.data("can-add") === true || $cfg.data("can-add") === "true",
        hasProject: $cfg.data("has-project") === true || $cfg.data("has-project") === "true",
        getParent: $cfg.data("get-parent"),
        getChildren: $cfg.data("get-children"),
        addCategory: $cfg.data("add-category"),
        renameCategory: $cfg.data("rename-category"),
        moveCategory: $cfg.data("move-category"),
        deleteCategory: $cfg.data("delete-category"),
        addWiki: $cfg.data("add-wiki"),
        renameWiki: $cfg.data("rename-wiki"),
        moveWiki: $cfg.data("move-wiki"),
        deleteWiki: $cfg.data("delete-wiki"),
        getContent: $cfg.data("get-content"),
        saveWiki: $cfg.data("save-wiki"),
        getTags: $cfg.data("get-tags"),
        saveTags: $cfg.data("save-tags"),
        getModules: $cfg.data("get-modules"),
        getSubModules: $cfg.data("get-submodules"),
        uploadImage: $cfg.data("upload-image"),
        getVersions: $cfg.data("get-versions"),
        restoreVersion: $cfg.data("restore-version"),
        getLinkedTasks: $cfg.data("get-linked-tasks"),
        createFromWiki: $cfg.data("create-from-wiki"),
        projectContext: $cfg.data("project-context"),
        exportWord: $cfg.data("export-word")
    };

    var EDITOR_ID = "wikiEditor";
    var currentPageId = null;
    var currentTags = { moduleId: null, subModuleId: null };
    var expanded = false;

    function token() {
        return $('input[name="__RequestVerificationToken"]').val();
    }

    function projectName() {
        return $("#wikiProject option:selected").text() || "Wiki";
    }

    function branchProjectId(node) {
        if (cfg.projectId > 0) return cfg.projectId;
        var parents = node.parents || [];
        for (var i = 0; i < parents.length; i++) {
            if (parents[i].indexOf("proj_") === 0) return parents[i].replace("proj_", "");
        }
        return node.id && node.id.indexOf("proj_") === 0 ? node.id.replace("proj_", "") : 0;
    }

    // ---- TinyMCE ----
    function removeEditor() {
        var ed = tinymce.get(EDITOR_ID);
        if (ed) ed.remove();
        $("#wikiEditorContainer").empty();
    }

    function loadEditor(initial, onReady) {
        $("#wikiEditorContainer").html('<textarea id="' + EDITOR_ID + '">' + (initial || "") + "</textarea>");
        tinymce.init({
            selector: "#" + EDITOR_ID,
            content_style: "body{font-family:system-ui,-apple-system,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;font-size:14px;}",
            height: "68vh",
            menubar: false,
            plugins: "preview searchreplace autolink directionality code visualblocks visualchars fullscreen image link media codesample table charmap pagebreak nonbreaking anchor insertdatetime advlist lists wordcount help quickbars emoticons",
            toolbar: "fullscreen | undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright | bullist numlist outdent indent | link image media table codesample blockquote | charmap emoticons | removeformat code preview",
            quickbars_selection_toolbar: "bold italic | quicklink h2 h3 blockquote",
            toolbar_mode: "sliding",
            contextmenu: "link image table",
            image_caption: true,
            automatic_uploads: true,
            images_upload_handler: function (blobInfo) {
                return new Promise(function (resolve, reject) {
                    var fd = new FormData();
                    fd.append("file", blobInfo.blob(), blobInfo.filename());
                    fd.append("pageId", currentPageId || "");
                    fd.append("__RequestVerificationToken", token());
                    $.ajax({
                        url: cfg.uploadImage, type: "POST", data: fd,
                        processData: false, contentType: false,
                        success: function (r) { r && r.location ? resolve(r.location) : reject({ message: (r && r.error) || "Upload failed", remove: true }); },
                        error: function () { reject("Upload failed"); }
                    });
                });
            },
            setup: function (editor) {
                editor.on("init", function () { if (typeof onReady === "function") onReady(editor); });
                editor.on("Dirty keyup", function () {
                    if (!cfg.canAdd) return;
                    $("#btnSavePage").prop("disabled", false);
                    JSManager.NavGuard.markDirty();
                });
            }
        });
    }

    // ---- Tags / linked tasks panels ----
    function renderTagBar() {
        var $bar = $("#wikiTagBar");
        var parts = [];
        if (currentTags.moduleName) parts.push('<span class="wiki-tag-badge"><i class="fas fa-cube"></i>' + currentTags.moduleName + "</span>");
        if (currentTags.subModuleName) parts.push('<span class="wiki-tag-badge"><i class="fas fa-cubes"></i>' + currentTags.subModuleName + "</span>");
        if (parts.length) { $bar.html(parts.join("")).removeClass("d-none"); } else { $bar.addClass("d-none").empty(); }
    }

    function loadTags(pageId) {
        currentTags = { moduleId: null, subModuleId: null };
        $.getJSON(cfg.getTags + "?id=" + pageId, function (r) {
            currentTags = { moduleId: r.moduleId, subModuleId: r.subModuleId, moduleName: r.moduleName, subModuleName: r.subModuleName };
            renderTagBar();
        });
    }

    function loadLinkedTasks(pageId) {
        $.getJSON(cfg.getLinkedTasks + "?id=" + pageId, function (tasks) {
            var $box = $("#wikiLinkedTasks");
            if (!tasks || !tasks.length) { $box.addClass("d-none").empty(); return; }
            var html = '<span class="wiki-linked-title"><i class="fas fa-link me-1"></i>Linked tasks:</span>';
            tasks.forEach(function (t) {
                html += '<span class="wiki-linked-task">#' + t.id + " " + t.title + (t.status ? ' <span class="badge bg-light text-dark ms-1">' + t.status + "</span>" : "") + "</span>";
            });
            $box.html(html).removeClass("d-none");
        });
    }

    // ---- Page open ----
    function openPage(node) {
        currentPageId = node.id;
        $("#wikiPlaceholder").hide();
        $("#wikiPageActions").removeClass("d-none");
        $("#btnSavePage, #btnTagPage").toggleClass("d-none", !cfg.canAdd);
        $("#btnCreateTask").toggleClass("d-none", cfg.projectId <= 0);
        $("#btnSavePage").prop("disabled", true);
        $("#wikiEditorContainer").show();
        loadTags(node.id);
        loadLinkedTasks(node.id);

        removeEditor();
        loadEditor('<p class="text-muted">Loading…</p>', function (editor) {
            $.getJSON(cfg.getContent + "?id=" + node.id, function (res) {
                editor.setContent((res && res.content) || "");
                editor.mode.set(cfg.canAdd ? "design" : "readonly");
                editor.undoManager.clear();
                editor.setDirty(false);
                $("#btnSavePage").prop("disabled", true);
                JSManager.NavGuard.clear();

                if (res && res.modifiedBy)
                    $("#wikiMeta").text("Updated by " + res.modifiedBy + " · " + res.modifiedOn).removeClass("d-none");
                else
                    $("#wikiMeta").addClass("d-none").empty();
            });
        });
    }

    function closePage() {
        currentPageId = null;
        removeEditor();
        $("#wikiEditorContainer").hide();
        $("#wikiPageActions").addClass("d-none");
        $("#wikiTagBar").addClass("d-none").empty();
        $("#wikiLinkedTasks").addClass("d-none").empty();
        $("#wikiMeta").addClass("d-none").empty();
        JSManager.NavGuard.clear();
        $("#wikiPlaceholder").show();
    }

    function savePage() {
        if (!currentPageId) return;
        var ed = tinymce.get(EDITOR_ID);
        if (!ed) return;
        JSManager.ajaxPost(cfg.saveWiki, JSManager.addAntiForgeryToken({ id: currentPageId, content: ed.getContent() }), undefined, undefined, function (r) {
            if (r.success) {
                toastr.success("Saved");
                ed.setDirty(false);
                $("#btnSavePage").prop("disabled", true);
                JSManager.NavGuard.clear();
            } else {
                toastr.info(r.message || "No changes.");
            }
        });
    }

    // ---- jsTree ----
    function buildTree() {
        $("#wikiTree").jstree("destroy");
        $("#wikiTree").jstree({
            core: {
                check_callback: true,
                themes: { responsive: true, dots: false },
                data: function (node, cb) {
                    if (node.id === "#") {
                        if (cfg.projectId > 0) {
                            cb([{ id: "root_home", text: projectName(), icon: "fa fa-home text-danger", type: "default", state: { opened: true }, children: cfg.hasProject }]);
                        } else {
                            var projects = [];
                            $("#wikiProject option").each(function () {
                                var v = parseInt(this.value, 10);
                                if (v > 0) projects.push({ id: "proj_" + v, text: this.text, type: "project", children: true });
                            });
                            cb(projects);
                        }
                    } else if (node.id.indexOf("proj_") === 0) {
                        $.getJSON(cfg.getParent + "?projectId=" + node.id.replace("proj_", ""), cb);
                    } else if (node.id === "root_home") {
                        $.getJSON(cfg.getParent + "?projectId=" + cfg.projectId, cb);
                    } else if (node.type !== "file") {
                        $.getJSON(cfg.getChildren + "?id=" + node.id + "&projectId=" + branchProjectId(node), cb);
                    } else {
                        cb([]);
                    }
                }
            },
            types: {
                default: { icon: "fa fa-folder text-warning" },
                project: { icon: "fa fa-folder-tree text-info" },
                file: { icon: "far fa-file-alt text-secondary" }
            },
            plugins: cfg.canAdd
                ? ["wholerow", "types", "contextmenu", "dnd", "search"]
                : ["wholerow", "types", "search"],
            search: { show_only_matches: true, show_only_matches_children: true },
            contextmenu: { items: contextMenu }
        })
            .on("select_node.jstree", function (e, data) {
                setBreadcrumb(data);
                if (data.node.type === "file") openPage(data.node); else closePage();
            })
            .on("move_node.jstree", onMove)
            .on("open_node.jstree", function (e, data) {
                if (data.node.type !== "file" && data.node.id !== "root_home")
                    data.instance.set_icon(data.node, "fa fa-folder-open text-primary");
            })
            .on("close_node.jstree", function (e, data) {
                if (data.node.type !== "file" && data.node.id !== "root_home")
                    data.instance.set_icon(data.node, "fa fa-folder text-warning");
            });
    }

    function setBreadcrumb(data) {
        var names = data.node.parents.slice().reverse()
            .filter(function (p) { return p !== "#"; })
            .map(function (p) { return data.instance.get_node(p).text; });
        names.push(data.node.text);
        $("#wikiBreadcrumb").html('<i class="fas fa-pen-nib"></i> ' + names.join(" / "));
    }

    function onMove(e, data) {
        var tree = data.instance;
        var node = data.node;
        var newParentId = data.parent;

        if (node.type === "file") {
            if (newParentId === "root_home" || newParentId === "#") {
                toastr.error("A page must live inside a folder.");
                tree.refresh();
                return;
            }
            JSManager.ajaxPost(cfg.moveWiki, JSManager.addAntiForgeryToken({ id: node.id, categoryId: newParentId }), undefined, undefined, function (r) {
                if (!r.success) { toastr.error(r.message); tree.refresh(); }
            });
        } else {
            var parent = (newParentId === "root_home" || newParentId === "#") ? null : newParentId.replace("cat_", "");
            JSManager.ajaxPost(cfg.moveCategory, JSManager.addAntiForgeryToken({ id: node.id.replace("cat_", ""), parentId: parent }), undefined, undefined, function (r) {
                if (!r.success) { toastr.error(r.message); tree.refresh(); }
            });
        }
    }

    // ---- Context menu ----
    function contextMenu(node) {
        if (!cfg.canAdd || !cfg.hasProject) return {};
        var items = {};
        var isRoot = node.id === "root_home";
        var isFolder = node.type !== "file";

        if (isFolder) {
            items.addFolder = {
                label: isRoot ? "Add Folder" : "Add Subfolder",
                icon: "fas fa-folder-plus text-primary",
                action: function () { showPrompt(isRoot ? "newFolder" : "newSubfolder", isRoot ? "" : node.id, "Folder name", ""); }
            };
            items.addPage = {
                label: "Add Page",
                icon: "fas fa-file-circle-plus text-primary",
                action: function () {
                    if (isRoot) { toastr.info("Create a folder first, then add pages inside it."); return; }
                    showPrompt("newPage", node.id, "Page title", "");
                }
            };
            if (!isRoot) {
                items.rename = { label: "Rename", icon: "fas fa-edit", action: function () { showPrompt("renameFolder", node.id, "Folder name", node.text); } };
                items.del = { label: "Delete", icon: "fas fa-trash-alt text-danger", action: function () { deleteFolder(node); } };
            }
        } else {
            items.rename = { label: "Rename", icon: "fas fa-edit", action: function () { showPrompt("renamePage", node.id, "Page title", node.text); } };
            items.del = { label: "Delete", icon: "fas fa-trash-alt text-danger", action: function () { deletePage(node); } };
        }
        return items;
    }

    function deleteFolder(node) {
        JSManager.confirm("Delete this folder and its contents?", function () {
            JSManager.ajaxPost(cfg.deleteCategory, JSManager.addAntiForgeryToken({ id: node.id.replace("cat_", "") }), undefined, undefined, function (r) {
                r.success ? $("#wikiTree").jstree(true).delete_node(node) : toastr.error(r.message);
            });
        });
    }

    function deletePage(node) {
        JSManager.confirm("Delete this page?", function () {
            JSManager.ajaxPost(cfg.deleteWiki, JSManager.addAntiForgeryToken({ id: node.id }), undefined, undefined, function (r) {
                if (r.success) { $("#wikiTree").jstree(true).delete_node(node); if (currentPageId === node.id) closePage(); }
                else toastr.error(r.message);
            });
        });
    }

    // ---- Prompt dialog (create/rename) — jQuery-confirm ----
    function showPrompt(action, target, label, value) {
        $.confirm({
            title: label,
            columnClass: "medium",
            content: '<label class="form-label">' + label + '</label>' +
                     '<input type="text" class="form-control wiki-prompt-input" autocomplete="off" />',
            buttons: {
                save: {
                    text: "Save", btnClass: "btn-blue", keys: ["enter"],
                    action: function () {
                        var dlg = this;
                        var name = (dlg.$content.find(".wiki-prompt-input").val() || "").trim();
                        if (!name) return false;
                        submitPrompt(action, target, name, dlg);
                        return false;
                    }
                },
                cancel: { text: "Cancel" }
            },
            onContentReady: function () {
                this.$content.find(".wiki-prompt-input").val(value || "").trigger("focus").select();
            }
        });
    }

    function submitPrompt(action, target, name, dlg) {
        var tree = $("#wikiTree").jstree(true);
        if (action === "newFolder" || action === "newSubfolder") {
            var parentId = action === "newSubfolder" ? target.replace("cat_", "") : "";
            JSManager.ajaxPost(cfg.addCategory, JSManager.addAntiForgeryToken({ parentId: parentId || null, name: name, projectId: cfg.projectId }), undefined, undefined, function (r) {
                if (!r.success) { toastr.error(r.message); return; }
                var parentNode = action === "newSubfolder" ? tree.get_node(target) : tree.get_node("root_home");
                tree.create_node(parentNode, r, "last"); tree.open_node(parentNode); dlg.close();
            });
        } else if (action === "newPage") {
            JSManager.ajaxPost(cfg.addWiki, JSManager.addAntiForgeryToken({ categoryId: target.replace("cat_", ""), title: name }), undefined, undefined, function (r) {
                if (!r.success) { toastr.error(r.message); return; }
                var parentNode = tree.get_node(target);
                tree.create_node(parentNode, r, "last"); tree.open_node(parentNode); dlg.close();
            });
        } else if (action === "renameFolder") {
            JSManager.ajaxPost(cfg.renameCategory, JSManager.addAntiForgeryToken({ id: target.replace("cat_", ""), newName: name }), undefined, undefined, function (r) {
                if (!r.success) { toastr.error(r.message); return; }
                tree.rename_node(tree.get_node(target), name); dlg.close();
            });
        } else if (action === "renamePage") {
            JSManager.ajaxPost(cfg.renameWiki, JSManager.addAntiForgeryToken({ id: target, title: name }), undefined, undefined, function (r) {
                if (!r.success) { toastr.error(r.message); return; }
                tree.rename_node(tree.get_node(target), name);
                if (currentPageId === target) setBreadcrumb({ node: tree.get_node(target), instance: tree });
                dlg.close();
            });
        }
    }

    // ---- Tag dialog — jQuery-confirm ----
    function openTagModal() {
        if (!currentPageId) return;
        $.confirm({
            title: "Tag page",
            columnClass: "medium",
            content: '<div class="mb-3"><label class="form-label">Module</label><select class="form-control wiki-tag-module"></select></div>' +
                     '<div class="mb-3"><label class="form-label">Sub-Module</label><select class="form-control wiki-tag-submodule"></select></div>' +
                     '<div class="form-text">Optional — associate this page with a module / sub-module.</div>',
            buttons: {
                save: {
                    text: "Save tags", btnClass: "btn-blue",
                    action: function () {
                        var dlg = this;
                        var moduleId = dlg.$content.find(".wiki-tag-module").val() || null;
                        var subModuleId = dlg.$content.find(".wiki-tag-submodule").val() || null;
                        JSManager.ajaxPost(cfg.saveTags, JSManager.addAntiForgeryToken({ id: currentPageId, moduleId: moduleId, subModuleId: subModuleId }), undefined, undefined, function (r) {
                            if (!r.success) { toastr.error(r.message); return; }
                            toastr.success("Tags saved"); loadTags(currentPageId); dlg.close();
                        });
                        return false;
                    }
                },
                cancel: { text: "Cancel" }
            },
            onContentReady: function () {
                var $c = this.$content;
                var $mod = $c.find(".wiki-tag-module"), $sub = $c.find(".wiki-tag-submodule");
                $sub.html('<option value="">Select</option>');
                $.getJSON(cfg.getModules + "?projectId=" + cfg.projectId, function (mods) {
                    var opts = '<option value="">Select</option>';
                    mods.forEach(function (m) { opts += '<option value="' + m.id + '">' + m.text + "</option>"; });
                    $mod.html(opts).val(currentTags.moduleId || "");
                    if (currentTags.moduleId) loadSubModulesInto($sub, currentTags.moduleId, currentTags.subModuleId);
                });
                $mod.on("change", function () { loadSubModulesInto($sub, $(this).val(), null); });
            }
        });
    }

    function loadSubModulesInto($sub, moduleId, selected) {
        if (!moduleId) { $sub.html('<option value="">Select</option>'); return; }
        $.getJSON(cfg.getSubModules + "?module=" + moduleId, function (subs) {
            var opts = '<option value="">Select</option>';
            subs.forEach(function (s) { opts += '<option value="' + s.Value + '">' + s.Text + "</option>"; });
            $sub.html(opts).val(selected || "");
        });
    }

    // ---- History dialog — jQuery-confirm ----
    var historyDlg = null;
    function openHistory() {
        if (!currentPageId) return;
        historyDlg = $.confirm({
            title: "Version history",
            columnClass: "large",
            content: '<div class="wiki-history-body text-muted">Loading…</div>',
            buttons: { close: { text: "Close" } },
            onContentReady: function () {
                var $body = this.$content.find(".wiki-history-body");
                $.getJSON(cfg.getVersions + "?id=" + currentPageId, function (versions) {
                    if (!versions || !versions.length) { $body.html('<div class="text-muted">No previous versions.</div>'); return; }
                    var html = "";
                    versions.forEach(function (v) {
                        var restore = cfg.canAdd ? '<button type="button" class="btn btn-sm btn-outline-primary wiki-restore" data-version="' + v.id + '">Restore</button>' : "";
                        html += '<div class="wiki-version-row"><div><div><strong>' + (v.modifiedBy || "") + "</strong></div><div class=\"text-muted small\">" + v.modifiedOn + (v.note ? " · " + v.note : "") + "</div></div>" + restore + "</div>";
                    });
                    $body.removeClass("text-muted").html(html);
                });
            }
        });
    }

    function restoreVersion(versionId) {
        JSManager.confirm("Restore this version? Current content is saved to history first.", function () {
            JSManager.ajaxPost(cfg.restoreVersion, JSManager.addAntiForgeryToken({ id: currentPageId, versionId: versionId }), undefined, undefined, function (r) {
                if (!r.success) { toastr.error("Restore failed"); return; }
                if (historyDlg) historyDlg.close();
                var node = $("#wikiTree").jstree(true).get_node(currentPageId);
                if (node) openPage(node);
                toastr.success("Version restored");
            });
        });
    }

    // ---- Create task from wiki ----
    function createTask() {
        if (!currentPageId) return;
        var ed = tinymce.get(EDITOR_ID);
        var selected = ed ? ed.selection.getContent() : "";
        var description = selected || (ed ? ed.getContent() : "");
        var title = $("#wikiBreadcrumb").text().trim().split("/").pop().trim();

        var $f = $('<form method="post" target="_blank"></form>').attr("action", cfg.createFromWiki);
        function add(n, v) { $f.append($('<input type="hidden">').attr("name", n).val(v == null ? "" : v)); }
        add("__RequestVerificationToken", token());
        add("wikiId", currentPageId.replace("page_", ""));
        add("projectId", cfg.projectId);
        add("title", title);
        add("description", description);
        add("moduleId", currentTags.moduleId || "");
        add("subModuleId", currentTags.subModuleId || "");
        $f.appendTo("body").submit().remove();
    }

    // ---- Export ----
    function pageNumericId() {
        return currentPageId ? currentPageId.replace("page_", "") : null;
    }

    function warnIfUnsaved() {
        var ed = tinymce.get(EDITOR_ID);
        return ed && ed.isDirty();
    }

    function exportWord() {
        var id = pageNumericId();
        if (!id) return;
        if (warnIfUnsaved())
            toastr.info("Exporting the last saved version. Save your changes first to include them.");
        window.location.href = cfg.exportWord + "?id=" + id;
    }

    // ---- Project switch ----
    function applyPermissionUi() {
        $("#btnNewFolder").toggleClass("d-none", !cfg.canAdd);
    }

    function switchProject(projectId) {
        cfg.projectId = parseInt(projectId, 10) || 0;
        if (cfg.projectId === 0) {
            cfg.canAdd = false;
            cfg.hasProject = true;
            applyPermissionUi();
            closePage();
            buildTree();
            return;
        }
        $.getJSON(cfg.projectContext + "?projectId=" + cfg.projectId, function (ctx) {
            cfg.canAdd = !!ctx.canAddWiki;
            cfg.hasProject = !!ctx.hasProject;
            applyPermissionUi();
            closePage();
            buildTree();
        });
    }

    $(function () {
        applyPermissionUi();
        buildTree();

        $("#wikiProject").on("change", function () { switchProject($(this).val()); });

        $("#wikiSearch").on("keyup", function () {
            $("#wikiTree").jstree(true).search($(this).val());
        });

        $("#btnNewFolder").on("click", function () {
            if (!cfg.canAdd || !cfg.hasProject) { toastr.info("You don't have permission to add wiki content here."); return; }
            showPrompt("newFolder", "", "Folder name", "");
        });

        $("#btnExpandToggle").on("click", function () {
            var tree = $("#wikiTree").jstree(true);
            if (!tree) return;
            if (!expanded) { tree.open_all(); $(this).html('<i class="fas fa-angles-up"></i>').attr("title", "Collapse all"); }
            else { tree.close_all(); var r = tree.get_node("root_home"); if (r) tree.open_node(r); $(this).html('<i class="fas fa-angles-down"></i>').attr("title", "Expand all"); }
            expanded = !expanded;
        });

        $("#btnSavePage").on("click", savePage);
        $("#btnTagPage").on("click", openTagModal);
        $("#btnHistory").on("click", openHistory);
        $("#btnCreateTask").on("click", createTask);
        $("#btnExportWord").on("click", exportWord);

        $(document).on("click", ".wiki-restore", function () { restoreVersion($(this).data("version")); });
    });
})();
