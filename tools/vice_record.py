"""Record what a C64 program plays, by running it in its own VICE instance.

    python tools/vice_record.py --prg prg/x.prg --wav out.wav --seconds 30
    python tools/vice_record.py --prg prg/x.prg --dump out.txt --seconds 30

--wav   the SID's audio output, written by VICE itself (48 kHz mono), so
        nothing depends on the machine's sound card. Runs in real time:
        VICE only renders audio at 100% speed.
--dump  every SID register write with its cycle time ("<cycles since the
        previous write> <register> <value>" per line), VICE's own "dump"
        sound device. Runs in warp mode: a minute takes a couple of seconds.

No keys are sent, so the program has to make its sound by itself (for
Catacombs: an audio scene build, see Catacombs/audio/README.md). --seconds
counts emulated time from power-on; the program starts about 3 seconds in.

Safe next to other VICE instances: the one started here gets its own
monitor port and a throwaway config file (the user's VICE settings are
neither read nor changed), is told to quit through its monitor once the
emulated time is up (which is what makes VICE close the output file
properly), and is only ever killed by its own pid.
"""

import argparse
import os
import re
import socket
import subprocess
import sys
import time

PAL_CLOCK = 985248
VICE = os.environ.get('VICE_EXE', r'C:\tools\VICE\bin\x64sc.exe')


def _free_port():
    s = socket.socket()
    s.bind(('127.0.0.1', 0))
    port = s.getsockname()[1]
    s.close()
    return port


def _monitor(port, command, wait=0.25):
    """One monitor command; returns its output. Emulation is paused while
    the connection is open."""
    s = socket.create_connection(('127.0.0.1', port), timeout=5)
    try:
        s.sendall(command.encode() + b'\n')
        time.sleep(wait)
        s.settimeout(0.5)
        data = b''
        try:
            while True:
                chunk = s.recv(4096)
                if not chunk:
                    break
                data += chunk
        except (socket.timeout, OSError):
            pass
        return data.decode('latin-1')
    finally:
        try:
            s.sendall(b'x\n')      # resume (ignored if the command was quit)
        except OSError:
            pass
        s.close()


class Recording:
    """A VICE instance recording `prg`. start() launches it, finish() waits
    for the emulated time to pass and closes it."""

    def __init__(self, prg, out, seconds, dump=False, sid='8580'):
        self.prg = os.path.abspath(prg)
        self.out = os.path.abspath(out)
        self.seconds = seconds
        self.dump = dump
        self.sid = sid
        self.port = _free_port()
        self.proc = None

    def start(self):
        if not os.path.exists(self.prg):
            raise FileNotFoundError(self.prg)
        os.makedirs(os.path.dirname(self.out), exist_ok=True)
        if os.path.exists(self.out):
            os.remove(self.out)
        config = os.path.join(os.path.dirname(self.out), 'vice-record.ini')
        args = [VICE, '-config', config, '-pal',
                '-sidenginemodel', '257' if self.sid == '8580' else '256', '-sidfilters',
                '-remotemonitor', '-remotemonitoraddress', 'ip4://127.0.0.1:%d' % self.port,
                '-autostartprgmode', '1', '-minimized', '-soundoutput', '1',
                '-sounddev', 'dump' if self.dump else 'wav', '-soundarg', self.out]
        if self.dump:
            args += ['-warp', '-soundwarpmode', '1']
        args += ['-autostart', self.prg]
        self.proc = subprocess.Popen(args, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        self.started = time.time()
        return self

    def finish(self):
        target = self.seconds * PAL_CLOCK
        deadline = self.started + self.seconds + 90
        if not self.dump:
            time.sleep(max(0.0, self.started + self.seconds - time.time()))
        try:
            while True:
                if self.proc.poll() is not None:
                    raise RuntimeError('VICE exited by itself (code %s)' % self.proc.returncode)
                if time.time() > deadline:
                    raise RuntimeError('VICE never reached %d emulated seconds' % self.seconds)
                try:
                    reply = _monitor(self.port, 'sw')
                except OSError:
                    time.sleep(0.5)        # monitor not up yet
                    continue
                m = re.search(r'Stopwatch:\s*(\d+)', reply)
                if m and int(m.group(1)) >= target:
                    break
                time.sleep(0.2 if self.dump else 0.5)
            try:
                _monitor(self.port, 'quit', wait=0.5)
            except OSError:
                pass
            self.proc.wait(15)
        finally:
            if self.proc.poll() is None:
                self.proc.kill()           # only ever our own instance
        if not os.path.exists(self.out) or os.path.getsize(self.out) <= 44:
            raise RuntimeError('nothing was recorded to ' + self.out)
        return self.out


def record(prg, out, seconds, dump=False, sid='8580'):
    return Recording(prg, out, seconds, dump, sid).start().finish()


def read_dump(path):
    """A --dump file as a list of (absolute cycle, register, value)."""
    out = []
    now = 0
    with open(path) as f:
        for line in f:
            parts = line.split()
            if len(parts) != 3:
                continue
            now += int(parts[0])
            out.append((now, int(parts[1]), int(parts[2])))
    return out


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('--prg', required=True)
    ap.add_argument('--wav')
    ap.add_argument('--dump')
    ap.add_argument('--seconds', type=int, default=30)
    ap.add_argument('--sid', choices=['6581', '8580'], default='8580',
                    help='SID model for --wav (default 8580, as in VICE 3.10)')
    a = ap.parse_args()
    if bool(a.wav) == bool(a.dump):
        sys.exit('give exactly one of --wav / --dump')
    print('OK:' + record(a.prg, a.wav or a.dump, a.seconds, dump=bool(a.dump), sid=a.sid))
