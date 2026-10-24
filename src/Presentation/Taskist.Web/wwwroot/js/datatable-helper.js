let DataTable = {
    RenderActions: function (row, actions) {
        let result = '';
        for (const element of actions) {
            let curAction = element;
            if (curAction.VisibleConditions.length > 0) {
                for (const element of curAction.VisibleConditions) {
                    let curCondition = element;
                    let valueToCompare = row[curCondition.ReferenceParameter];
                    let match = DataTable.IsConditionSatisfied(valueToCompare, curCondition);
                    if (match) {
                        result += DataTable.GenerateLink(row, curAction);
                    }
                }
            }
            else {
                result += DataTable.GenerateLink(row, curAction);
            }
        }
        return result;
    },
    GenerateLink: function (row, curAction) {
        let url = curAction.Url;
        let btnClass = DataTable.ButtonColorToCssClass(curAction.ButtonColor);
        let title = JSManager.hasValue(curAction.Title) ? curAction.Title : curAction.Text;
        if (!JSManager.hasValue(title))
            title = 'N/A';

        if (JSManager.hasValue(curAction.TitlePrefixDataParam) && JSManager.hasValue(curAction.TitleSufixDataParam)) {
            title = row[curAction.TitlePrefixDataParam] + ' ' + title + ' ' + row[curAction.TitleSufixDataParam];
        }
        else if (JSManager.hasValue(curAction.TitlePrefixDataParam)) {
            title = row[curAction.TitlePrefixDataParam] + ' ' + title;
        } else if (JSManager.hasValue(curAction.TitleSufixDataParam)) {
            title = title + ' ' + row[curAction.TitleSufixDataParam];
        }

        if (JSManager.hasValue(curAction.ReferenceParameter)) {
            let params = curAction.ReferenceParameter.split(',');
            let queryString = '';
            for (const element of params) {
                let paramName = element;
                let refValue = row[element];
                if (JSManager.hasValue(queryString)) {
                    queryString += '&';
                }

                queryString += paramName + '=' + refValue;
            }
            url = curAction.Url + '?' + queryString;
        }

        if (curAction.NavigationType != 1) {
            if (curAction.DeleteConfirmBox) {
                let displayName = 'are you sure you want to delete this record?';
                if (JSManager.hasValue(curAction.DeleteConfirmBoxMsg)) {
                    displayName = curAction.DeleteConfirmBoxMsg;
                } else if (JSManager.hasValue(curAction.DisplayReferenceParameter)) {
                    displayName = row[curAction.DisplayReferenceParameter];
                }

                displayName = displayName.replace(/'/g, "\\'");
                url = "javascript:JSManager.showDeleteConfirmation('" + url + "','" + displayName + "')";

            } else if (curAction.ConfirmBox) {
                url = "javascript:JSManager.ShowConfirmation('" + url + "','" + curAction.ConfirmBoxMsg + "','" + curAction.ModalSize + "')";
            } else if (curAction.NavigationType == 2) {
                url = "javascript:JSManager.openForm('" + url + "','" + DataTable.CapitalizeText(title) + "','" + curAction.ModalSize + "')";
            } else {
                url = "javascript:JSManager.openOffCanvas('" + url + "','" + DataTable.CapitalizeText(title) + "')";
            }
        }

        let openIn = '_self';
        if (curAction.OpenInBlankPage) {
            openIn = '_blank';
        }

        if (curAction.HyperLinkType === 2) {
            finalUrl = `<a title="${title}" href="${url}" class="btn ${btnClass} btn-sm me-1" target="${openIn}"><i class="${curAction.Icon}"></i></a>`;
        }
        else if (curAction.HyperLinkType === 1) {
            finalUrl = `a title="${title}" href="${url}" class="btn ${btnClass} btn-sm  me-1" target="${openIn}">${curAction.Text}</a>`;
        }
        else if (curAction.HyperLinkType === 3) {
            finalUrl = `<a title="${title}" href="${url}" class="btn ${btnClass} btn-sm  me-1" target="${openIn}"><i class="${curAction.Icon}"></i>${curAction.Text}</a>`;
        }
        else if (curAction.HyperLinkType === 4) {
            finalUrl = `<a title="${title}" href="${url}">${curAction.Text}</a>`;
        }
        else if (curAction.HyperLinkType === 5) {
            finalUrl = `<a title="${title}" href="${url}" class=" me-1" target="${openIn}"><i class="${curAction.Icon}"></i></a>`;
        }
        else if (curAction.HyperLinkType === 6) {
            finalUrl = `<a title="${title}" href="${url}" class=" me-1" target="${openIn}"><i class="${curAction.Icon}"></i>${curAction.Text}</a>`;
        }
        else {
            finalUrl = `<a title="${title}" href="${url}" class=" me-1" target="${openIn}">${curAction.Text}</a>`;
        }
        return finalUrl;
    },
    CapitalizeText: function (str) {
        if (str === undefined || str === null || str === '')
            return str;
        str = str.toString().toLowerCase();
        return str.replace(/(?:^|\s)\w/g, function (match) {
            return match.toUpperCase();
        });
    },
    RenderText: function (data, capitalize, maxLength, isDate, isRating, isColor, isIconClass) {
        let newData = data;
        if (isDate)
            newData = data ? moment(data).format('DD-MMM-YYYY') : '';

        if (isRating) {
            let maxStars = 5;
            let noOfStars = parseInt(newData);
            let stars = '';
            for (let i = 1; i <= maxStars; i++) {
                if (i <= noOfStars)
                    stars += '<i class="fas fa-star fa-star-rating-active"></i>';
                else
                    stars += '<i class="far fa-star fa-star-rating-inactive"></i>';
            }
            return stars;
        }

        if (isColor && JSManager.hasValue(newData)) {
            return `<span class="me-2" style="background-color:${newData};">&nbsp;</span>${newData}`;
        }

        if (isIconClass) {
            return `<i class="${newData}"></i>`;
        }

        if (capitalize)
            newData = DataTable.CapitalizeText(data);

        return DataTable.TextEllipsis(newData, maxLength);
    },
    RenderDayCount: function (data, row) {
        var days = (data === null || data === undefined || data === '') ? '' : data;
        var isHalf = row && (row.IsHalfDay === true || row.isHalfDay === true);
        var portion = row ? (row.Portion || row.portion) : null;
        if (isHalf) {
            var half = (portion === 2 || portion === 'SecondHalf') ? 'Afternoon' : 'Morning';
            return days + ' <span class="badge-session">' + half + '</span>';
        }
        return days;
    },
    ButtonColorToCssClass: function (buttonColor) {
        let cssClass = 'btn-primary';
        switch (buttonColor) {
            case 1:
                cssClass = "btn-primary";
                break;
            case 2:
                cssClass = "btn-info";
                break;
            case 3:
                cssClass = "btn-secondary";
                break;
            case 4:
                cssClass = "btn-success";
                break;
            case 5:
                cssClass = "btn-warning";
                break;
            case 6:
                cssClass = "btn-danger";
                break;
            case 7:
                cssClass = "btn-dark";
                break;
            case 8:
                cssClass = "btn-light";
                break;
            case 9:
                cssClass = "btn-outline-primary";
                break;
            case 10:
                cssClass = "btn-outline-info";
                break;
            case 11:
                cssClass = "btn-outline-secondary";
                break;
            case 12:
                cssClass = "btn-outline-success";
                break;
            case 13:
                cssClass = "btn-outline-warning";
                break;
            case 14:
                cssClass = "btn-outline-danger";
                break;
            case 15:
                cssClass = "btn-outline-dark";
                break;
            case 16:
                cssClass = "btn-outline-light";
                break;
            default:
                cssClass = "btn-outline-primary";
                break;
        }
        return cssClass;
    },
    RenderCondition: function (data, conditions, capitalize, maxLength) {
        let result = data;
        for (const element of conditions) {
            let obj = element;
            let operator = obj.Operator;
            let dataType = obj.DataType;
            let value = obj.Value;
            let parsedData = DataTable.ParseValue(data, dataType);
            let parsedValue = DataTable.ParseValue(value, dataType);

            switch (operator) {
                //EQUAL
                case 1:
                    if (parsedData === parsedValue) {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
                //NOT_EQUAL
                case 2:
                    if (parsedData !== parsedValue) {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
                //LESSTHAN
                case 3:
                    if (parsedData < parsedValue) {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
                //LESSTHAN_EQUALTO
                case 4:
                    if (parsedData <= parsedValue) {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
                //GREATERTHAN
                case 5:
                    if (parsedData > parsedValue) {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
                //GREATERTHAN_EQUALTO
                case 6:
                    if (parsedData >= parsedValue) {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
                //EMPTY
                case 7:
                    if (data === '') {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
                //INCLUDES
                case 8:
                    if (parsedData.includes(parsedValue)) {
                        result = DataTable.GenerateTemplate(data, obj, capitalize, maxLength);
                    }
                    break;
            }
        }

        return result;
    },
    GenerateTemplate: function (data, obj, capitalize, maxLength) {
        let hidden = obj.HideIfTrue;
        let replaceWith = obj.ReplaceWith;

        if (capitalize)
            data = DataTable.CapitalizeText(data);

        let textColor = obj.TextColor;
        let bgColor = obj.BgColor;
        let icon = obj.Icon;

        let result = '';

        if (!hidden) {
            result += '<span style="color:' + textColor + ';background:' + bgColor + ';">';
            if (icon !== null && icon.length > 0) {
                result += '<i class="' + icon + '"></i>';
            } else {
                if (replaceWith != undefined && replaceWith != null && replaceWith != '' && replaceWith.length > 0) {
                    result += replaceWith;
                } else {
                    result += data;
                }
            }
            result += '</span>';
        }

        return result;
    },
    TextEllipsis: function (text, maxLength) {
        if (text === undefined || text === null || text === '')
            return text;

        let data = text.toString();
        if (data.length > maxLength) {
            let esc = function (s) {
                return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
            };
            return '<span title="' + esc(data) + '">' + esc(data.substr(0, maxLength)) + '…' + '</span>';
        }

        return text;
    },
    CheckVisibility: function (row, template, ref, condition) {
        let value = row[ref];
        let isTrue = DataTable.IsConditionSatisfied(value, condition);
        if (isTrue === false) {
            template = '';
        }
        return template;
    },
    IsConditionSatisfied: function (data, condition) {
        let result = false;
        let operator = condition.Operator;
        let dataType = condition.DataType;
        let value = condition.Value;

        if (!JSManager.hasValue(value))
            return true;

        let parsedData = DataTable.ParseValue(data, dataType);
        let parsedValue = DataTable.ParseValue(value, dataType);

        switch (operator) {
            //EQUAL
            case 1:
                if (parsedData === parsedValue) {
                    result = true;
                }
                break;
            //NOT_EQUAL
            case 2:
                if (parsedData !== parsedValue) {
                    result = true;
                }
                break;
            //LESSTHAN
            case 3:
                if (parsedData < parsedValue) {
                    result = true;
                }
                break;
            //LESSTHAN_EQUALTO
            case 4:
                if (parsedData <= parsedValue) {
                    result = true;
                }
                break;
            //GREATERTHAN
            case 5:
                if (parsedData > parsedValue) {
                    result = true;
                }
                break;
            //GREATERTHAN_EQUALTO
            case 6:
                if (parsedData >= parsedValue) {
                    result = true;
                }
                break;
            //EMPTY
            case 7:
                if (data === '') {
                    result = true;
                }
                break;
        }

        return result;
    },
    ParseValue: function (value, dataType) {
        switch (dataType) {
            //INT
            case 1:
                return parseInt(value);
            //DECIMAL
            case 2:
                return parseFloat(value);
            //STRING
            default:
                return value != undefined ? value.toString() : null;
        }
    },
    RenderSelect: function (row, key, actableKey) {
        let actable = !JSManager.hasValue(actableKey) || parseInt(row[actableKey]) === 1;
        if (!actable)
            return '';

        let id = row[key];
        return '<input type="checkbox" class="form-check-input dt-select-row" value="' + id + '" />';
    }
}

let DataTableSelect = {
    Init: function (gridName, config) {
        let $table = $('#' + gridName);

        $(document).on('change', '#selectAll_' + gridName, function () {
            let checked = $(this).is(':checked');
            $table.find('tbody .dt-select-row').prop('checked', checked);
        });

        $(document).on('change', '#' + gridName + ' tbody .dt-select-row', function () {
            let total = $table.find('tbody .dt-select-row').length;
            let checked = $table.find('tbody .dt-select-row:checked').length;
            $('#selectAll_' + gridName).prop('checked', total > 0 && total === checked);
        });

        $(document).on('click', '#' + config.ButtonId, function () {
            DataTableSelect.SubmitSelected(gridName, config);
        });
    },
    SelectedIds: function (gridName) {
        let ids = [];
        $('#' + gridName).find('tbody .dt-select-row:checked').each(function () {
            ids.push($(this).val());
        });
        return ids;
    },
    SubmitSelected: function (gridName, config) {
        let ids = DataTableSelect.SelectedIds(gridName);
        if (ids.length === 0) {
            JSManager.showError(config.NoneMsg);
            return;
        }

        let msg = config.ConfirmMsg.replace('{0}', ids.length);
        Swal.fire({
            text: msg,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#808080',
            confirmButtonText: 'Yes',
            cancelButtonText: 'No',
            allowOutsideClick: false,
        }).then((result) => {
            if (!result.isConfirmed)
                return;

            let dto = { ids: ids.join(',') };
            JSManager.addAntiForgeryToken(dto);
            JSManager.ajaxPost(config.Url, dto, undefined, undefined, function (response) {
                if (response.status === 2000) {
                    $('#selectAll_' + gridName).prop('checked', false);
                    $('#' + gridName).find('tbody .dt-select-row').prop('checked', false);
                    JSManager.reloadAllGrids();
                    JSManager.showSuccess(response.message);
                } else {
                    JSManager.showError(response.message);
                }
            });
        });
    }
};

/* Server-side grid search: one request after the user pauses typing instead of one per key press.
   Runs 500 ms after the last keystroke (2+ characters), Enter searches at once, Esc or an empty box resets,
   and an unchanged term never re-queries. Applied to every server-side DataTable via its init event. */
(function ($) {
    "use strict";

    if (!$ || !$.fn || !$.fn.dataTable) return;

    var DELAY = 500;
    var MIN_LENGTH = 2;

    $(document).on("init.dt", function (e, settings) {
        if (e.namespace !== "dt" || !settings || !settings.oFeatures || !settings.oFeatures.bServerSide) return;

        var api = new $.fn.dataTable.Api(settings);
        var $input = $(api.table().container()).find("div.dataTables_filter input");
        if (!$input.length || $input.data("searchDebounced")) return;

        var timer = null;

        function run(value, force) {
            clearTimeout(timer);
            var term = $.trim(value || "");
            if (!force && term.length > 0 && term.length < MIN_LENGTH) return;
            if (term === api.search()) return;
            api.search(term).draw();
        }

        $input.data("searchDebounced", true)
            .off()
            .on("input.dtsearch", function () {
                var value = this.value;
                clearTimeout(timer);
                if (!$.trim(value)) {
                    run("", true);
                    return;
                }
                timer = setTimeout(function () { run(value, false); }, DELAY);
            })
            .on("keydown.dtsearch", function (ev) {
                if (ev.key === "Enter") {
                    ev.preventDefault();
                    run(this.value, true);
                } else if (ev.key === "Escape") {
                    this.value = "";
                    run("", true);
                }
            });
    });
})(window.jQuery);

/* Empty grids: a proper empty state instead of "No records available". With a search term the grid says
   "No results" and offers Clear search (list endpoints often report the same total either way, so the search
   term — not recordsTotal — decides which message shows). Templates live in Sections/_EmptyTemplates. */
(function ($) {
    "use strict";

    if (!$ || !$.fn || !$.fn.dataTable) return;

    function template(id) {
        var tpl = document.getElementById(id);
        return tpl ? tpl.innerHTML : null;
    }

    $(document).on("preInit.dt", function (e, settings) {
        if (e.namespace !== "dt") return;
        var empty = template("wpGridEmptyTpl");
        if (!empty) return;
        settings.oLanguage.sEmptyTable = empty;
        settings.oLanguage.sZeroRecords = empty;
    });

    $(document).on("draw.dt", function (e, settings) {
        if (e.namespace !== "dt") return;
        var api = new $.fn.dataTable.Api(settings);
        var empty = api.page.info().recordsDisplay === 0;
        $(api.table().container()).find(".dataTables_info, .dataTables_paginate").toggleClass("d-none", empty);
        if (!empty || !$.trim(api.search())) return;
        var $cell = $(api.table().body()).find("td.dataTables_empty");
        var noMatch = template("wpGridNoMatchTpl");
        if ($cell.length && noMatch) $cell.html(noMatch);
    });

    $(document).on("click", ".js-grid-clear-search", function () {
        var $wrapper = $(this).closest(".dataTables_wrapper");
        var $table = $wrapper.find("table.dataTable").first();
        if (!$table.length) return;
        $wrapper.find("div.dataTables_filter input").val("");
        $table.DataTable().search("").draw();
    });
})(window.jQuery);
