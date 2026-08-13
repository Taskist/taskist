(function () {
    "use strict";

    $(function () {
        // greeting that matches the time of day
        var hour = new Date().getHours();
        var greeting = hour < 12 ? "Good morning" : (hour < 17 ? "Good afternoon" : "Good evening");
        $("#loginGreeting").text(greeting);

        var $pwd = $("#Password");
        var $toggle = $("#togglePassword");
        var $caps = $("#capsHint");

        // show / hide password
        $toggle.on("click", function () {
            var show = $pwd.attr("type") === "password";
            $pwd.attr("type", show ? "text" : "password");
            $toggle.find("i").attr("class", show ? "fas fa-eye-slash" : "fas fa-eye");
            $toggle.attr("aria-label", show ? "Hide password" : "Show password");
            $pwd.trigger("focus");
        });

        // caps-lock hint
        $pwd.on("keyup", function (e) {
            var on = e.getModifierState && e.getModifierState("CapsLock");
            $caps.css("display", on ? "flex" : "none");
        });
    });
})();
