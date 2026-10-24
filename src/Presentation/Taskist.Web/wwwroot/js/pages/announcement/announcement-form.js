(function () {
    "use strict";

    // Rich-text body
    if (typeof tinymce !== "undefined" && document.getElementById("annBody")) {
        tinymce.remove("#annBody");
        tinymce.init({
            selector: "#annBody",
            menubar: false,
            height: 260,
            plugins: "link lists autolink",
            toolbar: "undo redo | bold italic underline | bullist numlist | link | removeformat",
            setup: function (editor) {
                editor.on("change keyup", function () { editor.save(); });
            }
        });
        $(document).off("submit.rtbody").on("submit.rtbody", "form", function () {
            if (tinymce.triggerSave) tinymce.triggerSave();
        });
    }

    // Audience targeting: reveal role/department picker per selected audience.
    function applyAudience() {
        var val = String($("#annAudience").val());
        var showRoles = val === "1", showDepts = val === "2";
        $("#annRolesWrap").toggleClass("d-none", !showRoles);
        $("#annDepartmentsWrap").toggleClass("d-none", !showDepts);
        if (showRoles) $("#annRolesWrap select").trigger("change.select2");
        if (showDepts) $("#annDepartmentsWrap select").trigger("change.select2");
    }

    $(function () {
        if ($("#annAudience").length) {
            applyAudience();
            $("#annAudience").on("change", applyAudience);
        }
    });
})();
