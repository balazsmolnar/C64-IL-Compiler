"""Score compiler for the C64.Music player (asm/C64Music.asm).

A score is a Python file that builds a `Score`: instruments, patterns,
songs and sound effects, then writes

  * the data tables the player reads, as 64tass source (`write_asm`), to be
    embedded in the program with `[assembly: RawAssembly(Resource = ...)]`;
  * optionally a C# file of song/effect numbers (`write_cs`).

`Player` is a tick-for-tick model of the 6502 player working on the same
compiled bytes. It is what `Score.simulate` uses to say which SID register
values a song should produce, tick by tick -- the reference a recording is
checked against (see Catacombs/audio/analyze.py).

Score format
------------
Instrument: ADSR, a wave program, pulse width, pulse modulation, vibrato.

    s.instrument('lead', ad=0x28, sr=0xA9, wave=[(0x41, 0)],
                 pw=0x800, pws=0x18, vib=(5, 14))

  wave      rows of (waveform, pitch), one row per `wspd` ticks.
            waveform: the $D404 value with the gate bit set ($41 pulse, $21
            saw, $11 triangle, $81 noise; an even value has the gate off),
            or 0 to leave the waveform as it is.
            pitch: a number = semitones above the played note (0..127), or
            a note name ('c-6') = that absolute pitch, whatever is played.
            The last row repeats for as long as the note lasts; end the
            list with ('jump', n) to loop back to row n instead.
  wspd      ticks per wave row (default 1)
  pw        12-bit pulse width, $200..$DFF
  pws       pulse width change per tick; it swings between $200 and $E00
  vib       (half period in ticks, delay in ticks). Depth grows with the
            period: about 7 cents per tick of half period.

Pattern: a string of tokens, one voice, any number of rows.

    s.pattern('bassA', '@bass l2 a-1 a-1 a-2 a-1 | l4 e-2 r')

  @name     instrument for the notes that follow
  lN        default length, in rows
  c-3 c#3 eb3 c3   a note; append :N for its own length
  r         rest (gate off; the release rings on), r:N
  ~         tie: nothing changes for N rows
  |         ignored (bar line)

Song: one order list per voice.

    s.song('explore', tempo=8, voices=[[...], [...], [...]], end='loop',
           filter=dict(voices=0b001, res=10, mode='lp', cut=(0x30, 0x70, 40)))

  An order list holds pattern names, T(n) (transpose everything after it by
  n semitones) and at most one LOOP marker (where the song restarts from;
  default the beginning). end: 'loop', 'stop', or ('chain', 'songname').
  tempo is ticks per row; the player ticks 60 times a second.

Effect: one note of one instrument for a number of ticks, played on the
voice chosen by `Score(sfx_voice=...)`. Effects are numbered in the order
they are declared, and a higher number wins over a lower one still playing.
"""

import re

PAL_CLOCK = 985248
TICK_HZ = PAL_CLOCK / 16421.0      # the KERNAL's CIA1 timer on a PAL C64: 60 Hz

REST = 0x60
TIE = 0x61

_SEMI = {'c': 0, 'd': 2, 'e': 4, 'f': 5, 'g': 7, 'a': 9, 'b': 11}


def note_number(name):
    """'c-3', 'c#3', 'eb3', 'c3' -> 0..95 (0 = C-0)."""
    m = re.fullmatch(r'([a-g])([#b-]?)(\d)', name.lower())
    if not m:
        raise ValueError('bad note name: %r' % name)
    n = _SEMI[m.group(1)] + {'#': 1, 'b': -1, '-': 0, '': 0}[m.group(2)] + 12 * int(m.group(3))
    if not 0 <= n <= 95:
        raise ValueError('note out of range: %r' % name)
    return n


def note_name(n):
    return ['C-', 'C#', 'D-', 'D#', 'E-', 'F-', 'F#', 'G-', 'G#', 'A-', 'A#', 'B-'][n % 12] + str(n // 12)


def freq_value(n):
    """SID frequency register value of note n on a PAL machine."""
    return min(round(440.0 * 2 ** ((n - 57) / 12) * 16777216 / PAL_CLOCK), 65535)


FREQ = [freq_value(n) for n in range(96)]


def reg_to_hz(v):
    return v * PAL_CLOCK / 16777216.0


class T:
    """Order list entry: transpose what follows by n semitones."""
    def __init__(self, n):
        self.n = n


LOOP = 'LOOP!'


class Score:
    def __init__(self, sfx_voice=2, max_rows=24):
        self.sfx_voice = sfx_voice
        self.max_rows = max_rows
        self.instruments = []            # dicts
        self.ins_index = {}
        self.wave_l = []
        self.wave_r = []
        self.wave_programs = {}          # tuple(rows) -> start index
        self.patterns = [('(empty)', [0xFF], 0)]   # (name, bytes, rows)
        self.pat_index = {}
        self.pat_notes = {}              # name -> (lowest, highest) note, for range checks
        self.songs = []
        self.song_index = {}
        self.effects = []
        self.fx_index = {}
        self._compiled = None

    # -- instruments -------------------------------------------------------
    def _wave_program(self, rows):
        rows = list(rows)
        jump = None
        if rows and rows[-1][0] == 'jump':
            jump = rows.pop()[1]
        if not rows:
            raise ValueError('empty wave program')
        key = (tuple(rows), jump)
        if key in self.wave_programs:
            return self.wave_programs[key]
        start = len(self.wave_l)
        for wf, pitch in rows:
            if not 0 <= wf < 0xFF:
                raise ValueError('bad waveform %r' % (wf,))
            if isinstance(pitch, str):
                p = 0x80 | note_number(pitch)
            else:
                if not 0 <= pitch <= 0x7F:
                    raise ValueError('relative pitch must be 0..127')
                p = pitch
            self.wave_l.append(wf)
            self.wave_r.append(p)
        self.wave_l.append(0xFF)
        self.wave_r.append(start + (len(rows) - 1 if jump is None else jump))
        if len(self.wave_l) > 256:
            raise ValueError('wave table is over 256 rows')
        self.wave_programs[key] = start
        return start

    def instrument(self, name, ad, sr, wave, wspd=1, pw=0x800, pws=0, vib=(0, 0)):
        if name in self.ins_index:
            raise ValueError('duplicate instrument ' + name)
        if not 0x200 <= pw <= 0xDFF:
            raise ValueError('%s: pw must be $200..$DFF' % name)
        if not (0 <= pws <= 255 and 1 <= wspd <= 255 and 0 <= vib[0] <= 255 and 0 <= vib[1] <= 127):
            raise ValueError('%s: parameter out of range' % name)
        rel = [p for wf, p in wave if wf != 'jump' and not isinstance(p, str)]
        self.ins_index[name] = len(self.instruments)
        self.instruments.append(dict(
            name=name, ad=ad, sr=sr, wave=self._wave_program(wave), wspd=wspd,
            pw=(pw >> 8) | (pw & 0xF0), pws=pws, vspd=vib[0], vdly=vib[1],
            maxrel=max(rel) if rel else 0))
        if len(self.instruments) > 63:
            raise ValueError('too many instruments')

    # -- patterns ----------------------------------------------------------
    def pattern(self, name, text):
        if name in self.pat_index:
            raise ValueError('duplicate pattern ' + name)
        out = []
        rows = 0
        length = 4
        cur_len = None
        ins = None
        emitted_ins = None
        lo, hi = 999, -1

        def emit(event, n):
            nonlocal cur_len, rows
            first = True
            while n > 0:
                chunk = min(n, self.max_rows)
                if chunk != cur_len:
                    out.append(0x80 | (chunk - 1))
                    cur_len = chunk
                out.append(event if first else TIE)
                first = False
                n -= chunk
                rows += chunk

        for tok in text.split():
            if tok == '|':
                continue
            if tok[0] == '@':
                if tok[1:] not in self.ins_index:
                    raise ValueError('%s: unknown instrument %s' % (name, tok))
                ins = tok[1:]
                continue
            m = re.fullmatch(r'l(\d+)', tok)
            if m:
                length = int(m.group(1))
                continue
            body, _, n = tok.partition(':')
            n = int(n) if n else length
            if n < 1:
                raise ValueError('%s: bad length in %s' % (name, tok))
            if body == 'r':
                emit(REST, n)
            elif body == '~':
                emit(TIE, n)
            else:
                note = note_number(body)
                if ins is None:
                    raise ValueError('%s: note before any @instrument' % name)
                if ins != emitted_ins:
                    out.append(0xC0 + self.ins_index[ins])
                    emitted_ins = ins
                top = note + self.instruments[self.ins_index[ins]]['maxrel']
                lo, hi = min(lo, note), max(hi, top)
                emit(note, n)
        out.append(0xFF)
        if len(out) > 256:
            raise ValueError('%s: pattern is over 256 bytes' % name)
        self.pat_index[name] = len(self.patterns)
        self.pat_notes[name] = (lo, hi)
        self.patterns.append((name, out, rows))
        if len(self.patterns) > 128:
            raise ValueError('too many patterns')

    # -- songs -------------------------------------------------------------
    def song(self, name, tempo, voices, end='loop', filter=None, volume=15):
        if name in self.song_index:
            raise ValueError('duplicate song ' + name)
        if len(voices) != 3:
            raise ValueError('a song has three voices')
        if tempo < 3:
            raise ValueError('tempo must be at least 3 ticks per row')
        f = dict(voices=0, res=0, mode='lp', cut=(0, 0, 0))
        f.update(filter or {})
        mode = {'lp': 1, 'bp': 2, 'hp': 4, 'lp+hp': 5, 'off': 0}[f['mode']]
        cmin, cmax, cspd = f['cut']
        if cspd and not cmin < cmax:
            raise ValueError('%s: filter sweep needs min < max' % name)
        lengths = []
        for v, order in enumerate(voices):
            if order.count(LOOP) > 1:
                raise ValueError('%s: more than one LOOP in a voice' % name)
            total = before = 0
            trans = 0
            for item in order:
                if item == LOOP:
                    before = total
                elif isinstance(item, T):
                    trans = item.n
                    if not -32 <= trans <= 92:
                        raise ValueError('%s: transpose out of range' % name)
                else:
                    if item not in self.pat_index:
                        raise ValueError('%s: unknown pattern %s' % (name, item))
                    lo, hi = self.pat_notes[item]
                    if hi >= 0 and (lo + trans < 0 or hi + trans > 95):
                        raise ValueError('%s: %s transposed %+d leaves the note range' % (name, item, trans))
                    total += self.patterns[self.pat_index[item]][2]
            lengths.append((total, total - before))
        if end == 'loop' and len(set(lengths)) != 1:
            raise ValueError('%s: voices differ in (total rows, looped rows): %r' % (name, lengths))
        if self.max_rows * tempo > 255:
            raise ValueError('%s: max_rows * tempo must be <= 255' % name)
        self.song_index[name] = len(self.songs)
        self.songs.append(dict(
            name=name, tempo=tempo, voices=voices, end=end,
            filter=(f['res'] << 4) | f['voices'], modevol=(mode << 4) | volume,
            cutmin=cmin, cutmax=cmax, cutspd=cspd, rows=lengths[0][0], loop_rows=lengths[0][1]))

    def effect(self, name, ins, note, ticks):
        if not 1 <= ticks <= 255:
            raise ValueError('%s: effect length must be 1..255 ticks' % name)
        self.fx_index[name] = len(self.effects)
        self.effects.append(dict(name=name, ins=self.ins_index[ins], note=note_number(note), len=ticks))

    # -- compile -----------------------------------------------------------
    def compile(self):
        """Returns the data tables as a dict of name -> list of bytes."""
        if self._compiled:
            return self._compiled
        orders = []
        notes = []                        # (index, comment) for the listing
        starts = []
        for song in self.songs:
            row = []
            for order in song['voices']:
                row.append(len(orders))
                notes.append((len(orders), '%s voice %d' % (song['name'], len(row))))
                loop_at = len(orders)
                loop_trans = trans = 0
                for item in order:
                    if item == LOOP:
                        loop_at, loop_trans = len(orders), trans
                    elif isinstance(item, T):
                        trans = item.n
                        orders.append(0xA0 + item.n)
                    else:
                        orders.append(self.pat_index[item])
                end = song['end']
                if end == 'loop':
                    if trans != loop_trans:
                        orders.append(0xA0 + loop_trans)
                    orders += [0xFF, loop_at]
                elif end == 'stop':
                    orders.append(0xFE)
                else:
                    orders += [0xFD, self.song_index[end[1]]]
            starts.append(row)
        if len(orders) > 256:
            raise ValueError('order lists are over 256 bytes (%d)' % len(orders))
        d = {}
        d['music_song_ord0'] = [r[0] for r in starts]
        d['music_song_ord1'] = [r[1] for r in starts]
        d['music_song_ord2'] = [r[2] for r in starts]
        for key in ('tempo', 'filter', 'modevol', 'cutmin', 'cutmax', 'cutspd'):
            d['music_song_' + key] = [s[key] for s in self.songs]
        d['music_orders'] = orders
        for key in ('ad', 'sr', 'wave', 'wspd', 'pw', 'pws', 'vspd', 'vdly'):
            d['music_ins_' + key] = [i[key] for i in self.instruments]
        d['music_wave_l'] = list(self.wave_l)
        d['music_wave_r'] = list(self.wave_r)
        for key in ('ins', 'note', 'len'):
            d['music_sfx_' + key] = [e[key] for e in self.effects]
        d['patterns'] = [p[1] for p in self.patterns]
        d['order_notes'] = notes
        self._compiled = d
        return d

    def data_size(self):
        d = self.compile()
        n = sum(len(v) for k, v in d.items() if k.startswith('music_'))
        return n + sum(len(p) + 2 for p in d['patterns'])

    def write_asm(self, path, source_name):
        d = self.compile()
        L = []
        w = L.append

        def table(label, data, comment=''):
            w(label + (('    ; ' + comment) if comment else ''))
            for i in range(0, len(data), 16):
                w('    .byte ' + ', '.join('$%02x' % b for b in data[i:i + 16]))
            if not data:
                w('    ; (none)')

        w('; GENERATED by %s (tools/sidmusic.py) -- do not edit, edit the score' % source_name)
        w('; and run it again. Format: see the top of asm/C64Music.asm.')
        w('; %d bytes of data.' % self.data_size())
        w('')
        w('music_sfx_reg = %d    ; effects borrow SID voice %d' % (self.sfx_voice * 7, self.sfx_voice + 1))
        w('')
        w('; ---- songs: ' + ', '.join('%d %s' % (i, s['name']) for i, s in enumerate(self.songs)))
        for key in ('ord0', 'ord1', 'ord2', 'tempo', 'filter', 'modevol', 'cutmin', 'cutmax', 'cutspd'):
            table('music_song_' + key, d['music_song_' + key])
        w('')
        w('; ---- order lists')
        marks = dict(d['order_notes'])
        w('music_orders')
        bounds = sorted(marks) + [len(d['music_orders'])]
        for a, b in zip(bounds, bounds[1:]):
            w('    ; $%02x: %s' % (a, marks[a]))
            chunk = d['music_orders'][a:b]
            for i in range(0, len(chunk), 16):
                w('    .byte ' + ', '.join('$%02x' % x for x in chunk[i:i + 16]))
        w('')
        w('; ---- instruments: ' + ', '.join('%d %s' % (i, s['name']) for i, s in enumerate(self.instruments)))
        for key in ('ad', 'sr', 'wave', 'wspd', 'pw', 'pws', 'vspd', 'vdly'):
            table('music_ins_' + key, d['music_ins_' + key])
        w('')
        w('; ---- wave table (left: waveform / $ff jump, right: pitch / jump target)')
        table('music_wave_l', d['music_wave_l'])
        table('music_wave_r', d['music_wave_r'])
        w('')
        w('; ---- effects: ' + ', '.join('%d %s' % (i, s['name']) for i, s in enumerate(self.effects)))
        for key in ('ins', 'note', 'len'):
            table('music_sfx_' + key, d['music_sfx_' + key])
        w('')
        w('; ---- patterns')
        w('music_pat_lo')
        w('    .byte ' + ', '.join('<music_p%d' % i for i in range(len(self.patterns))))
        w('music_pat_hi')
        w('    .byte ' + ', '.join('>music_p%d' % i for i in range(len(self.patterns))))
        for i, (name, data, rows) in enumerate(self.patterns):
            table('music_p%d' % i, data, '%s, %d rows' % (name, rows))
        with open(path, 'w', newline='\n') as f:
            f.write('\n'.join(L) + '\n')

    def write_cs(self, path, namespace, class_name, source_name):
        def ident(prefix, name):
            return prefix + ''.join(p[:1].upper() + p[1:] for p in re.split(r'[^A-Za-z0-9]+', name) if p)
        L = ['// GENERATED by %s (tools/sidmusic.py) -- do not edit.' % source_name,
             'namespace %s;' % namespace, '',
             '// Song and effect numbers for C64.Music.Play / C64.Music.PlayEffect.',
             'static class %s' % class_name, '{']
        for i, s in enumerate(self.songs):
            L.append('    public const uint %s = %d;' % (ident('Song', s['name']), i))
        L.append('')
        for i, e in enumerate(self.effects):
            L.append('    public const uint %s = %d;' % (ident('Fx', e['name']), i))
        L.append('}')
        with open(path, 'w', newline='\n') as f:
            f.write('\n'.join(L) + '\n')

    # -- simulation --------------------------------------------------------
    def simulate(self, song=None, ticks=600, effects=()):
        """Runs the player model. effects: iterable of (tick, effect name).
        Returns one entry per tick: a `Frame` of the SID registers as they
        stand after that tick."""
        p = Player(self)
        if song is not None:
            p.play(self.song_index[song])
        fx = {}
        for t, name in effects:
            fx.setdefault(t, []).append(self.fx_index[name])
        frames = []
        for t in range(ticks):
            for e in fx.get(t, ()):
                p.play_effect(e)
            p.tick()
            frames.append(p.frame())
        return frames


class Frame:
    """SID state after one tick: per voice (freq, pw, ctrl, ad, sr)."""
    def __init__(self, voices, cutoff, filt, modevol):
        self.voices = voices
        self.cutoff = cutoff
        self.filter = filt
        self.modevol = modevol


class Player:
    """The 6502 player (asm/C64Music.asm), statement for statement."""

    def __init__(self, score):
        self.d = score.compile()
        self.regofs = [0, 7, 14, score.sfx_voice * 7]
        self.sid = [0] * 25
        self.playing = 0
        self.chain = 0
        self.tempo = 0
        self.modevol = 0x0F
        self.cut = self.cutfrac = self.cutdir = self.cutmin = self.cutmax = self.cutspd = 0
        self.sfx = 0
        z = lambda: [0, 0, 0, 0]
        self.count, self.pend, self.dur, self.ins, self.trans = z(), z(), z(), z(), z()
        self.ord, self.pat, self.ppos, self.note, self.cur = z(), z(), z(), z(), z()
        self.wptr, self.wcnt, self.wspd, self.wave, self.gate = z(), z(), z(), z(), z()
        self.ad, self.sr, self.pwlo, self.pwhi, self.pws, self.pwdir = z(), z(), z(), z(), z(), z()
        self.vspd, self.vcnt, self.vdir, self.vdly, self.vlo, self.vhi = z(), z(), z(), z(), z(), z()

    def play(self, song):
        d = self.d
        self.ord[0] = d['music_song_ord0'][song]
        self.ord[1] = d['music_song_ord1'][song]
        self.ord[2] = d['music_song_ord2'][song]
        self.tempo = d['music_song_tempo'][song]
        self.sid[0x17] = d['music_song_filter'][song]
        self.modevol = d['music_song_modevol'][song]
        self.cutmax = d['music_song_cutmax'][song]
        self.cutspd = d['music_song_cutspd'][song]
        self.cutmin = self.cut = d['music_song_cutmin'][song]
        for x in range(3):
            self.pat[x] = self.ppos[x] = self.trans[x] = self.ins[x] = 0
            self.pend[x] = REST
            self.gate[x] = 0xFE
            self.dur[x] = 1
            self.count[x] = 3
        self.playing = 0xFF
        self.cutdir = 0
        self.chain = 0

    def stop(self):
        self.playing = 0
        for x in range(3):
            self.gate[x] = 0xFE

    def play_effect(self, e):
        d = self.d
        if self.count[3] and e < self.sfx:
            return
        self.sfx = e
        self.count[3] = d['music_sfx_len'][e]
        self.note_init(3, d['music_sfx_note'][e], d['music_sfx_ins'][e])

    def note_init(self, x, note, y):
        d = self.d
        self.note[x] = note
        self.ad[x] = d['music_ins_ad'][y]
        self.sr[x] = d['music_ins_sr'][y]
        self.wptr[x] = d['music_ins_wave'][y]
        self.wspd[x] = d['music_ins_wspd'][y]
        self.wcnt[x] = 1
        self.pws[x] = d['music_ins_pws'][y]
        if self.pws[x] == 0 or self.pwhi[x] == 0:
            self.pwlo[x] = d['music_ins_pw'][y]
            self.pwhi[x] = self.pwlo[x] & 0x0F
        self.vdly[x] = d['music_ins_vdly'][y]
        self.vspd[x] = d['music_ins_vspd'][y]
        self.vcnt[x] = (self.vspd[x] >> 1) + (self.vspd[x] & 1)
        self.vlo[x] = self.vhi[x] = self.vdir[x] = 0
        self.gate[x] = 0xFF

    def tick(self):
        self.sid[0x18] = self.modevol
        if self.cutspd:
            s = self.cutfrac + self.cutspd
            self.cutfrac = s & 0xFF
            if s > 0xFF:
                if self.cutdir == 0:
                    self.cut = (self.cut + 1) & 0xFF
                    if self.cut >= self.cutmax:
                        self.cutdir = 1
                else:
                    self.cut = (self.cut - 1) & 0xFF
                    if self.cutmin >= self.cut:
                        self.cutdir = 0
        self.sid[0x16] = self.cut
        for x in (2, 1, 0):
            if self.playing:
                self.count[x] = (self.count[x] - 1) & 0xFF
                if self.count[x] == 0:
                    pend = self.pend[x]
                    if pend < REST:
                        self.note_init(x, pend, self.ins[x])
                    elif pend == REST:
                        self.gate[x] = 0xFE
                    self.count[x] = (self.dur[x] * self.tempo) & 0xFF
                elif self.count[x] == 2:
                    self.fetch(x)
            self.chan(x)
        if self.count[3]:
            self.chan(3)
            self.count[3] -= 1
        if self.chain:
            song = self.chain - 1
            self.play(song)

    def fetch(self, x):
        d = self.d
        while True:
            pat = d['patterns'][self.pat[x]]
            a = pat[self.ppos[x]]
            self.ppos[x] = (self.ppos[x] + 1) & 0xFF
            if a >= 0xC0:
                if a == 0xFF:
                    y = self.ord[x]
                    while True:
                        a = d['music_orders'][y]
                        y += 1
                        if a < 0x80:
                            self.pat[x] = a
                            self.ord[x] = y
                            self.ppos[x] = 0
                            break
                        if a < 0xFD:
                            self.trans[x] = (a - 0xA0) & 0xFF
                            continue
                        if a == 0xFD:
                            self.chain = d['music_orders'][y] + 1
                            self.pend[x] = REST
                            return
                        if a == 0xFF:
                            y = d['music_orders'][y]
                            continue
                        self.pend[x] = REST
                        return
                    continue
                self.ins[x] = a - 0xC0
                continue
            if a >= 0x80:
                self.dur[x] = (a & 0x3F) + 1
                continue
            if a < REST:
                a = (a + self.trans[x]) & 0xFF
                self.gate[x] = 0xFE
                self.ad[x] = self.sr[x] = 0
            self.pend[x] = a
            return

    def chan(self, x):
        d = self.d
        self.wcnt[x] = (self.wcnt[x] - 1) & 0xFF
        if self.wcnt[x] == 0:
            self.wcnt[x] = self.wspd[x]
            y = self.wptr[x]
            while True:
                a = d['music_wave_l'][y]
                if a == 0:
                    break
                if a != 0xFF:
                    self.wave[x] = a
                    break
                y = d['music_wave_r'][y]
            a = d['music_wave_r'][y]
            self.cur[x] = (a & 0x7F) if a & 0x80 else (a + self.note[x]) & 0xFF
            self.wptr[x] = (y + 1) & 0xFF
        if self.vspd[x]:
            if self.vdly[x]:
                self.vdly[x] -= 1
            else:
                step = FREQ[self.cur[x]] >> 8
                v = self.vlo[x] | (self.vhi[x] << 8)
                v = (v - step if self.vdir[x] else v + step) & 0xFFFF
                self.vlo[x], self.vhi[x] = v & 0xFF, v >> 8
                self.vcnt[x] = (self.vcnt[x] - 1) & 0xFF
                if self.vcnt[x] == 0:
                    self.vcnt[x] = self.vspd[x]
                    self.vdir[x] ^= 1
        if self.pws[x]:
            pw = self.pwlo[x] | (self.pwhi[x] << 8)
            if self.pwdir[x] == 0:
                pw = (pw + self.pws[x]) & 0xFFFF
                if (pw >> 8) >= 0x0E:
                    self.pwdir[x] = 1
            else:
                pw = (pw - self.pws[x]) & 0xFFFF
                if (pw >> 8) < 0x02:
                    self.pwdir[x] = 0
            self.pwlo[x], self.pwhi[x] = pw & 0xFF, pw >> 8
        f = (FREQ[self.cur[x]] + (self.vlo[x] | (self.vhi[x] << 8))) & 0xFFFF
        y = self.regofs[x]
        if x != 3 and self.count[3] and y == self.regofs[3]:
            return
        self.sid[y] = f & 0xFF
        self.sid[y + 1] = f >> 8
        self.sid[y + 2] = self.pwlo[x]
        self.sid[y + 3] = self.pwhi[x]
        self.sid[y + 5] = self.ad[x]
        self.sid[y + 6] = self.sr[x]
        self.sid[y + 4] = self.wave[x] & self.gate[x]

    def frame(self):
        s = self.sid
        voices = []
        for o in (0, 7, 14):
            voices.append((s[o] | (s[o + 1] << 8), s[o + 2] | ((s[o + 3] & 15) << 8), s[o + 4], s[o + 5], s[o + 6]))
        return Frame(voices, s[0x16], s[0x17], s[0x18])
