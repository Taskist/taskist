/* Notification channel form: the signing secret applies only to generic JSON webhooks. */
(function () {
    "use strict";

    var GENERIC_JSON = "7";

    function init() {
        var $form = $(this);
        if ($form.data("channelFormReady")) return;
        $form.data("channelFormReady", true);

        var $type = $form.find(".js-channel-type");
        var toggle = function () { $form.find(".js-signing-secret").toggle($type.val() === GENERIC_JSON); };
        $type.on("change", toggle);
        toggle();
    }

    $(document).off("partial:ready.channelForm").on("partial:ready.channelForm", function (e) {
        $(e.target).find(".notification-channel-form").each(init);
    });
    setTimeout(function () { $(".notification-channel-form").each(init); }, 0);
})();
