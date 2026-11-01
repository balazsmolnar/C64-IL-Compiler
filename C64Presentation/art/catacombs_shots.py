#!/usr/bin/env python3
"""
Captures the Catacombs screenshots the deck shows (pixel-exact VICE frames,
see vice_grab.py) into C64Presentation/art/src/shots/<SCENE>_<seconds>.png.

    python C64Presentation/art/catacombs_shots.py [SCENE ...]

Each scene is a Catacombs scene build (`#if SCENE_...` in Catacombs/*.cs),
built with tools/catacombs-shot.ps1 and then grabbed at the listed emulated
times. Slow (a build per scene) and needs VICE, so presentation_art.py does
NOT run it: it converts the PNGs already captured here (they are checked
in). Re-run it only when the game's look changes. It changes no file under
Catacombs/ (the scene builds' output goes to asm/ and prg/), but every
scene opens a VICE window, so keep re-captures rare.
"""
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
SHOTS = os.path.join(HERE, "src", "shots")
sys.path.insert(0, HERE)
import vice_grab   # noqa: E402

# scene -> emulated seconds after power-on to grab at
SCENES = {
    "FLOW": [6],             # the title screen (shot_title)
    "COMBAT": [10],          # the Bone Warden encounter (shot_boss)
    "CHAR_SKELETON": [11],   # the player meets a skeleton (shot_room)
}


def build(scene):
    script = os.path.join(ROOT, "tools", "catacombs-shot.ps1")
    out = os.path.join(ROOT, ".debug-tmp", "art", "build_%s_" % scene)
    os.makedirs(os.path.dirname(out), exist_ok=True)
    r = subprocess.run(["pwsh", "-NoProfile", "-File", script, "-Root", ROOT, "-Scene", scene,
                        "-OutPrefix", out, "-Shots", "1", "-WaitSeconds", "3"],
                       capture_output=True, text=True)
    if "BUILD_FAILED" in r.stdout or r.returncode != 0:
        raise RuntimeError("scene %s did not build:\n%s%s" % (scene, r.stdout, r.stderr))


def main(scenes):
    os.makedirs(SHOTS, exist_ok=True)
    prg = os.path.join(ROOT, "prg", "catacombs.prg")
    for scene in scenes:
        build(scene)
        for t in SCENES[scene]:
            png = vice_grab.grab(prg, os.path.join(SHOTS, "%s_%d.png" % (scene, t)), t)
            print("OK:" + png)


if __name__ == "__main__":
    main(sys.argv[1:] or list(SCENES))
