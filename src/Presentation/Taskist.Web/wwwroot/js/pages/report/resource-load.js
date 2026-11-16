(function () {
    "use strict";

    var cfg = document.getElementById("rl-config");
    if (!cfg) return;

    var dataUrl = cfg.dataset.dataUrl;
    var exportUrl = cfg.dataset.exportUrl;
    var backlogUrl = cfg.dataset.backlogUrl;
    var bandColor = { Free: "#2eb85c", Light: "#3399ff", Balanced: "#f9b115", Overloaded: "#e55353" };
    var chart = null;

    function vals(id) { var el = document.getElementById(id); return el ? ($(el).val() || []) : []; }
    function val(id) { var el = document.getElementById(id); return el ? el.value : ""; }
    function checked(id) { var el = document.getElementById(id); return el ? el.checked : false; }

    function buildQuery() {
        var p = new URLSearchParams();
        vals("rlProjects").forEach(function (v) { p.append("ProjectIds", v); });
        vals("rlUsers").forEach(function (v) { p.append("UserIds", v); });
        vals("rlStatusGroups").forEach(function (v) { p.append("StatusGroupIds", v); });
        vals("rlSeverities").forEach(function (v) { p.append("SeverityIds", v); });
        p.set("Role", val("rlRole"));
        p.set("LoadBasis", val("rlBasis"));
        p.set("DueWindow", val("rlDue"));
        if (val("rlFrom")) p.set("From", val("rlFrom"));
        if (val("rlTo")) p.set("To", val("rlTo"));
        if (checked("rlUnassigned")) p.set("IncludeUnassigned", "true");
        if (checked("rlWeighted")) p.set("Weighted", "true");
        if (val("rlThreshold")) p.set("BusyThreshold", val("rlThreshold"));
        return p.toString();
    }

    function esc(s) { return $("<div>").text(s == null ? "" : s).html(); }

    function badge(band) { return '<span class="rl-band rl-band--' + (band || "").toLowerCase() + '">' + esc(band) + '</span>'; }

    function renderSummary(d) {
        document.getElementById("rlFree").textContent = d.Free;
        document.getElementById("rlLight").textContent = d.Light;
        document.getElementById("rlBalanced").textContent = d.Balanced;
        document.getElementById("rlOverloaded").textContent = d.Overloaded;
        document.getElementById("rlUnavailable").textContent = d.Unavailable;
    }

    function renderChart(rows) {
        var top = rows.slice(0, 20);
        var ctx = document.getElementById("rlChart").getContext("2d");
        if (chart) chart.destroy();
        chart = new Chart(ctx, {
            type: "bar",
            data: {
                labels: top.map(function (r) { return r.Name; }),
                datasets: [{ label: "Open", data: top.map(function (r) { return r.Open; }), backgroundColor: top.map(function (r) { return bandColor[r.Band] || "#888"; }) }]
            },
            options: { responsive: true, maintainAspectRatio: false, indexAxis: "y", plugins: { legend: { display: false } } }
        });
    }

    function renderTable(rows) {
        if ($.fn.dataTable.isDataTable("#rlTable")) $("#rlTable").DataTable().destroy();
        var body = rows.map(function (r) {
            var name = r.UserId > 0
                ? "<a href='" + backlogUrl + "?filter=5&assignee=" + r.UserId + "' title='View open tasks'>" + esc(r.Name) + "</a>"
                : esc(r.Name);
            return "<tr>" +
                "<td>" + name + "</td>" +
                "<td>" + esc(r.Role) + "</td>" +
                "<td>" + esc(r.Projects) + "</td>" +
                "<td class='text-end'>" + r.Open + "</td>" +
                "<td class='text-end'>" + r.Overdue + "</td>" +
                "<td class='text-end'>" + r.DueSoon + "</td>" +
                "<td class='text-end'>" + r.Reopened + "</td>" +
                "<td class='text-end'>" + r.CriticalHigh + "</td>" +
                "<td>" + badge(r.Band) + "</td>" +
                "</tr>";
        }).join("");
        $("#rlTable tbody").html(body);
        $("#rlTable").DataTable({ paging: true, pageLength: 25, order: [], searching: true, info: true, autoWidth: false });
    }

    function load() {
        var q = buildQuery();
        document.getElementById("rlExport").href = exportUrl + "?" + q;
        fetch(dataUrl + "?" + q, { headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (r) { return r.json(); })
            .then(function (d) {
                renderSummary(d);
                renderChart(d.Rows || []);
                renderTable(d.Rows || []);
            })
            .catch(function () { });
    }

    $(function () {
        $(".rl-multi").each(function () {
            $(this).select2({ width: "100%", closeOnSelect: false, dropdownParent: $(this).parent() });
        });
        $(document).on("shown.bs.collapse", "#rlChartCollapse", function () { if (chart) chart.resize(); });
        $("#rlApply").on("click", load);
        $("#rlReset").on("click", function () {
            $(".rl-multi").val(null).trigger("change");
            $("#rlRole").val("both");
            $("#rlBasis").val("assignee");
            $("#rlDue").val("all");
            $("#rlFrom").val("");
            $("#rlTo").val("");
            $("#rlThreshold").val("");
            $("#rlUnassigned,#rlWeighted").prop("checked", false);
            load();
        });
        load();
    });
})();
