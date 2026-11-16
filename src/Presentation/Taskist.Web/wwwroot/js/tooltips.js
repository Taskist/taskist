// App-wide Bootstrap tooltips: any [data-bs-toggle="tooltip"] gets a styled
// tooltip on first hover/focus, including content loaded later (offcanvas,
// jquery-confirm modals, DataTables rows). Pages that create their own
// instances with custom options keep them (getInstance is checked first).
(function () {
    if (typeof bootstrap === "undefined" || !bootstrap.Tooltip) return;

    var SELECTOR = '[data-bs-toggle="tooltip"]';
    var shown = new Set();
    var observer = null;

    function lazyInit(e) {
        var el = e.target && e.target.closest ? e.target.closest(SELECTOR) : null;
        if (!el || bootstrap.Tooltip.getInstance(el)) return;

        bootstrap.Tooltip.getOrCreateInstance(el, { container: "body" }).show();
    }

    // A tooltip whose trigger is removed while open (offcanvas content swapped,
    // modal closed, grid redrawn) would stay on screen; dispose those.
    function disposeOrphans() {
        shown.forEach(function (el) {
            if (!el.isConnected) {
                var tip = bootstrap.Tooltip.getInstance(el);
                if (tip) tip.dispose();
                shown.delete(el);
            }
        });
        if (!shown.size && observer) { observer.disconnect(); observer = null; }
    }

    function hideAll() {
        shown.forEach(function (el) {
            var tip = bootstrap.Tooltip.getInstance(el);
            if (tip) tip.hide();
        });
    }

    document.addEventListener("mouseover", lazyInit);
    document.addEventListener("focusin", lazyInit);

    document.addEventListener("shown.bs.tooltip", function (e) {
        shown.add(e.target);
        if (!observer) {
            observer = new MutationObserver(disposeOrphans);
            observer.observe(document.body, { childList: true, subtree: true });
        }
    });
    document.addEventListener("hidden.bs.tooltip", function (e) {
        shown.delete(e.target);
        if (!shown.size && observer) { observer.disconnect(); observer = null; }
    });

    document.addEventListener("hide.bs.offcanvas", hideAll);
    document.addEventListener("hide.bs.modal", hideAll);
})();
