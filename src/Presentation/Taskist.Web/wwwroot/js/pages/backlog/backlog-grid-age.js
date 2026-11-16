var BacklogAge = (function () {
    "use strict";

    var OPEN_GROUPS = [1, 2, 4];

    function isOpen(groupId) {
        return OPEN_GROUPS.indexOf(parseInt(groupId)) >= 0;
    }

    function ageDays(row) {
        return parseInt(row.AgeDays) || 0;
    }

    function color(days) {
        if (days >= 5) return "red";
        if (days >= 3) return "pink";
        if (days >= 2) return "orange";
        return "blue";
    }

    function badgeClass(days) {
        return "backlog-age backlog-age--" + color(days);
    }

    function escapeHtml(value) {
        return String(value == null ? "" : value)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
    }

    function plural(days) {
        return days + (days === 1 ? " day" : " days");
    }

    function sinceText(date) {
        return date ? " · since " + moment(date).format("DD-MMM-YYYY") : "";
    }

    function tooltipHtml(row, age, inStatus) {
        return '<div class="backlog-age-tip">' +
            "<div><strong>Open " + plural(age) + "</strong>" + sinceText(row.OpenSince) + "</div>" +
            "<div><strong>" + plural(inStatus) + "</strong> in " + escapeHtml(row.Status.Name || "current status") + sinceText(row.StatusSince) + "</div>" +
            "</div>";
    }

    function render(row) {
        if (!row || !row.Status || !isOpen(row.Status.GroupId)) {
            return '<span class="text-muted">&ndash;</span>';
        }

        var age = ageDays(row);
        var inStatus = parseInt(row.StatusDays) || 0;

        return '<span class="' + badgeClass(age) + '" data-bs-toggle="tooltip" data-bs-html="true" data-bs-placement="top"' +
            ' data-bs-title="' + escapeHtml(tooltipHtml(row, age, inStatus)) + '">' + age + "d" +
            '<span class="backlog-age-status">· ' + inStatus + "d</span></span>";
    }

    function bindTooltips(tableSelector) {
        var $table = $(tableSelector);
        var selector = ".backlog-age[data-bs-toggle='tooltip']";

        $table.on("preDraw.dt", function () {
            $table.find(selector).each(function () {
                var tip = bootstrap.Tooltip.getInstance(this);
                if (tip) tip.dispose();
            });
        });

        $table.on("init.dt", function () {
            $(".backlog-age-help").each(function () {
                bootstrap.Tooltip.getOrCreateInstance(this, { container: "body", customClass: "backlog-age-tooltip" });
            }).on("click", function (e) { e.stopPropagation(); });
        });

        $table.on("draw.dt", function () {
            $table.find(selector).each(function () {
                bootstrap.Tooltip.getOrCreateInstance(this, { delay: { show: 150, hide: 0 }, container: "body", customClass: "backlog-age-tooltip" });
            });
        });
    }

    function isColumnChecked($checkbox, savedNames) {
        var name = $checkbox.data("columnname");
        if ($checkbox.data("default-visible")) return savedNames.indexOf("!" + name) < 0;
        return savedNames.indexOf(name) >= 0;
    }

    function columnTokens($checkboxes) {
        var tokens = [];
        $checkboxes.each(function () {
            var $cb = $(this);
            var name = $cb.data("columnname");
            if ($cb.prop("checked")) tokens.push(name);
            else if ($cb.data("default-visible")) tokens.push("!" + name);
        });
        return tokens;
    }

    return {
        render: render,
        bindTooltips: bindTooltips,
        isColumnChecked: isColumnChecked,
        columnTokens: columnTokens
    };
})();
