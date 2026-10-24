(function () {
    "use strict";

    var $body = $("#stageMetricsBody");
    if (!$body.length) return;

    var statusOptions = $("#stageStatusOptions").html();
    var projectOptions = $("#stageProjectOptions").html();

    function rowHtml(m) {
        m = m || {};
        return '<tr class="stage-metric-row">'
            + '<td><input type="text" class="form-control form-control-sm stg-name" value="' + esc(m.Name) + '" placeholder="Metric name" /></td>'
            + '<td><select class="form-control form-control-sm stg-from">' + statusOptions + '</select></td>'
            + '<td><select class="form-control form-control-sm stg-to">' + statusOptions + '</select></td>'
            + '<td><select class="form-control form-control-sm stg-project">' + projectOptions + '</select></td>'
            + '<td class="text-center"><div class="form-check form-switch d-inline-block"><input class="form-check-input stg-show" type="checkbox" ' + (m.ShowOnTask ? "checked" : "") + '></div></td>'
            + '<td class="text-center"><div class="form-check form-switch d-inline-block"><input class="form-check-input stg-active" type="checkbox" ' + (m.Active === false ? "" : "checked") + '></div></td>'
            + '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger stg-del" title="Remove"><i class="fas fa-trash"></i></button></td>'
            + '</tr>';
    }

    function esc(v) {
        return (v == null ? "" : String(v)).replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    function addRow(m) {
        var $row = $(rowHtml(m)).appendTo($body);
        $row.data("id", m && m.Id ? m.Id : 0);
        $row.find(".stg-from").val(m && m.FromStatusId ? m.FromStatusId : 0);
        $row.find(".stg-to").val(m && m.ToStatusId ? m.ToStatusId : 0);
        $row.find(".stg-project").val(m && m.ProjectId ? m.ProjectId : "");
    }

    function load() {
        var raw = $("#stageMetricsJson").val();
        var list = [];
        try { list = JSON.parse(raw || "[]") || []; } catch (e) { list = []; }
        $body.empty();
        list.forEach(addRow);
        if (!list.length) addRow();
    }

    function serialize() {
        var rows = [];
        $body.find(".stage-metric-row").each(function () {
            var $r = $(this);
            var name = $.trim($r.find(".stg-name").val());
            var from = parseInt($r.find(".stg-from").val(), 10) || 0;
            var to = parseInt($r.find(".stg-to").val(), 10) || 0;
            if (!name || !from || !to) return;
            var proj = $r.find(".stg-project").val();
            rows.push({
                Id: $r.data("id") || 0,
                Name: name,
                FromStatusId: from,
                ToStatusId: to,
                ProjectId: proj ? parseInt(proj, 10) : null,
                ShowOnTask: $r.find(".stg-show").is(":checked"),
                Active: $r.find(".stg-active").is(":checked")
            });
        });
        $("#stageMetricsJson").val(JSON.stringify(rows));
    }

    $("#btnAddStageMetric").on("click", function () { addRow(); });

    $body.on("click", ".stg-del", function () {
        $(this).closest("tr").remove();
    });

    $("#stageDurationForm").on("submit", function (e) {
        serialize();
        var invalid = false;
        $body.find(".stage-metric-row").each(function () {
            var from = $(this).find(".stg-from").val();
            var to = $(this).find(".stg-to").val();
            if (from !== "0" && from === to) invalid = true;
        });
        if (invalid) {
            e.preventDefault();
            if (window.JSManager && JSManager.showError) {
                JSManager.showError("From and To status must be different.");
            } else {
                alert("From and To status must be different.");
            }
        }
    });

    load();
})();
