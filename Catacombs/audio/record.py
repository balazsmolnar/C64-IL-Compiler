"""Build Catacombs audio scenes and record them in VICE.

    python Catacombs/audio/record.py                 # every scene
    python Catacombs/audio/record.py EXPLORE COMBAT  # just these
    python Catacombs/audio/record.py --sid 6581      # the older SID model

For each scene this builds the game with SCENE_AUDIO_<name> defined (it
then boots straight into that music/effect and stays there -- see
Audio.Init), and writes into .debug-tmp/audio/:

    <scene>.prg          the scene build itself (run it in VICE to listen)
    <scene>.dump.txt     every SID register write, cycle-timed
    <scene>-<sid>.wav    the audio, 48 kHz mono, recorded in real time

Scenes are built one after another (they share the build output folder)
and then recorded all at once, each in its own VICE. The normal game is
rebuilt at the end, so prg/catacombs.prg is the real game again.
Then run analyze.py on the results.
"""

import argparse
import os
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools'))
import vice_record  # noqa: E402

# scene -> emulated seconds to record (from power-on; the program starts ~3 s in)
SCENES = {
    'EXPLORE': 100,   # the 77 s tune and the start of its second time round
    'COMBAT': 50,     # 21 s loop, twice
    'TITLE': 22,      # the jingle, then the exploration tune taking over
    'DEATH': 20,
    'VICTORY': 16,
    'SFX': 26,        # every effect in turn, 2 s apart, no music
    'SFXMIX': 26,     # the same over the exploration tune
}


def run(args, **kw):
    r = subprocess.run(args, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, **kw)
    if r.returncode != 0:
        sys.exit('FAILED: %s\n%s' % (' '.join(args), r.stdout))
    return r.stdout


def build(scene):
    """Builds the game (scene '' = the real game) to prg/catacombs.prg."""
    run(['dotnet', 'build', os.path.join(ROOT, 'Catacombs', 'Catacombs.csproj'), '-nologo', '-v', 'q',
         '-p:Scene=' + (('AUDIO_' + scene) if scene else '')])
    for d in ('asm/catacombs', 'prg'):
        os.makedirs(os.path.join(ROOT, d), exist_ok=True)
    prg = os.path.join(ROOT, 'prg', 'catacombs.prg')
    out = run(['cmd', '/c', os.path.join(ROOT, 'Catacombs', 'measure.bat')])
    if not os.path.exists(prg):
        sys.exit('FAILED: no prg\n' + out)
    return prg


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('scenes', nargs='*', help='default: all of ' + ' '.join(SCENES))
    ap.add_argument('--sid', choices=['6581', '8580'], default='8580')
    ap.add_argument('--out', default=os.path.join(ROOT, '.debug-tmp', 'audio'))
    ap.add_argument('--no-wav', action='store_true', help='register dumps only (seconds, not minutes)')
    a = ap.parse_args()
    scenes = [s.upper() for s in a.scenes] or list(SCENES)
    for s in scenes:
        if s not in SCENES:
            sys.exit('unknown scene %s (have: %s)' % (s, ' '.join(SCENES)))
    os.makedirs(a.out, exist_ok=True)

    run(['dotnet', 'build', os.path.join(ROOT, 'Compiler', 'Compiler.csproj'), '-nologo', '-v', 'q'])
    for s in scenes:
        shutil.copyfile(build(s), os.path.join(a.out, s + '.prg'))
        print('built', s, flush=True)
    build('')

    for s in scenes:
        vice_record.record(os.path.join(a.out, s + '.prg'), os.path.join(a.out, s + '.dump.txt'),
                           SCENES[s], dump=True)
        print('dumped', s, flush=True)
    if not a.no_wav:
        jobs = [vice_record.Recording(os.path.join(a.out, s + '.prg'),
                                      os.path.join(a.out, '%s-%s.wav' % (s, a.sid)),
                                      SCENES[s], sid=a.sid).start() for s in scenes]
        for s, job in sorted(zip(scenes, jobs), key=lambda x: SCENES[x[0]]):
            print('recorded', job.finish(), flush=True)
