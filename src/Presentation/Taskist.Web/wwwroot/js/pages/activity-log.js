(function () {
    "use strict";

    $(function () {
        function reload() {
            ["ActivityGrid", "AuditGrid"].forEach(function (name) {
                var grid = $("#" + name);
                if (grid.length && $.fn.dataTable.isDataTable(grid)) {
                    grid.DataTable().ajax.reload(null, true);
                }
            });
        }

        $("#logApply").on("click", reload);

        $("#logReset").on("click", function () {
            $("#logFrom, #logTo").val("");
            $("#logUser, #logType, #logAction").val("").trigger("change");
            reload();
        });
    });
})();
