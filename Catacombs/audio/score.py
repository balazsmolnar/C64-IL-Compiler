"""The Catacombs soundtrack and sound effects.

Run this file to regenerate music.asm (the data the game embeds) and
AudioIds.cs (song/effect numbers):

    python Catacombs/audio/score.py

The notation is explained at the top of tools/sidmusic.py; the player is
asm/C64Music.asm. Rows are sixteenth notes, bars are 16 rows.

Voice 1: bass, through the low-pass filter. Voice 2: lead. Voice 3:
arpeggiated chords and percussion -- and the voice sound effects borrow.
"""

import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, '..', '..', 'tools'))
from sidmusic import Score, T, LOOP  # noqa: E402


def build():
    s = Score(sfx_voice=2, max_rows=24)

    PULSE, SAW, TRI, NOISE = 0x41, 0x21, 0x11, 0x81

    # ------------------------------------------------------------------
    # Instruments
    # ------------------------------------------------------------------
    # Bass: a pulse whose width keeps drifting, with an octave "pluck" on
    # the first tick. The song's filter sweep does the rest.
    s.instrument('bass', ad=0x09, sr=0x69, wave=[(PULSE, 12), (0, 0)], pw=0x500, pws=0x28)
    # Combat bass: shorter, harder.
    s.instrument('cbass', ad=0x07, sr=0x47, wave=[(SAW, 12), (PULSE, 0)], pw=0x400, pws=0x40)
    # Lead: slow-ish attack, vibrato that comes in once a note is held.
    s.instrument('lead', ad=0x28, sr=0xA9, wave=[(PULSE, 0)], pw=0x780, pws=0x14, vib=(5, 18))
    # Flute for the bridge: triangle, softer attack, wider vibrato.
    s.instrument('flute', ad=0x48, sr=0xC9, wave=[(TRI, 0)], vib=(6, 22))
    # Combat lead: sawtooth with an instant attack.
    s.instrument('clead', ad=0x07, sr=0x86, wave=[(SAW, 0)], vib=(4, 10))
    # Arpeggiated chords: one voice cycling through the chord 20 times a second.
    s.instrument('arpm', ad=0x08, sr=0x37, wave=[(PULSE, 0), (0, 3), (0, 7), ('jump', 0)], pw=0x300)
    s.instrument('arpM', ad=0x08, sr=0x37, wave=[(PULSE, 0), (0, 4), (0, 7), ('jump', 0)], pw=0x300)
    # The same chords as a slow, swelling pad (octave on top, slower cycle).
    s.instrument('padm', ad=0x6A, sr=0x8A, wave=[(PULSE, 0), (0, 7), (0, 3), (0, 12), ('jump', 0)],
                 wspd=2, pw=0x600, pws=0x10)
    s.instrument('padM', ad=0x6A, sr=0x8A, wave=[(PULSE, 0), (0, 7), (0, 4), (0, 12), ('jump', 0)],
                 wspd=2, pw=0x600, pws=0x10)
    # Percussion: noise for the attack, then a pulse dropping in pitch.
    s.instrument('kick', ad=0x06, sr=0x06,
                 wave=[(NOISE, 'a-5'), (PULSE, 'e-3'), (0, 'a-2'), (0, 'e-2'), (0, 'c-2'), (0x40, 'a-1')])
    s.instrument('snare', ad=0x07, sr=0x07,
                 wave=[(NOISE, 'd-6'), (PULSE, 'a-3'), (NOISE, 'a-5'), (0, 'f-5'), (0, 'd-5'), (0x80, 'd-5')])
    s.instrument('hat', ad=0x02, sr=0x02, wave=[(NOISE, 'g-7'), (0, 'a-7'), (0x80, 'a-7')])

    # Sound effects. All decay to nothing with the gate still on (sustain
    # 0), so there is no release tail for the music to cut off when it
    # takes the voice back.
    s.instrument('fx_step', ad=0x04, sr=0x00, wave=[(NOISE, 'c-5'), (0, 'f-4'), (0, 'c-4'), (0, 'g-3')])
    s.instrument('fx_door', ad=0x0A, sr=0x00, wspd=3,
                 wave=[(SAW, 'c-2'), (0, 'd-2'), (0, 'c#2'), (0, 'e-2'), (0, 'd#2'), (0, 'f#2'),
                       (0, 'f-2'), (0, 'g#2'), (NOISE, 'c-3'), (0, 'g-2')])
    s.instrument('fx_pickup', ad=0x09, sr=0x00, wspd=3, pw=0x800,
                 wave=[(PULSE, 'e-5'), (0, 'a-5'), (0, 'c#6'), (0, 'e-6'), (0, 'a-6')])
    s.instrument('fx_weapon', ad=0x0A, sr=0x00, wspd=2, pw=0x300,
                 wave=[(NOISE, 'c-7'), (PULSE, 'e-6'), (0, 'b-6'), (0, 'e-6'), (0, 'b-6'),
                       (0, 'e-7'), (0, 'b-6'), (0, 'e-7')])
    s.instrument('fx_sword', ad=0x08, sr=0x00,
                 wave=[(NOISE, 'c-7'), (0, 'a-6'), (0, 'f-6'), (0, 'd-6'), (0, 'b-5'), (0, 'g-5'),
                       (PULSE, 'c-3'), (NOISE, 'e-5'), (PULSE, 'g-2'), (NOISE, 'c-5')])
    s.instrument('fx_growl', ad=0x2A, sr=0x00, wspd=2,
                 wave=[(SAW, 'c-2'), (0, 'd#2'), (0, 'c-2'), (NOISE, 'c-3'), (SAW, 'b-1'), (0, 'd-2'),
                       (0, 'a#1'), (NOISE, 'a-2'), (SAW, 'a-1'), (0, 'c-2'), (0, 'g#1'), (0, 'g-1')])
    s.instrument('fx_hurt', ad=0x08, sr=0x00, wspd=2, pw=0x800,
                 wave=[(NOISE, 'c-6'), (PULSE, 'a-4'), (0, 'g-4'), (0, 'e-4'), (0, 'c-4'), (0, 'a-3'), (0, 'f-3')])
    s.instrument('fx_mdeath', ad=0x0B, sr=0x00, wspd=3,
                 wave=[(SAW, 'g-3'), (NOISE, 'c-5'), (SAW, 'e-3'), (0, 'c-3'), (NOISE, 'g-4'), (SAW, 'a-2'),
                       (0, 'f-2'), (NOISE, 'c-4'), (SAW, 'd-2'), (0, 'b-1'), (0, 'g-1'), (0, 'e-1'), (0, 'c-1')])
    s.instrument('fx_pdeath', ad=0x0B, sr=0x00, wspd=4, pw=0x800, vib=(3, 0),
                 wave=[(PULSE, 'a-4'), (0, 'g#4'), (0, 'g-4'), (0, 'f#4'), (0, 'f-4'), (0, 'e-4'), (0, 'd#4'),
                       (0, 'd-4'), (0, 'c#4'), (0, 'c-4'), (0, 'b-3'), (0, 'a#3'), (0, 'a-3'), (NOISE, 'c-3')])

    # Effect numbers double as priorities (see sidmusic.py): later = wins.
    s.effect('step', 'fx_step', 'c-3', 7)
    s.effect('door', 'fx_door', 'c-2', 34)
    s.effect('pickup', 'fx_pickup', 'e-5', 26)
    s.effect('weapon', 'fx_weapon', 'e-6', 32)
    s.effect('sword', 'fx_sword', 'c-3', 16)
    s.effect('growl', 'fx_growl', 'c-2', 28)
    s.effect('hurt', 'fx_hurt', 'a-4', 18)
    s.effect('monster death', 'fx_mdeath', 'g-3', 48)
    s.effect('player death', 'fx_pdeath', 'a-4', 62)

    # ------------------------------------------------------------------
    # Exploration: "Below the Chapel", A minor, 32 bars
    #   A  (8 bars)  Am Am F  F  Dm Dm E  E     the theme, sparse bass
    #   B  (8 bars)  Am G  F  E  Am G  F  E     moving eighths, snare enters
    #   C  (8 bars)  Dm Am Dm Am F  C  E  E     bridge: flute over pads
    #   A' (8 bars)  the theme an octave up over the driving bass
    # ------------------------------------------------------------------
    s.pattern('bassA', '@bass a-1:6 a-1:2 a-2:4 e-2:2 a-1:2')
    s.pattern('bassB', '@bass l2 a-1 a-2 a-1 a-1 | a-2 a-1 e-2 a-2')
    s.pattern('bassC', '@bass a-1:12 e-2:4')

    s.pattern('leadA1', '@lead e-4:8 c-4:4 a-3:4 | b-3:6 c-4:2 a-3:8 |'
                        ' f-4:8 c-4:4 a-3:4 | a-3:6 c-4:2 f-4:6 e-4:2')
    s.pattern('leadA2', '@lead d-4:8 f-4:4 a-4:4 | g-4:6 f-4:2 d-4:8 |'
                        ' e-4:8 g#4:4 b-4:4 | a-4:2 g#4:2 e-4:4 b-3:8')
    s.pattern('leadB1', '@lead l2 a-4 c-5 e-5:4 d-5 c-5 b-4 a-4 | b-4 d-5 g-4:4 b-4 a-4 g-4:4 |'
                        ' a-4 c-5 f-4:4 a-4 g-4 f-4:4 | g#4:4 b-4:4 e-5:8')
    s.pattern('leadB2', '@lead l2 e-5 c-5 a-4:4 c-5 e-5 a-5:4 | l4 g-5 d-5 b-4 d-5 |'
                        ' f-5 c-5 a-4 c-5 | e-5:6 d-5:2 b-4 g#4')
    s.pattern('leadC1', '@flute r:8 a-4:4 d-5:4 | c-5:8 a-4:8 | r:8 f-4:4 a-4:4 | e-4:16')
    s.pattern('leadC2', '@flute r:4 f-4:4 a-4:4 c-5:4 | e-5:8 c-5:4 g-4:4 | b-4:8 g#4:8 | e-4:12 r:4')

    for q in 'mM':
        s.pattern('arpA' + q, '@kick c-2:2 @arp%s a-3:2 @hat c-2:2 @arp%s a-3:2 |'
                              ' @kick c-2:2 @arp%s a-3:2 @hat c-2:1 c-2:1 @arp%s a-3:2' % (q, q, q, q))
        s.pattern('arpB' + q, '@kick c-2:2 @arp%s a-3:1 a-3:1 @snare c-2:2 @arp%s a-3:2 |'
                              ' @kick c-2:1 c-2:1 @arp%s a-3:2 @snare c-2:2 @arp%s a-4:2' % (q, q, q, q))
        s.pattern('padC' + q, '@pad%s a-3:12 @hat c-2:2 c-2:2' % q)

    def bars(pattern, chords):
        """An order list playing one bar of `pattern` per chord.
        chords: (transpose, 'm' or 'M') pairs; pattern '%' takes the quality."""
        out, last = [], None
        for trans, quality in chords:
            if trans != last:
                out.append(T(trans))
                last = trans
            out.append(pattern.replace('%', quality))
        return out

    Am, F, Dm, E, Ehi, G, C = (0, 'm'), (-4, 'M'), (5, 'm'), (-5, 'M'), (7, 'M'), (-2, 'M'), (3, 'M')
    secA = [Am, Am, F, F, Dm, Dm, Ehi, Ehi]
    secB = [Am, G, F, E, Am, G, F, E]
    secC = [Dm, Am, Dm, Am, F, C, E, E]
    arpA = [Am, Am, F, F, Dm, Dm, E, E]

    s.song('explore', tempo=9, end='loop',
           filter=dict(voices=0b001, res=9, mode='lp', cut=(0x30, 0x80, 20)),
           voices=[
               bars('bassA', secA) + bars('bassB', secB) + bars('bassC', secC) + bars('bassB', secA),
               ['leadA1', 'leadA2', 'leadB1', 'leadB2', 'leadC1', 'leadC2', T(12), 'leadA1', 'leadA2'],
               bars('arpA%', arpA) + bars('arpB%', secB) + bars('padC%', secC) + bars('arpB%', arpA),
           ])

    # ------------------------------------------------------------------
    # Combat: "Teeth in the Dark", D minor with a Phrygian Eb, 16 bars, fast
    #   Dm Dm Eb Dm | Dm Dm Eb A | Bb Bb C C | Dm Dm Eb A
    # ------------------------------------------------------------------
    s.pattern('cbass', '@cbass l1 d-2 d-2 d-3 d-2 d-2 d-3 d-2 d-2 | d-3 d-2 d-2 d-3 d-2 d-3 c-3 d-3')
    for q in 'mM':
        s.pattern('cperc' + q, '@kick c-2:2 @arp%s d-4:1 @hat c-2:1 @snare c-2:2 @arp%s d-4:1 @hat c-2:1 |'
                               ' @kick c-2:1 c-2:1 @arp%s d-4:1 @hat c-2:1 @snare c-2:2 @arp%s d-4:1 @snare c-2:1'
                               % (q, q, q, q))
    s.pattern('cleadA', '@clead d-4:2 d-4:1 f-4:1 r:1 d-4:1 a-4:2 g-4:2 f-4:2 d-4:2 c-4:2 |'
                        ' d-4:2 d-4:1 f-4:1 r:1 d-4:1 a-4:2 c-5:4 a-4:4')
    s.pattern('cleadB', '@clead eb4:2 eb4:1 g-4:1 r:1 eb4:1 bb4:2 a-4:2 g-4:2 eb4:2 g-4:2 |'
                        ' d-4:6 f-4:2 a-4:4 d-5:4')
    s.pattern('cleadC', '@clead eb4:2 eb4:1 g-4:1 r:1 eb4:1 bb4:2 a-4:2 g-4:2 eb4:2 g-4:2 |'
                        ' c#5:4 e-5:4 l2 a-4 c#5 e-5 g-5')
    s.pattern('cleadD', '@clead d-5:6 f-5:2 bb4:8 | l2 d-5 c-5 bb4 d-5 f-5:4 d-5:4 |'
                        ' e-5:6 g-5:2 c-5:8 | l2 e-5 d-5 c-5 e-5 g-5:4 e-5:4')
    s.pattern('cleadE', '@clead a-5:4 l2 g-5 f-5 e-5 d-5 f-5:4 | a-5 d-5 f-5 a-5 d-5:8 |'
                        ' l4 g-5 eb5 bb4 g-4 | l2 a-4 c#5 e-5 a-5 g-5 e-5 c#5 a-4')

    cDm, cEb, cA, cBb, cC = (0, 'm'), (1, 'M'), (-5, 'M'), (-4, 'M'), (-2, 'M')
    combat = [cDm, cDm, cEb, cDm, cDm, cDm, cEb, cA, cBb, cBb, cC, cC, cDm, cDm, cEb, cA]
    s.song('combat', tempo=5, end='loop',
           filter=dict(voices=0b001, res=12, mode='lp', cut=(0x40, 0x90, 90)),
           voices=[
               bars('cbass', combat),
               ['cleadA', 'cleadB', 'cleadA', 'cleadC', 'cleadD', 'cleadE'],
               bars('cperc%', combat),
           ])

    # ------------------------------------------------------------------
    # Jingles. Each hands over to the exploration tune when it is done.
    # ------------------------------------------------------------------
    flt = dict(voices=0b001, res=9, mode='lp', cut=(0x50, 0x50, 0))

    # Title: the A minor chord climbing out of the dark, a held E major
    # (the question), back to A minor.
    s.pattern('titleBass', '@bass a-1:16 | e-1:16 | a-1:16')
    s.pattern('titleLead', '@lead l4 a-3 c-4 e-4 a-4 | b-4:8 g#4:8 | a-4:16')
    s.pattern('titlePad', '@padm a-3:16 | @padM e-3:16 | @padm a-3:16')
    s.song('title', tempo=9, end=('chain', 'explore'), filter=flt,
           voices=[['titleBass'], ['titleLead'], ['titlePad']])

    # Death: a chromatic fall to a low A minor.
    s.pattern('deathBass', '@bass a-1:16 | d-2:16 | a-1:16')
    s.pattern('deathLead', '@lead l4 a-4 g#4 g-4 f#4 | f-4 e-4 d-4 c-4 | a-3:16')
    s.pattern('deathPad', '@padm a-3:16 | d-3:16 | a-2:16')
    s.song('death', tempo=7, end=('chain', 'explore'), filter=flt,
           voices=[['deathBass'], ['deathLead'], ['deathPad']])

    # Victory: A major fanfare.
    s.pattern('winBass', '@cbass a-2:8 e-2:8 | a-2:12 r:4')
    s.pattern('winLead', '@clead a-4:2 a-4:1 a-4:1 c#5:2 e-5:2 a-5:4 e-5:2 a-5:2 | c#6:12 r:4')
    s.pattern('winArp', '@arpM l4 a-3 a-3 e-4 e-4 | a-4:12 r:4')
    s.song('victory', tempo=6, end=('chain', 'explore'), filter=flt,
           voices=[['winBass'], ['winLead'], ['winArp']])
    return s


if __name__ == '__main__':
    score = build()
    score.write_asm(os.path.join(HERE, 'music.asm'), 'Catacombs/audio/score.py')
    score.write_cs(os.path.join(HERE, 'AudioIds.cs'), 'Catacombs', 'AudioIds', 'Catacombs/audio/score.py')
    print('music data: %d bytes' % score.data_size())
    for song in score.songs:
        ticks = song['rows'] * song['tempo']
        print('  %-8s %3d rows, tempo %d: %.1f s' % (song['name'], song['rows'], song['tempo'], ticks / 60.0))
