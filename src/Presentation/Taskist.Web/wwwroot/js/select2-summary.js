/* App-wide multi-select pattern: a select2 multi-select shows ONE summary chip instead of a tag per item —
   the item's name when one is picked, "N selected" when more (full list in the tooltip). Wraps $.fn.select2 so
   every page gets it no matter where select2 is initialised. Opt out per element with data-summary="false". */
var SelectSummary = (function ($) {
    "use strict";

    if (!$ || !$.fn || !$.fn.select2) return { render: function () { } };

    var NS = "change.select2.select2summary";

    function countText(count) {
        var template = document.documentElement.getAttribute("data-select-count-text") || "{0} selected";
        return template.replace("{0}", count);
    }

    function render(target) {
        var $select = $(target);
        var instance = $select.data("select2");
        if (!instance || !$select.prop("multiple") || $select.data("summary") === false) return;

        var $rendered = instance.$container.find(".select2-selection__rendered");
        $rendered.find(".select2-summary__chip").remove();

        var $selected = $select.find("option:selected");
        if (!$selected.length) return;

        var names = $selected.map(function () { return $.trim($(this).text()); }).get();
        var $chip = $('<li class="select2-selection__choice select2-summary__chip"></li>')
            .text(names.length === 1 ? names[0] : countText(names.length))
            .attr("title", names.join(", "));

        var $search = $rendered.children(".select2-search--inline");
        if ($search.length) $chip.insertBefore($search); else $rendered.append($chip);
    }

    function setup($select) {
        var instance = $select.data("select2");
        if (!instance || $select.data("summary") === false) return;

        instance.$container.addClass("select2-summary");
        $select.off(".select2summary")
            .on(NS, function () { render(this); })
            .on("select2:close.select2summary", function () { render(this); });
        render($select);
    }

    var original = $.fn.select2;
    $.fn.select2 = function (options) {
        var result = original.apply(this, arguments);
        if (typeof options !== "string") {
            this.each(function () {
                if (this.multiple) setup($(this));
            });
        }
        return result;
    };
    $.extend($.fn.select2, original);

    return { render: render };
})(window.jQuery);
