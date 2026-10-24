$(function () {
    var $board = $("#kanban");

    // Move the project picker + Columns button into the page header bar.
    var $headerControls = $("#boardHeaderControls");
    if ($headerControls.length && $("#pageButtons").length) {
        $("#pageButtons").append($headerControls.removeClass("d-none").addClass("d-inline-flex"));
    }

    // Project switch reloads the board for that project.
    $("#boardProject").on("change", function () {
        var url = $(this).data("url");
        window.location = url + "?project=" + ($(this).val() || "");
    });

    if (!$board.length)
        return;

    var PAGE = 20;

    var cfg = {
        project: $board.data("project"),
        cardsUrl: $board.data("cards-url"),
        updateUrl: $board.data("update-url"),
        closingCheckUrl: $board.data("closing-check-url"),
        closingUrl: $board.data("closing-url"),
        closingLabel: $board.data("closing-label"),
        editUrl: $board.data("edit-url")
    };

    var dragging = false;
    var pendingReload = false;

    // Fit the board to the available viewport area; columns scroll internally.
    function fitBoardHeight() {
        var top = $board.offset().top;
        var height = $(window).height() - top - 20;
        $board.css("height", Math.max(height, 240) + "px");
    }

    fitBoardHeight();
    $(window).on("resize", fitBoardHeight);

    // Keep a column header's total in sync after a card moves in/out.
    function syncColumn($cards, totalDelta) {
        var total = (parseInt($cards.data("total"), 10) || 0) + totalDelta;
        $cards.data("total", total);
        $cards.data("loaded", $cards.find(".board-card").length);
        $cards.closest(".board-column").find(".board-column__count").text(total);
    }

    function changeStatus(taskId, taskType, newStatus, $sender, $target) {
        var check = JSManager.ajaxGetWithResponse(cfg.closingCheckUrl, { id: taskId, taskType: taskType, status: newStatus });

        if (check && check.Valid) {
            // Closing fields required — revert the drag and capture them via the existing form.
            if ($sender) $sender.sortable("cancel");
            pendingReload = true;
            JSManager.openOffCanvas(cfg.closingUrl + "?id=" + taskId + "&status=" + newStatus, cfg.closingLabel);
            return;
        }

        var dto = JSManager.addAntiForgeryToken({ id: taskId, name: "StatusId", value: newStatus });
        $.ajax({
            url: cfg.updateUrl,
            type: "PATCH",
            data: dto,
            success: function (r) {
                if (r && r.status === 2000) {
                    JSManager.showSuccess(r.message);
                    syncColumn($sender, -1);
                    syncColumn($target, 1);
                } else {
                    JSManager.showError(r ? r.message : "Error");
                    if ($sender) $sender.sortable("cancel");
                }
            },
            error: function () {
                if ($sender) $sender.sortable("cancel");
            }
        });
    }

    function initSortable() {
        $(".board-column__cards").sortable({
            connectWith: ".board-column__cards",
            items: ".board-card",
            placeholder: "board-card--placeholder",
            tolerance: "pointer",
            start: function (event, ui) {
                dragging = true;
                ui.item.data("origin-list", ui.item.parent()[0]);
            },
            stop: function () { setTimeout(function () { dragging = false; }, 50); },
            update: function (event, ui) {
                // Allow moving between columns only; revert reordering within the same column.
                if (ui.sender == null && ui.item.parent()[0] === ui.item.data("origin-list")) {
                    $(this).sortable("cancel");
                }
            },
            receive: function (event, ui) {
                var card = ui.item;
                changeStatus(card.data("id"), card.data("task-type"), $(this).data("status-id"), ui.sender, $(this));
            }
        });
    }

    initSortable();

    // Infinite scroll: load the next page of cards for a column as it nears the bottom.
    function loadMore($cards) {
        var loaded = parseInt($cards.data("loaded"), 10) || 0;
        var total = parseInt($cards.data("total"), 10) || 0;
        if ($cards.data("loading") || loaded >= total)
            return;

        var el = $cards[0];
        if (el.scrollTop + el.clientHeight < el.scrollHeight - 80)
            return;

        var page = parseInt($cards.data("page"), 10) || 1;
        $cards.data("loading", true);
        $.get(cfg.cardsUrl, { project: cfg.project, statusId: $cards.data("status-id"), skip: page * PAGE, take: PAGE })
            .done(function (html) {
                // De-dupe against cards already shown — drag moves can shift page boundaries.
                $("<div>").html(html).children(".board-card").each(function () {
                    var id = $(this).data("id");
                    if (!$cards.find('.board-card[data-id="' + id + '"]').length)
                        $cards.append(this);
                });
                $cards.data("page", page + 1);
                $cards.data("loaded", $cards.find(".board-card").length);
                $(".board-column__cards").sortable("refresh");
            })
            .always(function () { $cards.data("loading", false); });
    }

    $(".board-column__cards").on("scroll", function () {
        loadMore($(this));
    });

    // Configure which statuses appear as columns.
    $("#btnBoardColumns").on("click", function () {
        JSManager.openOffCanvas($(this).data("url"), $(this).data("label"));
    });

    // Select / deselect all statuses in the columns config.
    $(document).on("change", "#boardColsAll", function () {
        $(".board-col-opt").prop("checked", this.checked);
    });

    // "Show all" = clear the preference (empty selection) and save.
    $(document).on("click", "#btnBoardShowAll", function () {
        $(".board-col-opt").prop("checked", false);
        $("#boardColumnsForm").submit();
    });

    // Only reload the board when the config is actually saved (not on cancel).
    $(document).on("submit", "#boardColumnsForm", function () {
        pendingReload = true;
    });

    // Card click opens the task (suppressed right after a drag).
    $board.on("click", ".board-card", function () {
        if (dragging)
            return;
        window.location = cfg.editUrl + "/" + $(this).data("id");
    });

    // After the closing-comment form closes, refresh the board so the card lands correctly.
    $(document).on("hidden.bs.offcanvas", "#pdOffCanvas", function () {
        if (pendingReload) {
            pendingReload = false;
            window.location.reload();
        }
    });
});
