/* Shared collapsible filter panel helper.
   Counts active filters inside a panel, updates the badge + button active state.
   Usage:
     WpFilter.init({ panelId: "sdFilter", buttonId: "btnFilters", badgeId: "sdFilterCount" });
     // then call WpFilter.refresh("sdFilter") after Apply/Reset, or rely on input events.
   Auto-counts any filled control (select/text/date/multiselect) inside #panelId,
   ignoring buttons and controls marked data-wp-filter-ignore. */
window.WpFilter = (function () {
    "use strict";

    var registry = {};

    function countActive(panelId) {
        var $panel = $("#" + panelId);
        if (!$panel.length) return 0;
        var n = 0;
        $panel.find("input, select, textarea").each(function () {
            var $el = $(this);
            if ($el.is("[type=button], [type=submit], [type=reset], [data-wp-filter-ignore]")) return;
            if ($el.closest("[data-wp-filter-ignore]").length) return;
            var v = $el.val();
            if (Array.isArray(v)) { if (v.length) n++; }
            else if ($el.is("[type=checkbox]")) { if ($el.is(":checked")) n++; }
            else if (v != null && String(v).trim() !== "") n++;
        });
        return n;
    }

    function refresh(panelId) {
        var cfg = registry[panelId];
        if (!cfg) return;
        var n = cfg.count ? cfg.count() : countActive(panelId);
        $("#" + cfg.buttonId).toggleClass("wp-filter-btn--active", n > 0);
        if (cfg.badgeId) $("#" + cfg.badgeId).text(n > 0 ? n : "");
        return n;
    }

    function init(cfg) {
        if (!cfg || !cfg.panelId) return;
        registry[cfg.panelId] = cfg;
        var $panel = $("#" + cfg.panelId);
        // Recount on any change/typing inside the panel.
        $panel.on("change keyup", "input, select, textarea", function () { refresh(cfg.panelId); });
        refresh(cfg.panelId);
    }

    // Auto-init any panel rendered by the <wp-filter-panel> tag helper:
    // a .wp-filter-btn button whose data-bs-target points at the collapse holding #<panelId>.
    $(function () {
        $(".wp-filter-btn").each(function () {
            var buttonId = this.id;
            var target = $(this).data("bs-target") || $(this).attr("data-bs-target");
            var $panel = $(target).find(".card-body").first();
            if (!$panel.length || !$panel.attr("id")) return;
            init({ panelId: $panel.attr("id"), buttonId: buttonId, badgeId: buttonId.replace(/Btn$/, "Count") });
        });
    });

    return { init: init, refresh: refresh, count: countActive };
})();
