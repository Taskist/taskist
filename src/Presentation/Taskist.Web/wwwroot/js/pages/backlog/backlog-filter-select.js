/* Backlog filter multi-selects (select2; the one-chip summary comes from the app-wide select2-summary.js).
   set() mirrors a programmatic value change without firing the page's "change" handlers; callers trigger
   "change" themselves when they want the counters to react. */
var BacklogFilterSelect = (function () {
    "use strict";

    var SELECTOR = "select.backlog-filter-select";

    function $el(target) {
        return typeof target === "string" ? $("#" + target) : $(target);
    }

    function texts() {
        var $root = $(".backlog-filter-root");
        return {
            any: $root.data("any-text") || "Any"
        };
    }

    function init(context, onChange) {
        if (!$.fn.select2) return;
        var t = texts();

        $(SELECTOR, context).each(function () {
            var $select = $(this);
            if ($select.data("select2")) return;

            $select.wrap('<div class="position-relative backlog-filter-select-wrap"></div>').select2({
                width: "100%",
                closeOnSelect: false,
                allowClear: true,
                placeholder: t.any,
                dropdownParent: $select.parent()
            });

            if (typeof onChange === "function") $select.on("change", onChange);
        });
    }

    function get(target) {
        var value = $el(target).val();
        return value ? [].concat(value) : [];
    }

    function set(target, values) {
        var $select = $el(target);
        if (!$select.length) return;
        var list = [].concat(values || []).filter(function (v) { return v !== null && v !== undefined && v !== ""; }).map(String);
        $select.val(list).trigger("change.select2");
    }

    function clear(target) {
        var $select = $el(target);
        $select.find("option").prop("selected", false);
        set($select, []);
    }

    function selectedTexts(target) {
        return $el(target).find("option:selected").map(function () { return $.trim($(this).text()); }).get();
    }

    function hasOption(target, value) {
        return $el(target).find("option").filter(function () { return String(this.value) === String(value); }).length > 0;
    }

    function addOption(target, value, text) {
        var $select = $el(target);
        if (hasOption($select, value)) return;
        $select.append(new Option(text, value, false, false));
    }

    function removeOptions(target, values) {
        var ids = [].concat(values || []).map(String);
        var $select = $el(target);
        var kept = get($select).filter(function (v) { return ids.indexOf(String(v)) < 0; });
        $select.find("option").filter(function () { return ids.indexOf(String(this.value)) >= 0; }).remove();
        set($select, kept);
    }

    function isReady(target) {
        return !!$el(target).data("select2");
    }

    return {
        init: init,
        get: get,
        set: set,
        clear: clear,
        selectedTexts: selectedTexts,
        hasOption: hasOption,
        addOption: addOption,
        removeOptions: removeOptions,
        isReady: isReady,
        selector: SELECTOR
    };
})();
