/* Backlog Comments (chat) partial - relative times, auto-grow input, Enter-to-send.
   Loaded globally; the AJAX-injected partial calls BacklogComments.init({ backlogId, createUrl, photoUrl }). */

window.BacklogComments = (function () {
    "use strict";

    return {
        init: function (cfg) {
            cfg = cfg || {};
            var $list = $('#comments-list');
            var $input = $('#tbComment');
            var $btn = $('#btnAddComment');
            var photoBase = cfg.photoUrl;

            function refreshTimes() {
                $('.comment-time').each(function () {
                    var t = $(this).data('time');
                    if (t && window.moment) $(this).text(moment(t).fromNow());
                });
            }

            function scrollToBottom() {
                var c = document.getElementById('comments');
                if (c) c.scrollTop = c.scrollHeight;
            }

            function autoGrow(el) {
                el.style.height = 'auto';
                el.style.height = Math.min(el.scrollHeight, 120) + 'px';
            }

            function addComment() {
                var comment = $input.val().trim();
                if (!comment) return;

                JSManager.ajaxPost(cfg.createUrl, { id: cfg.backlogId, comment: comment }, 'comment-box', 'Processing...', function (response) {
                    if (!response || !response.success) {
                        JSManager.showError(response && response.message ? response.message : 'Unable to add comment.');
                        return;
                    }

                    // my own new comment renders right-aligned, no avatar
                    var $item = $('<div class="comment-item comment-mine">'
                        + '<div class="comment-body">'
                        + '<div class="comment-meta"><span class="comment-author">You</span><span class="comment-time">just now</span></div>'
                        + '<div class="comment-text"></div>'
                        + '</div></div>');
                    $item.find('.comment-text').text(comment);
                    $list.append($item);
                    $('#comments-empty').addClass('d-none');
                    $input.val('').trigger('input').focus();
                    scrollToBottom();
                });
            }

            refreshTimes();
            scrollToBottom();
            $input.focus();

            $input.off('.bkchat').on('input.bkchat', function () {
                $btn.prop('disabled', $(this).val().trim().length === 0);
                autoGrow(this);
            });

            $input.on('keydown.bkchat', function (e) {
                if (e.which === 13 && !e.shiftKey) {
                    e.preventDefault();
                    addComment();
                }
            });

            $btn.off('.bkchat').on('click.bkchat', addComment);
        }
    };
})();
