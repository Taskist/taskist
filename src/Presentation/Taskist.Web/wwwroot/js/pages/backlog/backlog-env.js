(function () {
    "use strict";

    var panel = document.getElementById("envPanel");
    if (!panel) return;

    var raw = panel.getAttribute("data-env");
    if (!raw) return;

    var env;
    try { env = JSON.parse(raw); } catch (e) { panel.textContent = "Environment data could not be read."; return; }
    if (!env) return;

    function esc(v) {
        return String(v == null ? "" : v)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    function section(title, rows) {
        var body = rows
            .filter(function (r) { return r[1] !== undefined && r[1] !== null && String(r[1]).trim() !== ""; })
            .map(function (r) {
                return '<div class="env-row"><span class="env-key">' + esc(r[0]) + '</span>' +
                    '<span class="env-val">' + esc(r[1]) + "</span></div>";
            }).join("");
        if (!body) return "";
        return '<div class="env-block"><div class="env-head">' + esc(title) + "</div>" + body + "</div>";
    }

    var displays = [];
    if (Array.isArray(env.displays) && env.displays.length) {
        displays = env.displays.map(function (d, i) {
            return [
                "Display " + (i + 1) + (d.primary ? " (primary)" : ""),
                (d.resolution || "") + (d.scaleFactor ? " @" + d.scaleFactor + "x" : "")
            ];
        });
    }

    var html = "";
    html += section("System", [
        ["OS", env.operatingSystem],
        ["OS version", env.osVersion],
        ["Architecture", env.architecture],
        ["Machine", env.machineName],
        ["Locale", env.locale],
        ["Time zone", env.timeZone],
        ["Theme", env.theme],
        ["Uptime", env.uptime]
    ]);
    html += section("Hardware", [
        ["CPU", env.cpu],
        ["CPU load", env.cpuLoad],
        ["Memory (total)", env.memory],
        ["Memory (free)", env.freeMemory]
    ]);
    html += section("Displays", displays.length ? displays : [["Primary", env.screenResolution]]);
    html += section("App & session", [
        ["App version", env.appVersion],
        ["Electron", env.electronVersion],
        ["Chrome", env.chromeVersion],
        ["Node", env.nodeVersion],
        ["Environment", env.baseUrl]
    ]);
    html += section("Network", [
        ["Status", env.networkStatus],
        ["Captured at", env.capturedAt]
    ]);

    panel.innerHTML = html || '<div class="text-muted">No environment details.</div>';
})();
