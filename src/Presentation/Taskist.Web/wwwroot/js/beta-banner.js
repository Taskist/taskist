(function () {
    "use strict";

    $(function () {
        var el = document.getElementById("betaBanner");
        if (!el) return;

        var report = document.getElementById("betaReport");
        if (report) {
            var version = el.dataset.version || "";
            var subject = "Taskist beta v" + version + " — issue";
            var body = "Describe what happened:\n\n\n---\nVersion: " + version + "\nPage: " + window.location.href;
            report.setAttribute("href", "mailto:" + (el.dataset.support || "") +
                "?subject=" + encodeURIComponent(subject) + "&body=" + encodeURIComponent(body));
        }
    });
})();
