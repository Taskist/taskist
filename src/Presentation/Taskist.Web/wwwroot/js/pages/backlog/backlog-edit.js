/* Backlog Edit page - page buttons (back / history / comments), delete, copy-link, task age.
   Reads #backlog-edit-config. confirmDelete + copyTaskLink stay global (inline onclick handlers). */

(function () {
    "use strict";

    var $cfg = $("#backlog-edit-config");
    if (!$cfg.length) return;

    var cfg = $cfg.data();
    var L = JSON.parse($cfg.attr("data-labels"));

    // called from an inline onclick on the Delete button
    window.confirmDelete = function (id) {
        JSManager.confirm("Are you sure you want to delete this task? This action cannot be undone.", function () {
            var dto = JSManager.addAntiForgeryToken({ id: id });
            JSManager.ajaxPost(cfg.deleteUrl, dto, undefined, undefined, function (r) {
                if (r && r.success) {
                    JSManager.showSuccess(r.message);
                    window.location.href = cfg.indexUrl;
                } else {
                    JSManager.showError(r && r.message ? r.message : 'Unable to delete the task.');
                }
            });
        });
    };

    // called from an inline onclick on the copy-link button
    window.copyTaskLink = function () {
        var url = window.location.href;

        function done() {
            if (window.JSManager && JSManager.showSuccess) JSManager.showSuccess('Link copied to clipboard');
            var $btn = $('.task-meta-copy');
            var original = $btn.html();
            $btn.html('<i class="fas fa-check"></i>').addClass('copied');
            setTimeout(function () { $btn.html(original).removeClass('copied'); }, 1500);
        }

        function fallbackCopy() {
            var ta = document.createElement('textarea');
            ta.value = url;
            ta.style.position = 'fixed';
            ta.style.opacity = '0';
            document.body.appendChild(ta);
            ta.select();
            try { document.execCommand('copy'); done(); }
            catch (e) { if (window.JSManager) JSManager.showError('Could not copy link'); }
            document.body.removeChild(ta);
        }

        // the modern API only works on HTTPS/localhost; fall back to execCommand otherwise
        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(url).then(done).catch(fallbackCopy);
        } else {
            fallbackCopy();
        }
    };

    $(function () {
        JSManager.setPageTitle(L.pageTitle + ' #' + cfg.backlogId);

        var buttons = '';
        buttons += '<a href="' + cfg.indexUrl + '" title="' + L.back + '" class="btn btn-sm btn-secondary me-2"><i class="fas fa-chevron-left me-1"></i>' + L.back + '</a>';
        buttons += '<a href="javascript:JSManager.openOffCanvas(\'' + cfg.historyUrl + '\',\'' + L.historyLabel + '\')" title="' + L.historyLabel + '" class="btn btn-sm btn-primary text-white me-2"><i class="fa-solid fa-history"></i></a>';
        buttons += '<a href="javascript:JSManager.openOffCanvas(\'' + cfg.commentsUrl + '\',\'' + L.commentsLabel + '\')" title="' + L.commentsLabel + '" class="btn btn-sm btn-success text-white me-2"><i class="fa-solid fa-comments"></i></a>';
        JSManager.setPageButtons(buttons);

        // task age (relative)
        var $age = $('.task-meta-age');
        var t = $age.data('time');
        if (t && window.moment) $age.text('opened ' + moment(t).fromNow());
    });
})();
