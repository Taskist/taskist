/* Backlog filter panel: multi-selects, saved presets, reset/search and the active-filter counter.
   The panel is loaded as a partial; Index calls BacklogFilter.init() once it is in the DOM. */
var backlogPresets = {};

var BacklogFilter = (function () {
    "use strict";

    function root() {
        return $(".backlog-filter-root");
    }

    function init() {
        $("#DueDate").datepicker({
            format: "dd-M-yyyy",
            viewMode: "date",
            autoclose: true,
            todayHighlight: true
        });

        $(".age-filter").on("input change", countSelectedMultiSelects);

        $("#btnStale").on("click", function () {
            $("#StatusDaysMin").val(7);
            $("#btnSearch").trigger("click");
        });

        $("#DueDate").on("change", function () {
            countSelectedMultiSelects();
        });

        BacklogFilterSelect.init(root(), countSelectedMultiSelects);

        $("#btnSearch").on("click", function () {
            saveFilters();
            renderFilterChips();
            JSManager.reloadGrid("BacklogGrid");
            var oc = bootstrap.Offcanvas.getInstance(document.getElementById("backlogFilterOffcanvas"));
            if (oc) oc.hide();
        });

        $("#btnReset").on("click", function () {
            deleteFilter();
            if (typeof clearPreset === "function") clearPreset();

            $(BacklogFilterSelect.selector).each(function () {
                BacklogFilterSelect.clear(this);
            });

            $("#DueDate").val("");
            $(".age-filter").val("");
            $("#DueDateType").val("1");
            $("#filterPreset").val("");

            var $search = $("div.dataTables_filter input");
            if ($search.length) $search.val("");
            if (table) table.search("");

            saveFilters();
            countSelectedMultiSelects();
            renderFilterChips();
            JSManager.reloadGrid("BacklogGrid");
        });

        $("#btnSavePreset").on("click", function () {
            var name = ($("#presetName").val() || "").trim();
            if (!name) { JSManager.showError("Enter a preset name."); return; }
            backlogPresets[name] = captureFilters();
            persistPresets();
            $("#presetName").val("");
            refreshPresetList();
            $("#filterPreset").val(name);
            JSManager.showSuccess("Filter saved.");
            $(".filter-presets-save").addClass("d-none");
        });

        $("#filterPreset").on("change", function () {
            var name = $(this).val();
            if (!name) return;
            if (!backlogPresets[name]) return;
            applyPresetFilters(backlogPresets[name]);
            saveFilters();
            countSelectedMultiSelects();
            renderFilterChips();
            JSManager.reloadGrid("BacklogGrid");
        });

        $("#btnDeletePreset").on("click", function () {
            var name = $("#filterPreset").val();
            if (!name) return;
            delete backlogPresets[name];
            persistPresets();
            refreshPresetList();
        });

        loadPresetsFromServer();
        if (filterMode <= 0) {
            loadFilters();
        }

        setDefaultFilters();
        JSManager.reloadGrid("BacklogGrid");
        countSelectedMultiSelects();
        renderFilterChips();
        if (typeof BacklogFilterSections !== "undefined") BacklogFilterSections.init();
    }

    return { init: init, root: root };
})();

function loadPresetsFromServer() {
    var resp = JSManager.ajaxGetWithResponse(BacklogFilter.root().data("presets-url"), {});
    backlogPresets = resp || {};
    refreshPresetList();
}

function persistPresets() {
    $.ajax({
        type: "POST",
        url: BacklogFilter.root().data("save-presets-url"),
        data: JSManager.addAntiForgeryToken({ presets: JSON.stringify(backlogPresets) }),
        dataType: "json"
    });
}

function refreshPresetList() {
    var $sel = $("#filterPreset");
    $sel.find("option:not(:first)").remove();
    Object.keys(backlogPresets).forEach(function (name) {
        $sel.append($("<option></option>").val(name).text(name));
    });
}

function captureFilters() {
    return {
        project: $("#ProjectId").val(),
        createdby: $("#CreatedById").val(),
        module: $("#ModuleId").val(),
        subModule: $("#SubModuleId").val(),
        taskType: $("#TaskTypeId").val(),
        severity: $("#SeverityId").val(),
        reporter: $("#ReporterId").val(),
        assignee: $("#AssigneeId").val(),
        tester: $("#TesterId").val(),
        status: $("#StatusId").val(),
        statusGroup: $("#StatusGroupId").val(),
        release: $("#ReleaseId").val(),
        dueDateType: $("#DueDateType").val(),
        dueDate: $("#DueDate").val(),
        statusDays: $("#StatusDaysMin").val(),
        openDays: $("#OpenDaysMin").val()
    };
}

function applyPresetFilters(f) {
    $(BacklogFilterSelect.selector).each(function () {
        BacklogFilterSelect.set(this, []);
    });

    BacklogInactiveUsers.ensureFor({ AssigneeId: f.assignee, TesterId: f.tester, CreatedById: f.createdby });

    var map = {
        ProjectId: f.project, CreatedById: f.createdby, ModuleId: f.module, SubModuleId: f.subModule,
        TaskTypeId: f.taskType, SeverityId: f.severity, ReporterId: f.reporter, AssigneeId: f.assignee,
        TesterId: f.tester, StatusId: f.status, StatusGroupId: f.statusGroup, ReleaseId: f.release
    };
    Object.keys(map).forEach(function (id) {
        if (map[id] && map[id].length) BacklogFilterSelect.set(id, map[id]);
    });

    $("#DueDateType").val(f.dueDateType || "1");
    $("#DueDate").val(f.dueDate || "");
    $("#StatusDaysMin").val(f.statusDays || "");
    $("#OpenDaysMin").val(f.openDays || "");
}

function countSelectedMultiSelects() {
    var count = 0;
    $(BacklogFilterSelect.selector).each(function () {
        if (BacklogFilterSelect.get(this).length > 0) count++;
    });

    $(".age-filter").each(function () {
        if (parseInt($(this).val(), 10) > 0) count++;
    });

    if (JSManager.hasValue($("#DueDate").val())) {
        count++;
    }

    if (typeof filterMode !== "undefined" && parseInt(filterMode) > 0 && parseInt(filterMode) !== 4) {
        count++;
    }

    $("#filter-counter").text(count > 0 ? count : "");

    if (typeof BacklogFilterSections !== "undefined") BacklogFilterSections.refresh(false);
}
