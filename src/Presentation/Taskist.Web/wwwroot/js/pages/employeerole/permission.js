(function () {
    "use strict";

    function refreshGroup($group) {
        var $items = $group.find(".perm-item-check");
        var total = $items.length;
        var checked = $items.filter(":checked").length;

        $group.find(".perm-group__badge").text(checked + "/" + total);

        var $all = $group.find(".perm-group-all").get(0);
        if ($all) {
            $all.checked = total > 0 && checked === total;
            $all.indeterminate = checked > 0 && checked < total;
        }
    }

    function initAll() {
        var $root = $("#permGroups");
        if (!$root.length) return;
        $root.find(".perm-group").each(function () { refreshGroup($(this)); });
    }

    $(document).on("show.bs.offcanvas", "#pdOffCanvas", function () {
        $(this).toggleClass("pd-offcanvas--wide", $("#permGroups").length > 0);
    });

    $(document).on("shown.bs.offcanvas", "#pdOffCanvas", initAll);

    $(document).on("change", ".perm-item-check", function () {
        refreshGroup($(this).closest(".perm-group"));
    });

    $(document).on("change", ".perm-group-all", function () {
        var $group = $(this).closest(".perm-group");
        var on = this.checked;
        $group.find(".perm-item-check:not(:disabled)").prop("checked", on);
        refreshGroup($group);
    });

    $(document).on("click", "#permSelectAll", function () {
        $("#permGroups .perm-item-check:not(:disabled)").prop("checked", true);
        initAll();
    });

    $(document).on("click", "#permClearAll", function () {
        $("#permGroups .perm-item-check:not(:disabled)").prop("checked", false);
        initAll();
    });

    $(document).on("click", "#permExpandAll", function () {
        $("#permGroups .perm-group__body").collapse("show");
    });

    $(document).on("click", "#permCollapseAll", function () {
        $("#permGroups .perm-group__body").collapse("hide");
    });

    $(document).on("input", "#permSearch", function () {
        var q = (this.value || "").trim().toLowerCase();

        $("#permGroups .perm-group").each(function () {
            var $group = $(this);
            var visible = 0;

            $group.find(".perm-item").each(function () {
                var match = !q || ($(this).data("name") || "").indexOf(q) !== -1;
                $(this).toggleClass("d-none", !match);
                if (match) visible++;
            });

            $group.toggleClass("d-none", visible === 0);
            if (q && visible > 0)
                $group.find(".perm-group__body").collapse("show");
        });
    });
})();
