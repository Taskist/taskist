(function () {
    "use strict";

    const PALETTE = ['#1cc7c9', '#ffc21a', '#3399ff', '#2eb85c', '#e55353', '#9b59b6', '#20a8d8'];
    let charts = [];

    function destroyCharts() {
        charts.forEach(function (c) { try { c.destroy(); } catch (e) { } });
        charts = [];
    }

    function payloadFor(canvas) {
        const container = canvas.closest('.chart-container');
        const el = container ? container.querySelector('.chart-payload') : null;
        if (!el) return {};
        try { return JSON.parse(el.textContent || '{}'); } catch (e) { return {}; }
    }

    function buildChart(canvas) {
        const type = canvas.dataset.chart;
        const d = payloadFor(canvas);
        let config = null;

        if (type === 'pie') {
            config = {
                type: 'pie',
                data: { labels: d.labels || [], datasets: [{ data: d.values || [], backgroundColor: PALETTE, borderColor: '#fff', borderWidth: 1 }] },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }
            };
        } else if (type === 'releaseBar' || type === 'workloadBar') {
            config = {
                type: 'bar',
                data: {
                    labels: d.labels || [],
                    datasets: [
                        { label: 'Open', data: d.open || [], backgroundColor: 'rgba(229,83,83,.75)' },
                        { label: 'Closed', data: d.closed || [], backgroundColor: 'rgba(28,199,201,.75)' }
                    ]
                },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    plugins: { legend: { position: 'bottom' } },
                    scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true } }
                }
            };
        } else if (type === 'trendLine') {
            config = {
                type: 'line',
                data: {
                    labels: d.labels || [],
                    datasets: [
                        { label: 'Created', data: d.created || [], borderColor: '#ffc21a', backgroundColor: 'rgba(255,194,26,.15)', tension: .3, fill: true },
                        { label: 'Resolved', data: d.resolved || [], borderColor: '#1cc7c9', backgroundColor: 'rgba(28,199,201,.15)', tension: .3, fill: true }
                    ]
                },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    plugins: { legend: { position: 'bottom' } },
                    scales: { y: { beginAtZero: true } }
                }
            };
        }

        if (config && window.Chart) charts.push(new Chart(canvas, config));
    }

    function initCharts(root) {
        (root || document).querySelectorAll('canvas[data-chart]').forEach(buildChart);
    }

    function refresh() {
        const cfg = document.getElementById('dashConfig');
        if (!cfg) return;
        const $content = $('#dashboardContent');
        $content.addClass('wp-loading');
        $.get(cfg.dataset.contentUrl, { projectId: $('#dashProject').val() || 0, days: $('#dashPeriod').val() || 90 })
            .done(function (html) {
                destroyCharts();
                $content.html(html);
                initCharts(document.getElementById('dashboardContent'));
                if (window.DashboardMyCode) window.DashboardMyCode.load(false);
            })
            .always(function () { $content.removeClass('wp-loading'); });
    }

    function loadOrgDashboard() {
        var $org = $('#orgDashboardContent');
        if (!$org.length || $org.data('loaded')) return;
        $org.data('loaded', true);
        $.get($org.data('url'), { days: $('#dashPeriod').val() || 90 })
            .done(function (html) {
                $org.html(html);
                initCharts($org[0]);
            });
    }

    $(function () {
        initCharts(document);
        $('#dashProject, #dashPeriod').on('change', refresh);
        $('#dashboardTabs a[href="#tabOrgDash"]').on('shown.bs.tab', loadOrgDashboard);
    });
})();

(function () {
    "use strict";

    function loadMyCode(refresh) {
        var $card = $("#myCodeCard");
        if (!$card.length) return;
        var $body = $card.find(".my-code__body");
        $card.find(".my-code__refresh i").addClass("fa-spin");
        $.get($card.data("url"), { refresh: !!refresh })
            .done(function (html) { $body.html(html); })
            .fail(function () { $body.html("<div class='wp-empty wp-empty--danger wp-empty--compact' role='status'><div class='wp-empty__icon' aria-hidden='true'><i class='fas fa-triangle-exclamation'></i></div><p class='wp-empty__text'>Request to the Git server failed.</p></div>"); })
            .always(function () { $card.find(".my-code__refresh i").removeClass("fa-spin"); });
    }

    window.DashboardMyCode = { load: loadMyCode };

    $(function () { loadMyCode(false); });

    $(document)
        .on("click", "#myCodeCard .my-code__refresh", function () { loadMyCode(true); })
        .on("click", "#myCodeCard .my-code__create-mr", function () {
            if (!window.BacklogBranch) return;
            var d = $("#myCodeCard").data();
            BacklogBranch.openCreateMergeRequest({
                trigger: this,
                taskId: $(this).data("task-id"),
                branchId: $(this).data("branch-id"),
                contextUrl: d.mrContextUrl,
                createUrl: d.mrCreateUrl,
                labels: {
                    title: d.labelMrTitle, fieldTitle: d.labelMrFieldTitle, from: d.labelMrFrom, into: d.labelMrInto,
                    description: d.labelMrDescription, reviewer: d.labelMrReviewer, noReviewer: d.labelMrNoReviewer,
                    draft: d.labelMrDraft, remove: d.labelMrRemove, moves: d.labelMrMoves, existing: d.labelMrExisting,
                    create: d.labelMrCreate, titleRequired: d.labelMrTitleRequired
                },
                onDone: function () { loadMyCode(true); }
            });
        });
})();
