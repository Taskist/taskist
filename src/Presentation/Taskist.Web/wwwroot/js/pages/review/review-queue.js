/* Reviews page: loads the open-MR queue and handles everything client-side:
   view (mine / lead / all), filters, "no reviewer" chip, search, tiles, sort, group by project,
   clear filters, and opening the review drawer. Preferences are remembered per browser. */
(function () {
    "use strict";

    var $page = $("#reviewPage");
    if (!$page.length) return;

    var STORE = "review-queue-prefs";
    var state = { view: "mine", tile: null, unassigned: false, sort: "updated", group: false, collapsed: {} };

    try { $.extend(state, JSON.parse(localStorage.getItem(STORE) || "{}")); state.tile = null; } catch (e) { }

    function save() {
        try { localStorage.setItem(STORE, JSON.stringify({ view: state.view, sort: state.sort, group: state.group, collapsed: state.collapsed })); } catch (e) { }
    }

    function escapeHtml(value) {
        return $("<div>").text(String(value == null ? "" : value)).html();
    }

    function formatTimes($root) {
        $root.find(".review-time[data-time]").each(function () {
            var m = moment($(this).data("time"));
            $(this).text(m.fromNow()).attr("title", m.format("DD-MMM-YYYY HH:mm"));
        });
        $root.find(".review-time-tip[data-time]").each(function () {
            var m = moment($(this).data("time"));
            $(this).attr("title", m.format("DD-MMM-YYYY HH:mm") + " (" + m.fromNow() + ")");
        });
    }

    function fillSelect(filter, values, allLabel) {
        var $sel = $page.find('.review-filter[data-filter="' + filter + '"]');
        var current = $sel.val();
        $sel.empty().append($("<option>").val("").text(allLabel));
        values.forEach(function (v) { $sel.append($("<option>").val(v).text(v)); });
        if (current && values.indexOf(current) >= 0) $sel.val(current);
        $sel.toggleClass("d-none", values.length < 2 && !current);
    }

    function distinct($rows, attr) {
        var seen = {};
        $rows.each(function () { var v = $(this).attr("data-" + attr); if (v) seen[v] = true; });
        return Object.keys(seen).sort();
    }

    function matchesTile($row, tile) {
        var status = $row.data("status");
        switch (tile) {
            case "waiting": return status !== "draft";
            case "mergeable": return status === "mergeable";
            case "conflict": return status === "conflict" || status === "needs-rebase";
            case "stale": return String($row.data("stale")) === "1";
            default: return true;
        }
    }

    function sortRows() {
        var $tbody = $page.find(".review-table tbody");
        var rows = $tbody.children(".review-row").get();
        rows.sort(function (a, b) {
            var $a = $(a), $b = $(b);
            if (state.sort === "project" || state.group) {
                var p = String($a.attr("data-project")).localeCompare(String($b.attr("data-project")));
                if (p !== 0) return p;
            }
            if (state.sort === "oldest") return String($a.data("created")).localeCompare(String($b.data("created")));
            return String($b.data("updated")).localeCompare(String($a.data("updated")));
        });
        $tbody.children(".review-group").remove();
        $.each(rows, function (_, r) { $tbody.append(r); });
    }

    function groupChip(kind, count) {
        if (!count && kind !== "open") return "";
        var label = String($page.data("label-group-" + kind) || "{0}").replace("{0}", count);
        return "<span class='review-group__chip review-group__chip--" + kind + "'>" + escapeHtml(label) + "</span>";
    }

    function groupNames() {
        return $page.find(".review-table tbody > .review-group").map(function () { return $(this).attr("data-group"); }).get();
    }

    function updateExpandAll() {
        var names = groupNames();
        var anyCollapsed = names.some(function (p) { return !!state.collapsed[p]; });
        $page.find(".review-group-expand")
            .toggleClass("d-none", !state.group || names.length < 2)
            .attr("title", anyCollapsed ? $page.data("label-expand-all") : $page.data("label-collapse-all"))
            .find("i").attr("class", anyCollapsed ? "fas fa-angles-down" : "fas fa-angles-up");
    }

    function renderGroups() {
        var $tbody = $page.find(".review-table tbody");
        $tbody.children(".review-group").remove();
        if (!state.group) return;

        var cols = $page.find(".review-table thead th").length;
        var groups = {};
        $tbody.children(".review-row:not(.d-none)").each(function () {
            var p = $(this).attr("data-project");
            var status = $(this).data("status");
            groups[p] = groups[p] || { first: this, total: 0, ready: 0, conflict: 0, unassigned: 0 };
            groups[p].total++;
            if (status === "mergeable") groups[p].ready++;
            if (status === "conflict" || status === "needs-rebase") groups[p].conflict++;
            if (String($(this).data("unassigned")) === "1") groups[p].unassigned++;
        });

        var names = Object.keys(groups);
        names.forEach(function (p) {
            var g = groups[p];
            var collapsed = names.length > 1 && !!state.collapsed[p];
            var $head = $("<tr class='review-group' tabindex='0'><td colspan='" + cols + "'><div class='review-group__toggle'>" +
                "<i class='fas fa-chevron-" + (collapsed ? "right" : "down") + " review-group__chevron'></i><i class='fas fa-folder review-group__folder'></i>" +
                "<strong>" + escapeHtml(p) + "</strong>" +
                groupChip("open", g.total) + groupChip("ready", g.ready) + groupChip("conflict", g.conflict) + groupChip("unassigned", g.unassigned) +
                "</div></td></tr>");
            $head.attr("data-group", p).attr("aria-expanded", String(!collapsed));
            $(g.first).before($head);
            if (collapsed) $tbody.children('.review-row[data-project="' + p.replace(/"/g, '\\"') + '"]').addClass("is-collapsed");
        });
    }

    function apply() {
        var $rows = $page.find(".review-row");
        var project = $page.find('.review-filter[data-filter="project"]').val();
        var target = $page.find('.review-filter[data-filter="target"]').val();
        var author = $page.find('.review-filter[data-filter="author"]').val();
        var term = $.trim($page.find(".review-search").val() || "").toLowerCase();

        var counts = { waiting: 0, mergeable: 0, conflict: 0, stale: 0 };
        var visible = 0;

        $rows.removeClass("is-collapsed").each(function () {
            var $r = $(this);
            var base = (state.view === "all"
                    || (state.view === "mine" && String($r.data("mine")) === "1")
                    || (state.view === "lead" && String($r.data("lead")) === "1"))
                && (!project || $r.attr("data-project") === project)
                && (!target || $r.attr("data-target") === target)
                && (!author || $r.attr("data-author") === author)
                && (!state.unassigned || String($r.data("unassigned")) === "1")
                && (!term || String($r.data("search")).indexOf(term) >= 0);

            if (base) ["waiting", "mergeable", "conflict", "stale"].forEach(function (t) { if (matchesTile($r, t)) counts[t]++; });

            var show = base && matchesTile($r, state.tile);
            $r.toggleClass("d-none", !show);
            if (show) visible++;
        });

        sortRows();
        renderGroups();
        updateExpandAll();
        $page.toggleClass("review-grouped", !!state.group);

        Object.keys(counts).forEach(function (k) { $page.find('[data-count="' + k + '"]').text(counts[k]); });
        $page.find(".review-tile").removeClass("is-active").filter('[data-tile="' + state.tile + '"]').addClass("is-active");
        $page.find(".review-views [data-view]").removeClass("active").filter('[data-view="' + state.view + '"]').addClass("active");
        $page.find('[data-toggle-filter="unassigned"]').toggleClass("is-active", state.unassigned);
        $page.find(".review-sort").val(state.sort);
        $page.find(".review-group-toggle").toggleClass("active", state.group).attr("aria-pressed", state.group);

        var $chip = $page.find(".review-tile-filter");
        $chip.toggleClass("d-none", !state.tile);
        if (state.tile) $chip.find(".review-tile-filter__text").text($page.find('.review-tile[data-tile="' + state.tile + '"] .review-tile__label').text());

        var active = [project, target, author, term].filter(Boolean).length + (state.unassigned ? 1 : 0) + (state.tile ? 1 : 0);
        $page.find(".review-clear-filters").toggleClass("d-none", active === 0).find(".review-clear-count").text(active);

        $page.find(".review-none-match").toggleClass("d-none", visible > 0 || !$rows.length);
        $page.find(".review-table-wrap").toggleClass("d-none", visible === 0 && $rows.length > 0);
    }

    function load(refresh) {
        var $q = $("#reviewQueue");
        var $btn = $page.find(".review-refresh");
        $btn.prop("disabled", true).find("i").addClass("fa-spin");

        $.get($page.data("queue-url"), { refresh: !!refresh })
            .done(function (html) {
                $q.html(html);
                formatTimes($q);
                var $rows = $q.find(".review-row");
                fillSelect("project", distinct($rows, "project"), $page.data("label-all-projects"));
                fillSelect("target", distinct($rows, "target"), $page.data("label-all-targets"));
                fillSelect("author", distinct($rows, "author"), $page.data("label-all-authors"));
                $page.toggleClass("review-single-project", distinct($rows, "project").length < 2);
                apply();
                var err = $q.find(".review-queue-error").data("error");
                if (refresh && err) JSManager.showSwal(err, "error");
            })
            .fail(function (xhr) {
                $q.html("<div class='alert alert-warning m-3'>Could not load the review queue (" + xhr.status + ").</div>");
            })
            .always(function () { $btn.prop("disabled", false).find("i").removeClass("fa-spin"); });
    }

    function openRow($row) {
        var url = $page.data("detail-url") + "?repositoryId=" + $row.data("repository-id") + "&iid=" + $row.data("iid");
        JSManager.openForm(url, escapeHtml(($page.data("label-review") || "Review") + " !" + $row.data("iid")), "col-md-11");
    }

    $page
        .on("click", ".review-views [data-view]", function () { state.view = $(this).data("view"); save(); apply(); })
        .on("change", ".review-filter", apply)
        .on("input", ".review-search", apply)
        .on("click", '[data-toggle-filter="unassigned"]', function () { state.unassigned = !state.unassigned; apply(); })
        .on("change", ".review-sort", function () { state.sort = $(this).val(); save(); apply(); })
        .on("click", ".review-group-toggle", function () { state.group = !state.group; save(); apply(); })
        .on("click", ".review-group-expand", function () {
            var names = groupNames();
            var expand = names.some(function (p) { return !!state.collapsed[p]; });
            names.forEach(function (p) { state.collapsed[p] = !expand; });
            save();
            apply();
        })
        .on("keydown", ".review-group", function (e) {
            if (e.key === "Enter" || e.key === " ") { e.preventDefault(); $(this).trigger("click"); }
        })
        .on("click", ".review-group", function () {
            var p = $(this).attr("data-group");
            state.collapsed[p] = !state.collapsed[p];
            save();
            apply();
        })
        .on("click", ".review-tile", function () {
            var tile = $(this).data("tile");
            state.tile = state.tile === tile ? null : tile;
            apply();
        })
        .on("click", ".review-tile-filter .btn-close", function () { state.tile = null; apply(); })
        .on("click", ".review-empty__clear", function () { $page.find(".review-clear-filters").trigger("click"); })
        .on("click", ".review-clear-filters", function () {
            $page.find(".review-filter").val("");
            $page.find(".review-search").val("");
            state.unassigned = false;
            state.tile = null;
            apply();
        })
        .on("click", ".review-refresh", function () { load(true); })
        .on("click", ".review-check-conflicts", function () {
            var $btn = $(this);
            var $icon = $btn.find("i");
            $btn.prop("disabled", true);
            $icon.attr("class", "fas fa-spinner fa-spin me-1");
            $.ajax({ type: "POST", url: $btn.data("url"), data: JSManager.addAntiForgeryToken({}) })
                .done(function (r) {
                    var text = String($btn.data("result")).replace("{0}", r.conflicting).replace("{1}", r.notified);
                    JSManager.showSwal(text, r.conflicting > 0 ? "warning" : "success");
                    load(false);
                })
                .fail(function (xhr) { JSManager.showError("Request failed (" + xhr.status + ")."); })
                .always(function () {
                    $btn.prop("disabled", false);
                    $icon.attr("class", "fas fa-code-compare me-1");
                });
        })
        .on("click", ".review-open", function (e) { e.stopPropagation(); openRow($(this).closest(".review-row")); })
        .on("click", ".review-row", function (e) {
            if ($(e.target).closest("a, button").length) return;
            openRow($(this));
        })
        .on("keydown", ".review-row", function (e) {
            if (e.key === "Enter") openRow($(this));
        });

    $(document).on("review:changed", function () { load(false); });

    $(function () { load(false); });
})();
