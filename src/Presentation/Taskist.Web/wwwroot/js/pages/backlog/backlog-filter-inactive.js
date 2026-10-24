var BacklogInactiveUsers = (function () {
    "use strict";

    var cache = null;
    var shown = false;
    var fieldMap = { AssigneeId: "developers", TesterId: "testers", CreatedById: "creators" };

    function fetch() {
        if (cache) return cache;
        var url = $("#lnkShowInactive").data("url");
        if (!url) return null;
        var resp = JSManager.ajaxGetWithResponse(url, {});
        if (resp) cache = resp;
        return cache;
    }

    function renderLink() {
        var $link = $("#lnkShowInactive");
        $link.find(".inactive-label").text(shown ? $link.data("hide-text") : $link.data("show-text"));
        $link.toggleClass("text-danger", shown);
    }

    function show() {
        if (shown) return;
        var data = fetch();
        if (!data) return;

        Object.keys(fieldMap).forEach(function (id) {
            var items = data[fieldMap[id]] || [];
            if (!BacklogFilterSelect.isReady(id) || !items.length) return;

            var current = BacklogFilterSelect.get(id);
            items.forEach(function (item) {
                BacklogFilterSelect.addOption(id, item.value, item.text);
            });
            BacklogFilterSelect.set(id, current);
        });

        shown = true;
        renderLink();
    }

    function hide() {
        if (!shown || !cache) return;

        Object.keys(fieldMap).forEach(function (id) {
            var items = cache[fieldMap[id]] || [];
            if (!BacklogFilterSelect.isReady(id) || !items.length) return;

            BacklogFilterSelect.removeOptions(id, items.map(function (i) { return i.value; }));
        });

        shown = false;
        renderLink();

        if (typeof countSelectedMultiSelects === "function") countSelectedMultiSelects();
    }

    function ensureFor(values) {
        if (shown || !values) return;
        var missing = Object.keys(values).some(function (id) {
            var list = [].concat(values[id] || []).filter(function (v) { return v !== null && v !== ""; });
            return BacklogFilterSelect.isReady(id) && list.some(function (v) { return !BacklogFilterSelect.hasOption(id, v); });
        });
        if (missing) show();
    }

    $(document).on("click", "#lnkShowInactive", function (e) {
        e.preventDefault();
        if (shown) hide(); else show();
    });

    return { show: show, hide: hide, ensureFor: ensureFor };
})();
