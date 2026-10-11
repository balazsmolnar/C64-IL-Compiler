"""Check the recordings made by record.py against the score.

    python Catacombs/audio/analyze.py                # every recorded scene
    python Catacombs/audio/analyze.py EXPLORE SFX

Two independent checks per scene, both against the tick-by-tick model of
the player in tools/sidmusic.py (which works on the same compiled data the
game embeds):

1. Register dump (<scene>.dump.txt): every tick of the real 6502 player
   running in VICE must leave the SID registers exactly as the model says.
   This is exact: any wrong note, length, waveform, ADSR, pulse width,
   vibrato step or filter value is a mismatch. For the effect scenes the
   tick an effect was triggered on is found from the dump itself.

2. Audio (<scene>-<sid>.wav): what actually comes out of the emulated SID.
   Level, clipping, DC, silence at the ends of effects, clicks, and -- for
   every note the model says a voice plays -- whether its fundamental is
   really in the spectrum at that time and in tune. Also writes
   <scene>-<sid>.png: the spectrogram with the model's notes drawn on top.

Exit code 0 only if every dump matches.
"""

import argparse
import copy
import os
import sys
import wave

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools'))
sys.path.insert(0, HERE)
import sidmusic  # noqa: E402
import vice_record  # noqa: E402
import score as score_module  # noqa: E402

TICK_CYCLES = 16421                      # KERNAL's CIA1 timer A period on PAL
TICK_S = TICK_CYCLES / sidmusic.PAL_CLOCK

# scene -> (song started at boot or None, effects played in order)
ALL_FX = ['step', 'door', 'pickup', 'weapon', 'sword', 'growl', 'hurt', 'monster death', 'player death']
SCENES = {
    'EXPLORE': ('explore', []),
    'COMBAT': ('combat', []),
    'TITLE': ('title', []),
    'DEATH': ('death', []),
    'VICTORY': ('victory', []),
    'SFX': (None, ALL_FX),
    'SFXMIX': ('explore', ALL_FX),
}


# ---------------------------------------------------------------------------
# 1. register dump against the model
# ---------------------------------------------------------------------------
def split_ticks(writes):
    """Groups a dump into player ticks. Returns (ticks, setup) where each
    tick is (cycle, [(reg, value), ...]) and `setup` maps a tick index to
    the writes made between the previous tick and that one."""
    ticks, between = [], {}
    i = 0
    n = len(writes)
    while i < n:
        cyc, reg, val = writes[i]
        # A tick always starts with $D418 then $D416.
        if reg == 24 and i + 1 < n and writes[i + 1][1] == 22:
            group = []
            start = cyc
            while i < n and writes[i][0] - start < 3000:
                group.append((writes[i][1], writes[i][2]))
                i += 1
            ticks.append((start, group))
        else:
            between.setdefault(len(ticks), []).append((reg, val))
            i += 1
    return ticks[:-1], between       # the last tick may have been cut off by the end of the recording


def check_dump(sc, scene, path):
    song, effects = SCENES[scene]
    ticks, between = split_ticks(vice_record.read_dump(path))
    model = sidmusic.Player(sc)
    sid = [0] * 25
    pending = list(effects)
    triggers = []
    mismatches = []
    started = None
    timeline = []                         # per tick after the song started: model Frame
    for t, (cyc, group) in enumerate(ticks):
        for reg, val in between.get(t, ()):
            sid[reg] = val
            if reg == 23 and song is not None and started is None:
                model.play(sc.song_index[song])   # C64.Music.Play: the only caller-side SID write
                started = t
        for reg, val in group:
            sid[reg] = val
        before = copy.deepcopy(model)
        model.tick()
        if model.sid != sid and pending:
            trial = copy.deepcopy(before)
            trial.play_effect(sc.fx_index[pending[0]])
            trial.tick()
            if trial.sid == sid:
                model = trial
                triggers.append((pending.pop(0), t, cyc))
        if model.sid != sid:
            if len(mismatches) < 5:
                diff = [(r, sid[r], model.sid[r]) for r in range(25) if sid[r] != model.sid[r]]
                mismatches.append((t, diff))
            else:
                mismatches.append((t, None))
            model.sid = list(sid)          # resynchronise the register view only
        timeline.append((cyc, model.frame()))
    ok = not mismatches and not pending and (song is None or started is not None)
    print('  dump : %d ticks, tick period %.1f cycles, %s' % (
        len(ticks), (ticks[-1][0] - ticks[0][0]) / max(1, len(ticks) - 1),
        'EXACT MATCH with the model' if ok else 'MISMATCH'))
    for name, t, cyc in triggers:
        print('         effect %-14s triggered at tick %d (%.2f s)' % (name, t, cyc / sidmusic.PAL_CLOCK))
    if pending:
        print('         effects never seen: %s' % ', '.join(pending))
    for t, diff in mismatches[:5]:
        print('         tick %d: (reg, VICE, model) %s' % (t, diff))
    if len(mismatches) > 5:
        print('         ... %d mismatching ticks in all' % len(mismatches))
    return ok, timeline, triggers


# ---------------------------------------------------------------------------
# 2. audio
# ---------------------------------------------------------------------------
def read_wav(path):
    import numpy as np
    with wave.open(path, 'rb') as w:
        rate, ch, width, n = w.getframerate(), w.getnchannels(), w.getsampwidth(), w.getnframes()
        raw = w.readframes(n)
    x = np.frombuffer(raw, dtype='<i2').astype(np.float64) / 32768.0
    if ch > 1:
        x = x.reshape(-1, ch).mean(axis=1)
    return rate, x


def note_segments(timeline):
    """From the model's per-tick frames: (voice, first tick, last tick + 1,
    mean register frequency, waveform) for every stretch a voice holds one
    note with its gate on (vibrato stays inside one segment; every step of
    an arpeggio is its own, one tick long)."""
    segs = []
    for v in range(3):
        cur = None
        for i, (cyc, fr) in enumerate(timeline):
            freq, pw, ctrl, ad, sr = fr.voices[v]
            on = (ctrl & 1) and (ctrl & 0xF0)
            if cur and on and cur[3] == (ctrl & 0xF0) and abs(freq - cur[2][0]) <= cur[2][0] * 0.02:
                cur[1] = i + 1
                cur[2].append(freq)
                continue
            if cur:
                segs.append((v, cur[0], cur[1], sum(cur[2]) / len(cur[2]), cur[3]))
            cur = [i, i + 1, [freq], ctrl & 0xF0] if on else None
        if cur:
            segs.append((v, cur[0], cur[1], sum(cur[2]) / len(cur[2]), cur[3]))
    return segs


def check_wav(scene, path, timeline, triggers, png):
    import numpy as np
    rate, x = read_wav(path)
    dur = len(x) / rate
    peak = float(np.abs(x).max())
    clipped = int((np.abs(x) >= 0.999).sum())
    rms = float(np.sqrt((x ** 2).mean()))
    print('  audio: %s: %.1f s at %d Hz, peak %.2f of full scale, rms %.3f, %d clipped samples, DC %+.4f'
          % (os.path.basename(path), dur, rate, peak, rms, clipped, float(x.mean())))

    # The recording and the register dump are two separate runs whose
    # clocks start a few seconds apart, so the model's timeline has to be
    # laid over the audio. Coarse guess: the first real sound is the
    # model's first gate. Then slide it (+-1.5 s) to where the model's
    # longest notes have the most energy at their own frequencies.
    hop = int(rate * TICK_S)
    env = np.sqrt(np.convolve(x ** 2, np.ones(hop) / hop, mode='same'))
    t0 = timeline[0][0] / sidmusic.PAL_CLOCK
    gate = np.array([sum(1 for v in fr.voices if (v[2] & 1) and (v[2] & 0xF0)) for _, fr in timeline], float)
    first_gate = int(np.argmax(gate > 0))
    loud = np.nonzero(env > 0.1 * env.max())[0]
    coarse = loud[0] / rate - (t0 + first_gate * TICK_S)
    probe = sorted((s for s in note_segments(timeline) if s[4] != 0x80 and sidmusic.reg_to_hz(s[3]) > 150),
                   key=lambda s: s[1] - s[2])[:40]
    pw = 2048
    pwin = np.hanning(pw)
    pt = np.arange(pw) / rate

    def energy(s):
        total = 0.0
        for v, a, b, freq, wf in probe:
            f0 = sidmusic.reg_to_hz(freq)
            for tick in (a + 1, b - 3):                # the note's start and its end
                lo = int((t0 + s + tick * TICK_S) * rate)
                if lo < 0 or lo + pw > len(x):
                    continue
                seg = x[lo:lo + pw] * pwin
                total += abs(np.dot(seg, np.exp(-2j * np.pi * f0 * pt))) / (np.sqrt((seg ** 2).sum()) + 1e-9)
        return total
    c = max((energy(coarse + k * 0.01), coarse + k * 0.01) for k in range(-150, 151))[1]
    shift = max((energy(c + k * 0.002), c + k * 0.002) for k in range(-8, 9))[1]

    def tick_time(i):
        return t0 + shift + i * TICK_S
    print('         aligned to the dump with a %+.2f s offset' % shift)

    # --- pitch: is each expected note's fundamental really there?
    segs = [s for s in note_segments(timeline) if s[4] != 0x80 and s[2] - s[1] >= 6]
    found = 0
    cents = []
    missing = []
    for v, a, b, freq, wf in segs:
        f0 = sidmusic.reg_to_hz(freq)
        # Analyse the note itself: from just after its attack to its end,
        # at most a third of a second (long enough to resolve bass notes).
        lo = int(tick_time(a + 1) * rate)
        hi = min(int(tick_time(b) * rate), lo + 16384)
        if lo < 0 or hi > len(x) or hi - lo < 2048:
            continue
        seg = x[lo:hi] * np.hanning(hi - lo)
        nfft = 1 << 18
        spec = np.abs(np.fft.rfft(seg, nfft))
        freqs = np.fft.rfftfreq(nfft, 1.0 / rate)
        lobe = 2.0 * rate / (hi - lo)              # half width of a Hann main lobe
        hit = None
        # The fundamental or, for notes the filter or the pulse width
        # leaves without one, the next harmonics.
        for h in (1, 2, 3, 4):
            f = f0 * h
            band = (freqs > f * 0.97) & (freqs < f * 1.03)
            k = int(np.argmax(spec * band))
            around = (freqs > f * 0.6) & (freqs < f * 1.6) & (np.abs(freqs - freqs[k]) > lobe)
            floor = np.percentile(spec[around], 30)
            if spec[k] > 6 * floor and spec[k] >= spec[max(0, k - 1)] and spec[k] >= spec[k + 1]                     and 0 < k and (freqs[k] - f * 0.97) > 0.5 and (f * 1.03 - freqs[k]) > 0.5:
                hit = 1200 * np.log2(freqs[k] / f)
                break
        if hit is None:
            missing.append((v, a, f0))
        else:
            found += 1
            cents.append(hit)
    total = found + len(missing)
    if total:
        c = np.array(cents) if cents else np.zeros(1)
        print('         notes held >= 0.1 s: %d of %d found at the expected pitch; tuning error mean %+.1f, worst %.1f cents'
              % (found, total, c.mean(), np.abs(c).max()))
        for v, a, f0 in missing[:8]:
            print('           not found: voice %d at %.2f s, %.1f Hz' % (v + 1, tick_time(a), f0))

    # --- stuck notes / tails: level while the model says every gate is off
    quiet = np.array([all(not (v[2] & 1) or not (v[2] & 0xF0) for v in fr.voices) for _, fr in timeline])
    runs = []
    i = 0
    while i < len(quiet):
        if quiet[i]:
            j = i
            while j < len(quiet) and quiet[j]:
                j += 1
            if j - i >= 30 and i > 0:      # (not the lead-in before the first sound)
                runs.append((i, j))
            i = j
        else:
            i += 1
    worst = 0.0
    for a, b in runs:
        seg = x[int(tick_time(a + 24) * rate):int(tick_time(b) * rate)]   # 0.4 s for the release to die
        if len(seg) > 4800:
            k = int(rate / 40)                 # ignore sub-40 Hz drift (the SID's DC settling)
            seg = seg - np.convolve(seg, np.ones(k) / k, mode='same')
            worst = max(worst, float(np.sqrt((seg[k:-k] ** 2).mean())))
    if runs:
        print('         %d silent stretches expected (all gates off for >= 0.5 s); loudest residue above 40 Hz: rms %.4f (%s)'
              % (len(runs), worst, 'silent' if worst < 0.004 else 'NOT silent'))

    # --- effects: measured length and envelope of each one (SFX scene)
    if triggers and scene == 'SFX':
        for name, t, cyc in triggers:
            a = int(tick_time(t) * rate)
            seg = env[a:a + int(1.6 * rate)]
            if not len(seg):
                continue
            pk = float(seg.max())
            above = np.nonzero(seg > 0.1 * pk)[0]
            print('         effect %-14s peak %.2f, attack %3.0f ms, audible for %4.0f ms'
                  % (name, pk, 1000 * int(np.argmax(seg > 0.7 * pk)) / rate,
                     1000 * (above[-1] - above[0]) / rate if len(above) else 0))

    # --- clicks: single-sample jumps far beyond what the waveforms themselves do
    d = np.abs(np.diff(x))
    print('         largest sample-to-sample jump %.2f of full scale (pulse/saw edges are the normal maximum)'
          % float(d.max()))

    # --- spectrogram with the model's notes on top
    try:
        import matplotlib
        matplotlib.use('Agg')
        import matplotlib.pyplot as plt
    except ImportError:
        return
    n = 2048
    step = 512
    frames = 1 + (len(x) - n) // step
    idx = np.arange(n)[None, :] + step * np.arange(frames)[:, None]
    S = np.abs(np.fft.rfft(x[idx] * np.hanning(n), axis=1)).T
    S = 20 * np.log10(S + 1e-6)
    fmax = 4000
    rows = int(fmax / (rate / n))
    fig, ax = plt.subplots(figsize=(max(12, min(60, dur * 0.45)), 6), dpi=100)
    ax.imshow(S[:rows], origin='lower', aspect='auto', cmap='magma',
              extent=[0, dur, 0, fmax], vmin=S.max() - 70, vmax=S.max())
    colors = ['#4cc9f0', '#80ed99', '#ffd166']
    for v, a, b, freq, wf in note_segments(timeline):
        if wf == 0x80:
            continue
        f0 = sidmusic.reg_to_hz(freq)
        ax.plot([tick_time(a), tick_time(b)], [f0, f0], color=colors[v], lw=1.2)
    ax.set_yscale('symlog', linthresh=200)
    ax.set_ylim(30, fmax)
    ax.set_yticks([55, 110, 220, 440, 880, 1760, 3520])
    ax.set_yticklabels(['A1', 'A2', 'A3', 'A4', 'A5', 'A6', 'A7'])
    ax.set_xlabel('seconds (lines: fundamentals the score expects -- blue voice 1, green voice 2, yellow voice 3)')
    ax.set_title('%s: recorded SID output' % os.path.basename(path))
    fig.tight_layout()
    fig.savefig(png)
    plt.close(fig)
    print('         spectrogram: ' + png)


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('scenes', nargs='*')
    ap.add_argument('--dir', default=os.path.join(ROOT, '.debug-tmp', 'audio'))
    a = ap.parse_args()
    sc = score_module.build()
    all_ok = True
    for scene in ([s.upper() for s in a.scenes] or list(SCENES)):
        dump = os.path.join(a.dir, scene + '.dump.txt')
        if not os.path.exists(dump):
            continue
        print(scene)
        ok, timeline, triggers = check_dump(sc, scene, dump)
        all_ok &= ok
        for sid in ('8580', '6581'):
            wav = os.path.join(a.dir, '%s-%s.wav' % (scene, sid))
            if os.path.exists(wav):
                check_wav(scene, wav, timeline, triggers, wav[:-4] + '.png')
    sys.exit(0 if all_ok else 1)
