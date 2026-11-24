// Mobile menu
(function () {
    var btn = document.querySelector('.nav-toggle'), links = document.getElementById('navLinks');
    if (btn && links) btn.addEventListener('click', function () {
        var open = links.classList.toggle('open');
        btn.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
    // Remember the language the visitor picks (used when they open the bare domain next time).
    document.querySelectorAll('.lang-switch').forEach(function (a) {
        a.addEventListener('click', function () {
            document.cookie = 'ux-lang=' + a.dataset.lang + ';path=/;max-age=31536000;samesite=lax';
        });
    });

    // Day / night switch: remembered in this browser.
    document.querySelectorAll('.theme-toggle').forEach(function (t) {
        t.addEventListener('click', function () {
            var next = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-theme', next);
            try { localStorage.setItem('ux-theme-v3', next); } catch (e) { }
        });
    });

    // Products menu: opens on hover (desktop) and on click/tap (all screens).
    document.querySelectorAll('.nav-dd').forEach(function (dd) {
        var b = dd.querySelector('.nav-dd-btn');
        b.addEventListener('click', function (e) {
            e.stopPropagation();
            var open = dd.classList.toggle('open');
            b.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
        document.addEventListener('click', function (e) { if (!dd.contains(e.target)) { dd.classList.remove('open'); b.setAttribute('aria-expanded', 'false'); } });
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape') { dd.classList.remove('open'); b.setAttribute('aria-expanded', 'false'); } });
    });
    if (location.pathname.toLowerCase().indexOf('/products') > 0) {
        var pb = document.querySelector('.nav-dd-btn'); if (pb) pb.classList.add('active');
    }

    // Highlight the menu item for the current page, and on the home page the section in view.
    var path = location.pathname.toLowerCase();
    document.querySelectorAll('.nav-links a[data-page]').forEach(function (a) {
        if (path.endsWith('/' + a.dataset.page)) a.classList.add('active');
    });
    var secLinks = Array.prototype.slice.call(document.querySelectorAll('.nav-links a[data-section]'));
    var sections = secLinks.map(function (a) { return document.getElementById(a.dataset.section); });
    function mark(id) { secLinks.forEach(function (a) { a.classList.toggle('active', a.dataset.section === id); }); }
    if (sections.some(Boolean)) {
        // Click: highlight at once, scroll smoothly, close the mobile menu.
        secLinks.forEach(function (a, i) {
            a.addEventListener('click', function (e) {
                if (!sections[i]) return;
                e.preventDefault();
                mark(a.dataset.section);
                sections[i].scrollIntoView({ behavior: 'smooth', block: 'start' });
                history.replaceState(null, '', '#' + a.dataset.section);
                if (links) links.classList.remove('open');
            });
        });
        // Scroll: the last section whose top has passed below the menu bar.
        var ticking = false;
        function onScroll() {
            ticking = false;
            var current = null;
            sections.forEach(function (s) { if (s && s.getBoundingClientRect().top <= 120) current = s.id; });
            if ((window.innerHeight + window.scrollY) >= document.body.scrollHeight - 2) {
                var last = sections.filter(Boolean).pop(); if (last && last.getBoundingClientRect().top < window.innerHeight) current = current || last.id;
            }
            mark(current);
        }
        window.addEventListener('scroll', function () { if (!ticking) { ticking = true; requestAnimationFrame(onScroll); } }, { passive: true });
        onScroll();
    }
})();
