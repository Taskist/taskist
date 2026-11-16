/* Backlog grid selection bar: Clear button and Esc to drop the current row selection. */
(function () {
    "use strict";

    function clearSelection() {
        if (typeof resetSelection === "function") resetSelection();
        $("#row-checkbox-select-all").prop("checked", false);
    }

    $(document)
        .on("click", "#btnClearSelection", clearSelection)
        .on("keydown", function (e) {
            if (e.key !== "Escape" || $("#gridSelectionBar").hasClass("d-none")) return;
            if ($(".modal.show, .offcanvas.show, .jconfirm, .swal2-container").length) return;
            clearSelection();
        });
})();
