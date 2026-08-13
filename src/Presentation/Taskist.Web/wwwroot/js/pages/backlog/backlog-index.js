/* Backlog Index - grid, grouping, column toggle, export, project switcher.
   Reads its server values from #backlog-index-config (URLs, flags, localized labels). */

(function () {
    "use strict";

    var $cfg = $("#backlog-index-config");
    if (!$cfg.length) return;

    var cfg = $cfg.data();                       // urls + flags
    var L = JSON.parse($cfg.attr("data-labels")); // localized strings

    var filterLoaded = false;

    $(document).ready(function () {
        JSManager.setPageTitle(L.pageTitle);

        // contextual project switcher: set active project, reload the page
        $('#activeProjectSwitcher').on('change', function () {
            window.location.href = cfg.setProjectUrl + '?id=' + $(this).val();
        });

        filterLoaded = true;
        JSManager.renderPartial(cfg.filterUrl, {}, 'filter-container');

        if (cfg.canReport) {
            $('#pageButtons').append('<a href="' + cfg.createUrl + '" class="btn btn-sm btn-primary me-2"><i class="fas fa-plus me-1"></i>Add New</a>');
        }

        var editUrl = cfg.editUrl;

        var table = $("#BacklogGrid").DataTable({
            processing: true,
            serverSide: true,
            stateSave: false,
            info: true,
            autoWidth: false,
            filter: false,
            lengthChange: true,
            pageLength: 10,
            searching: false,
            ordering: true,
            rowGroup: {
                dataSrc: [],
                enable: false,
                startRender: function (rows, group, level) {
                    var count = rows.count();
                    var indent = "&nbsp;&nbsp;&nbsp;".repeat(level);
                    return $('<tr class="group-header"><td colspan="4">' + indent + '<strong>' + group + ' (' + count + ')</strong></td></tr>');
                },
                endRender: function () {
                    $('#columnToggleMenu input[type="checkbox"]').trigger('change');
                }
            },
            language: {
                search: '<span>' + L.searchLabel + '</span> _INPUT_',
                searchPlaceholder: L.searchPlaceholder,
                lengthMenu: '_MENU_ ' + L.lengthMenu,
                paginate: { 'first': 'First', 'last': 'Last', 'next': '&rarr;', 'previous': '&larr;' },
                zeroRecords: L.zeroRecords,
                info: L.info,
                infoEmpty: L.infoEmpty
            },
            ajax: {
                url: cfg.readUrl,
                type: "POST",
                datatype: "json",
                data: function (dto) {
                    setFilterParam(dto);
                    JSManager.addAntiForgeryToken(dto);
                }
            },
            columns: [{
                title: L.colTaskId,
                data: 'Id',
                sortable: true,
                render: function (data, type, row) {
                    return data;
                }
            }, {
                title: L.colTitle,
                data: 'Title',
                sortable: true,
                render: function (data, type, row) {
                    if (data.length > 50) {
                        var truncatedText = data.substr(0, 50) + "...";
                        return '<a title="' + L.edit + '" href="' + editUrl + '/' + row.Id + '"><span class="dt-truncate"'
                            + ' tabindex="0" role="button" data-bs-toggle="popover" data-bs-trigger="hover focus"'
                            + ' data-bs-content="' + $('<div>').text(data).html() + '">' + truncatedText + '</span></a>';
                    }
                    return '<a title="' + L.edit + '" href="' + editUrl + '/' + row.Id + '">' + data + '</a>';
                }
            }, {
                title: L.colTaskType,
                sortable: true,
                render: function (data, type, row) {
                    return '<span class="badge" style="color:' + row.TaskType.TextColor + ';background-color:' + row.TaskType.BackgroundColor + ';"><i class="' + row.TaskType.IconClass + ' me-1"></i>' + row.TaskType.Name + '</span>';
                }
            }, {
                title: L.colSeverity,
                sortable: true,
                render: function (data, type, row) {
                    return '<span class="badge" style="color:' + row.Severity.TextColor + ';background-color:' + row.Severity.BackgroundColor + ';"><i class="' + row.Severity.IconClass + ' me-1"></i>' + row.Severity.Name + '</span>';
                }
            }, {
                title: L.colStatus,
                sortable: true,
                render: function (data, type, row) {
                    var reopenCounter = '';
                    if (parseInt(row.ReOpenCount) > 2) {
                        reopenCounter = '<span class="badge rounded-pill bg-danger me-1" title="Reopened count">' + row.ReOpenCount + '</span>';
                    } else if (parseInt(row.ReOpenCount) > 0) {
                        reopenCounter = '<span class="badge rounded-pill bg-warning me-1" title="Reopened count">' + row.ReOpenCount + '</span>';
                    }
                    return reopenCounter + '<span class="badge" style="color:' + row.Status.TextColor + ';background-color:' + row.Status.BackgroundColor + ';"><i class="' + row.Status.IconClass + ' me-1"></i>' + row.Status.Name + '</span>';
                }
            }, {
                title: L.colProject, data: 'Project', sortable: true, render: function (d) { return d; }
            }, {
                title: L.colModule, data: 'Module', sortable: true, render: function (d) { return d; }
            }, {
                title: L.colSubModule, data: 'SubModule', sortable: true, render: function (d) { return d; }
            }, {
                title: L.colAssignee, data: 'Assignee', sortable: true, render: function (d) { return d; }
            }, {
                title: L.colDueDate,
                data: 'DueDate',
                sortable: true,
                render: function (data, type, row) {
                    if (JSManager.hasValue(data)) {
                        var today = moment();
                        var targetDate = moment(data);
                        var dayDifference = targetDate.diff(today, "days");
                        if (dayDifference < 0) {
                            return '<i class="far fa-calendar-times text-danger me-1" title="Overdue by ' + Math.abs(dayDifference) + ' days"></i>' + moment(data).format('DD-MMM-YYYY');
                        } else if (dayDifference === 0) {
                            return '<i class="far fa-calendar-check text-success me-1" title="' + dayDifference + ' Due today!"></i>' + moment(data).format('DD-MMM-YYYY');
                        }
                        return '<i class="far fa-calendar-plus text-primary me-1" title="' + dayDifference + ' days remaining"></i>' + moment(data).format('DD-MMM-YYYY');
                    }
                    return 'Not set';
                }
            }, {
                title: L.colCreatedBy, data: 'CreatedBy', sortable: true, render: function (d) { return d; }
            }, {
                title: L.colCreatedOn,
                data: 'CreatedOn',
                sortable: true,
                render: function (data) { return moment(data).format('DD-MMM-YYYY'); }
            }]
        });

        if (!$.fn.dataTable.RowGroup) {
            console.error("RowGroup is NOT loaded. Check your script imports.");
        }

        $('#groupingToggleMenu input[type="checkbox"]').on('change', updateColumnGrouping);
        $('#columnToggleMenu input[type="checkbox"]').on('change', updateColumnVisibility);

        function updateColumnGrouping() {
            var selectedGroups = $('#groupingToggleMenu input[type="checkbox"]:checked').map(function () {
                return $(this).val();
            }).get();

            if (selectedGroups.length > 0) {
                table.rowGroup().dataSrc(selectedGroups).enable(true).draw();
            } else {
                table.rowGroup().dataSrc([]).enable(false).draw();
            }

            $('#groupingToggleBtn').text('Group By (' + selectedGroups.length + ')');
        }

        function updateColumnVisibility() {
            $('#columnToggleMenu input[type="checkbox"]').each(function () {
                table.column($(this).data('column')).visible($(this).prop('checked'));
            });
            var selectedCount = $('#columnToggleMenu input[type="checkbox"]:checked').length;
            $('#columnToggleBtn').text('Showing ' + selectedCount + '/10 Columns');
        }

        $("#BacklogGrid").closest('div').addClass("table-responsive");

        $('#BacklogGrid').on('draw.dt', function () {
            [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
                .map(function (el) { return new bootstrap.Popover(el); });
        });

        $('#filterContent').on('shown.bs.collapse', function () {
            $('#filterIcon').removeClass('fa-plus').addClass('fa-minus');
            if (!filterLoaded) {
                filterLoaded = true;
                JSManager.renderPartial(cfg.filterBaseUrl, {}, 'filter-container');
            }
        });

        $('#filterContent').on('hidden.bs.collapse', function () {
            $('#filterIcon').removeClass('fa-minus').addClass('fa-plus');
        });

        $('#resetGroupBy').on('click', function () {
            $('#groupingToggleMenu input[type="checkbox"]').prop('checked', false);
            updateColumnGrouping();
        });

        $('#resetColumn').on('click', function () {
            $('#columnToggleMenu input[type="checkbox"]').prop('checked', true);
            updateColumnVisibility();
        });

        $('#btnExcelExport').on('click', function () {
            var visibleColumns = [];
            var groupByColumns = [];

            $('#columnToggleMenu input:checked').each(function () { visibleColumns.push($(this).data('columnname')); });
            $('#groupingToggleMenu input:checked').each(function () { groupByColumns.push($(this).data('columnname')); });

            var exportUrl = cfg.exportUrl
                + "?columns=" + visibleColumns.join(',')
                + "&createdby=" + $('#CreatedById').val()
                + "&module=" + $('#ModuleId').val()
                + "&subModule=" + $('#SubModuleId').val()
                + "&taskType=" + $('#TaskTypeId').val()
                + "&severity=" + $('#SeverityId').val()
                + "&reporter=" + $('#ReporterId').val()
                + "&assignee=" + $('#AssigneeId').val()
                + "&status=" + $('#StatusId').val()
                + "&sprint=" + $('#SprintId').val()
                + "&groups=" + groupByColumns.join(',');

            window.location.href = exportUrl;
        });
    });

    function setFilterParam(dto) {
        dto.createdby = $('#CreatedById').val();
        dto.module = $('#ModuleId').val();
        dto.subModule = $('#SubModuleId').val();
        dto.taskType = $('#TaskTypeId').val();
        dto.severity = $('#SeverityId').val();
        dto.reporter = $('#ReporterId').val();
        dto.assignee = $('#AssigneeId').val();
        dto.status = $('#StatusId').val();
        dto.sprint = $('#SprintId').val();
        return dto;
    }
})();
