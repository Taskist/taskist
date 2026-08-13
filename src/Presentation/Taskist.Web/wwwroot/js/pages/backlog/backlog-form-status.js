/* Backlog form status panel (_FormStatus partial) - cascading project/module/sub-module
   dropdowns and avatar pickers for Assignee/Reporter. Reads #backlog-status-config. */

(function () {
    "use strict";

    var $cfg = $("#backlog-status-config");
    if (!$cfg.length) return;

    var cfg = $cfg.data();

    $(function () {
        // cascading dropdowns: project -> module -> sub-module
        $("#ProjectId").change(function () {
            JSManager.bindSelect("ModuleId", cfg.modulesUrl, { project: $(this).val() });
        });

        $("#ModuleId").change(function () {
            JSManager.bindSelect("SubModuleId", cfg.subModulesUrl, { module: $(this).val() });
        });

        // avatar pickers: show each person's photo in the Assignee / Reporter dropdowns
        var avatarBase = cfg.avatarUrl;
        function avatarOption(state) {
            if (!state.id || state.id === '-1' || state.id === '') return state.text;
            var $wrap = $('<span class="select2-avatar"></span>');
            $('<img>').attr('src', avatarBase + '?userId=' + state.id).attr('alt', state.text).appendTo($wrap);
            $('<span></span>').text(state.text).appendTo($wrap);
            return $wrap;
        }
        ['#AssigneeId', '#ReporterId'].forEach(function (sel) {
            var $s = $(sel);
            if (!$s.length) return;
            if ($s.hasClass('select2-hidden-accessible')) $s.select2('destroy');
            $s.select2({
                templateResult: avatarOption,
                templateSelection: avatarOption,
                dropdownParent: $s.parent()
            });
        });
    });
})();
