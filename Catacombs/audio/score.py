"""The Catacombs soundtrack and sound effects.

Run this file to regenerate music.asm (the data the game embeds) and
AudioIds.cs (song/effect numbers):

    python Catacombs/audio/score.py

The notation is explained at the top of tools/sidmusic.py; the player is
asm/C64Music.asm. Rows are sixteenth notes, bars are 16 rows, a tick is one
PAL frame.

Voice 1: bass and percussion (each drum is the attack of a bass note).
Voice 2: lead. Voice 3: chords, pads, bell -- and the voice sound effects
borrow, so an effect never takes the beat or the tune away.

Filter values are chosen on the 8580 SID (VICE's default model).
"""

import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, '..', '..', 'tools'))
from sidmusic import Score, T, LOOP  # noqa: E402

PULSE, SAW, TRI, NOISE = 0x41, 0x21, 0x11, 0x81


def bars(pattern, chords):
    """An order list playing `pattern` once per chord. chords: (transpose,
    quality) pairs; a '%' in the pattern name is replaced by the quality."""
    out, last = [], None
    for trans, quality in chords:
        if trans != last:
            out.append(T(trans))
            last = trans
        out.append(pattern.replace('%', quality))
    return out


def build():
    s = Score(sfx_voice=2, max_rows=28)

    # ------------------------------------------------------------------
    # Exploration instruments. Sustain levels are kept around 7-9 of 15 so
    # that combat (10-12) and the effects (15) stand above them.
    # ------------------------------------------------------------------
    # Drone: a narrow pulse that never re-attacks (legato); each note
    # starts with the filter nearly shut and lets it open slowly.
    s.instrument('drone', ad=0x69, sr=0x9A, wave=[(PULSE, 0)], pw=0x380, pws=0x0C,
                 legato=True, filt=0x0C, fdel=1)
    # Plucked bass: octave click, then the filter closing on the note.
    s.instrument('bass', ad=0x08, sr=0x87, wave=[(PULSE, 12), (0, 0)], pw=0x300, pws=0x18,
                 filt=0x50, fdel=-4)
    # Heartbeat: the same bass note with a kick for an attack.
    s.instrument('kbass', ad=0x08, sr=0x87, pw=0x300, pws=0x18, filt=0x60, fdel=-5,
                 wave=[(NOISE, 'a-4'), (PULSE, 'c-3'), (0, 'f-2'), (PULSE, 0)])
    # Lead: slow-ish attack, vibrato once a note is held. 'leadL' is the
    # same voice slurred from the note before (the motif's falling sigh).
    s.instrument('lead', ad=0x38, sr=0x89, wave=[(PULSE, 0)], pw=0x780, pws=0x14, vib=(5, 16))
    s.instrument('leadL', ad=0x38, sr=0x89, wave=[(PULSE, 0)], pw=0x780, pws=0x14, vib=(5, 10),
                 legato=True)
    # Flute for the bridge.
    s.instrument('flute', ad=0x58, sr=0xC9, wave=[(TRI, 0)], vib=(6, 20))
    # Chords: one voice cycling through the chord 17 times a second, with
    # a moving pulse width, heard through whatever the bass is doing to
    # the filter. m minor, M major, d diminished, s dominant seventh.
    for q, (third, top) in dict(m=(3, 7), M=(4, 7), d=(3, 6), s=(4, 10)).items():
        s.instrument('pad' + q, ad=0x79, sr=0x7A, pw=0x500, pws=0x1C, filt='keep',
                     wave=[(PULSE, 0), (0, top), (0, third), ('jump', 0)])
    # Bell: struck an octave high, rings off by itself.
    s.instrument('bell', ad=0x0A, sr=0x0A, wave=[(PULSE, 12), (TRI, 12), (0, 0)], pw=0x200)

    # ------------------------------------------------------------------
    # Combat instruments: louder, brighter, everything through the filter,
    # which every bass note kicks open and lets fall shut again.
    # ------------------------------------------------------------------
    s.instrument('cbass', ad=0x08, sr=0xB8, wave=[(SAW, 12), (PULSE, 0)], pw=0x400, pws=0x50,
                 legato=True, filt=0x78, fdel=-8)
    s.instrument('ckick', ad=0x08, sr=0xB8, pw=0x400, pws=0x50, filt=0x78, fdel=-8,
                 wave=[(NOISE, 'a-5'), (PULSE, 'e-3'), (0, 'a-2'), (PULSE, 0)])
    s.instrument('csnare', ad=0x08, sr=0xB8, pw=0x400, pws=0x50,
                 wave=[(NOISE, 'd-6'), (PULSE, 'a-3'), (NOISE, 'a-5'), (0, 'f-5'), (PULSE, 0)])
    s.instrument('clead', ad=0x08, sr=0xC8, wave=[(SAW, 0)], vib=(4, 8), filt='keep')
    for q, (third, top) in dict(m=(3, 7), M=(4, 7), d=(3, 6)).items():
        s.instrument('arp' + q, ad=0x08, sr=0x98, pw=0x500, pws=0x40,
                     wave=[(PULSE, 0), (0, third), (0, top), ('jump', 0)])

    # ------------------------------------------------------------------
    # Sound effects. Full level (the music sits lower), one tick of hard
    # restart before each (the player does that), and shaped with real
    # envelopes: gate=N releases the note N ticks before the effect ends.
    # Numbers double as priorities (see sidmusic.py): later = wins.
    # ------------------------------------------------------------------
    # Footsteps, two alternating: a tick of high noise (the scuff), two of
    # low noise (the weight).
    s.instrument('fx_stepA', ad=0x03, sr=0x00, wave=[(NOISE, 'a-6'), (NOISE, 'c-3'), (0, 'c-3')])
    s.instrument('fx_stepB', ad=0x03, sr=0x00, wave=[(NOISE, 'f-6'), (NOISE, 'a-2'), (0, 'a-2')])
    s.effect('step a', 'fx_stepA', 'c-3', 3)
    s.effect('step b', 'fx_stepB', 'c-3', 3)

    # Door: a narrow pulse creaking upwards from 90 Hz with a jittering
    # pitch, a breath, then the slam -- re-gated noise dropping an octave.
    s.instrument('fx_door', ad=0x08, sr=0xF0, wspd=2, pw=0x200, slide=30,
                 wave=[(PULSE, 'f#2'), (0, 'g-2'), (0, 'f-2'), (0, 'g#2'), (0, 'f#2'), (0, 'a-2'),
                       (0, 'f#2'), (0, 'g-2'), (0, 'f-2'), (0, 'g#2'), (0, 'f#2'), (0, 'a-2'),
                       (0x80, 'c-3'), (NOISE, 'a-2'), (0, 'e-2'), (0, 'b-1'), (0, 'a-1')])
    s.effect('door', 'fx_door', 'c-2', 34)

    # Treasure: E-A-E-A rising through two octaves (no third, so it sits
    # in A minor), pulse width sweeping.
    s.instrument('fx_pickup', ad=0x09, sr=0x00, wspd=3, pw=0x800, pws=0x60,
                 wave=[(PULSE, 'e-5'), (0, 'a-5'), (0, 'e-6'), (0, 'a-6')])
    s.effect('pickup', 'fx_pickup', 'e-5', 24)

    # Weapon: a blade drawn -- noise, then a high shimmer sliding upwards.
    s.instrument('fx_weapon', ad=0x0A, sr=0x00, pw=0x300, pws=0x90, slide=60,
                 wave=[(NOISE, 'c-7'), (PULSE, 'e-6'), (0, 'b-6'), ('jump', 1)])
    s.effect('weapon', 'fx_weapon', 'e-6', 30)

    # Growl: a low pulse wobbling 25 times a second, sinking, pulse width
    # churning; swells in and dies away.
    s.instrument('fx_growl', ad=0x4A, sr=0xD9, pw=0x200, pws=0x70, slide=-3, gate=14,
                 wave=[(PULSE, 'a-1'), (0, 'c-2'), ('jump', 0)])
    s.effect('growl', 'fx_growl', 'a-1', 56)

    # Hurt: a crack of noise, then a pulse falling more than an octave.
    s.instrument('fx_hurt', ad=0x08, sr=0x00, pw=0x600, pws=0x30, slide=-100,
                 wave=[(NOISE, 'c-6'), (0, 'c-6'), (PULSE, 'f-3')])
    s.effect('hurt', 'fx_hurt', 'e-3', 16)

    # Sword: two ticks of swish, then the clang -- re-gated at full level,
    # two pitches a tritone apart alternating every tick, ringing off.
    s.instrument('fx_sword', ad=0x09, sr=0x00, pw=0x280,
                 wave=[(NOISE, 'c-7'), (0, 'c-7'), (0x40, 'b-6'), (PULSE, 'b-6'), (0, 'f-7'), ('jump', 3)])
    s.effect('sword', 'fx_sword', 'b-6', 20)

    # Monster death: a sawtooth roar with noise in it, falling away.
    s.instrument('fx_mdeath', ad=0x0A, sr=0xE9, wspd=2, slide=-14, gate=16,
                 wave=[(SAW, 'g-2'), (0, 'a#2'), (NOISE, 'g-3'), (SAW, 'g-2'), (0, 'a#2'), (0, 'g-2'), ('jump', 0)])
    s.effect('monster death', 'fx_mdeath', 'g-2', 54)

    # Player death: a blow, then a long low fall (the death jingle's own
    # line is in the lead's register; this stays under it).
    s.instrument('fx_pdeath', ad=0x0A, sr=0x00, pw=0x800, pws=0x28, slide=-9,
                 wave=[(NOISE, 'c-6'), (0, 'e-5'), (PULSE, 'a-2')])
    s.effect('player death', 'fx_pdeath', 'a-2', 36)

    # ------------------------------------------------------------------
    # Exploration: "Below the Chapel". A minor, 32 bars, 84 BPM.
    #
    # The motif (2 bars): E up a minor sixth to C, down by step C-B-Bb
    # with the Bb held over the barline, sighing to A.
    #
    #   A  bars 1-8    over an A pedal drone, no drums:
    #                  Am | Bb/A Am | Dm/A | Eb/A Dm/A | Am | G#dim/A Am | Bb | E7
    #                  motif; motif a fourth up; motif inverted (C down to
    #                  E, G# rising to A); cadence through the Neapolitan.
    #   B  bars 9-16   lament bass A G# G F# F E, then Bb, E; heartbeat.
    #                  The motif's leap in rising sequence against the
    #                  falling bass (E-C, G-E, A-F, D-Bb), each top note
    #                  tied over the barline; peak Bb5 on the Neapolitan.
    #   C  bars 17-24  D minor, no drums: Dm Dm/C Bb A | Gm Dm/F Bdim E7.
    #                  Flute, in fragments of the motif, over a bass
    #                  walking down D C Bb A G F E.
    #   A' bars 25-32  motif and its fourth-up answer over the moving
    #                  bass, a bell counter-line filling the held notes;
    #                  motif an octave up as the climax; Bb (with G#: an
    #                  augmented sixth) falling to E, and a bass fill.
    # ------------------------------------------------------------------
    s.pattern('motif', '@lead e-4:4 c-5:6 b-4:2 bb4:8 @leadL a-4:8 r:4')
    s.pattern('motifInv', '@lead c-5:4 e-4:6 f-4:2 g#4:8 @leadL a-4:8 r:4')
    s.pattern('cadence', '@lead d-5:6 c-5:2 bb4:4 a-4:4 | g#4:8 f-4:4 d-4:4')
    s.pattern('leadB1', '@lead e-4:4 c-5:8 b-4:8 d-5:8 c-5:4 | g-4:4 e-5:8 d-5:8 f#5:8 e-5:4')
    s.pattern('leadB2', '@lead a-4:4 f-5:8 e-5:8 g#5:6 f-5:2 e-5:4 |'
                        ' d-5:4 bb5:6 a-5:2 g-5:4 | g#5:2 f-5:2 e-5:4 d-5:4 b-4:4')
    s.pattern('fluteC1', '@flute r:4 a-4:4 f-5:8 | e-5:4 d-5:8 r:4 | r:4 d-5:4 bb5:8 | a-5:4 g-5:2 f-5:2 e-5:8')
    s.pattern('fluteC2', '@flute r:4 g-4:4 eb5:8 | d-5:8 a-4:8 | r:4 b-4:4 f-5:6 e-5:2 | g#4:12 r:4')
    s.pattern('ending', '@lead f-5:4 d-5:4 bb4:4 g#4:4 | a-4:2 g#4:2 e-4:12')

    s.pattern('drone2', '@drone a-2:16 | a-1:8 a-2:8')
    s.pattern('drone1', '@drone a-2:16')
    s.pattern('bassB', '@kbass a-2:3 a-2:5 @bass a-3:6 a-2:2')
    s.pattern('bassFill', '@kbass a-2:3 a-2:3 a-2:2 | a-2:2 a-2:2 l1 a-2 a-2 a-2 a-2')

    for q in 'mMs':
        s.pattern('pad' + q, '@pad%s a-3:16' % q)
    for q in 'mM':
        s.pattern('padBell' + q, '@pad%s a-3:12 @bell e-5:4' % q)
        s.pattern('padB' + q, '@pad%s a-3:6 a-3:6 a-3:4' % q)
    for q in 'mMd':
        s.pattern('half' + q, '@pad%s a-3:8' % q)
    s.pattern('counter1', '@bell r:6 a-5:2 g-5:2 f-5:2 e-5:4 | r:4 d-5:2 f-5:2 e-5:4 c-5:4')
    s.pattern('counter2', '@bell r:6 e-5:2 d-5:2 c-5:2 b-4:4 | r:4 b-4:2 d-5:2 c-5:4 a-4:4')

    lament = [(0, 'm'), (-1, 'M'), (-2, 'M'), (-3, 'M'), (-4, 'M'), (-5, 'M'), (1, 'M'), (-5, 's')]
    explore_bass = (
        ['drone2', 'drone2', 'drone2', T(1), 'drone1', T(-5), 'drone1'] +
        bars('bassB', lament) +
        bars('drone1', [(5, ''), (3, ''), (1, ''), (0, ''), (-2, ''), (-4, ''), (-5, ''), (-5, '')]) +
        bars('bassB', [(0, ''), (0, ''), (5, ''), (5, ''), (0, ''), (-1, ''), (1, '')]) + [T(-5), 'bassFill'])
    explore_lead = (
        ['motif', T(5), 'motif', T(0), 'motifInv', 'cadence', 'leadB1', 'leadB2', 'fluteC1', 'fluteC2',
         'motif', T(5), 'motif', T(12), 'motif', T(0), 'ending'])
    explore_chords = (
        # A: Am | Bb/A Am | Dm | Eb Dm | Am | G#dim Am | Bb | E7
        ['padBellm', T(1), 'halfM', T(0), 'halfm', T(5), 'padBellm', T(6), 'halfM', T(5), 'halfm',
         T(0), 'padBellm', T(-1), 'halfd', T(0), 'halfm', T(1), 'padBellM', T(-5), 'pads'] +
        # B: Am E/G# C/G D/F# F E Bb E7
        bars('padB%', [(0, 'm'), (-5, 'M'), (3, 'M'), (5, 'M'), (-4, 'M'), (-5, 'M'), (1, 'M')]) + [T(-5), 'pads'] +
        # C: Dm Dm Bb A Gm Dm Bdim E7
        bars('padBell%', [(5, 'm'), (5, 'm'), (1, 'M'), (0, 'M'), (-2, 'm'), (5, 'm')]) +
        [T(2), 'halfd', 'halfd', T(-5), 'pads'] +
        # A': bell counter-line, then Bb | E7
        [T(0), 'counter1', T(5), 'counter1', T(0), 'counter2', T(1), 'padM', T(-5), 'pads'])
    s.song('explore', tempo=9, end='loop', volume=15,
           filter=dict(res=11, mode='lp', cut=(0x0A, 0x58)),
           voices=[explore_bass, explore_lead, explore_chords])

    # ------------------------------------------------------------------
    # Combat: "Teeth in the Dark". D minor with a Phrygian Eb, 16 bars,
    # 188 BPM, bass in 3+3+2 with kick and snare on its own notes.
    #   bars 1-8    Dm Dm Eb Dm | Dm Dm Eb A    the riff and its answers
    #   bars 9-14   one bar of riff climbing by semitones: Dm Ebm Em Fm F#m Gm
    #   bar 15      G#dim: the peak, a diminished arpeggio up to F6
    #   bar 16      A: a falling/rising A7 run and a snare fill into the loop
    # ------------------------------------------------------------------
    s.pattern('cbass', '@ckick d-2:3 @cbass d-3:3 @csnare d-2:2 | @ckick d-2:3 @cbass d-3:3 @csnare d-2:1 d-2:1')
    s.pattern('cbassFill', '@csnare d-2:1 d-2:1 @ckick d-2:2 @csnare d-2:1 d-2:1 @ckick d-2:2 |'
                           ' @csnare l1 d-2 d-2 d-2 d-2 d-2 d-2 d-2 d-2')
    for q in 'mMd':
        s.pattern('cstab' + q, '@arp%s d-4:3 d-4:3 d-4:2 d-4:3 d-4:3 d-4:2' % q)
    s.pattern('cleadA', '@clead d-4:2 d-4:1 f-4:1 r:1 d-4:1 a-4:2 g-4:2 f-4:2 d-4:2 c-4:2 |'
                        ' d-4:2 d-4:1 f-4:1 r:1 d-4:1 a-4:2 c-5:4 a-4:4')
    s.pattern('cleadB', '@clead eb4:2 eb4:1 g-4:1 r:1 eb4:1 bb4:2 a-4:2 g-4:2 eb4:2 g-4:2 |'
                        ' d-4:6 f-4:2 a-4:4 d-5:4')
    s.pattern('cleadC', '@clead eb4:2 eb4:1 g-4:1 r:1 eb4:1 bb4:2 a-4:2 g-4:2 eb4:2 g-4:2 |'
                        ' c#5:4 e-5:4 l2 a-4 c#5 e-5 g-5')
    s.pattern('cseq', '@clead d-5:3 d-5:3 f-5:2 a-5:3 g-5:3 f-5:2')
    s.pattern('cpeak', '@clead g#5:3 b-5:3 d-6:2 f-6:3 d-6:3 b-5:2')
    s.pattern('cfill', '@clead l1 a-5 g-5 e-5 c#5 a-4 c#5 e-5 g-5 a-5 r a-5 r a-5 a-5 r r')

    combat = [(0, 'm'), (0, 'm'), (1, 'M'), (0, 'm'), (0, 'm'), (0, 'm'), (1, 'M'), (-5, 'M'),
              (0, 'm'), (1, 'm'), (2, 'm'), (3, 'm'), (4, 'm'), (5, 'm'), (6, 'd')]
    s.song('combat', tempo=4, end='loop', volume=15,
           filter=dict(res=13, mode='lp', cut=(0x18, 0x78)),
           voices=[
               bars('cbass', combat) + [T(-5), 'cbassFill'],
               ['cleadA', 'cleadB', 'cleadA', 'cleadC'] +
               bars('cseq', [(n, '') for n in range(6)]) + [T(0), 'cpeak', 'cfill'],
               bars('cstab%', combat) + [T(-5), 'cstabM'],
           ])

    # ------------------------------------------------------------------
    # Jingles.
    # ------------------------------------------------------------------
    flt = dict(res=11, mode='lp', cut=(0x0A, 0x58))

    # Title: the motif alone over the drone, then the question (E7); the
    # exploration tune answers it.
    s.pattern('titleLead', '@lead r:16')
    s.song('title', tempo=9, end=('chain', 'explore'), filter=flt,
           voices=[['drone2', T(-5), 'drone1'],
                   ['motif', 'titleLead'],
                   ['padBellm', T(1), 'halfM', T(0), 'halfm', T(-5), 'pads']])

    # Death: a chromatic fall to a low A minor; then the game starts over.
    s.pattern('deathBass', '@drone a-2:16 | d-2:16 | a-1:16')
    s.pattern('deathLead', '@lead l4 a-4 g#4 g-4 f#4 | f-4 e-4 d-4 c-4 | a-3:16')
    s.song('death', tempo=7, end=('chain', 'explore'), filter=flt,
           voices=[['deathBass'], ['deathLead'], ['padm', T(-7), 'padm', T(-12), 'padm']])

    # Victory: Am-C | F-G | E -- a fanfare that climbs out of the minor
    # and stops on the dominant, so the exploration tune (which resumes
    # where it was) is its resolution.
    s.pattern('winBass', '@bass a-2:8 c-3:8 | f-2:8 g-2:8 | e-2:12 r:4')
    s.pattern('winLead', '@clead a-4:2 a-4:1 a-4:1 c-5:2 e-5:2 g-5:6 e-5:2 |'
                         ' f-5:2 f-5:1 f-5:1 a-5:2 c-6:2 b-5:6 g-5:2 | g#5:12 r:4')
    s.pattern('winChords', '@arpm a-3:8 @arpM c-4:8 | f-3:8 g-3:8 | e-3:12 r:4')
    s.song('victory', tempo=5, end='resume', filter=flt,
           voices=[['winBass'], ['winLead'], ['winChords']])

    # Test fixture for the SFXGATED scene: voice 3 holding a chord, gate
    # on, nothing else -- the worst case for starting an effect.
    s.pattern('holdRest', 'r:24')
    s.pattern('holdPad', '@padm a-3:24')
    s.song('hold', tempo=9, end='loop', filter=flt, voices=[['holdRest'], ['holdRest'], ['holdPad']])
    return s


if __name__ == '__main__':
    score = build()
    score.write_asm(os.path.join(HERE, 'music.asm'), 'Catacombs/audio/score.py')
    score.write_cs(os.path.join(HERE, 'AudioIds.cs'), 'Catacombs', 'AudioIds', 'Catacombs/audio/score.py')
    print('music data: %d bytes' % score.data_size())
    for song in score.songs:
        ticks = song['rows'] * song['tempo']
        print('  %-8s %3d rows, tempo %d: %.1f s' % (song['name'], song['rows'], song['tempo'], ticks / 50.12))
