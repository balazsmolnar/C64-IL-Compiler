#!/usr/bin/env python3
"""
Pixel-exact screenshot of a C64 program: runs `prg` in its own VICE instance
(warp mode, throwaway config, own monitor port), waits until `seconds` of
emulated time have passed and saves VICE's own screenshot through the
monitor -- the emulated frame buffer, border included, without the window's
CRT filter -- then closes that instance (only ever by its own pid).

    python C64Presentation/art/vice_grab.py --prg prg/catacombs.prg --png out.png --seconds 8

Used by catacombs_shots.py; the scene build has to be made first
(tools/catacombs-shot.ps1 -Scene X builds prg/catacombs.prg).
"""
import argparse
import os
import re
import socket
import subprocess
import sys
import time

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "tools"))
import vice_record   # noqa: E402  (the monitor helpers)

PAL_CLOCK = 985248


def grab(prg, png, seconds):
    return grab_many(prg, [(png, seconds)])[0]


def grab_many(prg, shots):
    """One VICE instance, several screenshots: shots = [(png, seconds), ...]."""
    prg = os.path.abspath(prg)
    shots = sorted(((os.path.abspath(p), t) for p, t in shots), key=lambda s: s[1])
    for png, _ in shots:
        os.makedirs(os.path.dirname(png), exist_ok=True)
        if os.path.exists(png):
            os.remove(png)
    png = shots[0][0]
    port = vice_record._free_port()
    config = os.path.join(os.path.dirname(png), "vice-grab.ini")
    args = [vice_record.VICE, "-config", config, "-pal", "-warp",
            "-remotemonitor", "-remotemonitoraddress", "ip4://127.0.0.1:%d" % port,
            "-autostartprgmode", "1", "-minimized", "+sound", "-autostart", prg]
    proc = subprocess.Popen(args, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    deadline = time.time() + 120
    try:
        for png, seconds in shots:
            while True:
                if proc.poll() is not None:
                    raise RuntimeError("VICE exited by itself (code %s)" % proc.returncode)
                if time.time() > deadline:
                    raise RuntimeError("VICE never reached %d emulated seconds" % seconds)
                try:
                    reply = vice_record._monitor(port, "sw")
                except OSError:
                    time.sleep(0.5)
                    continue
                m = re.search(r"Stopwatch:\s*(\d+)", reply)
                if m and int(m.group(1)) >= seconds * PAL_CLOCK:
                    break
                time.sleep(0.1)
            vice_record._monitor(port, 'screenshot "%s" 2' % png.replace("\\", "/"), wait=1.0)
        try:
            vice_record._monitor(port, "quit", wait=0.5)
        except OSError:
            pass
        proc.wait(15)
    finally:
        if proc.poll() is None:
            proc.kill()
    for png, _ in shots:
        if not os.path.exists(png):
            raise RuntimeError("no screenshot written to " + png)
    return [png for png, _ in shots]


if __name__ == "__main__":
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--prg", required=True)
    ap.add_argument("--png", required=True)
    ap.add_argument("--seconds", type=float, default=8)
    a = ap.parse_args()
    print("OK:" + grab(a.prg, a.png, a.seconds))
