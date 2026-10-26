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

TICK_CYCLES = sidmusic.TICK_CYCLES       # one PAL frame
TICK_S = TICK_CYCLES / sidmusic.PAL_CLOCK

# scene -> what the scene build does, in order (see Audio.Init)
ALL_FX = [('fx', n) for n in ('step a', 'step b', 'door', 'pickup', 'weapon', 'sword', 'growl', 'hurt',
                              'monster death', 'player death')]
SCENES = {
    'EXPLORE': [('play', 'explore')],
    'COMBAT': [('play', 'combat')],
    'TITLE': [('play', 'title')],
    'DEATH': [('play', 'death')],
    'VICTORY': [('play', 'explore'), ('interrupt', 'victory')],
    'RESUME': [('play', 'explore'), ('interrupt', 'combat'), ('resume',)],
    'SFX': ALL_FX,
    'SFXMIX': [('play', 'explore')] + ALL_FX,
    'SFXCOMBAT': [('play', 'combat')] + ALL_FX,
    'SFXGATED': [('play', 'hold')] + ALL_FX,
}


# ---------------------------------------------------------------------------
# 1. register dump against the model
# ---------------------------------------------------------------------------
def split_ticks(writes):
    """Groups a dump into player ticks. Returns (ticks, between) where each
    tick is (cycle, [(reg, value), ...]) and `between` maps a tick index to
    the writes made between the previous tick and that one."""
    ticks, between = [], {}
    i = 0
    n = len(writes)
    while i < n:
        cyc, reg, val = writes[i]
        # A tick starts with $D418, $D417, $D416 (Install's clearing loop
        # does too, but carries on down to $D415).
        if reg == 24 and i + 3 < n and writes[i + 1][1] == 23 and writes[i + 2][1] == 22 and writes[i + 3][1] != 21:
            group = []
            start = cyc
            while i < n and writes[i][0] - start < 4000:
                group.append((writes[i][1], writes[i][2]))
                i += 1
            ticks.append((start, group, writes[i - 1][0] - start))
        else:
            between.setdefault(len(ticks), []).append((reg, val))
            i += 1
    return ticks[:-1], between       # the last tick may have been cut off by the end of the recording


def apply_event(model, sc, ev):
    if ev[0] == 'play':
        model.play(sc.song_index[ev[1]])
    elif ev[0] == 'interrupt':
        model.interrupt(sc.song_index[ev[1]])
    elif ev[0] == 'resume':
        model.resume()
    else:
        model.play_effect(sc.fx_index[ev[1]])


def polite(sc, ev):
    return False                # (the player no longer has polite effects)


def check_dump(sc, scene, path):
    ticks, between = split_ticks(vice_record.read_dump(path))
    model = sidmusic.Player(sc)
    sid = [0] * 25
    pending = list(SCENES[scene])
    skipped = []
    triggers = []
    mismatches = []
    timeline = []                         # per tick: (cycle, model Frame)
    for t, (cyc, group, span) in enumerate(ticks):
        for reg, val in between.get(t, ()):
            sid[reg] = val
        for reg, val in group:
            sid[reg] = val
        before = copy.deepcopy(model)
        model.tick()
        # What the program called between two ticks is not in the dump:
        # when the model stops matching, the next call the scene makes is
        # tried, and has to make it match again.
        # (A polite effect may have been refused by the player and left no
        # trace, so the call after it is tried as well.)
        for k in range(len(pending)):
            if model.sid == sid:
                break
            if k and not polite(sc, pending[k - 1]):
                break
            trial = copy.deepcopy(before)
            apply_event(trial, sc, pending[k])
            trial.tick()
            if trial.sid != sid:
                # (a call made just after this tick, inside its 4000-cycle window)
                trial = copy.deepcopy(model)
                apply_event(trial, sc, pending[k])
            if trial.sid == sid:
                model = trial
                for _ in range(k):
                    skipped.append(' '.join(pending.pop(0)))
                triggers.append((' '.join(pending.pop(0)), t, cyc))
        if model.sid != sid:
            if len(mismatches) < 5:
                diff = [(r, sid[r], model.sid[r]) for r in range(25) if sid[r] != model.sid[r]]
                mismatches.append((t, diff))
            else:
                mismatches.append((t, None))
            model.sid = list(sid)          # resynchronise the register view only
        timeline.append((cyc, model.frame()))
    ok = not mismatches and not pending
    iv = [b[0] - a[0] for a, b in zip(ticks, ticks[1:])]
    spans = sorted(tk[2] for tk in ticks)
    print('  dump : %d ticks, %s' % (len(ticks), 'EXACT MATCH with the model' if ok else 'MISMATCH'))
    print('         tick interval %d..%d cycles (one frame = %d); first to last register write of a tick: median %d, max %d cycles'
          % (min(iv), max(iv), TICK_CYCLES, spans[len(spans) // 2], spans[-1]))
    last = None
    for name, t, cyc in triggers:
        print('         %-18s at tick %4d (%.2f s)%s' % (name, t, cyc / sidmusic.PAL_CLOCK,
                                                          '' if last is None else '  +%d ticks' % (t - last)))
        last = t
    if skipped:
        print('         left no trace (a polite effect the player refused): %s' % ', '.join(skipped))
    if pending:
        print('         never seen: %s' % ', '.join(' '.join(e) for e in pending))
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
    sc_effect_ticks = {e['name']: e['len'] for e in score_module.build().effects}
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
    levels = []
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
        # Level: the strongest of the first three harmonics, as a sine
        # amplitude in dB of full scale. A voice the score has playing must
        # be audible, not just "present" as leakage.
        wsum = np.hanning(hi - lo).sum()
        lvl = max(spec[(freqs > f0 * h * 0.97) & (freqs < f0 * h * 1.03)].max() for h in (1, 2, 3)) * 2 / wsum
        levels.append((v, a, b, f0, 20 * np.log10(lvl + 1e-9)))
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
        # Per voice: notes of 0.3 s or more that come out more than 40 dB
        # under full scale -- a voice the score plays but nobody hears.
        for v in range(3):
            long_ = [l for l in levels if l[0] == v and l[2] - l[1] >= 15]
            if not long_:
                continue
            quiet = [l for l in long_ if l[4] < -40]
            print('         voice %d: %d notes >= 0.3 s, level median %.1f dB, quietest %.1f dB; %d under -40 dB%s'
                  % (v + 1, len(long_), float(np.median([l[4] for l in long_])), min(l[4] for l in long_), len(quiet),
                     (': ' + ', '.join('%.1f s' % tick_time(l[1]) for l in quiet[:8])) if quiet else ''))

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

    # --- effects: level, attack and length of each one
    fx_levels = {}
    for name, t, cyc in triggers:
        if not name.startswith('fx '):
            continue
        a = int(tick_time(t) * rate)
        n = sc_effect_ticks[name[3:]]
        seg = x[a:a + int((n + 2) * TICK_S * rate)]
        if len(seg) < 100:
            continue
        seg = seg - seg.mean()
        pk = float(np.abs(seg).max())
        rms = float(np.sqrt((seg ** 2).mean()))
        e = env[a:a + len(seg)]
        fx_levels[name[3:]] = (20 * np.log10(pk + 1e-9), 20 * np.log10(rms + 1e-9))
        print('         %-16s peak %6.1f dB, rms %6.1f dB, reaches 70%% of its peak after %3.0f ms'
              % (name[3:], fx_levels[name[3:]][0], fx_levels[name[3:]][1],
                 1000 * int(np.argmax(e > 0.7 * e.max())) / rate))

    m = x[int(tick_time(0) * rate):]
    if len(m) > rate and not fx_levels:
        print('         music level: rms %.1f dB, peak %.1f dB' % (
            10 * np.log10((m ** 2).mean() + 1e-12), 20 * np.log10(np.abs(m).max() + 1e-9)))

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
        return fx_levels
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
    return fx_levels


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('scenes', nargs='*')
    ap.add_argument('--dir', default=os.path.join(ROOT, '.debug-tmp', 'audio'))
    a = ap.parse_args()
    sc = score_module.build(test=True)
    all_ok = True
    levels = {}
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
                levels[(scene, sid)] = check_wav(scene, wav, timeline, triggers, wav[:-4] + '.png')
    for sid in ('8580', '6581'):
        solo, gated = levels.get(('SFX', sid)), levels.get(('SFXGATED', sid))
        if solo and gated:
            print('Effects started while voice 3 is sounding a chord, against the same effect alone (%s), peak / rms dB:' % sid)
            for name in solo:
                if name in gated:
                    print('  %-16s %+5.1f / %+5.1f' % (name, gated[name][0] - solo[name][0], gated[name][1] - solo[name][1]))
    sys.exit(0 if all_ok else 1)
