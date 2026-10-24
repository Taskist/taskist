(function () {
    "use strict";

    if (!("serviceWorker" in navigator) || !window.isSecureContext)
        return;

    window.addEventListener("load", function () {
        navigator.serviceWorker.register("/js/sw.js", { scope: "/" }).catch(function () { });
    });

    var deferredPrompt = null;

    window.addEventListener("beforeinstallprompt", function (e) {
        e.preventDefault();
        deferredPrompt = e;
        document.querySelectorAll("[data-pwa-install]").forEach(function (btn) {
            btn.classList.remove("d-none");
        });
    });

    document.addEventListener("click", function (e) {
        var trigger = e.target.closest("[data-pwa-install]");
        if (!trigger || !deferredPrompt) return;
        e.preventDefault();
        deferredPrompt.prompt();
        deferredPrompt.userChoice.finally(function () {
            deferredPrompt = null;
            trigger.classList.add("d-none");
        });
    });

    window.addEventListener("appinstalled", function () {
        deferredPrompt = null;
        document.querySelectorAll("[data-pwa-install]").forEach(function (btn) {
            btn.classList.add("d-none");
        });
    });
})();
