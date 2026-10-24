var BacklogActivity = (function () {
    "use strict";

    var $card;
    var loaded = {};
    var panes = { comments: "activityComments", history: "activityHistory" };

    function updateCount() {
        var n = $("#" + panes.comments).find(".comment-item").length;
        $card.find(".activity-count").text(n).toggleClass("d-none", n === 0);
    }

    function load(which, force) {
        if (!$card || !$card.length || (loaded[which] && !force)) return;
        loaded[which] = true;
        JSManager.renderPartial($card.data(which + "-url"), {}, panes[which], function () {
            if (which === "comments") updateCount();
        });
    }

    function show(which) {
        if (!$card || !$card.length) return;
        var tab = $card.find('[data-activity="' + which + '"]')[0];
        if (tab) bootstrap.Tab.getOrCreateInstance(tab).show();
        load(which);
        $card[0].scrollIntoView({ behavior: "smooth", block: "start" });
        if (which === "comments") setTimeout(function () { $("#" + panes.comments).find(".comment-input").trigger("focus"); }, 350);
    }

    $(function () {
        $card = $("#taskActivityCard");
        if (!$card.length) return;

        $card.on("shown.bs.tab", "[data-activity]", function () { load($(this).data("activity")); });
        $card.on("comments:added", updateCount);

        $(document).on("click", "[data-activity-open]", function (e) {
            e.preventDefault();
            show($(this).data("activity-open"));
        });

        $(document).on("backlog:saved", function () {
            loaded.history = false;
            if ($("#" + panes.history).hasClass("active")) load("history", true);
        });

        load("comments");
    });

    return { show: show };
})();
