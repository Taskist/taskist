(function () {
    "use strict";

    function acknowledge(id, done) {
        $.ajax({
            url: "/AnnouncementFeed/Acknowledge?id=" + id,
            type: "POST",
            data: JSManager && JSManager.addAntiForgeryToken ? JSManager.addAntiForgeryToken({}) : {}
        }).always(function () { if (done) done(); });
    }

    // ---- Sticky banners: session-dismiss (returns next login) ----
    function isDismissed(id) {
        try { return sessionStorage.getItem("annDismissed_" + id) === "1"; } catch (e) { return false; }
    }

    function initStickyDismiss() {
        // Banners render hidden; reveal only those not dismissed this session.
        // (Prevents the dismissed-then-flash when the server re-renders on navigation.)
        $(".ann-banner").each(function () {
            var $b = $(this);
            if (!isDismissed($b.attr("data-ann-id")))
                $b.removeClass("ann-banner--pending");
        });
        $(document).off("click.anndismiss").on("click.anndismiss", "[data-ann-dismiss]", function () {
            var id = $(this).attr("data-ann-dismiss");
            try { sessionStorage.setItem("annDismissed_" + id, "1"); } catch (e) { }
            $(this).closest(".ann-banner").remove();
        });
    }

    // ---- Standard banners: explicit Acknowledge (permanent) ----
    function initAcknowledge() {
        $(document).off("click.annack").on("click.annack", "[data-ann-ack]", function () {
            var $btn = $(this);
            var id = $btn.attr("data-ann-ack");
            $btn.prop("disabled", true);
            acknowledge(id, function () { $btn.closest(".ann-banner").remove(); });
        });
    }

    // ---- One-time modal ----
    function readData() {
        var el = document.getElementById("annData");
        if (!el) return [];
        try { return JSON.parse(el.textContent) || []; } catch (e) { return []; }
    }

    function showOneTime(items) {
        if (!items.length || typeof bootstrap === "undefined") return;
        var modalEl = document.getElementById("annModal");
        if (!modalEl) return;

        // Modal markup is rendered inside the sticky header (its own stacking
        // context), which traps it under the body-level backdrop. Re-parent to
        // <body> so it layers correctly and is clickable.
        if (modalEl.parentElement !== document.body)
            document.body.appendChild(modalEl);

        var modal = new bootstrap.Modal(modalEl);
        var queue = items.slice();

        var SEV = { 0: "ann-modal--info", 1: "ann-modal--success", 2: "ann-modal--warning", 3: "ann-modal--critical" };
        var SEV_ICON = { 0: "fas fa-circle-info", 1: "fas fa-circle-check", 2: "fas fa-triangle-exclamation", 3: "fas fa-circle-exclamation" };

        function next() {
            if (!queue.length) return;
            var a = queue.shift();
            document.getElementById("annModalTitle").textContent = a.title;
            document.getElementById("annModalBody").innerHTML = a.body;
            var head = document.getElementById("annModalHead");
            head.className = "ann-modal__head " + (SEV[a.severity] || SEV[0]);
            document.getElementById("annModalIcon").className = SEV_ICON[a.severity] || SEV_ICON[0];
            $("#annModalAck").off("click").on("click", function () {
                acknowledge(a.id, function () { modal.hide(); });
            });
            modalEl.addEventListener("hidden.bs.modal", function handler() {
                modalEl.removeEventListener("hidden.bs.modal", handler);
                next();
            });
            modal.show();
        }
        next();
    }

    $(function () {
        initStickyDismiss();
        initAcknowledge();
        showOneTime(readData());
    });
})();
