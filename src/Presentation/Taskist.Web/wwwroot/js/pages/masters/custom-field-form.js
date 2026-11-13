(function () {
    "use strict";

    var clean = function (value) {
        return String(value || "").replace(/[^a-zA-Z0-9]/g, "");
    };

    $(document).off("input.customFieldForm").on("input.customFieldForm", ".js-custom-field-label", function () {
        var $label = $(this);
        var $key = $label.closest("form").find("#ResourceKey");
        $key.val(clean($label.data("key-prefix")) + "." + clean($label.val()));
    });
})();
