/* Project custom-field modal - auto-generates the ResourceKey from the Label,
   prefixed with the (sanitized) project name. Reads the prefix from config. */

window.ProjectCustomField = (function () {
    "use strict";

    function sanitize(v) {
        return (v || "").replace(/[^a-zA-Z0-9]/g, "");
    }

    function init(cfg) {
        cfg = cfg || {};
        var prefix = sanitize(cfg.projectName) + ".";

        $("#Label").off("input.customField").on("input.customField", function () {
            $("#ResourceKey").val(prefix + sanitize($(this).val()));
        });
    }

    return { init: init };
})();
