/* Git connection form: show only the fields and hints that apply to the selected provider and service authentication. */
(function () {
    "use strict";

    var GITHUB = "2";

    function init() {
        var $form = $(this);
        if ($form.data("gitConnectionFormReady")) return;
        $form.data("gitConnectionFormReady", true);

        var $provider = $form.find(".js-git-provider");
        var $auth = $form.find(".js-git-auth-mode");
        var $baseUrl = $form.find(".js-git-base-url");
        var $appOption = $auth.find("option[value='" + $auth.data("github-app") + "']");

        var toggle = function () {
            var provider = String($provider.val());
            var isGitHub = provider === GITHUB;

            if (!isGitHub && $appOption.prop("selected"))
                $auth.val(String($auth.data("access-token")));
            $appOption.prop("disabled", !isGitHub).prop("hidden", !isGitHub);

            var auth = String($auth.val());
            $form.find("[data-git-provider]").each(function () {
                $(this).toggle(String($(this).data("git-provider")) === provider);
            });
            $form.find("[data-git-auth]").each(function () {
                $(this).toggle(String($(this).data("git-auth")) === auth);
            });

            $baseUrl.attr("placeholder", $baseUrl.data(isGitHub ? "github-placeholder" : "gitlab-placeholder"));
        };

        $provider.on("change", toggle);
        $auth.on("change", toggle);
        toggle();
    }

    $(document).off("partial:ready.gitConnectionForm").on("partial:ready.gitConnectionForm", function (e) {
        $(e.target).find(".git-connection-form").each(init);
    });
    setTimeout(function () { $(".git-connection-form").each(init); }, 0);
})();
