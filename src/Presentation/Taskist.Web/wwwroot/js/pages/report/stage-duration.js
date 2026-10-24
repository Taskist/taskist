(function () {
    "use strict";

    var dataUrl = "/Report/StageDurationData";
    var tasksUrl = "/Report/StageDurationTasks";
    var exportUrl = "/Report/ExportStageDuration";
    var tasksExport = "/Report/ExportStageDurationTasks";

    function isoDate(id) {
        var dp = $("#" + id).data("datepicker");
        var d = dp && dp.getDate ? dp.getDate() : null;
        if (!d || isNaN(d.getTime())) return "";
        var mm = ("0" + (d.getMonth() + 1)).slice(-2);
        var dd = ("0" + d.getDate()).slice(-2);
        return d.getFullYear() + "-" + mm + "-" + dd;
    }

    function filterParams() {
        return {
            projectId: $("#fltProject").val() || "",
            from: isoDate("fltFrom"),
            to: isoDate("fltTo"),
            taskTypeId: $("#fltTaskType").val() || ""
        };
    }

    function queryString(extra) {
        var p = filterParams();
        if (extra) $.extend(p, extra);
        return $.param(p);
    }

    function tasksExportUrl(metricId) {
        return tasksExport + "?" + queryString({ metricId: metricId });
    }

    function esc(v) {
        return (v == null ? "" : String(v)).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    function load() {
        var $body = $("#stageTableBody");
        $body.html('<tr><td colspan="8" class="text-center text-muted py-4">Loading…</td></tr>');

        $.getJSON(dataUrl, filterParams(), function (rows) {
            if (!rows || !rows.length) {
                $body.html('<tr><td colspan="8" class="text-center text-muted py-4">No metrics configured, or no tasks matched. Configure metrics under Settings → Stage Durations.</td></tr>');
                return;
            }

            var html = "";
            rows.forEach(function (r) {
                var hasData = r.Count > 0;
                html += '<tr>'
                    + '<td class="fw-semibold">' + esc(r.Metric) + '</td>'
                    + '<td class="text-muted small">' + esc(r.FromStatus) + ' &rarr; ' + esc(r.ToStatus) + '</td>'
                    + '<td class="text-end">' + r.Count + '</td>'
                    + '<td class="text-end">' + (hasData ? esc(r.AvgText) : "—") + '</td>'
                    + '<td class="text-end">' + (hasData ? esc(r.MedianText) : "—") + '</td>'
                    + '<td class="text-end">' + (hasData ? esc(r.MinText) : "—") + '</td>'
                    + '<td class="text-end">' + (hasData ? esc(r.MaxText) : "—") + '</td>'
                    + '<td class="text-end text-nowrap">' + (hasData
                        ? '<button type="button" class="btn btn-sm btn-outline-secondary stage-drill me-1" data-id="' + r.MetricId + '" data-name="' + esc(r.Metric) + '" title="View tasks"><i class="fas fa-list"></i></button>'
                          + '<a class="btn btn-sm btn-outline-success stage-row-export" href="' + tasksExportUrl(r.MetricId) + '" title="Export tasks to Excel"><i class="fa-solid fa-file-excel"></i></a>'
                        : '') + '</td>'
                    + '</tr>';
            });
            $body.html(html);
        }).fail(function () {
            $body.html('<tr><td colspan="8" class="text-center text-danger py-4">Failed to load.</td></tr>');
        });
    }

    function barClass(days, max) {
        var r = max > 0 ? days / max : 0;
        if (r >= 0.75) return "is-critical";
        if (r >= 0.5) return "is-slow";
        if (r >= 0.25) return "is-normal";
        return "is-fast";
    }

    function drill(metricId, name) {
        var $body = $("#stageTasksBody");
        $("#stageTasksTitle").text(name);
        $body.html('<div class="text-center text-muted py-3">Loading…</div>');

        var modal = new bootstrap.Modal(document.getElementById("stageTasksModal"));
        modal.show();

        var params = filterParams();
        params.metricId = metricId;

        $.getJSON(tasksUrl, params, function (rows) {
            if (!rows || !rows.length) {
                $body.html('<div class="text-center text-muted py-3">No tasks.</div>');
                return;
            }

            var max = rows.reduce(function (m, t) { return t.Days > m ? t.Days : m; }, 0);

            var html = "";
            rows.forEach(function (t) {
                var pct = max > 0 ? Math.max(2, Math.round((t.Days / max) * 100)) : 2;
                var meta = esc(t.Project) + (t.TaskType ? " &middot; " + esc(t.TaskType) : "");
                html += '<div class="stage-rank-row">'
                    + '<div class="stage-rank-head">'
                    + '<a class="stage-rank-id" href="/Backlog/Edit/' + t.TaskId + '" target="_blank">#' + t.TaskId + '</a>'
                    + '<span class="stage-rank-title" title="' + esc(t.Title) + '">' + esc(t.Title) + '</span>'
                    + '<span class="stage-rank-val">' + esc(t.DurationText) + '</span>'
                    + '</div>'
                    + '<div class="stage-rank-bar-track">'
                    + '<div class="stage-rank-bar ' + barClass(t.Days, max) + '" style="width:' + pct + '%;"></div>'
                    + '</div>'
                    + '<div class="stage-rank-meta">' + meta + '</div>'
                    + '</div>';
            });
            $body.html(html);
        }).fail(function () {
            $body.html('<div class="text-center text-danger py-3">Failed to load.</div>');
        });
    }

    function updateExport() {
        $("#btnExport").attr("href", exportUrl + "?" + queryString());
    }

    $("#btnApply").on("click", function () { load(); updateExport(); });

    $("#btnReset").on("click", function () {
        $("#fltFrom, #fltTo").datepicker("clearDates");
        $("#fltProject").val("");
        $("#fltTaskType").val("");
        load();
        updateExport();
    });

    $("#stageTableBody").on("click", ".stage-drill", function () {
        drill($(this).data("id"), $(this).data("name"));
    });

    $(function () {
        if (window.JSManager && JSManager.setPageTitle) JSManager.setPageTitle("Stage Duration");

        $("#fltFrom, #fltTo").datepicker({
            format: "dd-M-yyyy",
            viewMode: "date",
            autoclose: true,
            todayHighlight: true,
            clearBtn: true
        });

        updateExport();
        load();
    });
})();
