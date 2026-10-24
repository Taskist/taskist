(function () {
    "use strict";

    var $list = $("#notifList");
    if (!$list.length) return;

    var loaded = false;

    function load() {
        $.get($list.data("url"))
            .done(function (html) { $list.html(html); loaded = true; })
            .fail(function () { $list.html(""); });
    }

    function isOpen() {
        return $("#alertBell .dropdown-menu").hasClass("show");
    }

    window.WpNotifications = {
        refresh: function () { if (loaded || isOpen()) load(); }
    };

    var toggleButton = document.querySelector("#alertBell .header-bell");
    if (toggleButton)
        toggleButton.addEventListener("click", function () { if (!isOpen()) load(); });

    $list.on("click", ".header-notif__item", function (e) {
        var $item = $(this);
        var href = $item.attr("href");
        var external = $item.attr("target") === "_blank";
        if (!external) e.preventDefault();

        $item.removeClass("is-unread");
        $.ajax({
            type: "POST",
            url: $list.data("read-url"),
            data: JSManager.addAntiForgeryToken({ id: $item.data("id") })
        }).always(function () {
            if (!external && href && href !== "#") window.location.href = href;
        });
    });

    $(document).on("click", "#alertBell .header-notif__readall", function () {
        $.ajax({
            type: "POST",
            url: $list.data("read-all-url"),
            data: JSManager.addAntiForgeryToken({})
        }).done(function () {
            $list.find(".header-notif__item").removeClass("is-unread");
        });
    });
})();
