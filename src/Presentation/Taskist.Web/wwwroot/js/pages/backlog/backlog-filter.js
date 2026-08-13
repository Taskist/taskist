/* Backlog Filter partial - kendo multiselects + search/reset driving the grid.
   Loaded globally; the AJAX-injected partial calls BacklogFilter.init(). */

window.BacklogFilter = (function () {
    "use strict";

    return {
        init: function () {
            $(".kendo-select").kendoMultiSelect({
                autoClose: false,
                tagMode: "single"
            });

            $('#btnSearch').off('click.bkfilter').on('click.bkfilter', function () {
                JSManager.reloadGrid('BacklogGrid');
            });

            $('#btnReset').off('click.bkfilter').on('click.bkfilter', function () {
                $(".kendo-select-filter").each(function () {
                    var multiSelect = $(this).data("kendoMultiSelect");
                    if (multiSelect) {
                        multiSelect.value([]);
                        multiSelect.refresh();
                    }
                });
                JSManager.reloadGrid('BacklogGrid');
            });

            JSManager.reloadGrid('BacklogGrid');
        }
    };
})();
