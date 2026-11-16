/* Project → Repositories: repository picker (searches the chosen Git connection; a full path can also be typed). */
(function () {
    "use strict";

    setTimeout(function () { $(".project-repository-form").each(init); }, 0);

    function init() {
        var $form = $(this);
        var $connection = $form.find(".project-repository-connection");
        var $ref = $form.find(".project-repository-ref");
        if ($ref.data("select2")) return;

        $ref.wrap('<div class="position-relative"></div>').select2({
            tags: true,
            allowClear: false,
            width: "100%",
            minimumInputLength: 0,
            dropdownParent: $ref.parent(),
            ajax: {
                url: $form.data("search-url"),
                dataType: "json",
                delay: 300,
                data: function (params) {
                    return { connectionId: $connection.val(), q: params.term };
                },
                processResults: function (data) {
                    if (data && data.error) {
                        if ($ref.data("last-error") !== data.error) {
                            $ref.data("last-error", data.error);
                            JSManager.showSwal(data.error, "error");
                        }
                        return { results: [] };
                    }
                    $ref.removeData("last-error");
                    return {
                        results: (data || []).map(function (item) {
                            return { id: item.Value, text: item.Text };
                        })
                    };
                }
            }
        });

        $connection.on("change", function () {
            $ref.val(null).trigger("change");
        });
    }
})();
