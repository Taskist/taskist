let jcModal = null;
let bsOffCanvas = null;
let AppJS = (function () {
    let instance;
    let options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "preventDuplicates": true,
        "positionClass": "toast-top-center",
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

    let Toast = Swal.mixin({
        toast: true,
        position: 'bottom',
        iconColor: 'white',
        customClass: {
            popup: 'colored-toast'
        },
        showConfirmButton: false,
        timerProgressBar: true,
        timer: 30000
    });

    function createInstance() {
        return {
            init: function () {
                $('#show-error').click(function () {
                    $('#error-details').removeClass("no-display").slideToggle('slow');
                });

                $('.select2-with-search').select2({
                    placeholder: 'Choose',
                    searchInputPlaceholder: 'Search',
                    theme: 'bootstrap-5',
                    width: '100%'
                });

                let select2 = $(".select2"),
                    datepickerDob = $(".datepicker-dob"),
                    datepicker = $(".datepicker"),
                    datepickerRange = $(".datepicker-range"),
                    datepickerFrom = $(".datepicker-from"),
                    datepickerTo = $(".datepicker-to");

                if (select2.length) {
                    select2.each(function () {
                        let $this = $(this);
                        $this.wrap('<div class="position-relative"></div>').select2({
                            placeholder: "Select value",
                            dropdownParent: $this.parent()
                        });
                    });
                }

                if (datepickerDob.length) {
                    let today = new Date();
                    let endDate = new Date(today.setFullYear(today.getFullYear() - 18));
                    datepickerDob.datepicker({
                        format: "dd-M-yyyy",
                        viewMode: "date",
                        endDate: endDate,
                        autoclose: true,
                        todayHighlight: true
                    });
                }

                if (datepicker.length) {
                    datepicker.datepicker({
                        format: "dd-M-yyyy",
                        viewMode: "date",
                        autoclose: true,
                        todayHighlight: true
                    });
                }

                if (datepickerRange.length) {
                    datepickerRange.flatpickr({
                        mode: 'range',
                        dateFormat: "d-m-Y",
                    });
                }

                if (datepickerFrom.length) {
                    datepickerFrom.datepicker({
                        format: "dd-M-yyyy",
                        viewMode: "date",
                        autoclose: true,
                        todayHighlight: true
                    }).on('changeDate', function (e) {
                        datepickerTo.datepicker('clearDates');
                        datepickerTo.datepicker('setStartDate', e.date);
                    });
                }

                if (datepickerTo.length) {
                    datepickerTo.datepicker({
                        format: "dd-M-yyyy",
                        viewMode: "date",
                        autoclose: true,
                        todayHighlight: true
                    });
                }

                $(".numeric-textbox").on("keypress keyup blur", function (event) {
                    $(this).val($(this).val().replace(/[^0-9\.]/g, ''));
                    if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
                        event.preventDefault();
                    }
                });
            },
            bindPartialViewEvent: function () {
                $('.close-form').click(function () {
                    JSManager.closeForm();
                });
            },
            hasValue: function (value) {
                return !!(value != undefined && value != null && value != '' && value.length > 0);
            },
            setMandatorySymbol: function () {
                $('.form-control').each(function () {
                    let val = $(this).data('val');
                    let valRequired = $(this).data('val-required');
                    if (val && JSManager.hasValue(valRequired)) {
                        var label = $(this).closest(".mb-3").find("label");
                        if (label.length && !label.find(".text-danger").length) {
                            label.append('<i class="far fa-asterisk mandatory-symbol mx-1"></i>');
                        }
                    }
                });
            },
            openOffCanvas: function (loadurl, title) {
                JSManager.renderPartial(loadurl, {}, 'pdOffCanvasBody', function (response) {
                    $('#pdOffCanvasBodyTitle').text(title);
                    bsOffCanvas = new bootstrap.Offcanvas('#pdOffCanvas');
                    bsOffCanvas.show();
                });
            },
            openForm: function (loadurl, title, columnClass, mode = 4, callbackContentReady) {
                //Mode: Create = 1, Update = 2, Delete = 3, View = 4
                let buttonText = 'Save';
                switch (mode) {
                    case 1:
                        buttonText = '<i class="fas fa-save mr-1"></i>Save';
                        break;
                    case 2:
                        buttonText = '<i class="fas fa-save mr-1"></i>Update';
                        break;
                    case 3:
                        buttonText = '<i class="fas fa-trash mr-1"></i>Delete';
                        break;
                }

                if (columnClass === undefined || columnClass === null || columnClass === '')
                    columnClass = 'col-md-10';
                jcModal = $.confirm({
                    title: title,
                    closeIcon: true,
                    content: function () {
                        let self = this;
                        return $.ajax({
                            url: loadurl
                        }).done(function (response) {
                            self.setContentAppend(response);
                        }).fail(function (response) {
                            JSManager.showAppError(response.responseJSON);
                            jcModal.close();
                        });
                    },
                    type: 'blue',
                    confirmButtonClass: 'hide',
                    theme: 'bootstrap',
                    animation: 'left',
                    closeAnimation: 'scale',
                    useBootstrap: true,
                    columnClass: columnClass,
                    containerFluid: true,
                    buttons: {
                        yes: {
                            text: buttonText,
                            btnClass: 'btn-blue',
                            icon: 'fa fa-user',
                            isHidden: (mode === 4 || mode === 3),
                            action: function () {
                                $('.jconfirm-content form').submit();
                                return false;
                            }
                        },
                        no: {
                            text: '<i class="fa fa-ban mr-1"></i>Cancel',
                            btnClass: 'btn-secondary'
                        }
                    },
                    escapeKey: false,
                    backgroundDismiss: false,
                    onContentReady: function () {
                        if (callbackContentReady !== undefined)
                            callbackContentReady();
                    }
                });
            },
            closeOffCanvas: function () {
                if (bsOffCanvas !== null)
                    bsOffCanvas.hide();
            },
            closeForm: function () {
                if (jcModal !== null)
                    jcModal.close();
            },
            confirm: function (message, callback) {
                $.confirm({
                    title: 'Confirm',
                    content: message,
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    closeAnimation: 'scale',
                    opacity: 0.5,
                    buttons: {
                        'confirm': {
                            text: 'Yes',
                            btnClass: 'btn-red btn-submit-error',
                            action: function (result) {
                                if (callback !== undefined)
                                    callback(result);
                            }
                        },
                        cancel: function () {
                            text: 'No'
                        }
                    }
                });
            },
            showDeleteConfirmation: function (url, displayName, deleteButtonText, cancelButtonText) {
                if (displayName === undefined || displayName === null || displayName === '')
                    displayName = 'this record';
                if (!JSManager.hasValue(deleteButtonText))
                    deleteButtonText = 'Yes';
                if (!JSManager.hasValue(cancelButtonText))
                    cancelButtonText = 'No';

                let dto = this.addAntiForgeryToken();
                Swal.fire({
                    text: displayName,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#808080',
                    confirmButtonText: deleteButtonText,
                    cancelButtonText: cancelButtonText,
                    allowOutsideClick: false,
                }).then((result) => {
                    if (result.isConfirmed) {
                        JSManager.ajaxPost(url, dto, undefined, undefined, function (response) {
                            console.log(response)
                            if (response.status === 2000) {
                                JSManager.reloadAllGrids();
                                JSManager.showSuccess(response.message);
                            } else {
                                JSManager.showError(response.message);
                            }
                        });
                    }
                });
            },
            bindSelect: function (element, url, dto) {
                var select = $("#" + element).empty().append('<option value="">Select</option>');
                if (dto === undefined) dto = {};
                JSManager.blockUI(element);
                var data = JSManager.ajaxGetWithResponse(url, dto);
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    select.append('<option value="' + item.Value + '">' + item.Text + "</option>");
                }
                JSManager.unblockUI(element);
            },
            addAntiForgeryToken: function (data) {
                if (!data) {
                    data = {};
                }
                let tokenInput = $("input[name=__RequestVerificationToken]");
                if (tokenInput.length) {
                    data.__RequestVerificationToken = tokenInput.val();
                }
                return data;
            },
            getAntiForgeryToken: function () {
                let tokenInput = $("input[name=__RequestVerificationToken]");
                return tokenInput.val();
            },
            renderPartial: function (url, dto, target, callback) {
                JSManager.blockUI(target);
                $.ajax({
                    type: "GET",
                    url: url,
                    dataType: "html",
                    data: dto,
                    success: function (result) {
                        if (target !== undefined)
                            $("#" + target).html(result);
                        if (callback !== undefined)
                            callback(result);
                        JSManager.unblockUI(target);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("error: " + xhr + "\n" + ajaxOptions + "\n" + thrownError);
                        JSManager.unblockUI(target);
                    }
                });
            },
            renderPartialWithResponse: function (url, dto, target) {
                let response;
                JSManager.blockUI(target);
                $.ajax({
                    type: "GET",
                    url: url,
                    dataType: "html",
                    data: dto,
                    async: false,
                    success: function (result) {
                        response = result;
                        JSManager.unblockUI(target);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("error: " + xhr + "\n" + ajaxOptions + "\n" + thrownError);
                        JSManager.unblockUI(target);
                    }
                });
                return response;
            },
            ajaxGet: function (url, dto, blockElement, callback) {
                if (blockElement !== undefined)
                    this.blockUI(blockElement);
                $.ajax({
                    type: "GET",
                    url: url,
                    dataType: "json",
                    data: dto,
                    success: function (result) {
                        if (callback !== undefined)
                            callback(result);
                        if (blockElement !== undefined)
                            this.unblockUI(blockElement);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("error: " + xhr + "\n" + ajaxOptions + "\n" + thrownError);
                        if (blockElement !== undefined)
                            this.unblockUI(blockElement);
                    }
                });
            },
            ajaxGetWithResponse: function (url, dto) {
                let response;
                $.ajax({
                    type: "GET",
                    url: url,
                    dataType: "json",
                    data: dto,
                    async: false,
                    success: function (result) {
                        response = result;
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("error: " + xhr + "\n" + ajaxOptions + "\n" + thrownError);
                    }
                });
                return response;
            },
            ajaxPost: function (url, dto, blockElement, message, callback) {
                if (blockElement !== undefined)
                    JSManager.blockUI(blockElement, message);
                $.ajax({
                    type: "POST",
                    url: url,
                    data: JSManager.addAntiForgeryToken(dto),
                    dataType: "json",
                    success: function (result) {
                        if (callback !== undefined)
                            callback(result);
                        if (blockElement !== undefined)
                            JSManager.unblockUI(blockElement);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("error: " + xhr + "\n" + ajaxOptions + "\n" + thrownError);
                        if (blockElement !== undefined)
                            JSManager.unblockUI(blockElement);
                    }
                });
            },
            ajaxPostWithResponse: function (url, dto, blockElement) {
                if (blockElement !== undefined)
                    this.blockUI(blockElement, message);
                let response = null;
                $.ajax({
                    type: "POST",
                    url: url,
                    data: JSManager.addAntiForgeryToken(dto),
                    dataType: "json",
                    async: false,
                    success: function (result) {
                        response = result;
                        if (blockElement !== undefined)
                            this.unblockUI(blockElement);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("error: " + xhr + "\n" + ajaxOptions + "\n" + thrownError);
                        if (blockElement !== undefined)
                            this.unblockUI(blockElement);
                    }
                });
                return response;
            },
            ajaxPatch: function (url, dto, blockElement, message, callback) {
                if (blockElement !== undefined)
                    this.blockUI(blockElement, message);
                $.ajax({
                    type: "PATCH",
                    url: url,
                    data: this.addAntiForgeryToken(dto),
                    dataType: "json",
                    success: function (result) {
                        if (callback !== undefined)
                            callback(result);
                        if (blockElement !== undefined)
                            JSManager.unblockUI(blockElement);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.error("error: " + xhr + "\n" + ajaxOptions + "\n" + thrownError);
                        if (blockElement !== undefined)
                            JSManager.unblockUI(blockElement);
                    }
                });
            },
            reloadGrid: function (grid) {
                if (grid != undefined)
                    $('#' + grid).DataTable().ajax.reload(this.postGridReload(grid));
                else
                    $('.dataTable').DataTable().ajax.reload(this.initPopover);
            },
            reloadChildGrid: function (grid) {
                if (grid != undefined)
                    $('#' + grid).DataTable().ajax.reload(this.initPopover);
                else
                    $('.dataTable-child').DataTable().ajax.reload(this.initPopover);
            },
            postGridReload: function (grid) {
                let g = $('#' + grid).DataTable();
            },
            reloadAllGrids: function () {
                $('.dataTable').each(function () {
                    $(this).DataTable().ajax.reload(this.initPopover);
                });
            },
            initPopover: function () {
                $('.emp-details-popover').popover({
                    content: this.popoverContent,
                    html: true,
                    trigger: 'hover'
                });
            },
            blockUI: function (element, message) {
                if (message === undefined)
                    message = "Processing...";
                let block = $("#" + element);
                $(block).block({
                    message: '<i class="icon-spinner4 spinner"></i> ' + message,
                    overlayCSS: {
                        backgroundColor: '#fff',
                        opacity: 0.8,
                        cursor: 'wait'
                    },
                    css: {
                        border: 'none',
                        padding: '15px',
                        backgroundColor: '#000',
                        '-webkit-border-radius': '10px',
                        '-moz-border-radius': '10px',
                        opacity: .5,
                        color: '#fff'
                    }
                });
            },
            unblockUI: function (element) {
                let block = $("#" + element);
                $(block).unblock();
            },
            showInfo: function (message) {
                toastr.options = options;
                toastr["info"](message);
            },
            showError: function (message) {
                toastr.options = options;
                toastr["error"](message);
            },
            showWarning: function (message) {
                toastr.options = options;
                toastr["warning"](message);
            },
            showSuccess: function (message) {
                toastr.options = options;
                toastr["success"](message);
            },
            showAppError: function (message) {
                debugger
                Swal.fire({
                    title: 'Application Error!',
                    text: message,
                    icon: 'error',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#808080',
                    confirmButtonText: "Report Bug",
                    cancelButtonText: "Cancel",
                    allowOutsideClick: false,
                }).then((result) => {
                    if (result.isConfirmed) {
                        JSManager.reportBug();
                    }
                });
            },
            closeMessage: function (message) {
                toastr.options = options;
                toastr["remove"](message);
            },
            showSwal: function (message, type) {
                if (type === undefined)
                    type = 'info';
                Swal.fire('', message, type)
            },
            setPageTitle: function (title) {
                if (JSManager.hasValue(title)) {
                    $('#pageTitle').html(title);
                }
            },
            setPageButtons: function (buttons) {
                $('#pageButtons').html(buttons);
            },
            renderActions: function (row, actions) {
                let result = '';
                for (const element of actions) {
                    let curAction = element;
                    if (curAction.VisibleConditions.length > 0) {
                        for (const subElement of curAction.VisibleConditions) {
                            let curCondition = subElement;
                            let valueToCompare = row[curCondition.ReferenceParameter];
                            let match = this.isConditionSatisfied(valueToCompare, curCondition);
                            if (match) {
                                result += this.generateLink(row, curAction);
                            }
                        }
                    }
                    else {
                        result += this.generateLink(row, curAction);
                    }
                }
                return result;
            },
            generateLink: function (row, curAction) {
                let url = curAction.Url;
                let btnClass = curAction.ButtonColor;
                let modalTitle = curAction.Title;
                if (this.hasValue(curAction.TitlePrefixDataParam) && this.hasValue(curAction.TitleSufixDataParam)) {
                    modalTitle = row[curAction.TitlePrefixDataParam] + ' ' + curAction.Title + ' ' + row[curAction.TitleSufixDataParam];
                }
                else if (this.hasValue(curAction.TitlePrefixDataParam)) {
                    modalTitle = row[curAction.TitlePrefixDataParam] + ' ' + curAction.Title;
                } else if (this.hasValue(curAction.TitleSufixDataParam)) {
                    modalTitle = curAction.Title + ' ' + row[curAction.TitleSufixDataParam];
                }
                if (this.hasValue(curAction.ReferenceParameter)) {
                    let params = curAction.ReferenceParameter.split(',');
                    let queryString = '';
                    for (const element of params) {
                        let paramName = element;
                        let refValue = row[element];
                        if (this.hasValue(queryString)) {
                            queryString += '&';
                        }
                        queryString += paramName + '=' + refValue;
                    }
                    url = curAction.Url + '?' + queryString;
                }
                if (curAction.NavigationType != 1) {
                    if (curAction.DeleteConfirmBox) {
                        let displayName = 'record';
                        if (this.hasValue(curAction.DisplayReferenceParameter)) {
                            displayName = row[curAction.DisplayReferenceParameter];
                        }
                        url = "javascript:JSManager.showDeleteConfirmation('" + url + "','" + displayName + "')";
                    } else {
                        url = "javascript:JSManager.openForm('" + url + "','" + (curAction.FormatTitle ? this.capitalizeText(modalTitle) : modalTitle) + "','" + curAction.ModalSize + "')";
                    }
                }
                if (curAction.HyperLinkType === 2) {
                    finalUrl = '<a title="' + curAction.Title + '" href="' + url + '" class="btn btn-' + btnClass + ' btn-xs"><i class="' + curAction.Icon + '"></i></a>';
                }
                else if (curAction.HyperLinkType === 1) {
                    finalUrl = '<a title="' + curAction.Title + '" href=\"' + url + '" class="btn btn-' + btnClass + ' btn-xs">' + curAction.Text + '</a>';
                }
                else if (curAction.HyperLinkType === 3) {
                    finalUrl = '<a title="' + curAction.Title + '" href="' + url + '" class="btn btn-' + btnClass + ' btn-xs"><i class="' + curAction.Icon + '"></i> ' + curAction.Text + '</a>';
                }
                else if (curAction.HyperLinkType === 4) {
                    finalUrl = '<a title="' + curAction.Title + '" href="' + url + '">' + curAction.Text + '</a>';
                }
                else if (curAction.HyperLinkType === 5) {
                    finalUrl = '<a title="' + curAction.Title + '" href="' + url + '"><i class="' + curAction.Icon + '"></i></a>';
                }
                else if (curAction.HyperLinkType === 6) {
                    finalUrl = '<a title="' + curAction.Title + '" href="' + url + '"><i class="' + curAction.Icon + '"></i> ' + curAction.Text + '</a>';
                }
                else {
                    finalUrl = '<a title="' + curAction.Title + '" href="' + url + '">' + curAction.Text + '</a>';
                }
                return finalUrl;
            },
            capitalizeText: function (str) {
                if (str === undefined || str === null || str === '')
                    return str;
                str = str.toString().toLowerCase();
                return str.replace(/(?:^|\s)\w/g, function (match) {
                    return match.toUpperCase();
                });
            },
            renderText: function (data, capitalize, maxLength, showUserDetails, showProgressBar, showStep, showHijri) {
                let newData = data;
                if (showUserDetails) {
                    let userDetailsLink = '<a href="#" data-id="' + newData + '" class="emp-details-popover">' + newData + '</a>';
                    return userDetailsLink;
                }
                if (showHijri) {
                    if (newData != "") {
                        let FromDate = isDate(newData);
                        if (FromDate == false) return "";
                        return moment(newData, "DD-MMM-YYYY").format('iDD-iMM-iYYYY')
                    }
                }
                if (showProgressBar) {
                    let pBarClass = 'bg-primary';
                    if (newData < 50) {
                        pBarClass = 'bg-danger';
                    } else if (newData >= 50 && newData < 80) {
                        pBarClass = 'bg-warning';
                    } else if (newData >= 80 && newData < 100) {
                        pBarClass = 'bg-primary';
                    } else if (newData >= 100) {
                        pBarClass = 'bg-success';
                    }
                    let pBar = '<div class="progress"><div class="progress-bar progress-bar-striped progress-bar-animated ' + pBarClass + '" role="progressbar" style="width: ' + newData + '%;" aria-valuenow="' + newData + '" aria-valuemin="0" aria-valuemax="100">' + newData + '%</div></div>';
                    return pBar;
                }
                if (showStep) {
                    let jsonData = data;
                    let step = '';
                    let circleActiveClass = 'step-color-none';
                    for (const element of jsonData) {
                        let stepItem = element;
                        if (this.hasValue(step)) {
                            step += '<i class="fas fa-long-arrow-alt-right step-gray"></i>';
                        }
                        switch (parseInt(stepItem.Color)) {
                            case 1:
                                circleActiveClass = 'step-green';
                                break;
                            case 2:
                                circleActiveClass = 'step-red';
                                break;
                            case 3:
                                circleActiveClass = 'step-blue';
                                break;
                            case 4:
                                circleActiveClass = 'step-orange';
                                break;
                            case 5:
                                circleActiveClass = 'step-yellow';
                                break;
                            case 6:
                                circleActiveClass = 'step-gray';
                                break;
                        }
                        step += '<i class="fas fa-circle ' + circleActiveClass + '" title="' + stepItem.ToolTip + '"></i>';
                    }
                    return step;
                }
                if (capitalize)
                    newData = this.capitalizeText(data);
                return this.textEllipsis(newData, maxLength);
            },
            renderCondition: function (data, conditions, capitalize, maxLength) {
                let result = data;
                for (const element of conditions) {
                    var obj = element;
                    let operator = obj.Operator;
                    let dataType = obj.DataType;
                    let value = obj.Value;
                    let parsedData = this.parseValue(data, dataType);
                    let parsedValue = this.parseValue(value, dataType);
                    switch (operator) {
                        //EQUAL
                        case 1:
                            if (parsedData === parsedValue) {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                        //NOT_EQUAL
                        case 2:
                            if (parsedData !== parsedValue) {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                        //LESSTHAN
                        case 3:
                            if (parsedData < parsedValue) {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                        //LESSTHAN_EQUALTO
                        case 4:
                            if (parsedData <= parsedValue) {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                        //GREATERTHAN
                        case 5:
                            if (parsedData > parsedValue) {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                        //GREATERTHAN_EQUALTO
                        case 6:
                            if (parsedData >= parsedValue) {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                        //EMPTY
                        case 7:
                            if (data === '') {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                        //INCLUDES
                        case 8:
                            if (parsedData.includes(parsedValue)) {
                                result = this.generateTemplate(data, obj, capitalize, maxLength);
                            }
                            break;
                    }
                }
                return result;
            },
            generateTemplate: function (data, obj, capitalize, maxLength) {
                let hidden = obj.HideIfTrue;
                let replaceWith = obj.ReplaceWith;
                if (capitalize)
                    data = this.capitalizeText(data);
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
            textEllipsis: function (text, maxLength) {
                if (text === undefined || text === null || text === '')
                    return text;
                let data = text.toString();
                if (data.length > maxLength) {
                    let newData = '<span data-toggle="popover" data-trigger="hover" data-content="' + data + '">' + data.substr(0, maxLength) + '…' + '</span>';
                    return newData;
                }
                return text;
            },
            checkVisibility: function (row, template, ref, condition) {
                let value = row[ref];
                let isTrue = this.isConditionSatisfied(value, condition);
                if (isTrue === false) {
                    template = '';
                }
                return template;
            },
            isConditionSatisfied: function (data, condition) {
                debugger
                let result = false;
                let operator = condition.Operator;
                let dataType = condition.DataType;
                let value = condition.Value;
                if (!this.hasValue(value))
                    return true;
                let parsedData = this.parseValue(data, dataType);
                let parsedValue = this.parseValue(value, dataType);
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
            parseValue: function (value, dataType) {
                switch (dataType) {
                    //INT
                    case 1:
                        return parseInt(value);
                    //DECIMAL
                    case 2:
                        return parseFloat(value);
                    //STRING
                    default:
                        return value;
                }
            },
            capture: function (isModal = false) {
                let element = '#body-app';
                if (isModal)
                    element = '.jconfirm-box';
                html2canvas(document.querySelector(element)).then(canvas => {
                    $('#CapturedScreenShot').val(canvas.toDataURL("image/png"));
                });
            },
            reportBug: function (isModal = false) {
                this.capture();
                $.confirm({
                    title: 'Send Error Report',
                    content: 'Are you sure you want to send the error report to support team?',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    closeAnimation: 'scale',
                    opacity: 0.5,
                    buttons: {
                        'confirm': {
                            text: 'Submit',
                            btnClass: 'btn-red btn-submit-error',
                            action: function () {
                                $('.btn-submit-error').attr('disabled', 'disabled');
                                let image = $('#CapturedScreenShot').val();
                                let url = window.location.href;
                                let dto = {
                                    image,
                                    url,
                                    isModal
                                };
                                let submitUrl = $('#BaseUrl').val() + 'Common/SubmitError';
                                let response = JSManager.ajaxPostWithResponse(submitUrl, dto);
                                if (response.status === 2000) {
                                    JSManager.showSuccess(response.message);
                                } else {
                                    JSManager.showError(response.message);
                                }
                            }
                        },
                        cancel: function () {

                        }
                    }
                });
            }
        };
    };

    return {
        getInstance: function () {
            if (!instance) {
                instance = createInstance();
            }
            instance.init();
            return instance;
        }
    };

})();