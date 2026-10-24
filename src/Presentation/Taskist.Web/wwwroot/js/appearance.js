/* Appearance switching: My profile tiles/swatches and the header quick toggle. Applies instantly via
   TaskistAppearance (appearance-boot.js) and saves to the user's profile in the background. */
(function () {
    "use strict";

    if (!window.TaskistAppearance) return;

    function save(url, data) {
        $.ajax({
            type: "POST",
            url: url,
            data: JSManager.addAntiForgeryToken(data),
            success: function (r) {
                if (!r || r.status !== 2000) JSManager.showError((r && r.message) || "Could not save the appearance.");
            },
            error: function () { JSManager.showError("Could not save the appearance."); }
        });
    }

    function markSelected($items, attr, value) {
        $items.each(function () {
            var on = String($(this).data(attr)) === value;
            $(this).toggleClass("is-selected", on).attr("aria-checked", on ? "true" : "false");
        });
    }

    function refreshToggle() {
        var dark = TaskistAppearance.theme() === "dark";
        $(".js-theme-toggle").each(function () {
            var $t = $(this);
            $t.find(".js-theme-toggle-text").text(dark ? $t.data("light-text") : $t.data("dark-text"));
            $t.find(".js-theme-toggle-icon").toggleClass("fa-moon", !dark).toggleClass("fa-sun", dark);
        });
    }

    $(document).on("click", ".js-appearance-mode", function () {
        var mode = String($(this).data("mode"));
        TaskistAppearance.set(mode, null);
        markSelected($(".js-appearance-mode"), "mode", mode);
        save($(this).closest(".appearance-card").data("url"), { mode: mode });
    });

    $(document).on("click", ".js-appearance-accent", function () {
        var accent = String($(this).data("accent"));
        TaskistAppearance.set(null, accent);
        markSelected($(".js-appearance-accent"), "accent", accent);
        save($(this).closest(".appearance-card").data("url"), { accent: accent });
    });

    $(document).on("click", ".js-theme-toggle", function (e) {
        e.preventDefault();
        var mode = TaskistAppearance.theme() === "dark" ? "light" : "dark";
        TaskistAppearance.set(mode, null);
        markSelected($(".js-appearance-mode"), "mode", mode);
        save($(this).data("url"), { mode: mode });
    });

    var chartLight = null;

    function libraryDefaults() {
        var dark = TaskistAppearance.theme() === "dark";
        if (window.tinymce && typeof tinymce.overrideDefaults === "function") {
            tinymce.overrideDefaults({ license_key: "gpl", skin: dark ? "oxide-dark" : "oxide", content_css: dark ? "dark" : "default" });
        }
        if (window.Chart && Chart.defaults) {
            if (!chartLight) chartLight = { color: Chart.defaults.color, borderColor: Chart.defaults.borderColor };
            Chart.defaults.color = dark ? "rgba(255, 255, 255, 0.75)" : chartLight.color;
            Chart.defaults.borderColor = dark ? "rgba(255, 255, 255, 0.1)" : chartLight.borderColor;
        }
    }

    libraryDefaults();
    document.addEventListener("taskist:themechange", function () { refreshToggle(); libraryDefaults(); });
    $(refreshToggle);
})();
