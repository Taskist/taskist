/* Root cause & fix tab + closing checklist.
   Shows the tab for Bug / CR, marks which items the selected status requires,
   and keeps the tab badge and the status-panel checklist in sync. Never clears content. */
(function () {
    "use strict";

    var $cfg = $("#backlog-form-config");
    if (!$cfg.length) return;

    var cfg = {
        id: parseInt($cfg.data("id"), 10) || 0,
        mergeRequestId: parseInt($cfg.data("merge-request-id"), 10) || 0,
        bugIds: String($cfg.data("bug-task-type-ids") || "").split(",").filter(Boolean),
        crIds: String($cfg.data("cr-task-type-ids") || "").split(",").filter(Boolean),
        rules: $cfg.data("closing-rules") || [],
        labels: {
            requiredFor: $cfg.data("label-required-for") || "Required for {0}",
            requiredNow: $cfg.data("label-required-now") || "Required to move to {0}",
            optional: $cfg.data("label-optional") || "Optional",
            filled: $cfg.data("label-filled") || "Added",
            empty: $cfg.data("label-empty") || "Not added yet"
        }
    };

    var EDITORS = { rc: "RootCauseAnalysis", res: "ResolutionDetails" };

    function group() {
        var type = String($("#TaskTypeId").val() || "");
        if (cfg.bugIds.indexOf(type) !== -1) return "bug";
        if (cfg.crIds.indexOf(type) !== -1) return "cr";
        return null;
    }

    function selectedStatusRule() {
        var id = parseInt($('[name="StatusId"]').val(), 10) || 0;
        for (var i = 0; i < cfg.rules.length; i++) {
            if (cfg.rules[i].id === id) return cfg.rules[i];
        }
        return null;
    }

    function hasText(item) {
        var name = EDITORS[item];
        var ed = window.tinymce ? tinymce.get(name) : null;
        if (ed && ed.initialized) {
            if (ed.getContent({ format: "text" }).replace(/ /g, " ").trim().length) return true;
            return /<(img|table|video)\b/i.test(ed.getContent());
        }
        return $.trim($("#" + name).val() || "").length > 0;
    }

    function isFilled(item) {
        if (item === "mr") return cfg.mergeRequestId > 0;
        return hasText(item);
    }

    function requiredFor(item, g) {
        return cfg.rules.filter(function (r) { return r[g] && r[g][item]; }).map(function (r) { return r.name; });
    }

    function requiredNow(item, g) {
        var rule = selectedStatusRule();
        return !!(rule && rule[g] && rule[g][item]);
    }

    function fmt(template, value) {
        return String(template).replace("{0}", value);
    }

    function toggleTab(g) {
        var show = !!g;
        $("#rca-tab-item").toggleClass("d-none", !show);
        $("#rca").toggleClass("d-none", !show);
        if (!show && $("#rca-tab").hasClass("active"))
            $("#description-tab").trigger("click");
    }

    function renderSection(item, g) {
        var $s = $('.rcf-step[data-item="' + item + '"]');
        if (!$s.length) return;

        var filled = isFilled(item);
        var now = requiredNow(item, g);
        var later = requiredFor(item, g);
        var rule = selectedStatusRule();

        $s.toggleClass("rcf-step--done", filled)
            .toggleClass("rcf-step--alert", now && !filled);

        var $num = $s.find(".rcf-marker span");
        $num.html(filled ? '<i class="fas fa-check"></i>' : $num.data("step"));

        $s.find(".rcf-state")
            .attr("class", "rcf-state " + (filled ? "rcf-state--done" : "rcf-state--empty"))
            .html(filled
                ? '<i class="fas fa-circle-check me-1"></i>' + $("<span>").text(cfg.labels.filled).html()
                : '<i class="far fa-circle me-1"></i>' + $("<span>").text(cfg.labels.empty).html());

        var $req = $s.find(".rcf-req");
        if (now && !filled) {
            $req.removeClass("d-none rcf-req--later").addClass("rcf-req--now")
                .html('<i class="fas fa-triangle-exclamation me-1"></i>').append(document.createTextNode(fmt(cfg.labels.requiredNow, rule.name)));
        } else if (later.length) {
            $req.removeClass("d-none rcf-req--now").addClass("rcf-req--later")
                .html('<i class="fas fa-flag me-1"></i>').append(document.createTextNode(fmt(cfg.labels.requiredFor, later.join(", "))));
        } else {
            $req.removeClass("rcf-req--now rcf-req--later").addClass("d-none").empty();
        }
    }

    function renderBadge(g) {
        var $b = $("#rcfTabBadge");
        if (!g) { $b.attr("class", "rcf-tab-badge").empty(); return; }

        var count = (isFilled("rc") ? 1 : 0) + (isFilled("res") ? 1 : 0);
        var alert = (requiredNow("rc", g) && !isFilled("rc")) || (requiredNow("res", g) && !isFilled("res"));

        if (alert) $b.attr("class", "rcf-tab-badge rcf-tab-badge--alert").html('<i class="fas fa-exclamation"></i>');
        else if (count === 2) $b.attr("class", "rcf-tab-badge rcf-tab-badge--done").html('<i class="fas fa-check"></i>');
        else $b.attr("class", "rcf-tab-badge rcf-tab-badge--partial").text(count + "/2");
    }

    function renderChecklist(g) {
        var $list = $("#closingChecklist");
        if (!$list.length) return;

        $list.toggleClass("d-none", !g || cfg.id === 0);
        if (!g) return;

        var rule = selectedStatusRule();
        var ruleApplies = rule && rule[g] && (rule[g].rc || rule[g].res || rule[g].mr);
        $("#closingChecklistFor").text(ruleApplies ? "· " + rule.name : "");

        $list.find(".closing-chip").each(function () {
            var $chip = $(this);
            var item = $chip.data("item");
            var filled = isFilled(item);
            var now = requiredNow(item, g);
            var later = requiredFor(item, g);
            var state, icon, tip;

            if (filled) { state = "done"; icon = "fas fa-circle-check"; }
            else if (now) { state = "missing"; icon = "fas fa-circle-exclamation"; }
            else if (later.length) { state = "pending"; icon = "far fa-circle"; }
            else { state = "optional"; icon = "far fa-circle"; }

            tip = later.length ? fmt(cfg.labels.requiredFor, later.join(", ")) : cfg.labels.optional;

            $chip.attr("class", "closing-chip closing-chip--" + state).attr("title", tip);
            $chip.find(".closing-chip__icon").attr("class", "closing-chip__icon " + icon);
        });
    }

    function render() {
        var g = group();
        toggleTab(g);
        renderSection("rc", g);
        renderSection("res", g);
        renderBadge(g);
        renderChecklist(g);
    }

    function flash($el) {
        $el.removeClass("rcf-flash");
        void $el[0].offsetWidth;
        $el.addClass("rcf-flash");
    }

    $(document)
        .on("backlog:editor-input", function (e, name) {
            if (name === EDITORS.rc || name === EDITORS.res) render();
        })
        .on("change", '[name="StatusId"], #TaskTypeId', render)
        .on("click", "#closingChecklist .closing-chip", function () {
            var $chip = $(this);
            if ($chip.data("goto") === "code") {
                var $card = $("#taskCodeCard");
                if ($card.length) {
                    $card[0].scrollIntoView({ behavior: "smooth", block: "center" });
                    flash($card);
                }
                return;
            }

            $("#rca-tab").trigger("click");
            var $section = $('.rcf-step[data-item="' + $chip.data("item") + '"]');
            setTimeout(function () {
                if (!$section.length) return;
                $section[0].scrollIntoView({ behavior: "smooth", block: "center" });
                flash($section.find(".rcf-card"));
                var ed = window.tinymce ? tinymce.get(EDITORS[$chip.data("item")]) : null;
                if (ed) ed.focus();
            }, 180);
        });

    $(function () {
        render();
    });
})();
