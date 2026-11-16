/* Two-factor sign-in step: focus the code field, and switch between an authenticator code and a recovery code. */
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var input = document.getElementById("Code");
        if (input) input.focus();
    });

    document.addEventListener("click", function (e) {
        var toggle = e.target.closest(".js-tf-toggle");
        if (!toggle) return;
        e.preventDefault();

        var mode = document.querySelector(".js-tf-mode");
        var recovery = String(mode.value).toLowerCase() !== "true";
        mode.value = recovery ? "true" : "false";

        document.querySelectorAll(".js-tf-intro-code").forEach(function (el) { el.hidden = recovery; });
        document.querySelectorAll(".js-tf-intro-recovery").forEach(function (el) { el.hidden = !recovery; });

        var input = document.getElementById("Code");
        input.value = "";
        input.setAttribute("inputmode", recovery ? "text" : "numeric");
        input.setAttribute("placeholder", recovery ? "xxxxx-xxxxx" : "123456");
        input.focus();
    });
})();
