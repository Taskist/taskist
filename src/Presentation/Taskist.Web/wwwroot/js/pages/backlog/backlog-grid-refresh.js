var BacklogRefresh = (function () {
    "use strict";

    var refreshRequested = false;
    var busy = false;
    var lastLoaded = null;

    function grid() {
        return (typeof table !== "undefined" && table) ? table : null;
    }

    function updateTitle() {
        var $btn = $("#btnGridRefresh");
        var title = $btn.data("label");
        if (lastLoaded)
            title += " · " + String($btn.data("updated-label")).replace("{0}", moment(lastLoaded).fromNow());
        $btn.attr("title", title);
    }

    function setBusy(value) {
        busy = value;
        $("#btnGridRefresh").prop("disabled", value).find("i").toggleClass("fa-spin", value);
    }

    function refresh() {
        var dt = grid();
        if (!dt || busy) return;
        refreshRequested = true;
        setBusy(true);
        dt.ajax.reload(null, false);
    }

    function consume() {
        var requested = refreshRequested;
        refreshRequested = false;
        return requested;
    }

    $(document)
        .on("click", "#btnGridRefresh", function (e) {
            e.preventDefault();
            refresh();
        })
        .on("xhr.dt", "#BacklogGrid", function (e, settings, json) {
            setBusy(false);
            if (json) lastLoaded = new Date();
            updateTitle();
        })
        .on("mouseenter", "#btnGridRefresh", updateTitle);

    return { refresh: refresh, consume: consume };
})();
