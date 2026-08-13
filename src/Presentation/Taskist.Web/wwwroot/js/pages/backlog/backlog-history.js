/* Backlog History partial - relative timestamps + before/after chips.
   Loaded globally; the AJAX-injected partial calls BacklogHistory.init(). */

window.BacklogHistory = (function () {
    "use strict";

    return {
        init: function () {
            // relative timestamps
            $('.history-time').each(function () {
                var t = $(this).data('time');
                if (t && window.moment) $(this).text(moment(t).fromNow());
            });

            // turn "Field: Old -> New" into a label + before/after chips
            $('.history-text').each(function () {
                var $el = $(this);
                var html = $el.html();
                var m = html.match(/^(<strong>.*?<\/strong>):\s*(.*?)\s*&rarr;\s*(.*)$/i)
                     || html.match(/^(<strong>.*?<\/strong>):\s*(.*?)\s*→\s*(.*)$/i);
                if (m) {
                    $el.html(
                        m[1] +
                        ' <span class="hist-chip hist-chip-old">' + m[2] + '</span>' +
                        ' <i class="fas fa-arrow-right-long hist-arrow"></i> ' +
                        ' <span class="hist-chip hist-chip-new">' + m[3] + '</span>'
                    );
                }
            });
        }
    };
})();
