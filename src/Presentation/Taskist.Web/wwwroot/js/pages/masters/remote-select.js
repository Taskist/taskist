(function () {
    "use strict";

    $(function () {
        $(".select2-remote").each(function () {
            var $this = $(this);
            $this.wrap('<div class="position-relative"></div>').select2({
                placeholder: $this.data("placeholder") || "Search...",
                allowClear: true,
                width: "100%",
                minimumInputLength: 0,
                dropdownParent: $this.parent(),
                ajax: {
                    url: $this.data("url"),
                    dataType: "json",
                    delay: 250,
                    data: function (params) {
                        return { q: params.term };
                    },
                    processResults: function (data) {
                        if (data && data.error) {
                            if ($this.data("last-error") !== data.error) {
                                $this.data("last-error", data.error);
                                $this.select2("close");
                                JSManager.showSwal(data.error, "error");
                            }
                            return { results: [] };
                        }
                        $this.removeData("last-error");
                        return {
                            results: (data || []).map(function (item) {
                                return { id: item.Value, text: item.Text };
                            })
                        };
                    },
                    cache: true
                }
            });
        });
    });
})();
