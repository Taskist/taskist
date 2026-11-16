"""Generate wwwroot/css/theme-dark.generated.css from the app's own stylesheets.

Every rule that paints a light surface/border or dark text gets a dark-mode twin, scoped to
[data-coreui-theme=dark] (so it only applies when the user picks Dark or "Match my system" resolves to dark):
  - light backgrounds/tints/borders  -> dark surfaces of the same hue
  - dark text                        -> light text
  - saturated brand/status colours   -> unchanged (and text sitting on them is left alone)
Sidebar and login rules are skipped (already designed for their own background).

Run after changing any CSS under wwwroot/css:
    python tools/theme/generate-dark-css.py
Hand-made dark fixes belong in wwwroot/css/theme-dark.css, never in the generated file.
"""

import colorsys
import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "src", "Presentation", "Taskist.Web", "wwwroot", "css"))
OUTPUT = os.path.join(ROOT, "theme-dark.generated.css")
SKIP_FILES = {"appearance.css", "icons.css", "theme-dark.css", "theme-dark.generated.css", os.path.join("pages", "login.css")}
SKIP_SELECTOR = re.compile(r"\.sidebar|\.nav-group|\.login|\.auth-|#sidebar|\[data-coreui-theme", re.I)
DARK = "[data-coreui-theme=dark]"

NAMED = {"white": (255, 255, 255), "black": (0, 0, 0), "whitesmoke": (245, 245, 245), "gainsboro": (220, 220, 220),
         "lightgray": (211, 211, 211), "lightgrey": (211, 211, 211), "silver": (192, 192, 192), "gray": (128, 128, 128),
         "grey": (128, 128, 128), "dimgray": (105, 105, 105), "darkgray": (169, 169, 169), "ghostwhite": (248, 248, 255),
         "aliceblue": (240, 248, 255), "snow": (255, 250, 250), "ivory": (255, 255, 240), "linen": (250, 240, 230)}

COLOR = re.compile(r"#[0-9a-fA-F]{8}\b|#[0-9a-fA-F]{6}\b|#[0-9a-fA-F]{3,4}\b|rgba?\([^()]*\)|\b(?:" + "|".join(NAMED) + r")\b", re.I)


def parse_color(token):
    t = token.strip().lower()
    if t in NAMED:
        return NAMED[t] + (1.0,)
    if t.startswith("#"):
        h = t[1:]
        if len(h) in (3, 4):
            h = "".join(c * 2 for c in h)
        r, g, b = int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16)
        a = int(h[6:8], 16) / 255 if len(h) == 8 else 1.0
        return r, g, b, a
    inner = t[t.index("(") + 1:-1].replace("/", " ").replace(",", " ").split()
    if len(inner) < 3 or any("var" in x for x in inner):
        return None
    try:
        vals = [float(x[:-1]) * 2.55 if x.endswith("%") else float(x) for x in inner[:3]]
        a = inner[3] if len(inner) > 3 else "1"
        a = float(a[:-1]) / 100 if a.endswith("%") else float(a)
    except ValueError:
        return None
    return vals[0], vals[1], vals[2], a


def hls(rgb):
    return colorsys.rgb_to_hls(rgb[0] / 255, rgb[1] / 255, rgb[2] / 255)


def fmt(h, l, s, a):
    r, g, b = colorsys.hls_to_rgb(h, max(0, min(1, l)), max(0, min(1, s)))
    r, g, b = round(r * 255), round(g * 255), round(b * 255)
    if a >= 0.999:
        return "#%02x%02x%02x" % (r, g, b)
    return "rgba(%d, %d, %d, %s)" % (r, g, b, ("%.3f" % a).rstrip("0").rstrip("."))


def is_saturated_mid(c):
    h, l, s = hls(c)
    return s >= 0.25 and l < 0.62


def dark_surface(c, offset=0.0):
    h, l, s = hls(c)
    a = c[3]
    if l <= 0.12 and s < 0.2 and a < 0.6:
        return fmt(0, 1, 0, a)
    if l < 0.62 or (l >= 0.95 and a < 0.35):
        return None
    if s < 0.15:
        h, s = 0.612, 0.19
    else:
        s = s * 0.5
    return fmt(h, 0.16 + offset + (1 - l) * 0.9, s, a)


def light_text(c):
    h, l, s = hls(c)
    a = c[3]
    if l > 0.45:
        return None
    if s < 0.15:
        h, s = 0.612, 0.12
    else:
        s = s * 0.7
    return fmt(h, 0.92 - l * 0.5, s, a)


def transform(value, role):
    changed = False

    def repl(m):
        nonlocal changed
        c = parse_color(m.group(0))
        if c is None:
            return m.group(0)
        if role == "bg":
            out = dark_surface(c)
        elif role == "border":
            out = dark_surface(c, 0.08)
        elif role == "text":
            out = light_text(c)
        elif role == "shadow":
            h, l, s = hls(c)
            out = dark_surface(c) if l >= 0.8 and c[3] > 0.3 else None
        else:
            out = None
        if out is None:
            return m.group(0)
        changed = True
        return out

    new = COLOR.sub(repl, value)
    return new if changed else None


def role_of(prop):
    p = prop.strip().lower()
    if p.startswith("--"):
        if re.search(r"border|outline|divider|line", p):
            return "border"
        if re.search(r"bg|background|surface|stripe|header|hover|fill|panel|card|tint|shade|track", p):
            return "bg"
        if re.search(r"color|text|fg|ink|muted", p):
            return "text"
        return None
    if p in ("background", "background-color", "background-image"):
        return "bg"
    if p.startswith("border") or p.startswith("outline") or p == "column-rule" or p == "column-rule-color":
        return "border"
    if p in ("color", "fill", "stroke", "caret-color", "text-decoration-color", "-webkit-text-fill-color"):
        return "text"
    if p in ("box-shadow", "text-shadow"):
        return "shadow"
    return None


def split_top(s, sep):
    out, depth, cur, quote = [], 0, [], None
    for ch in s:
        if quote:
            cur.append(ch)
            if ch == quote:
                quote = None
            continue
        if ch in "\"'":
            quote = ch
        elif ch == "(":
            depth += 1
        elif ch == ")":
            depth -= 1
        if ch == sep and depth == 0:
            out.append("".join(cur))
            cur = []
        else:
            cur.append(ch)
    out.append("".join(cur))
    return out


def scope(selector):
    out = []
    for sel in split_top(selector, ","):
        sel = sel.strip()
        if not sel:
            continue
        if sel.startswith(":root"):
            out.append(":root" + DARK + sel[5:])
        elif re.match(r"html\b", sel):
            out.append("html" + DARK + sel[4:])
        elif sel.startswith("body"):
            out.append(DARK + " " + sel)
        else:
            out.append(DARK + " " + sel)
    return ",\n".join(out)


def blocks(css):
    i, n = 0, len(css)
    while i < n:
        j = css.find("{", i)
        if j < 0:
            return
        head = css[i:j].strip()
        depth, k = 1, j + 1
        while k < n and depth:
            if css[k] == "{":
                depth += 1
            elif css[k] == "}":
                depth -= 1
            k += 1
        yield head, css[j + 1:k - 1]
        i = k


def process(css, out, indent=""):
    for head, body in blocks(css):
        if head.startswith("@"):
            name = head.split()[0].lower()
            if name in ("@media", "@supports", "@layer", "@container"):
                if "print" in head.lower() and "screen" not in head.lower():
                    continue
                inner = []
                process(body, inner, indent + "    ")
                if inner:
                    out.append(indent + head + " {\n" + "\n".join(inner) + "\n" + indent + "}")
            continue
        if SKIP_SELECTOR.search(head):
            continue
        decls = [d for d in split_top(body, ";") if ":" in d]
        parsed = []
        for d in decls:
            prop, val = d.split(":", 1)
            parsed.append((prop.strip(), val.strip()))
        kept_saturated_bg = any(role_of(p) == "bg" and any((c := parse_color(m.group(0))) and is_saturated_mid(c) for m in COLOR.finditer(v)) for p, v in parsed)
        new_decls = []
        for prop, val in parsed:
            role = role_of(prop)
            if role is None or (role == "text" and kept_saturated_bg):
                continue
            important = "!important" in val.lower()
            base = re.sub(r"\s*!important\s*$", "", val, flags=re.I)
            new = transform(base, role)
            if new is not None:
                new_decls.append("%s    %s: %s%s;" % (indent, prop, new, " !important" if important else ""))
        if new_decls:
            sel = scope(head).replace("\n", "\n" + indent)
            out.append(indent + sel + " {\n" + "\n".join(new_decls) + "\n" + indent + "}")


def main():
    files = []
    for dirpath, _, names in os.walk(ROOT):
        for name in sorted(names):
            if name.endswith(".css"):
                rel = os.path.relpath(os.path.join(dirpath, name), ROOT)
                if rel not in SKIP_FILES and not name.endswith(".min.css"):
                    files.append(rel)
    files.sort(key=lambda r: (os.sep in r, r))

    parts = ["/* GENERATED by tools/theme/generate-dark-css.py - do not edit. Hand-made dark fixes go in theme-dark.css. */"]
    total = 0
    for rel in files:
        css = open(os.path.join(ROOT, rel), encoding="utf-8-sig").read()
        css = re.sub(r"/\*.*?\*/", "", css, flags=re.S)
        out = []
        process(css, out)
        if out:
            total += len(out)
            parts.append("\n/* " + rel.replace(os.sep, "/") + " */\n" + "\n".join(out))
    with open(OUTPUT, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(parts) + "\n")
    print("wrote %s (%d rules from %d files)" % (os.path.relpath(OUTPUT), total, len(files)))


if __name__ == "__main__":
    sys.exit(main())
