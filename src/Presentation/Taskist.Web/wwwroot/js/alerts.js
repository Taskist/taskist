(function () {
    "use strict";

    if (typeof signalR === "undefined") return;
    if (!document.getElementById("alertBell")) return;

    function toggle(el, hidden) {
        if (el) el.classList.toggle("d-none", hidden);
    }

    function setText(id, value) {
        var el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function apply(data) {
        var pending = (data && data.pendingApprovals) || 0;
        var overdue = (data && data.overdueTasks) || 0;
        var unread = (data && data.unreadNotifications) || 0;
        var total = pending + overdue + unread;

        setText("notifUnreadCount", unread);
        toggle(document.getElementById("notifUnreadCount"), unread === 0);
        if (window.WpNotifications) window.WpNotifications.refresh();

        setText("alertBadge", total);
        toggle(document.getElementById("alertBadge"), total === 0);

        setText("alertHeaderCount", total);
        toggle(document.getElementById("alertHeaderCount"), total === 0);

        toggle(document.getElementById("alertEmpty"), total !== 0);

        setText("alertApprovalsCount", pending);
        toggle(document.getElementById("alertApprovals"), pending === 0);

        setText("alertOverdueCount", overdue);
        toggle(document.getElementById("alertOverdue"), overdue === 0);
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/alerts")
        .withAutomaticReconnect()
        .build();

    connection.on("alerts", apply);
    connection.start().catch(function () { });
})();
