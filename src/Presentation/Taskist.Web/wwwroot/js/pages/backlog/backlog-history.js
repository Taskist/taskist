var BacklogHistory = (function () {
    "use strict";

    var ALLOWED_TAGS = ["P", "BR", "B", "STRONG", "I", "EM", "U", "S", "STRIKE", "UL", "OL", "LI", "A", "SPAN", "DIV",
        "BLOCKQUOTE", "CODE", "PRE", "H1", "H2", "H3", "H4", "H5", "H6", "TABLE", "THEAD", "TBODY", "TR", "TD", "TH",
        "HR", "SUB", "SUP"];
    var DROP_TAGS = ["SCRIPT", "STYLE", "IFRAME", "OBJECT", "EMBED", "LINK", "META", "FORM", "INPUT", "BUTTON",
        "TEXTAREA", "SELECT", "SVG", "MATH", "NOSCRIPT", "TEMPLATE"];
    var BLOCK_TAGS = ["P", "DIV", "LI", "BR", "TR", "H1", "H2", "H3", "H4", "H5", "H6", "BLOCKQUOTE", "PRE", "HR"];
    var COLLAPSE_HEIGHT = 110;
    var MAX_DIFF_CELLS = 4000000;

    function parse(html) {
        return new DOMParser().parseFromString("<body>" + (html || "") + "</body>", "text/html").body;
    }

    function cleanNode(node) {
        Array.prototype.slice.call(node.childNodes).forEach(function (child) {
            if (child.nodeType === Node.TEXT_NODE) return;
            if (child.nodeType !== Node.ELEMENT_NODE) { child.remove(); return; }

            var tag = child.tagName.toUpperCase();
            if (DROP_TAGS.indexOf(tag) >= 0) { child.remove(); return; }

            cleanNode(child);

            if (ALLOWED_TAGS.indexOf(tag) < 0) {
                while (child.firstChild) node.insertBefore(child.firstChild, child);
                child.remove();
                return;
            }

            var href = tag === "A" ? (child.getAttribute("href") || "") : "";
            Array.prototype.slice.call(child.attributes).forEach(function (attr) { child.removeAttribute(attr.name); });

            if (tag === "A" && /^(https?:|mailto:)/i.test(href.trim())) {
                child.setAttribute("href", href.trim());
                child.setAttribute("target", "_blank");
                child.setAttribute("rel", "noopener noreferrer");
            }
        });
    }

    function sanitize(html) {
        var body = parse(html);
        cleanNode(body);
        return body.innerHTML;
    }

    function toText(html) {
        var body = parse(html);
        cleanNode(body);
        var out = [];
        (function walk(node) {
            Array.prototype.forEach.call(node.childNodes, function (child) {
                if (child.nodeType === Node.TEXT_NODE) { out.push(child.nodeValue); return; }
                walk(child);
                if (BLOCK_TAGS.indexOf(child.tagName) >= 0) out.push("\n");
            });
        })(body);
        return out.join("").replace(/ /g, " ").replace(/[ \t]+\n/g, "\n").replace(/\n{3,}/g, "\n\n").trim();
    }

    function escapeHtml(value) {
        return String(value).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    function tokenize(text) {
        return text.match(/\s+|[^\s]+/g) || [];
    }

    function wordDiff(oldText, newText) {
        var a = tokenize(oldText), b = tokenize(newText);
        var start = 0;
        while (start < a.length && start < b.length && a[start] === b[start]) start++;
        var endA = a.length, endB = b.length;
        while (endA > start && endB > start && a[endA - 1] === b[endB - 1]) { endA--; endB--; }

        var midA = a.slice(start, endA), midB = b.slice(start, endB);
        var n = midA.length, m = midB.length;
        if (n * m > MAX_DIFF_CELLS) return null;

        var dp = [];
        for (var i = 0; i <= n; i++) dp.push(new Uint32Array(m + 1));
        for (i = n - 1; i >= 0; i--) {
            for (var j = m - 1; j >= 0; j--) {
                dp[i][j] = midA[i] === midB[j] ? dp[i + 1][j + 1] + 1 : Math.max(dp[i + 1][j], dp[i][j + 1]);
            }
        }

        var parts = [];
        var push = function (type, token) {
            var last = parts[parts.length - 1];
            if (last && last.type === type) last.text += token; else parts.push({ type: type, text: token });
        };

        a.slice(0, start).forEach(function (t) { push("same", t); });
        i = 0; j = 0;
        while (i < n && j < m) {
            if (midA[i] === midB[j]) { push("same", midA[i]); i++; j++; }
            else if (dp[i + 1][j] >= dp[i][j + 1]) { push("del", midA[i]); i++; }
            else { push("ins", midB[j]); j++; }
        }
        while (i < n) push("del", midA[i++]);
        while (j < m) push("ins", midB[j++]);
        a.slice(endA).forEach(function (t) { push("same", t); });

        return parts.map(function (p) {
            var text = escapeHtml(p.text);
            if (p.type === "ins") return '<ins class="history-diff-ins">' + text + "</ins>";
            if (p.type === "del") return '<del class="history-diff-del">' + text + "</del>";
            return text;
        }).join("");
    }

    function renderChange($change, view) {
        var oldHtml = $change.attr("data-old") || "";
        var newHtml = $change.attr("data-new") || "";
        var $view = $change.find(".history-change-view");

        $change.find("[data-view]").removeClass("active").filter("[data-view='" + view + "']").addClass("active");

        if (view === "old" || view === "new") {
            var html = sanitize(view === "old" ? oldHtml : newHtml);
            $view.removeClass("history-diff").html(html || '<span class="text-muted fst-italic">(empty)</span>');
            return;
        }

        var diff = wordDiff(toText(oldHtml), toText(newHtml));
        if (diff === null) {
            $view.removeClass("history-diff").html('<span class="text-muted fst-italic">Too large to compare &mdash; use Before / After.</span>');
            return;
        }
        $view.addClass("history-diff").html(diff || '<span class="text-muted fst-italic">(no text changes)</span>');
    }

    function applyCollapse($el) {
        if ($el[0].scrollHeight <= COLLAPSE_HEIGHT + 20) return;
        $el.addClass("history-collapsed");
        $('<a href="#" class="history-more">Show more</a>').insertAfter($el);
    }

    function formatTimes($root) {
        $root.find(".history-time").each(function () {
            var t = $(this).data("time");
            if (!t) return;
            var m = moment(t);
            $(this).text(m.fromNow()).attr("title", m.format("DD-MMM-YYYY HH:mm"));
        });
    }

    function init(root) {
        $(root).find(".history-timeline[data-history-ready='false']").each(function () {
            var $timeline = $(this).attr("data-history-ready", "true");
            formatTimes($timeline);
            $timeline.find(".history-collapsible").each(function () {
                var $el = $(this);
                $el.html(sanitize($el.attr("data-html")));
                applyCollapse($el);
            });
        });

        $(root).find(".parked-details:not([data-ready])").each(function () {
            var $panel = $(this).attr("data-ready", "true");
            formatTimes($panel);
            $panel.find(".history-rich[data-html]").each(function () {
                var $el = $(this);
                $el.html(sanitize($el.attr("data-html")));
            });
        });
    }

    $(document)
        .off(".backlogHistory")
        .on("click.backlogHistory", ".history-change-toggle", function (e) {
            e.preventDefault();
            var $change = $(this).closest(".history-change");
            var $body = $change.find(".history-change-body");
            var opening = $body.hasClass("d-none");
            $body.toggleClass("d-none", !opening);
            $(this).text(opening ? "Hide changes" : "View changes");
            if (opening && !$change.data("rendered")) {
                $change.data("rendered", true);
                renderChange($change, "diff");
            }
        })
        .on("click.backlogHistory", ".history-change [data-view]", function () {
            renderChange($(this).closest(".history-change"), $(this).data("view"));
        })
        .on("click.backlogHistory", ".history-more", function (e) {
            e.preventDefault();
            var $el = $(this).prev(".history-text");
            var collapsed = $el.toggleClass("history-collapsed").hasClass("history-collapsed");
            $(this).text(collapsed ? "Show more" : "Show less");
        });

    if (window.MutationObserver) {
        new MutationObserver(function () { init(document); })
            .observe(document.body, { childList: true, subtree: true });
    }
    $(function () { init(document); });

    return { init: init, sanitize: sanitize };
})();
