(function () {
    "use strict";

    function formatTimes($root) {
        $root.find(".task-code-time").each(function () {
            var t = $(this).data("time");
            if (!t) return;
            var m = moment(t);
            $(this).text(m.fromNow()).attr("title", m.format("DD-MMM-YYYY HH:mm"));
        });
    }

    function card() { return $("#taskCodeCard"); }

    function labels($card) {
        var d = $card.data();
        return {
            createTitle: d.labelCreateTitle, create: d.labelCreate, name: d.labelName, base: d.labelBase,
            linkInfo: d.labelLinkInfo, nameRequired: d.labelNameRequired, disabled: d.labelDisabled,
            linkTitle: d.labelLinkTitle, searchPlaceholder: d.labelSearchPlaceholder, noResults: d.labelNoResults,
            unlinkConfirm: d.labelUnlinkConfirm, connectWhy: d.labelConnectWhy,
            existingSome: d.labelExistingSome, existingAll: d.labelExistingAll, includeExisting: d.labelIncludeExisting
        };
    }

    function load(refresh) {
        var $card = card();
        if (!$card.length) return;

        var $body = $card.find(".task-code-body");
        var $btn = $card.find(".task-code-refresh");
        $btn.prop("disabled", true).find("i").addClass("fa-spin");

        $.get($card.data("url"), { refresh: !!refresh })
            .done(function (html) {
                $body.html(html);
                formatTimes($body);
                var error = $body.find(".task-code-error").data("error");
                if (refresh && error) BacklogBranch.gitLabError(error);
                var $sum = $body.find(".task-code-summary");
                $(document).trigger("taskcode:loaded", { mrCount: parseInt($sum.data("mr-count"), 10) || 0, state: $sum.data("state") || "" });
            })
            .fail(function (xhr) {
                $body.html('<div class="text-muted small">Could not load code information.</div>');
                if (refresh) BacklogBranch.gitLabError("Could not load code information (" + xhr.status + ").");
            })
            .always(function () {
                $btn.prop("disabled", false).find("i").removeClass("fa-spin");
            });
    }

    $(document)
        .on("click", "#taskCodeCard .task-code-refresh", function () { load(true); })
        .on("click", "#taskCodeCard .task-code-copy", function () {
            var text = $(this).data("copy");
            if (navigator.clipboard && text) {
                navigator.clipboard.writeText(text).then(function () { JSManager.showSuccess("Copied: " + text); });
            }
        })
        .on("click", "#taskCodeCard .task-code-create", function () {
            var $card = card();
            BacklogBranch.openCreate({
                trigger: this,
                ids: [$card.data("task-id")],
                contextUrl: $card.data("context-url"),
                createUrl: $card.data("create-url"),
                editUrl: $card.data("edit-url"),
                labels: labels($card),
                onDone: function () { load(true); }
            });
        })
        .on("click", "#taskCodeCard .task-code-link", function () {
            var $card = card();
            BacklogBranch.openLink({
                taskId: $card.data("task-id"),
                searchUrl: $card.data("search-url"),
                linkUrl: $card.data("link-url"),
                labels: labels($card),
                onDone: function () { load(true); }
            });
        })
        .on("click", "#taskCodeCard .task-code-unlink", function () {
            var $card = card();
            BacklogBranch.unlink({
                taskId: $card.data("task-id"),
                branchId: $(this).data("branch-id"),
                branchName: $(this).data("branch-name"),
                unlinkUrl: $card.data("unlink-url"),
                labels: labels($card),
                onDone: function () { load(true); }
            });
        })
        .on("click", "#taskCodeCard .task-code-link-source", function () {
            var $card = card();
            BacklogBranch.linkByName({
                taskId: $card.data("task-id"),
                linkUrl: $card.data("link-url"),
                name: $(this).data("name"),
                $btn: $(this),
                onDone: function () { load(true); }
            });
        });

    $(function () { load(false); });
})();

/* Code card: open the review drawer for an MR, and refresh the card after a review action. */
(function () {
    "use strict";

    $(document)
        .on("click", "#taskCodeCard .task-code-review", function () {
            var $btn = $(this);
            JSManager.openForm($btn.data("url"), $("<div>").text($btn.data("title")).html(), "col-md-11");
        })
        .on("review:changed", function () {
            $("#taskCodeCard .task-code-refresh").trigger("click");
        });
})();

/* Code card: create a merge request for a linked branch with the developer's own GitHub/GitLab account. */
(function () {
    "use strict";

    $(document).on("click", "#taskCodeCard .task-code-create-mr", function () {
        var $card = $("#taskCodeCard");
        var d = $card.data();
        BacklogBranch.openCreateMergeRequest({
            trigger: this,
            taskId: d.taskId,
            branchId: $(this).data("branch-id"),
            contextUrl: d.mrContextUrl,
            createUrl: d.mrCreateUrl,
            labels: {
                title: d.labelMrTitle, fieldTitle: d.labelMrFieldTitle, from: d.labelMrFrom, into: d.labelMrInto,
                description: d.labelMrDescription, reviewer: d.labelMrReviewer, noReviewer: d.labelMrNoReviewer,
                draft: d.labelMrDraft, remove: d.labelMrRemove, moves: d.labelMrMoves, existing: d.labelMrExisting,
                create: d.labelMrCreate, titleRequired: d.labelMrTitleRequired
            },
            onDone: function (r) {
                $card.find(".task-code-refresh").trigger("click");
                if (r && r.moved > 0) {
                    var unsaved = window.BacklogEdit && window.BacklogEdit.hasChanges && window.BacklogEdit.hasChanges();
                    if (unsaved) JSManager.showSwal(d.labelMrStatusMoved, "info");
                    else setTimeout(function () { window.location.reload(); }, 1200);
                }
            }
        });
    });
})();
