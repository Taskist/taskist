(function () {
    "use strict";

    var UPDATE_CLASS = "ajax-update-custom-field";

    var config = function () {
        var $cfg = $("#backlog-form-config");
        return {
            id: parseInt($cfg.data("id"), 10) || 0,
            url: $cfg.data("custom-field-url")
        };
    };

    var valueOf = function ($input) {
        if ($input.is(":checkbox"))
            return $input.is(":checked") ? "true" : "false";
        if ($input.is(":radio"))
            return $input.closest("form").find('input[name="' + $input.attr("name") + '"]:checked').val() || "";
        return ($input.val() || "").toString();
    };

    var isValid = function ($input) {
        if (String($input.data("val")) !== "true" || typeof $input.valid !== "function")
            return true;
        return $input.valid();
    };

    var save = function (reference, value) {
        if (window.BacklogEdit) {
            window.BacklogEdit.markCustomDirty(reference, value);
            return;
        }

        var cfg = config();
        if (cfg.id <= 0 || !cfg.url) return;

        JSManager.ajaxPatch(cfg.url, { id: cfg.id, reference: reference, value: value }, undefined, undefined, function (response) {
            JSManager.showSuccess(response.message);
        });
    };

    var initSliders = function (root) {
        if (!$.fn.ionRangeSlider) return;

        $(root).find(".js-range-slider").each(function () {
            var $input = $(this);
            if ($input.data("ionRangeSlider")) return;

            $input.ionRangeSlider({
                skin: "big",
                min: 0,
                max: 100,
                postfix: "%",
                hide_min_max: true,
                onFinish: function (data) {
                    if (!$input.hasClass(UPDATE_CLASS)) return;
                    save(parseInt($input.data("reference"), 10), data.from);
                }
            });
        });
    };

    $(document).off("change.customFields").on("change.customFields", "." + UPDATE_CLASS + ":not(.js-range-slider)", function () {
        var $input = $(this);
        if (!isValid($input)) return;

        save(parseInt($input.data("reference"), 10), valueOf($input));
    });

    $(function () { initSliders(document); });
})();
