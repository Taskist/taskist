$(function () {
    JSManager = AppJS.getInstance();
    JSManager.bindPartialViewEvent();
    JSManager.setMandatorySymbol();
    JSManager.initLadda("#pdOffCanvasBody");

    // Initialise select2 inside ajax-loaded partials/offcanvas
    // (common.js init() only runs on full page load, not for modal content).
    JSManager.initPartialSelect2();

    // Initialise date pickers inside ajax-loaded partials/offcanvas
    // (common.js init() only runs on full page load, not for modal content).
    $('.datepicker').datepicker({ format: 'dd-M-yyyy', viewMode: 'date', autoclose: true, todayHighlight: true });
    var dpFrom = $('.datepicker-from'), dpTo = $('.datepicker-to');
    if (dpFrom.length) {
        dpFrom.datepicker({ format: 'dd-M-yyyy', viewMode: 'date', autoclose: true, todayHighlight: true })
            .on('changeDate', function (e) { dpTo.datepicker('setStartDate', e.date); });
    }
    if (dpTo.length) {
        dpTo.datepicker({ format: 'dd-M-yyyy', viewMode: 'date', autoclose: true, todayHighlight: true });
    }

    // Generic cascading dropdown for ajax-loaded partials: a source select with
    // data-cascade-target / data-cascade-url / data-cascade-param repopulates its
    // dependent select(s) via bindSelect. Each attribute may be a comma-separated
    // list to refresh several dependents from one source. Delegated + namespaced.
    $(document).off('change.cascade').on('change.cascade', 'select[data-cascade-target]', function () {
        var $src = $(this);
        var val = $src.val();
        var targets = String($src.data('cascade-target')).split(',');
        var urls = String($src.data('cascade-url')).split(',');
        var params = String($src.data('cascade-param')).split(',');
        for (var i = 0; i < targets.length; i++) {
            var target = targets[i].trim();
            if (!target) continue;
            var url = (urls[i] || urls[0] || '').trim();
            var paramName = (params[i] || params[0] || '').trim();
            var dto = {};
            dto[paramName] = val;
            JSManager.bindSelect(target, url, dto);
            $('#' + target).trigger('change');
        }
    });

    // Rich-text body inside offcanvas (announcements). TinyMCE + save-on-submit.
    if (typeof tinymce !== 'undefined') {
        $('.rt-body').each(function () {
            var id = this.id;
            tinymce.remove('#' + id);
            tinymce.init({
                selector: '#' + id,
                menubar: false,
                height: 240,
                plugins: 'link lists autolink',
                toolbar: 'undo redo | bold italic underline | bullist numlist | link | removeformat',
                setup: function (editor) {
                    editor.on('change keyup', function () { editor.save(); });
                }
            });
        });
        $(document).off('submit.rtbody').on('submit.rtbody', 'form', function () {
            if (tinymce.triggerSave) tinymce.triggerSave();
        });
    }

    // Audience targeting: show role/department picker based on the selected audience.
    // Enum select values are "0" Everyone / "1" Roles / "2" Departments.
    function applyAudience() {
        var val = String($('#annAudience').val());
        var showRoles = val === '1', showDepts = val === '2';
        $('#annRolesWrap').toggleClass('d-none', !showRoles);
        $('#annDepartmentsWrap').toggleClass('d-none', !showDepts);
        // select2 inited while hidden renders 0-width; nudge it when revealed.
        if (showRoles) $('#annRolesWrap select').trigger('change.select2');
        if (showDepts) $('#annDepartmentsWrap select').trigger('change.select2');
    }
    var $annAudience = $('#annAudience');
    if ($annAudience.length) {
        applyAudience();
        $annAudience.off('change.audience').on('change.audience', applyAudience);
    }

    $('.time-picker').each(function () { TimePicker.init(this); });
    $(document).off('mousedown.tp').on('mousedown.tp', function (e) {
        if (!$(e.target).closest('.tp-wrap').length)
            $('.tp-menu').addClass('d-none');
    });
});

var TimePicker = {
    step: 15,
    init: function (input) {
        var $input = $(input);
        if ($input.data('tpReady'))
            return;
        $input.data('tpReady', true);

        var $wrap = $('<div class="tp-wrap"></div>');
        $input.before($wrap);
        $wrap.append($input);
        $wrap.append('<span class="tp-icon"><i class="far fa-clock"></i></span>');

        var html = '';
        for (var m = 0; m < 24 * 60; m += TimePicker.step) {
            var hh = Math.floor(m / 60), mm = m % 60;
            var pad = function (n) { return n < 10 ? '0' + n : '' + n; };
            html += '<div class="tp-option" data-value="' + pad(hh) + ':' + pad(mm) + '">' + pad(hh) + ':' + pad(mm) + '</div>';
        }
        var $menu = $('<div class="tp-menu d-none"></div>').html(html);
        $wrap.append($menu);

        function open() {
            $('.tp-menu').not($menu).addClass('d-none');
            $menu.removeClass('d-none');
            var $sel = $menu.find('.tp-option[data-value="' + $input.val() + '"]');
            if ($sel.length)
                $menu.scrollTop($sel[0].offsetTop - 70);
        }
        function close() { $menu.addClass('d-none'); }

        $input.on('focus click', open);
        $wrap.find('.tp-icon').on('click', function () { $input.trigger('focus'); });
        $menu.on('mousedown', '.tp-option', function (e) {
            e.preventDefault();
            $input.val($(this).data('value')).trigger('change');
            if ($input.valid) $input.valid();
            close();
        });
    }
};

$(document).off("click.formCancel").on("click.formCancel", ".js-form-cancel", function () {
    let offcanvas = $(this).closest(".offcanvas")[0];
    if ($(this).closest(".jconfirm-box").length)
        JSManager.closeForm();
    else if (offcanvas)
        bootstrap.Offcanvas.getOrCreateInstance(offcanvas).hide();
    else
        JSManager.closeOffCanvas();
});

function ajaxFormContainer(form) {
    let $box = form && form.nodeType === 1 ? $(form).closest(".jconfirm-box") : $();
    return $box.length ? $box : "pdOffCanvas";
}

function ajaxBegin() {
    JSManager.blockUI(ajaxFormContainer(this));
}

function ajaxComplete(response) {
    JSManager.unblockUI(ajaxFormContainer(this));
}

function ajaxSuccess(response) {
    if (response.status === 2000) {
        if (response.child_action) {
            JSManager.reloadChildGrid();
        } else {
            JSManager.reloadGrid();
        }
        JSManager.closeOffCanvas();
        JSManager.closeForm();
        if (JSManager.hasValue(response.message))
            JSManager.showSuccess(response.message);
    } else if (response.status === 2001) {
        let errorHtml = "";
        $.each(response.errors || [], function (index, item) {
            let message = item && (item.Message || item.message);
            if (message && $.trim(message).length)
                errorHtml += "<li>" + message + "</li>";
        });

        if (errorHtml.length) {
            $(".validation-errors").html(
                '<div class="alert alert-danger alert-dismissible d-flex align-items-center alert-validation-summary" role="alert"><i class="bx bx-xs bx-alarm-exclamation me-2"></i>' +
                errorHtml +
                '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>'
            );
        } else if (JSManager.hasValue(response.message)) {
            JSManager.showError(response.message);
        }
    } else if (response.status === 2002) {
        JSManager.showError(response.message);
    } else {
        JSManager.closeOffCanvas();
        JSManager.closeForm();
        JSManager.showAppError(response.message);
    }
}

function ajaxFailure(response) {
    JSManager.closeOffCanvas();
    JSManager.closeForm();
    JSManager.showAppError(response.responseJSON.message);
}
