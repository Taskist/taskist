$(function () {
    // Move the project picker into the page header bar (same as the board).
    var $headerControls = $("#boardHeaderControls");
    if ($headerControls.length && $("#pageButtons").length) {
        $("#pageButtons").append($headerControls.removeClass("d-none").addClass("d-inline-flex"));
    }

    $("#boardProject").on("change", function () {
        var url = $(this).data("url");
        window.location = url + "?project=" + ($(this).val() || "");
    });

    var el = document.getElementById("taskTimeline");
    if (!el || typeof FullCalendar === "undefined")
        return;

    var feedUrl = el.getAttribute("data-feed-url");
    var updateUrl = el.getAttribute("data-update-url");
    var project = el.getAttribute("data-project");

    function pad(n) { return n < 10 ? "0" + n : "" + n; }
    function fmt(d) { return d.getFullYear() + "-" + pad(d.getMonth() + 1) + "-" + pad(d.getDate()); }
    function esc(s) { return $("<div>").text(s == null ? "" : s).html(); }

    function patchField(id, name, value) {
        return $.ajax({ url: updateUrl, type: "PATCH", data: JSManager.addAntiForgeryToken({ id: id, name: name, value: value }) });
    }

    function reschedule(info) {
        var ev = info.event;
        var due = ev.end ? new Date(ev.end.getTime() - 86400000) : ev.start;

        // Persist both the planned start and the due date (whole-task move or resize).
        patchField(ev.id, "PlannedStartDate", fmt(ev.start))
            .then(function () { return patchField(ev.id, "DueDate", fmt(due)); })
            .done(function (r) {
                if (r && r.status === 2000)
                    JSManager.showSuccess(r.message);
                else {
                    JSManager.showError(r ? r.message : "Error");
                    info.revert();
                }
            })
            .fail(function () { info.revert(); });
    }

    function paintHeat(events) {
        var counts = {};
        events.forEach(function (e) {
            var d = e.extendedProps.due;
            if (d) counts[d] = (counts[d] || 0) + 1;
        });

        $(".fc-daygrid-day").css("background-color", "");
        Object.keys(counts).forEach(function (d) {
            var alpha = Math.min(0.08 + counts[d] * 0.06, 0.45);
            $('.fc-daygrid-day[data-date="' + d + '"]').css("background-color", "rgba(229,83,83," + alpha + ")");
        });
    }

    function updateSummary(events) {
        var today = new Date(); today.setHours(0, 0, 0, 0);
        var soon = new Date(today.getTime() + 7 * 86400000);
        var overdue = 0, dueSoon = 0;

        events.forEach(function (e) {
            var d = e.extendedProps.due;
            if (!d) return;
            if (e.extendedProps.overdue) { overdue++; return; }
            var dt = new Date(d); dt.setHours(0, 0, 0, 0);
            if (dt <= soon) dueSoon++;
        });

        var html = "";
        if (overdue) html += '<span class="badge bg-danger me-2"><i class="fas fa-triangle-exclamation me-1"></i>' + overdue + " overdue</span>";
        if (dueSoon) html += '<span class="badge bg-warning text-dark"><i class="far fa-clock me-1"></i>' + dueSoon + " due this week</span>";
        $("#timelineSummary").html(html);
    }

    var calendar = new FullCalendar.Calendar(el, {
        initialView: "dayGridMonth",
        height: "auto",
        nowIndicator: true,
        editable: true,
        eventStartEditable: true,
        eventDurationEditable: true,
        eventDisplay: "block",
        headerToolbar: {
            left: "prev,next today",
            center: "title",
            right: "dayGridMonth,timeGridWeek,listMonth"
        },
        events: {
            url: feedUrl,
            method: "GET",
            extraParams: { project: project }
        },
        eventClassNames: function (arg) {
            return arg.event.extendedProps.overdue ? ["tl-overdue"] : [];
        },
        eventContent: function (arg) {
            var p = arg.event.extendedProps;
            var icon = p.typeIcon ? '<i class="' + p.typeIcon + ' me-1"></i>' : "";
            var dot = '<span class="tl-sev" style="background:' + (p.severityColor || "#adb5bd") + '"></span>';
            var assignee = p.assignee ? '<span class="tl-assignee">' + esc(p.assignee) + "</span>" : "";
            return { html: '<div class="tl-event">' + dot + icon + '<span class="tl-title">' + esc(arg.event.title) + "</span>" + assignee + "</div>" };
        },
        eventDrop: reschedule,
        eventResize: reschedule,
        eventsSet: function (events) {
            updateSummary(events);
            paintHeat(events);
        }
    });

    calendar.render();

    // ---- Gantt (swimlanes by assignee) ----------------------------------
    var ganttEl = document.getElementById("taskGantt");
    var ganttLoaded = false;

    function parseDate(s) { var p = s.split("-"); return new Date(+p[0], +p[1] - 1, +p[2]); }
    function dayMs() { return 86400000; }
    function fmtAxis(d) { return d.getDate() + " " + d.toLocaleString("en", { month: "short" }); }

    function renderGantt(events) {
        var today = new Date(); today.setHours(0, 0, 0, 0);
        var starts = [], ends = [];
        events.forEach(function (e) {
            var s = parseDate(e.start);
            var en = e.end ? new Date(parseDate(e.end).getTime() - dayMs()) : s;
            starts.push(s.getTime()); ends.push(en.getTime());
        });
        var min = Math.min.apply(null, starts.concat(today.getTime() - 14 * dayMs()));
        var max = Math.max.apply(null, ends.concat(today.getTime() + 14 * dayMs()));
        var span = Math.max(max - min, dayMs());
        function pct(ms) { return ((ms - min) / span) * 100; }

        // group by assignee
        var groups = {};
        events.forEach(function (e) {
            var key = e.extendedProps.assignee || ganttEl.getAttribute("data-unassigned");
            (groups[key] = groups[key] || []).push(e);
        });

        var editUrl = ganttEl.getAttribute("data-edit-url");
        var html = '<div class="gantt">';

        // axis (weekly ticks)
        html += '<div class="gantt-axis"><div class="gantt-axis-label"></div><div class="gantt-axis-track">';
        for (var t = min; t <= max; t += 7 * dayMs()) {
            html += '<span class="gantt-tick" style="left:' + pct(t) + '%">' + fmtAxis(new Date(t)) + "</span>";
        }
        html += '<span class="gantt-today" style="left:' + pct(today.getTime()) + '%"></span>';
        html += "</div></div>";

        Object.keys(groups).sort().forEach(function (assignee) {
            html += '<div class="gantt-group">' + $("<div>").text(assignee).html() + "</div>";
            groups[assignee].forEach(function (e) {
                var s = parseDate(e.start);
                var en = e.end ? new Date(parseDate(e.end).getTime() - dayMs()) : s;
                var left = pct(s.getTime());
                var width = Math.max(pct(en.getTime() + dayMs()) - left, 1.5);
                var color = e.color || "#6f78e6";
                var over = e.extendedProps.overdue ? " gantt-bar--overdue" : "";
                html += '<div class="gantt-row">' +
                    '<a class="gantt-row-label" href="' + editUrl + "/" + e.id + '" title="' + $("<div>").text(e.title).html() + '">' + $("<div>").text(e.title).html() + "</a>" +
                    '<div class="gantt-track"><span class="gantt-today-line" style="left:' + pct(today.getTime()) + '%"></span>' +
                    '<a class="gantt-bar' + over + '" href="' + editUrl + "/" + e.id + '" style="left:' + left + "%;width:" + width + "%;background:" + color + '"></a>' +
                    "</div></div>";
            });
        });

        html += "</div>";
        $(ganttEl).html(events.length ? html : '<div class="text-muted text-center py-4">—</div>');
    }

    function loadGantt() {
        if (ganttLoaded) return;
        ganttLoaded = true;
        var today = new Date();
        var from = new Date(today.getTime() - 30 * dayMs());
        var to = new Date(today.getTime() + 120 * dayMs());
        $.get(ganttEl.getAttribute("data-feed-url"), {
            project: ganttEl.getAttribute("data-project"),
            start: from.toISOString().slice(0, 10),
            end: to.toISOString().slice(0, 10)
        }).done(renderGantt);
    }

    $("#tlViewGantt").on("click", function () {
        $(this).addClass("active");
        $("#tlViewCalendar").removeClass("active");
        $("#tlCalendarPane").addClass("d-none");
        $("#tlGanttPane").removeClass("d-none");
        loadGantt();
    });

    $("#tlViewCalendar").on("click", function () {
        $(this).addClass("active");
        $("#tlViewGantt").removeClass("active");
        $("#tlGanttPane").addClass("d-none");
        $("#tlCalendarPane").removeClass("d-none");
        calendar.updateSize();
    });
});
