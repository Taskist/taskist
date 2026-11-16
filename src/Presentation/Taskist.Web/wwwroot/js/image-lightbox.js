/* Shared image preview: any element with .js-img-preview (data-src, data-name) inside a
   .thumbnail-container opens a full-screen lightbox with prev/next, Esc and arrow keys. */
(function () {
    "use strict";

    var images = [];
    var index = 0;

    function ensureLightbox() {
        if (document.getElementById("imgLightbox")) return;
        $("body").append(
            '<div class="img-lightbox" id="imgLightbox">' +
            '<span class="img-lightbox-close">&times;</span>' +
            '<span class="img-lightbox-nav img-lightbox-prev">&#10094;</span>' +
            '<img class="img-lightbox-img" alt="" />' +
            '<span class="img-lightbox-nav img-lightbox-next">&#10095;</span>' +
            '<div class="img-lightbox-caption"></div>' +
            "</div>");

        var $lb = $("#imgLightbox");
        $lb.on("click", function (e) { if (e.target === this) close(); });
        $lb.find(".img-lightbox-close").on("click", close);
        $lb.find(".img-lightbox-prev").on("click", function (e) { e.stopPropagation(); step(-1); });
        $lb.find(".img-lightbox-next").on("click", function (e) { e.stopPropagation(); step(1); });
    }

    function render() {
        var el = images[index];
        if (!el) return;
        var $lb = $("#imgLightbox");
        $lb.find(".img-lightbox-img").attr("src", $(el).data("src"));
        $lb.find(".img-lightbox-caption").text($(el).data("name") || "");
        $lb.find(".img-lightbox-nav").toggle(images.length > 1);
    }

    function step(delta) {
        if (!images.length) return;
        index = (index + delta + images.length) % images.length;
        render();
    }

    function close() {
        $("#imgLightbox").css("display", "none");
    }

    $(document)
        .off("click.imgpreview")
        .on("click.imgpreview", ".js-img-preview", function (e) {
            e.preventDefault();
            ensureLightbox();
            images = $(this).closest(".thumbnail-container").find(".js-img-preview").toArray();
            index = images.indexOf(this);
            render();
            $("#imgLightbox").css("display", "flex");
        })
        .off("keydown.imglightbox")
        .on("keydown.imglightbox", function (e) {
            var $lb = $("#imgLightbox");
            if (!$lb.length || $lb.css("display") === "none") return;
            if (e.key === "Escape") close();
            else if (e.key === "ArrowLeft") step(-1);
            else if (e.key === "ArrowRight") step(1);
        });
})();
