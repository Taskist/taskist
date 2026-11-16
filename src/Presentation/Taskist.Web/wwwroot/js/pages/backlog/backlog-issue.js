/* Issue card on the task page: shows the linked GitHub/GitLab issue and lets editors publish, sync or stop syncing. */
(function () {
    "use strict";

    var $card = $("#taskIssueCard");
    if (!$card.length)
        return;

    function load() {
        $.get($card.data("url")).done(function (html) {
            $card.find(".task-issue-body").html(html);
            $card.find(".task-code-time").each(function () {
                var time = $(this).data("time");
                if (time)
                    $(this).text(new Date(time).toLocaleString());
            });
        }).fail(function () {
            $card.find(".task-issue-body").html("<span class='text-muted small'>-</span>");
        });
    }

    function post(url, data, $btn) {
        $btn.prop("disabled", true);
        $.ajax({
            url: url,
            type: "POST",
            data: JSManager.addAntiForgeryToken($.extend({ id: $card.data("task-id") }, data || {})),
            success: function (r) {
                if (r && r.status === 2000)
                    toastr.success(r.message);
                else
                    JSManager.showSwal((r && r.message) || "Request failed.", "error");
                load();
            },
            error: function (xhr) {
                JSManager.showSwal("Request failed (" + xhr.status + ").", "error");
                $btn.prop("disabled", false);
            }
        });
    }

    $card
        .on("click", ".task-issue-sync", function () {
            post($card.data("sync-url"), {}, $(this));
        })
        .on("click", ".task-issue-unlink", function () {
            var $btn = $(this);
            $.confirm({
                title: false,
                content: $card.data("label-unlink-confirm"),
                buttons: {
                    confirm: { btnClass: "btn-danger", action: function () { post($card.data("unlink-url"), { linkId: $btn.data("link-id") }, $btn); } },
                    cancel: function () { }
                }
            });
        })
        .on("click", ".task-issue-publish-btn", function () {
            var $btn = $(this);
            var message = $("<div>").text($card.data("label-publish-confirm").replace("{0}", $btn.data("name"))).html();
            if (String($btn.data("public")) === "1")
                message += "<div class='alert alert-warning small mt-2 mb-0'>" + $("<div>").text($card.data("label-public-warning")).html() + "</div>";
            $.confirm({
                title: false,
                content: message,
                buttons: {
                    confirm: { btnClass: "btn-primary", action: function () { post($card.data("publish-url"), { repositoryId: $btn.data("repository-id") }, $btn); } },
                    cancel: function () { }
                }
            });
        });

    load();
})();
