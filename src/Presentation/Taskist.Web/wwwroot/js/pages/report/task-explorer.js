(function () {
    "use strict";

    var cfg = document.getElementById("tx-config");
    if (!cfg) return;
    var dataUrl = cfg.getAttribute("data-data-url");
    var detailsUrl = cfg.getAttribute("data-details-url");

    var allData = [];
    var currentPreset = "workload";

    // On-theme colours for every renderer (heatmaps + plotly charts).
    var THEME_COLORWAY = ["#1cc7c9", "#ffc21a", "#3b82f6", "#22c55e", "#8b5cf6", "#ef4444", "#14b8a6", "#f59e0b"];

    // Heatmap ramp: white (low) -> brand teal (high). Returns a value->rgb() function.
    function tealScale(values) {
        var min = Math.min.apply(null, values);
        var max = Math.max.apply(null, values);
        return function (x) {
            var t = max > min ? (x - min) / (max - min) : 0;
            var r = Math.round(255 + t * (28 - 255));
            var g = Math.round(255 + t * (199 - 255));
            var b = Math.round(255 + t * (201 - 255));
            return "rgb(" + r + "," + g + "," + b + ")";
        };
    }

    var PRESETS = {
        workload:  { rows: ["Assignee"], cols: ["Status"], aggregatorName: "Count", rendererName: "Heatmap" },
        hotspots:  { rows: ["Module"], cols: ["Severity"], aggregatorName: "Count", rendererName: "Heatmap" },
        reopens:   { rows: ["Assignee"], cols: [], aggregatorName: "Integer Sum", vals: ["ReOpenCount"], rendererName: "Bar Chart" },
        portfolio: { rows: ["Project"], cols: ["StatusGroup"], aggregatorName: "Count", rendererName: "Heatmap" },
        type:      { rows: ["TaskType"], cols: ["Status"], aggregatorName: "Count", rendererName: "Heatmap" }
    };

    var renderers = $.extend({}, $.pivotUtilities.renderers, $.pivotUtilities.plotly_renderers);

    function distinct(field) {
        var set = {};
        allData.forEach(function (r) { if (r[field]) set[r[field]] = true; });
        return Object.keys(set).sort();
    }

    function fillFilter(id, field) {
        var $s = $("#" + id).empty();
        distinct(field).forEach(function (v) { $s.append(new Option(v, v)); });
    }

    function buildFilterDropdowns() {
        fillFilter("fltStatusGroups", "StatusGroup");
        fillFilter("fltTaskTypes", "TaskType");
        fillFilter("fltSeverities", "Severity");
        fillFilter("fltAssignees", "Assignee");
        $(".tx-multi").trigger("change.select2");
    }

    function multi(id) { return ($("#" + id).val() || []); }

    function activeCount() {
        return multi("fltStatusGroups").length + multi("fltTaskTypes").length
            + multi("fltSeverities").length + multi("fltAssignees").length;
    }

    function updateFilterState() {
        var n = activeCount();
        $("#btnFilters").toggleClass("tlr-filter-btn--active", n > 0);
        $("#txFilterCount").text(n > 0 ? n : "");
    }

    function filteredData() {
        var sg = multi("fltStatusGroups"), tt = multi("fltTaskTypes"),
            sv = multi("fltSeverities"), as = multi("fltAssignees");
        return allData.filter(function (r) {
            if (sg.length && sg.indexOf(r.StatusGroup) < 0) return false;
            if (tt.length && tt.indexOf(r.TaskType) < 0) return false;
            if (sv.length && sv.indexOf(r.Severity) < 0) return false;
            if (as.length && as.indexOf(r.Assignee) < 0) return false;
            return true;
        });
    }

    function renderPivot() {
        updateFilterState();
        var preset = PRESETS[currentPreset] || PRESETS.workload;
        $("#pivot").pivotUI(filteredData(), {
            rows: preset.rows.slice(),
            cols: preset.cols.slice(),
            vals: (preset.vals || []).slice(),
            aggregatorName: preset.aggregatorName,
            rendererName: preset.rendererName,
            renderers: renderers,
            rendererOptions: {
                heatmap: { colorScaleGenerator: tealScale },
                plotly: { colorway: THEME_COLORWAY },
                table: {
                    clickCallback: function (e, value, filters) {
                        var params = new URLSearchParams(filters).toString();
                        if (!value || parseInt(value, 10) === 0 || !JSManager.hasValue(params)) return;
                        JSManager.openForm(detailsUrl + "?" + params, "Tasks", "col-md-10", undefined, false);
                    }
                }
            }
        }, true);
    }

    function loadData() {
        var project = $("#txProject").val() || "";
        $("#pivot").html('<div class="text-muted text-center py-4">Loading…</div>');
        $.getJSON(dataUrl, { project: project }, function (data) {
            allData = data || [];
            buildFilterDropdowns();
            renderPivot();
        }).fail(function () {
            $("#pivot").html('<div class="text-danger text-center py-4">Failed to load.</div>');
        });
    }

    // ---- exports ----
    function exportExcel() {
        var table = document.querySelector("#pivot table.pvtTable");
        if (!table) return;
        var wb = XLSX.utils.table_to_book(table, { sheet: "Tasks" });
        XLSX.writeFile(wb, "Task-Explorer.xlsx");
    }
    function exportPdf() {
        var table = document.querySelector("#pivot table.pvtTable");
        if (!table) return;
        var doc = new window.jspdf.jsPDF();
        var rows = [];
        table.querySelectorAll("tr").forEach(function (tr) {
            var row = [];
            tr.querySelectorAll("th,td").forEach(function (td) { row.push(td.innerText.trim()); });
            rows.push(row);
        });
        doc.autoTable({ head: [rows[0]], body: rows.slice(1), theme: "grid" });
        doc.save("Task-Explorer.pdf");
    }

    // ---- wire ----
    $(".tx-preset").on("click", function (e) {
        e.preventDefault();
        $(".tx-preset").removeClass("active");
        $(this).addClass("active");
        currentPreset = $(this).attr("data-preset");
        $("#txPresetLabel").text($(this).text());
        renderPivot();
    });
    $("#txProject").on("change", loadData);
    $(".tx-multi").on("change", function () { /* apply on Reset/explicit only? apply live */ renderPivot(); });
    $("#btnReset").on("click", function () { $(".tx-multi").val(null).trigger("change.select2"); renderPivot(); });
    $("#exportExcel").on("click", exportExcel);
    $("#exportPdf").on("click", exportPdf);

    $(function () {
        $("#txProject").select2({ width: "100%" });
        $(".tx-multi").select2({ width: "100%", placeholder: "All", allowClear: true });
        var $def = $('.tx-preset[data-preset="workload"]').addClass("active");
        $("#txPresetLabel").text($def.text());
        loadData();
    });
})();
