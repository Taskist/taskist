/* Applies the user's appearance before first paint: "auto" follows the OS colour scheme (and keeps following it live).
   TaskistAppearance.set(mode, accent) switches without a reload; listeners get a "taskist:themechange" event. */
(function () {
    "use strict";

    var root = document.documentElement;
    var media = window.matchMedia ? window.matchMedia("(prefers-color-scheme: dark)") : null;

    function notify() {
        try {
            document.dispatchEvent(new CustomEvent("taskist:themechange", { detail: { theme: root.getAttribute("data-coreui-theme"), accent: root.getAttribute("data-app-accent") } }));
        } catch (e) { }
    }

    function apply() {
        var mode = root.getAttribute("data-app-theme-mode") || "light";
        var theme = mode === "auto" ? (media && media.matches ? "dark" : "light") : mode;
        if (root.getAttribute("data-coreui-theme") !== theme) {
            root.setAttribute("data-coreui-theme", theme);
            notify();
        }
    }

    apply();

    if (media) {
        if (media.addEventListener) media.addEventListener("change", apply);
        else if (media.addListener) media.addListener(apply);
    }

    window.TaskistAppearance = {
        theme: function () { return root.getAttribute("data-coreui-theme") || "light"; },
        mode: function () { return root.getAttribute("data-app-theme-mode") || "light"; },
        accent: function () { return root.getAttribute("data-app-accent") || "indigo"; },
        set: function (mode, accent) {
            if (mode) root.setAttribute("data-app-theme-mode", mode);
            if (accent) root.setAttribute("data-app-accent", accent);
            apply();
            notify();
        }
    };
})();
