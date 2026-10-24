var BacklogComments = (function () {
    "use strict";

    function refreshTimes($root) {
        $root.find(".comment-time[data-time]").each(function () {
            var m = moment($(this).data("time"));
            $(this).text(m.fromNow()).attr("title", m.format("DD-MMM-YYYY HH:mm"));
        });
    }

    function autoGrow(el) {
        el.style.height = "auto";
        el.style.height = Math.min(el.scrollHeight, 120) + "px";
    }

    function setup($root) {
        var $scroll = $root.find(".comments-container");
        var $list = $root.find(".comments-list");
        var $input = $root.find(".comment-input");
        var $btn = $root.find(".comment-send");
        var inDrawer = $root.closest(".offcanvas").length > 0;

        function scrollToBottom() {
            if ($scroll.length) $scroll[0].scrollTop = $scroll[0].scrollHeight;
        }

        function addComment() {
            var comment = $input.val().trim();
            if (!comment) return;

            var dto = { id: $root.data("task-id"), comment: comment, postToIssue: $root.find(".comment-post-to-issue").is(":checked") };
            JSManager.ajaxPost($root.data("create-url"), dto, "comment-box", "Processing...", function (response) {
                var $item = $('<div class="comment-item">'
                    + '<div class="comment-avatar"></div>'
                    + '<div class="comment-body">'
                    + '<div class="comment-meta"><span class="comment-author"></span><span class="comment-time"></span></div>'
                    + '<div class="comment-text"></div>'
                    + "</div></div>");
                $item.find(".comment-avatar").html($('<img alt="">').attr("src", $root.data("photo-url") + "?userId=" + response.commentbyid));
                $item.find(".comment-author").text(response.commentby);
                $item.find(".comment-time").text("just now");
                $item.find(".comment-text").text(comment);
                $list.append($item);
                $root.find(".comments-empty").addClass("d-none");
                $input.val("").trigger("input").focus();
                scrollToBottom();
                $root.trigger("comments:added", [$list.children(".comment-item").length]);
            });
        }

        refreshTimes($root);
        scrollToBottom();
        if (inDrawer) $input.focus();

        $input.on("input", function () {
            $btn.prop("disabled", $(this).val().trim().length === 0);
            autoGrow(this);
        });

        $input.on("keydown", function (e) {
            if (e.which === 13 && !e.shiftKey) {
                e.preventDefault();
                addComment();
            }
        });

        $btn.on("click", addComment);
    }

    function init(root) {
        $(root).find(".comments-root:not([data-ready])").each(function () {
            setup($(this).attr("data-ready", "true"));
        });
    }

    if (window.MutationObserver) {
        new MutationObserver(function () { init(document); })
            .observe(document.body, { childList: true, subtree: true });
    }
    $(function () { init(document); });

    return { init: init };
})();
