var BacklogFilterSections = (function () {
    "use strict";

    var STORAGE_KEY = "backlog-filter-sections";

    function readState() {
        try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || "{}") || {}; } catch (e) { return {}; }
    }

    function writeState() {
        var state = {};
        $(".filter-section[data-section]").not(".filter-section--main").each(function () {
            state[$(this).data("section")] = $(this).hasClass("is-open");
        });
        try { localStorage.setItem(STORAGE_KEY, JSON.stringify(state)); } catch (e) { }
    }

    function activeCount($section) {
        var count = 0;
        $section.find(BacklogFilterSelect.selector).each(function () {
            if (BacklogFilterSelect.get(this).length) count++;
        });
        $section.find("#DueDate, .age-filter").each(function () {
            if ($.trim($(this).val() || "") !== "") count++;
        });
        return count;
    }

    function setOpen($section, open) {
        $section.toggleClass("is-open", open);
        $section.find(".filter-section-head").attr("aria-expanded", open ? "true" : "false");
    }

    function updateToggleAll() {
        var $link = $(".filter-sections-toggle-all");
        var $sections = $(".filter-section").not(".filter-section--main");
        var allOpen = $sections.length > 0 && $sections.filter(".is-open").length === $sections.length;
        $link.text(allOpen ? $link.data("collapse-text") : $link.data("expand-text"));
    }

    function refresh(autoOpen) {
        $(".filter-section").not(".filter-section--main").each(function () {
            var $section = $(this);
            var count = activeCount($section);
            $section.find(".filter-section-count").text(count > 0 ? count : "").toggleClass("has-count", count > 0);
            if (autoOpen && count > 0) setOpen($section, true);
        });
        updateToggleAll();
    }

    function init() {
        var state = readState();
        $(".filter-section").not(".filter-section--main").each(function () {
            setOpen($(this), state[$(this).data("section")] === true);
        });
        refresh(true);
    }

    $(document)
        .on("click", ".filter-section-head", function () {
            var $section = $(this).closest(".filter-section");
            setOpen($section, !$section.hasClass("is-open"));
            writeState();
            updateToggleAll();
        })
        .on("click", ".filter-sections-toggle-all", function (e) {
            e.preventDefault();
            var $sections = $(".filter-section").not(".filter-section--main");
            var open = $sections.filter(".is-open").length !== $sections.length;
            $sections.each(function () { setOpen($(this), open); });
            writeState();
            updateToggleAll();
        })
        .on("click", "#btnTogglePresetSave", function () {
            var $save = $(".filter-presets-save");
            $save.toggleClass("d-none");
            if (!$save.hasClass("d-none")) $("#presetName").trigger("focus");
        })
        .on("change input", "#filter-container select, #filter-container input", function () {
            refresh(false);
        })
        .on("show.bs.offcanvas", "#backlogFilterOffcanvas", function () {
            refresh(true);
        });

    return { init: init, refresh: refresh };
})();
