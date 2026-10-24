(function () {
    "use strict";

    var $form = $("#reportSettingsForm");
    if (!$form.length) return;

    function serializeVisual() {
        var out = {};
        $(".report-layout").each(function () {
            out[$(this).data("report")] = ReportColumnEditor.serialize($(this));
        });
        return out;
    }

    function applyJsonToVisual(obj) {
        Object.keys(obj || {}).forEach(function (key) {
            var $ul = $('.report-layout[data-report="' + key + '"] .report-columns');
            if (!$ul.length) return;
            (obj[key] || []).forEach(function (col) {
                var $row = $ul.find('.report-col-row[data-key="' + col.Key + '"]');
                if (!$row.length) return;
                $row.find(".report-col-title").val(col.Title);
                $row.find(".report-col-included").prop("checked", !!col.Included);
                $ul.append($row);
            });
        });
    }

    $(function () {
        ReportColumnEditor.initSortable($form);

        $("#reportSelect").on("change", function () {
            var key = $(this).val();
            $(".report-layout").addClass("d-none");
            $('.report-layout[data-report="' + key + '"]').removeClass("d-none");
        });

        $("#reportJsonToggle").on("change", function () {
            if (this.checked) {
                $("#reportLayoutsJsonText").val(JSON.stringify(serializeVisual(), null, 2));
                $("#reportBuilders").addClass("d-none");
                $("#reportJsonEditor").removeClass("d-none");
            } else {
                var parsed;
                try {
                    parsed = JSON.parse($("#reportLayoutsJsonText").val() || "{}");
                } catch (e) {
                    JSManager.showError("Invalid JSON — fix it before switching back.");
                    $(this).prop("checked", true);
                    return;
                }
                applyJsonToVisual(parsed);
                $("#reportJsonEditor").addClass("d-none");
                $("#reportBuilders").removeClass("d-none");
            }
        });

        $form.on("submit", function (e) {
            var json;
            if ($("#reportJsonToggle").is(":checked")) {
                try {
                    json = JSON.stringify(JSON.parse($("#reportLayoutsJsonText").val() || "{}"));
                } catch (err) {
                    e.preventDefault();
                    JSManager.showError("Invalid JSON — cannot save.");
                    return false;
                }
            } else {
                json = JSON.stringify(serializeVisual());
            }
            $("#reportLayoutsJson").val(json);
        });
    });
})();
