#!/usr/bin/env python3
"""
Captures the Catacombs screenshots the deck shows (pixel-exact VICE frames,
see vice_grab.py) into C64Presentation/art/src/shots/<SCENE>_<seconds>.png.

    python C64Presentation/art/catacombs_shots.py [SCENE ...]

Each scene is a Catacombs scene build (`#if SCENE_...` in Catacombs/*.cs),
built as tools/catacombs-shot.ps1 builds it (but without its VICE
window) and then grabbed in one VICE instance at the listed emulated
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
    "FLOW": [6],                    # the title screen (shot_title, A2)
    "COMBAT": [10],                 # the Bone Warden encounter (shot_boss, A3)
    "MONSTERROOM": [12],            # the room and its guardian (shot_room, A1);
                                    # at 6-13 s the two always overlap, 12 s is the clearest
}


def build(scene):
    """The scene build, as tools/catacombs-shot.ps1 makes it, without
    starting VICE: Compiler, Catacombs with -p:Scene, measure.bat."""
    def run(args):
        r = subprocess.run(args, capture_output=True, text=True)
        if r.returncode != 0:
            raise RuntimeError("scene %s did not build:\n%s%s" % (scene, r.stdout, r.stderr))
    run(["dotnet", "build", os.path.join(ROOT, "Compiler", "Compiler.csproj"), "-nologo", "-v", "q"])
    run(["dotnet", "build", os.path.join(ROOT, "Catacombs", "Catacombs.csproj"), "-nologo", "-v", "q",
         "-p:Scene=" + scene])
    os.makedirs(os.path.join(ROOT, "asm", "catacombs"), exist_ok=True)
    run(["cmd", "/c", os.path.join(ROOT, "Catacombs", "measure.bat")])


def main(scenes):
    os.makedirs(SHOTS, exist_ok=True)
    prg = os.path.join(ROOT, "prg", "catacombs.prg")
    for scene in scenes:
        build(scene)
        shots = [(os.path.join(SHOTS, "%s_%d.png" % (scene, t)), t) for t in SCENES[scene]]
        for png in vice_grab.grab_many(prg, shots):
            print("OK:" + png)


if __name__ == "__main__":
    main(sys.argv[1:] or list(SCENES))
