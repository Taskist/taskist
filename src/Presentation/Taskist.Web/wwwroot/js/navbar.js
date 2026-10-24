(function () {
    "use strict";

    var input = document.getElementById('navSearchInput');
    if (!input) return;

    var box = document.getElementById('navSearchResults');
    var url = input.dataset.searchUrl;
    var noResults = input.dataset.noresults || 'No results';
    var timer = null;
    var current = [];
    var activeIndex = -1;

    function itemEls() {
        return Array.prototype.slice.call(box.querySelectorAll('.header-search__item'));
    }

    function highlight() {
        var els = itemEls();
        els.forEach(function (el, i) { el.classList.toggle('active', i === activeIndex); });
        if (activeIndex >= 0 && els[activeIndex]) els[activeIndex].scrollIntoView({ block: 'nearest' });
    }

    function esc(s) {
        return (s || '').replace(/[&<>"]/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c];
        });
    }

    function close() { box.classList.remove('show'); }

    function render(items) {
        current = items || [];
        activeIndex = -1;
        if (!current.length) {
            box.innerHTML = '<div class="header-search__empty">' + esc(noResults) + '</div>';
            box.classList.add('show');
            return;
        }
        var groups = {};
        current.forEach(function (r) { (groups[r.group] = groups[r.group] || []).push(r); });
        var html = '';
        Object.keys(groups).forEach(function (g) {
            html += '<div class="header-search__group">' + esc(g) + '</div>';
            groups[g].forEach(function (r) {
                html += '<a class="header-search__item" href="' + r.url + '">' +
                    '<i class="' + r.icon + '"></i>' +
                    '<span class="hs-label">' + esc(r.label) + '</span>' +
                    (r.sublabel ? '<span class="hs-sub">' + esc(r.sublabel) + '</span>' : '') +
                    '</a>';
            });
        });
        box.innerHTML = html;
        box.classList.add('show');
    }

    function run() {
        var q = input.value.trim();
        if (q.length < 2) { close(); box.innerHTML = ''; return; }
        $.getJSON(url, { q: q }).done(render).fail(close);
    }

    input.addEventListener('input', function () {
        clearTimeout(timer);
        timer = setTimeout(run, 250);
    });

    input.addEventListener('keydown', function (e) {
        var els = itemEls();
        if (e.key === 'ArrowDown') {
            if (!els.length) return;
            e.preventDefault();
            activeIndex = Math.min(activeIndex + 1, els.length - 1);
            highlight();
        } else if (e.key === 'ArrowUp') {
            if (!els.length) return;
            e.preventDefault();
            activeIndex = Math.max(activeIndex - 1, 0);
            highlight();
        } else if (e.key === 'Enter') {
            if (!els.length) return;
            e.preventDefault();
            var target = activeIndex >= 0 ? els[activeIndex] : els[0];
            if (target) window.location.href = target.getAttribute('href');
        } else if (e.key === 'Escape') {
            close();
        }
    });

    document.addEventListener('click', function (e) {
        if (!e.target.closest('#navSearch')) close();
    });

    function isTypingTarget(el) {
        if (!el) return false;
        var t = (el.tagName || '').toLowerCase();
        return t === 'input' || t === 'textarea' || t === 'select' || el.isContentEditable;
    }

    document.addEventListener('keydown', function (e) {
        var key = (e.key || '').toLowerCase();
        if ((e.ctrlKey || e.metaKey) && key === 'k') {
            e.preventDefault();
            input.focus();
            input.select();
        } else if (key === '/' && !isTypingTarget(e.target)) {
            e.preventDefault();
            input.focus();
        }
    });
})();
