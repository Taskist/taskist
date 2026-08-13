/* Dashboard widgets - status/severity/type charts + activity relative times,
   plus the project/period filter bar that AJAX-reloads #dashboardContent.
   Chart datasets come from the #dashboard-config data attributes (re-rendered on reload). */

(function () {
    "use strict";

    if (typeof Chart === "undefined") return;

    var charts = [];

    function destroyCharts() {
        charts.forEach(function (c) { try { c.destroy(); } catch (e) { } });
        charts = [];
    }

    function donut(id, d) {
        var el = document.getElementById(id);
        if (!el || !d.labels.length) return;
        charts.push(new Chart(el.getContext('2d'), {
            type: 'doughnut',
            data: { labels: d.labels, datasets: [{ data: d.data, backgroundColor: d.colors, borderWidth: 1 }] },
            options: {
                responsive: true, maintainAspectRatio: false, cutout: '62%',
                plugins: { legend: { position: 'bottom', labels: { boxWidth: 12, font: { size: 11 } } } }
            }
        }));
    }

    function bar(id, d) {
        var el = document.getElementById(id);
        if (!el || !d.labels.length) return;
        charts.push(new Chart(el.getContext('2d'), {
            type: 'bar',
            data: { labels: d.labels, datasets: [{ data: d.data, backgroundColor: d.colors, borderRadius: 6 }] },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        }));
    }

    // (re)build charts + relative times from whatever is currently in the DOM
    function render() {
        destroyCharts();

        var $cfg = $("#dashboard-config");
        if ($cfg.length) {
            donut('dashStatusChart', JSON.parse($cfg.attr("data-status")));
            bar('dashSeverityChart', JSON.parse($cfg.attr("data-severity")));
            bar('dashTypeChart', JSON.parse($cfg.attr("data-type")));
        }

        $('.dash-activity-time').each(function () {
            var t = $(this).data('time');
            if (t && window.moment) $(this).text(moment(t).fromNow());
        });
    }

    // reload the widgets for the current filter selection
    function reload() {
        var $container = $("#dashboardContent");
        var url = $container.data("content-url");
        if (!url) return;

        var projectId = $("#dashProject").val() || 0;
        var days = $("#dashPeriod").val() || 0;

        $container.addClass("dash-loading");
        $.get(url, { projectId: projectId, days: days })
            .done(function (html) {
                $container.html(html);
                render();
            })
            .always(function () {
                $container.removeClass("dash-loading");
            });
    }

    // delegated so the handlers survive the container being replaced
    $(document).on("change", "#dashProject, #dashPeriod", reload);

    render();
})();
