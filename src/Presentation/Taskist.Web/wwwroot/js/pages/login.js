(function () {
    "use strict";

    $(function () {
        try { localStorage.removeItem("backlog-filters"); } catch (e) { }

        var hour = new Date().getHours();
        var greeting = hour < 12 ? "Good morning" : (hour < 17 ? "Good afternoon" : "Good evening");
        $("#loginGreeting").text(greeting);

        var $pwd = $("#Password");
        var $toggle = $("#togglePassword");
        var $caps = $("#capsHint");

        $toggle.on("click", function () {
            var show = $pwd.attr("type") === "password";
            $pwd.attr("type", show ? "text" : "password");
            $toggle.find("i").attr("class", show ? "fas fa-eye-slash" : "fas fa-eye");
            $toggle.attr("aria-label", show ? "Hide password" : "Show password");
            $pwd.trigger("focus");
        });

        $pwd.on("keyup", function (e) {
            var on = e.getModifierState && e.getModifierState("CapsLock");
            $caps.css("display", on ? "flex" : "none");
        });

        var $form = $("#loginForm");
        var btn = $form.find(".ladda-button")[0];

        if (btn && window.Ladda) {
            var ladda = Ladda.create(btn);

            $form.on("submit", function () {
                // Respect jQuery unobtrusive validation: don't spin if the form is invalid.
                if (typeof $form.valid === "function" && !$form.valid()) {
                    return;
                }
                ladda.start();
            });
        }
    });
})();
