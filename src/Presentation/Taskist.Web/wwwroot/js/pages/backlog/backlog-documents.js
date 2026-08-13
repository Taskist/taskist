/* Backlog Documents partial - delete files + self-contained image lightbox.
   Loaded globally; the AJAX-injected partial calls BacklogDocuments.init({ deleteUrl }). */

window.BacklogDocuments = (function () {
    "use strict";

    function ensureLightbox() {
        if (document.getElementById('imgLightbox')) return;
        $('body').append(
            '<div class="img-lightbox" id="imgLightbox">'
            + '<span class="img-lightbox-close">&times;</span>'
            + '<span class="img-lightbox-nav img-lightbox-prev">&#10094;</span>'
            + '<img class="img-lightbox-img" alt="" />'
            + '<span class="img-lightbox-nav img-lightbox-next">&#10095;</span>'
            + '<div class="img-lightbox-caption"></div>'
            + '</div>');

        var lbImages = [], lbIndex = 0;
        var $lb = $('#imgLightbox');

        function renderLightbox() {
            var el = lbImages[lbIndex];
            if (!el) return;
            $lb.find('.img-lightbox-img').attr('src', $(el).data('src'));
            $lb.find('.img-lightbox-caption').text($(el).data('name') || '');
            $lb.find('.img-lightbox-nav').toggle(lbImages.length > 1);
        }
        function stepLightbox(d) {
            if (!lbImages.length) return;
            lbIndex = (lbIndex + d + lbImages.length) % lbImages.length;
            renderLightbox();
        }
        function closeLightbox() { $lb.css('display', 'none'); }

        $lb.on('click', function (e) { if (e.target === this) closeLightbox(); });
        $lb.find('.img-lightbox-close').on('click', closeLightbox);
        $lb.find('.img-lightbox-prev').on('click', function (e) { e.stopPropagation(); stepLightbox(-1); });
        $lb.find('.img-lightbox-next').on('click', function (e) { e.stopPropagation(); stepLightbox(1); });
        $(document).on('keydown.imglightbox', function (e) {
            if ($lb.css('display') === 'none') return;
            if (e.key === 'Escape') closeLightbox();
            else if (e.key === 'ArrowLeft') stepLightbox(-1);
            else if (e.key === 'ArrowRight') stepLightbox(1);
        });

        // opening the lightbox is delegated so it works for content injected later
        $(document).off('click.imgpreview').on('click.imgpreview', '.js-img-preview', function (e) {
            e.preventDefault();
            lbImages = $(this).closest('.thumbnail-container').find('.js-img-preview').toArray();
            lbIndex = lbImages.indexOf(this);
            renderLightbox();
            $lb.css('display', 'flex');
        });
    }

    return {
        init: function (cfg) {
            cfg = cfg || {};

            $('.file-icon-delete').off('click.bkdoc').on('click.bkdoc', function () {
                var token = $(this).data('token');
                JSManager.confirm("Are you sure you want to delete this file?", function () {
                    var container = 'file-icon-container-' + token;
                    JSManager.ajaxPost(cfg.deleteUrl, { token: token }, container, 'Deleting...', function (response) {
                        $('#' + container).remove();
                        if (response.success) JSManager.showSuccess(response.message);
                        else JSManager.showError(response.message);
                    });
                });
            });

            ensureLightbox();
        }
    };
})();
