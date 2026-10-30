"""
The sprite frames themselves, as text grids (see characters_art.py for the
colour letters). MC: 12 double-wide pixels x 21. HI: 24 x 21, one colour.

A character taller than one sprite is two sprites stacked (top, bottom); the
adventurer's top half also has a hi-res overlay sprite in front of it carrying
the torch flame's core and the torchlight catching his edges.
"""

MC = {}
HI = {}


def mirror(rows):
    return [r[::-1] for r in rows]


def blank(width=12):
    return [["."] * width for _ in range(21)]


def done(grid):
    return ["".join(r) for r in grid]


def hi(spec, letter="Y"):
    """A hi-res frame from {row: "x x0-x1 ..."}."""
    g = blank(24)
    for y, runs in spec.items():
        for run in runs.split():
            a, _, b = run.partition("-")
            for x in range(int(a), int(b or a) + 1):
                g[y][x] = letter
    return done(g)


def shadow(g, cx, y, half, rows=3):
    """A dithered black ground shadow, drawn only where the frame is empty."""
    for dy in range(rows):
        h = half - (0 if dy == rows // 2 else 1)
        for x in range(cx - h, cx + h + 1):
            if 0 <= x < 12 and 0 <= y + dy < 21 and g[y + dy][x] == "." and (x + y + dy) % 2 == 0:
                g[y + dy][x] = "K"


def outline(g, letters, over=""):
    """Black around the pixels whose letter is in `letters`: into empty pixels,
    and (left and right only) into pixels whose letter is in `over`."""
    h, w = len(g), len(g[0])
    put = []
    for y in range(h):
        for x in range(w):
            if g[y][x] not in letters:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < w and 0 <= ny < h:
                    if g[ny][nx] == "." or (dy == 0 and g[ny][nx] in over):
                        put.append((nx, ny))
    for x, y in put:
        g[y][x] = "K"


import characters_player as _player   # noqa: E402  (each adds its frames to MC / HI)
import characters_others as _others   # noqa: E402

# The order of the frames in the two sheets = the frame numbers in Sprites.cs.
MC_ORDER = _player.PLAYER_MC + _others.MC_NAMES
HI_ORDER = _player.PLAYER_HI + _others.HI_NAMES


def player(d, step, top=None):
    legs_ = "down" if d == "up" else d
    t = top or d
    return [("p_ov_" + t, 0, 0), ("p_top_" + t, 0, 0), ("p_leg_%s%d" % (legs_, step), 0, 21)]


def previews():
    walk = [("%s%d" % (d, i), player(d, i)) for d in ("down", "up", "left", "right") for i in range(4)]
    poses = [("raise", player("down", 1, "raise")), ("slash", player("down", 1, "slash")),
             ("fallen", [("spark", -6, 18), ("p_fallen", 0, 21)])]
    return [("player", walk + poses, dict(cols=8, scale=6)),
            ("player_small", walk + poses, dict(cols=19, scale=2)),
            ("others", _others.preview_groups(), dict(cols=8, scale=5, cell=(34, 50)))]
