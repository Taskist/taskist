(function () {
    "use strict";

    // UserStatus.Active = 1 — the default selection on load and after reset.
    var DEFAULT_STATUS = ["1"];

    // Visible select2 -> hidden CSV input (so the grid's filter sends a string that binds to int[]).
    var MAP = [
        { sel: "#empStatusSel", hidden: "#empStatus" },
        { sel: "#empRoleSel", hidden: "#empRole" }
    ];

    function sync() {
        MAP.forEach(function (m) {
            var v = $(m.sel).val() || [];
            $(m.hidden).val(v.join(","));
        });
    }

    function grid() {
        return $("#UserGrid").DataTable();
    }

    function reload() {
        sync();
        grid().ajax.reload(null, true);
    }

    function applyDefault() {
        $("#empStatusSel").val(DEFAULT_STATUS).trigger("change.select2");
        $("#empRoleSel").val(null).trigger("change.select2");
        $("#empLockedSel").val("");
    }

    $(function () {
        $(".emp-multi").each(function () {
            $(this).select2({ width: "100%", placeholder: $(this).data("placeholder") || "All", allowClear: true, closeOnSelect: false, dropdownParent: $(this).parent() });
        });

        // Status is pre-selected to Active server-side (hidden #empStatus seeded with "1"),
        // so the grid's first load already shows active users. Keep hidden inputs in sync.
        sync();

        $("#empApply").on("click", reload);
        $("#empReset").on("click", function () {
            applyDefault();
            reload();
        });
    });
})();
