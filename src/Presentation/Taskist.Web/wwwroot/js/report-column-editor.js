var ReportColumnEditor = (function () {
    "use strict";

    function initSortable($scope) {
        if (!$.fn.sortable) return;
        $scope.find(".report-columns").sortable({
            handle: ".report-col-handle",
            placeholder: "report-col-placeholder",
            axis: "y",
            forcePlaceholderSize: true
        });
    }

    function serialize($layout) {
        var cols = [];
        $layout.find(".report-col-row").each(function () {
            var $row = $(this);
            var col = {
                Key: $row.data("key"),
                Included: $row.find(".report-col-included").is(":checked")
            };
            var $title = $row.find(".report-col-title");
            if ($title.length) col.Title = ($title.val() || "").trim();
            cols.push(col);
        });
        return cols;
    }

    function post(url, data, done) {
        JSManager.ajaxPost(url, data, undefined, undefined, function (r) {
            if (r && r.status === 2000) {
                if (r.message) JSManager.showSuccess(r.message);
                done();
            } else {
                JSManager.showError((r && r.message) || "Could not save the export columns.");
            }
        });
    }

    function open($trigger) {
        var d = $trigger.data();
        var key = d.reportLayout;

        $.confirm({
            title: d.title,
            icon: "fas fa-table-columns",
            columnClass: "medium jc-form-scroll",
            closeIcon: true,
            content: "URL:" + d.editorUrl + "?key=" + encodeURIComponent(key),
            onContentReady: function () {
                var $layout = this.$content.find(".report-layout-editor");
                initSortable(this.$content);
                if ($layout.data("custom") !== true) this.buttons.reset.hide();
            },
            buttons: {
                save: {
                    text: d.saveText,
                    btnClass: "btn-blue",
                    action: function () {
                        var dialog = this;
                        var cols = serialize(dialog.$content.find(".report-layout-editor"));
                        post(d.saveUrl, { key: key, columns: JSON.stringify(cols) }, function () { dialog.close(); });
                        return false;
                    }
                },
                reset: {
                    text: d.resetText,
                    btnClass: "btn-default",
                    action: function () {
                        var dialog = this;
                        post(d.resetUrl, { key: key }, function () { dialog.close(); });
                        return false;
                    }
                },
                cancel: {
                    text: d.cancelText
                }
            }
        });
    }

    $(document).on("click", "[data-report-layout]", function (e) {
        e.preventDefault();
        open($(this));
    });

    return {
        initSortable: initSortable,
        serialize: serialize
    };
})();
