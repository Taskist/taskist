/* Review drawer (opened from the Reviews page and from a task's Code card).
   Every action goes through the reviewer's own Git token on the server; the Git server's own rules apply.
   Errors are shown inside the drawer. Safe to load more than once (handlers are namespaced). */
(function () {
    "use strict";

    function root() { return $("#reviewDrawer"); }

    function escapeHtml(value) {
        return $("<div>").text(String(value == null ? "" : value)).html();
    }

    function formatTimes($scope) {
        $scope.find(".review-time[data-time]").each(function () {
            var m = moment($(this).data("time"));
            $(this).text(m.fromNow()).attr("title", m.format("DD-MMM-YYYY HH:mm"));
        });
    }

    function showError(message) {
        var $box = root().find(".review-drawer-error");
        $box.html("<i class='fas fa-circle-exclamation me-1'></i>" + escapeHtml(message || "Request to the Git server failed.")).removeClass("d-none");
        $box[0].scrollIntoView({ block: "nearest" });
    }

    function reload() {
        var $r = root();
        $.get($r.data("detail-url")).done(function (html) {
            $r.replaceWith($("<div>").html(html).find("#reviewDrawer"));
            init();
        });
    }

    function post(urlKey, data, $btn, onDone) {
        var $r = root();
        $r.find(".review-drawer-error").addClass("d-none").empty();
        if ($btn) $btn.prop("disabled", true);

        $.ajax({
            type: "POST",
            url: $r.data(urlKey),
            data: JSManager.addAntiForgeryToken($.extend({ repositoryId: $r.data("repository-id"), iid: $r.data("iid") }, data || {})),
            success: function (res) {
                if (res && res.status === 2000) {
                    JSManager.showSuccess(res.message);
                    $(document).trigger("review:changed");
                    if (onDone) onDone(res); else reload();
                } else {
                    showError(res ? res.message : null);
                }
            },
            error: function (xhr) { showError("Request failed (" + xhr.status + ")."); },
            complete: function () { if ($btn) $btn.prop("disabled", false); }
        });
    }

    function askReason(opts) {
        $.confirm({
            title: opts.title,
            icon: opts.icon,
            type: opts.type,
            columnClass: "medium",
            content: "<label class='form-label small'>" + escapeHtml(opts.prompt) + "</label>" +
                "<textarea class='form-control review-reason-text' rows='5'></textarea>" +
                "<div class='text-danger small mt-1 review-reason-required d-none'>" + escapeHtml(opts.required) + "</div>",
            onContentReady: function () { this.$content.find(".review-reason-text").trigger("focus"); },
            buttons: {
                send: {
                    text: opts.button,
                    btnClass: opts.btnClass,
                    action: function () {
                        var text = $.trim(this.$content.find(".review-reason-text").val() || "");
                        if (!text) { this.$content.find(".review-reason-required").removeClass("d-none"); return false; }
                        opts.onSend(text);
                    }
                },
                cancel: { text: "Cancel" }
            }
        });
    }

    function reject($btn) {
        var $r = root();
        askReason({
            title: $r.data("label-reject-title"), icon: "fas fa-ban", type: "red", btnClass: "btn-red",
            prompt: $r.data("label-reject-prompt"), required: $r.data("label-required"), button: $r.data("label-reject-button"),
            onSend: function (reason) { post("reject-url", { reason: reason }, $btn); }
        });
    }

    function requestChanges($btn) {
        var $r = root();
        askReason({
            title: $r.data("label-changes-title"), icon: "fas fa-pen-to-square", type: "orange", btnClass: "btn-orange",
            prompt: $r.data("label-changes-prompt"), required: $r.data("label-changes-required"), button: $r.data("label-changes-button"),
            onSend: function (notes) { post("request-changes-url", { notes: notes }, $btn); }
        });
    }

    function merge($btn) {
        var $r = root();
        var squash = $r.find(".review-merge-squash").is(":checked");
        var remove = $r.find(".review-merge-remove").is(":checked");
        JSManager.confirm(escapeHtml($r.data("label-merge-confirm")), function () {
            post("merge-url", { sha: $r.data("sha"), squash: squash, removeSourceBranch: remove }, $btn);
        });
    }

    function sanitize(html) {
        var doc = new DOMParser().parseFromString("<div>" + (html || "") + "</div>", "text/html");
        $(doc).find("script,style,iframe,object,embed,link,meta,form").remove();
        $(doc).find("*").each(function () {
            for (var i = this.attributes.length - 1; i >= 0; i--) {
                var name = this.attributes[i].name.toLowerCase();
                var value = this.attributes[i].value;
                if (name.indexOf("on") === 0 || ((name === "href" || name === "src") && /^\s*javascript:/i.test(value)))
                    this.removeAttribute(this.attributes[i].name);
            }
        });
        $(doc).find("a[href]").attr({ target: "_blank", rel: "noopener" });
        return doc.body.firstChild ? doc.body.firstChild.innerHTML : "";
    }

    function loadChanges() {
        var $box = root().find(".review-changes");
        if (!$box.length || $box.attr("data-loaded") === "true") return;
        $box.attr("data-loaded", "true");
        $.get(root().data("changes-url"))
            .done(function (html) { $box.html(html); })
            .fail(function (xhr) { $box.html("<div class='alert alert-warning'>Could not load the changes (" + xhr.status + ").</div>"); });
    }

    function init() {
        var $r = root();
        if (!$r.length || $r.data("ready")) return;
        $r.data("ready", true);
        formatTimes($r);
        $r.find(".review-rich[data-html]").each(function () { $(this).html(sanitize($(this).attr("data-html"))); });
        loadChanges();
    }

    $(document)
        .off(".reviewDrawer")
        .on("click.reviewDrawer", "#reviewDrawer .review-act", function () {
            var $btn = $(this);
            switch ($btn.data("act")) {
                case "approve": post("approve-url", { approve: true }, $btn); break;
                case "unapprove": post("approve-url", { approve: false }, $btn); break;
                case "comment":
                    root().find(".review-tab-discussion").trigger("click");
                    setTimeout(function () { root().find(".review-comment-text").trigger("focus"); }, 150);
                    break;
                case "post-comment":
                    var body = $.trim(root().find(".review-comment-text").val() || "");
                    if (!body) { root().find(".review-comment-text").trigger("focus"); return; }
                    post("comment-url", { body: body }, $btn);
                    break;
                case "request-changes": requestChanges($btn); break;
                case "reject": reject($btn); break;
                case "merge": merge($btn); break;
                case "rebase":
                    JSManager.confirm(escapeHtml(root().data("label-rebase-confirm")), function () { post("rebase-url", {}, $btn); });
                    break;
            }
        })
        .on("click.reviewDrawer", "#reviewDrawer .review-tab-changes", loadChanges)
        .on("click.reviewDrawer", "#reviewDrawer .review-copy", function () {
            var $btn = $(this);
            var text = String($btn.data("copy") || "");
            var copied = root().data("label-copied") || "Copied";
            var done = function () {
                JSManager.showSuccess(copied);
                if (!$btn.hasClass("review-step__copy")) return;
                var title = $btn.attr("title");
                $btn.addClass("is-copied").attr("title", copied).find("i").attr("class", "fas fa-check");
                setTimeout(function () {
                    $btn.removeClass("is-copied").attr("title", title).find("i").attr("class", "far fa-copy");
                }, 1500);
            };
            if (navigator.clipboard && window.isSecureContext) {
                navigator.clipboard.writeText(text).then(done);
            } else {
                var $tmp = $("<textarea>").val(text).css({ position: "fixed", opacity: 0 }).appendTo("body");
                $tmp[0].select();
                document.execCommand("copy");
                $tmp.remove();
                done();
            }
        })
        .on("click.reviewDrawer", "#reviewDrawer .review-file-nav__item", function () {
            var $file = root().find("#" + $(this).data("target"));
            if (!$file.length) return;
            $file.prop("open", true);
            root().find(".review-file-nav__item").removeClass("is-active");
            $(this).addClass("is-active");
            $file[0].scrollIntoView({ behavior: "smooth", block: "start" });
        })
        .on("click.reviewDrawer", "#reviewDrawer .review-files-expand", function () { root().find(".review-file").prop("open", true); })
        .on("click.reviewDrawer", "#reviewDrawer .review-files-collapse", function () { root().find(".review-file").prop("open", false); });

    $(init);
    init();
    setTimeout(init, 50);
})();
