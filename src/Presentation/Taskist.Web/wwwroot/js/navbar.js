/* Top-bar global search autocomplete.
   Reads the search endpoint from #navSearchInput's data-search-url, groups the
   JSON results by their "group" field, and supports keyboard navigation.
   Ctrl/Cmd+K (or "/") focuses the search box. */

(function () {
    "use strict";

    var $input = $("#navSearchInput");
    if (!$input.length) return;

    var $results = $("#navSearchResults");
    var $wrap = $("#navSearch");
    var url = $input.data("search-url");
    var noResults = $input.data("noresults") || "No results";
    var timer = null;
    var activeIndex = -1;

    function close() {
        $results.removeClass("show").empty();
        activeIndex = -1;
    }

    function open() {
        $results.addClass("show");
    }

    function items() {
        return $results.find(".header-search__item");
    }

    function highlight(i) {
        var $all = items();
        if (!$all.length) return;
        activeIndex = (i + $all.length) % $all.length;
        $all.removeClass("active").eq(activeIndex).addClass("active");
        // keep the active item in view
        var el = $all.get(activeIndex);
        if (el && el.scrollIntoView) el.scrollIntoView({ block: "nearest" });
    }

    function render(data) {
        $results.empty();

        if (!data || !data.length) {
            $results.append($('<div class="header-search__empty">').text(noResults));
            open();
            return;
        }

        var groups = {};
        var order = [];
        data.forEach(function (r) {
            var g = r.group || "";
            if (!groups[g]) { groups[g] = []; order.push(g); }
            groups[g].push(r);
        });

        order.forEach(function (g) {
            if (g) $results.append($('<div class="header-search__group">').text(g));
            groups[g].forEach(function (r) {
                var $item = $('<a class="header-search__item">').attr("href", r.url);
                $item.append($('<i>').addClass(r.icon || "fas fa-circle"));
                $item.append($('<span class="hs-label">').text(r.label || ""));
                if (r.sublabel) $item.append($('<span class="hs-sub">').text(r.sublabel));
                $results.append($item);
            });
        });

        open();
    }

    function search(q) {
        $.getJSON(url, { q: q })
            .done(render)
            .fail(close);
    }

    $input.on("input", function () {
        var q = $.trim($(this).val());
        clearTimeout(timer);
        if (q.length < 2) { close(); return; }
        timer = setTimeout(function () { search(q); }, 250);
    });

    $input.on("keydown", function (e) {
        var open = $results.hasClass("show");
        if (e.key === "ArrowDown") { e.preventDefault(); if (open) highlight(activeIndex + 1); }
        else if (e.key === "ArrowUp") { e.preventDefault(); if (open) highlight(activeIndex - 1); }
        else if (e.key === "Enter") {
            var $active = items().eq(activeIndex);
            if (activeIndex >= 0 && $active.length) { e.preventDefault(); window.location.href = $active.attr("href"); }
        }
        else if (e.key === "Escape") { close(); $(this).blur(); }
    });

    // close on outside click
    $(document).on("click", function (e) {
        if (!$wrap.length || !$wrap[0].contains(e.target)) close();
    });

    // global shortcut: Ctrl/Cmd+K or "/" focuses search (unless already typing)
    $(document).on("keydown", function (e) {
        var typing = /^(input|textarea|select)$/i.test((e.target.tagName || "")) || e.target.isContentEditable;
        if ((e.key === "k" || e.key === "K") && (e.ctrlKey || e.metaKey)) {
            e.preventDefault();
            $input.focus().select();
        } else if (e.key === "/" && !typing) {
            e.preventDefault();
            $input.focus();
        }
    });
})();
