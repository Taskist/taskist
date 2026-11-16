/* Closing pop-up: resolves the picked / manually typed MR into MergeRequestId,
   and reloads the task page once the status move is saved. */
(function () {
    "use strict";

    function sync($form) {
        var pick = $form.find('input[name="closingMrPick"]:checked').val();
        var value = pick === "manual" ? $.trim($form.find(".closing-mr-manual").val() || "") : (pick || "");
        $form.find(".closing-mr-value").val(value);
        $form.find(".closing-mr-repo").val(pick === "manual" ? "" : ($form.find('input[name="closingMrPick"]:checked').data("repository-id") || ""));
        $form.find(".closing-mr").each(function () {
            $(this).toggleClass("is-selected", $(this).find('input[type="radio"]').is(":checked"));
        });
    }

    $(document)
        .off(".closingForm")
        .on("change.closingForm", '.closing-form input[name="closingMrPick"]', function () {
            var $form = $(this).closest(".closing-form");
            if ($(this).val() === "manual") $form.find(".closing-mr-manual").trigger("focus");
            sync($form);
        })
        .on("input.closingForm focus.closingForm", ".closing-form .closing-mr-manual", function () {
            var $form = $(this).closest(".closing-form");
            $form.find('input[name="closingMrPick"][value="manual"]').prop("checked", true);
            sync($form);
        })
        .on("submit.closingForm", ".closing-form", function () {
            sync($(this));
        });

    function sanitize(html) {
        var doc = new DOMParser().parseFromString("<div>" + (html || "") + "</div>", "text/html");
        $(doc).find("script,style,iframe,object,embed,link,meta,form").remove();
        $(doc).find("*").each(function () {
            for (var i = this.attributes.length - 1; i >= 0; i--) {
                var attr = this.attributes[i];
                var name = attr.name.toLowerCase();
                if (name.indexOf("on") === 0 || ((name === "href" || name === "src") && /^\s*javascript:/i.test(attr.value)))
                    this.removeAttribute(attr.name);
            }
        });
        $(doc).find("a[href]").attr({ target: "_blank", rel: "noopener" });
        return doc.body.firstChild ? doc.body.firstChild.innerHTML : "";
    }

    $(".closing-form").each(function () { sync($(this)); });

    $(".closing-rich[data-html]").each(function () {
        $(this).html(sanitize($(this).attr("data-html")));
    });

    $(".closing-view-mr[data-url]").each(function () {
        var $el = $(this);
        JSManager.renderPartial($el.data("url"), { id: $el.data("task-id") }, this.id);
    });
})();

function closingCommentSaved(response) {
    ajaxSuccess(response);

    if (response && response.status === 2000 && $("#backlog-edit-config").length) {
        if (window.BacklogEdit) window.BacklogEdit.clear();
        window.location.reload();
    }
}
