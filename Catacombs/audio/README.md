# Catacombs audio

| File | What it is |
| --- | --- |
| `score.py` | The soundtrack and effects, as readable source. Run it to regenerate the two files below. |
| `music.asm` | Generated: the data the game embeds (`Audio.cs` pulls it in with `RawAssembly`). |
| `AudioIds.cs` | Generated: song and effect numbers. |
| `record.py` | Builds audio scenes and records them in VICE. |
| `analyze.py` | Checks the recordings against the score. |
| `../Audio.cs` | The game's audio API: one call per gameplay moment. |
| `../../asm/C64Music.asm` | The player (generic C64Lib runtime, `C64.Music` in `C64Lib/Music.cs`). Its header documents the data format and the interrupt hook. |
| `../../tools/sidmusic.py` | Score compiler plus a tick-exact Python model of the player. Its header documents the score notation. |
| `../../tools/vice_record.py` | Records any `.prg`'s SID output (WAV or register dump). |

## What plays when

- Boot: title jingle (8.6 s), which hands over to the exploration tune (92 s loop).
- Combat: the combat tune (31 s loop) breaks into the exploration tune
  (`Music.Interrupt`); when the monster dies the exploration tune carries
  on from the very note it was at (`Music.Resume`).
- Death: death effect and death jingle (6.7 s), then the exploration tune from the start.
- Last treasure collected: victory fanfare (6.4 s) breaks in, then the exploration tune resumes.
- Effects: footsteps (two, alternating), door, treasure pickup, weapon pickup, monster growl, player hurt, sword hit, monster death, player death.

Voice 1 is bass and percussion (each drum is the attack of a bass note),
voice 2 the lead, voice 3 chords, pads and bell. Effects borrow voice 3;
the music on it keeps counting silently and comes back in time, re-attacked,
when the effect ends. Footsteps are polite: they only take voice 3 while
its chord is ringing off, so walking never chops the harmony.

## Changing the music

Edit `score.py`, then from the repository root:

    python Catacombs/audio/score.py

and rebuild the game. It prints the data size and each song's length.

## Recording

Needs VICE at `C:\tools\VICE\bin\x64sc.exe` (or `VICE_EXE`), 64tass, and
Python 3 with numpy and matplotlib. From the repository root:

    python Catacombs/audio/record.py               # every scene, about 2 minutes
    python Catacombs/audio/record.py EXPLORE SFX   # just these
    python Catacombs/audio/record.py --sid 6581    # the older SID model (default 8580)
    python Catacombs/audio/analyze.py              # check them, write spectrograms

Everything lands in `.debug-tmp/audio/`:

- `<SCENE>.prg`: the scene build. Open it in VICE to listen yourself.
- `<SCENE>-8580.wav` (or `-6581`): the audio, 48 kHz mono. The first 3 seconds are the C64 booting.
- `<SCENE>.dump.txt`: every SID register write with its cycle time.
- `<SCENE>-8580.png`: spectrogram with the score's expected notes drawn on top (from `analyze.py`).

Scenes (each is the game built with `-p:Scene=AUDIO_<name>`, which boots
straight into that sound and stays there; see `Audio.Init`):

| Scene | Plays |
| --- | --- |
| `EXPLORE` | Exploration tune, once round and into the repeat |
| `COMBAT` | Combat tune, twice round |
| `TITLE` | Title jingle, then the exploration tune taking over |
| `DEATH` | Death jingle, then exploration |
| `VICTORY` | 5 s of exploration, the victory fanfare breaking in, exploration resuming |
| `RESUME` | 10 s of exploration, 5 s of combat, exploration resumed where it was |
| `SFX` | Every effect in turn, 2 s apart, no music. Order: step a, step b, door, pickup, weapon, growl, hurt, sword, monster death, player death |
| `SFXMIX` | The same effects over the exploration tune (polite steps only sound when voice 3 is free) |
| `SFXGATED` | The same effects while voice 3 holds a chord: each must sound as it does alone |

To build one scene by hand and run it:

    dotnet build Catacombs/Catacombs.csproj -p:Scene=AUDIO_COMBAT
    Catacombs\measure.bat
    python tools/vice_record.py --prg prg/catacombs.prg --wav .debug-tmp/combat.wav --seconds 40

`record.py` rebuilds the normal game at the end, so `prg/catacombs.prg` is
the real game again. No keys are ever sent to VICE; each VICE instance is
started with its own throwaway config and closed through its own monitor
port, so other running VICE windows are left alone.

VICE only renders audio at normal speed, so WAVs record in real time (all
scenes in parallel). Register dumps run in warp mode and take seconds:
`record.py --no-wav` for those alone.

## What `analyze.py` checks

1. Register dump: after every tick, the SID registers written by the real
   6502 player in VICE must equal what the Python model computes from the
   same score. This is exact, so it covers every note, length, waveform,
   ADSR, pulse width, vibrato step and filter value.
2. Audio: level and clipping; that every note held for 0.1 s or more is in
   the spectrum at its expected pitch; per voice, the level of every note
   of 0.3 s or more (a voice under -40 dB is reported: it plays but is not
   heard); that the output is silent between effects; each effect's peak,
   rms and attack; and, given SFX and SFXGATED, how each effect started
   over a held chord compares with the same effect alone.

## Interrupts

`Audio.Init` calls `C64.Music.Install()`, which chains the player onto the
KERNAL IRQ vector at `$0314` and re-times CIA1 timer A to exactly one PAL
frame, started on raster line 252: the player ticks 50.12 times a second,
always in the bottom border, so it never hides line 251 from
`Screen.WaitForVBlank`. With `Screen.SetSplit` on, the split's own raster
interrupt calls the same chain once per frame at line 252 instead, so the
tempo is identical either way and the calls can come in any order. Tempo
values in `score.py` are ticks per row at that rate. The player uses no
zero page. Side effect: the KERNAL jiffy clock counts 50ths of a second
after `Install()`. `C64.Music.Frame` is a 0-255 frame counter from the
same tick. Details are at the top of `asm/C64Music.asm`.
