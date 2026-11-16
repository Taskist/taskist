/*
    Taskist service worker.
    Strategy:
      - Static assets (lib / css / js / images / fonts / theme): cache-first only when the URL
        is versioned (asp-append-version "?v=" changes with the file); unversioned URLs are
        network-first with a cache fallback, so an edited file is never served stale.
      - Navigations (HTML pages): network-first, fall back to the cached offline page.
        Authenticated pages are NEVER cached (avoids leaking one user's page to another
        and serving stale data).
      - Anything non-GET, or under /api, /Login, /hangfire, /alertshub: bypass the SW entirely.
*/

const VERSION = 'v2';
const STATIC_CACHE = `wp-static-${VERSION}`;
const OFFLINE_URL = '/offline.html';

const PRECACHE = [
    OFFLINE_URL,
    '/images/brand/logo.png',
    '/images/brand/logo_icon.png',
    '/images/brand/icon-192.png'
];

const STATIC_PREFIXES = ['/lib/', '/css/', '/js/', '/images/', '/theme/', '/fonts/'];
const BYPASS_PREFIXES = ['/api', '/login', '/account', '/hangfire', '/alertshub', '/profiler', '/updates', '/download'];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then((cache) => Promise.allSettled(PRECACHE.map((url) => cache.add(url))))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((keys) => Promise.all(
                keys.filter((k) => k.startsWith('wp-static-') && k !== STATIC_CACHE)
                    .map((k) => caches.delete(k))
            ))
            .then(() => self.clients.claim())
    );
});

function isStaticAsset(url) {
    return STATIC_PREFIXES.some((p) => url.pathname.startsWith(p));
}

function isBypassed(url) {
    const path = url.pathname.toLowerCase();
    return BYPASS_PREFIXES.some((p) => path.startsWith(p));
}

self.addEventListener('fetch', (event) => {
    const req = event.request;

    if (req.method !== 'GET')
        return;

    const url = new URL(req.url);

    if (url.origin !== self.location.origin)
        return;

    if (isBypassed(url))
        return;

    if (isStaticAsset(url)) {
        const store = (res) => {
            if (res && res.ok && res.type === 'basic') {
                const copy = res.clone();
                caches.open(STATIC_CACHE).then((cache) => cache.put(req, copy));
            }
            return res;
        };

        if (url.searchParams.has('v')) {
            event.respondWith(
                caches.match(req).then((cached) => cached || fetch(req).then(store))
            );
        } else {
            event.respondWith(
                fetch(req).then(store).catch(() => caches.match(req))
            );
        }
        return;
    }

    if (req.mode === 'navigate') {
        event.respondWith(
            fetch(req).catch(() => caches.match(OFFLINE_URL))
        );
    }
});

self.addEventListener('message', (event) => {
    if (event.data === 'skipWaiting')
        self.skipWaiting();
});
