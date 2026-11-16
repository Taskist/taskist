$(function () {
    var GRID = "UserAccessGrid";

    function reload() {
        var table = $("#" + GRID).DataTable();
        if (table)
            table.ajax.reload();
    }

    function syncHidden($ms) {
        var values = $ms.val() || [];
        $("#" + $ms.data("target")).val(values.join(","));
    }

    $(".ea-filter").on("change", function () {
        syncHidden($(this));
        reload();
    });

    $("#btnResetAccess").on("click", function () {
        $(".ea-filter").each(function () {
            $(this).val(null).trigger("change.select2");
            $("#" + $(this).data("target")).val("");
        });
        reload();
    });

    $("#btnExportAccess").on("click", function () {
        var url = $(this).data("url");
        var qs = $.param({
            roleId: $("#filterRole").val() || "",
            departmentId: $("#filterDepartment").val() || "",
            permissionId: $("#filterPermission").val() || ""
        });
        window.location = url + "?" + qs;
    });
});
