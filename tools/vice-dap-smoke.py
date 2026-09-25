# Headless smoke test of the "debug a program on VICE" flow: talks DAP to
# TestDebugger.dll --dap the way VS Code would -- launch, set one breakpoint,
# run to it, inspect stack/locals/registers/evaluate, continue, step,
# disconnect -- and prints what comes back. Needs VICE installed. A stray
# x64sc.exe from an earlier run should be closed first.
#
#   python tools/vice-dap-smoke.py [Project] [File.cs] [line]
#   python tools/vice-dap-smoke.py Demo Program.cs 33
#
# A first argument containing a '.' (Class.Method) runs that unit test in
# SimpleEmulator instead (no VICE), to check the test-debugging path too.
import json, os, subprocess, threading, queue, sys, time

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
program = sys.argv[1] if len(sys.argv) > 1 else 'Demo'
bp_file = sys.argv[2] if len(sys.argv) > 2 else 'Program.cs'
bp_line = int(sys.argv[3]) if len(sys.argv) > 3 else 33
is_test = '.' in program

p = subprocess.Popen(['dotnet', os.path.join(ROOT, 'TestDebugger', 'bin', 'Debug', 'TestDebugger.dll'), '--dap'],
                     stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=ROOT)
q = queue.Queue()


def reader():
    f = p.stdout
    while True:
        header = b''
        while not header.endswith(b'\r\n\r\n'):
            c = f.read(1)
            if not c:
                q.put(None)
                return
            header += c
        n = int([l for l in header.decode().split('\r\n') if l.lower().startswith('content-length')][0].split(':')[1])
        q.put(json.loads(f.read(n)))


threading.Thread(target=reader, daemon=True).start()
threading.Thread(target=lambda: [sys.stderr.write('[adapter stderr] ' + l.decode(errors='replace')) for l in p.stderr], daemon=True).start()

seq = 0


def send(command, arguments=None):
    global seq
    seq += 1
    msg = {'seq': seq, 'type': 'request', 'command': command}
    if arguments is not None:
        msg['arguments'] = arguments
    data = json.dumps(msg).encode()
    p.stdin.write(b'Content-Length: %d\r\n\r\n' % len(data) + data)
    p.stdin.flush()
    return seq


events = []


def wait_for(pred, timeout=120, label=''):
    end = time.time() + timeout
    while time.time() < end:
        try:
            m = q.get(timeout=1)
        except queue.Empty:
            continue
        if m is None:
            raise SystemExit('adapter closed: ' + label)
        if m['type'] == 'event':
            if m['event'] == 'output':
                print('  output:', m['body']['output'].rstrip())
            else:
                print('  event:', m['event'], json.dumps(m.get('body')))
            events.append(m)
        if pred(m):
            return m
    raise SystemExit('timeout waiting for ' + label)


def request(command, arguments=None, timeout=120):
    s = send(command, arguments)
    r = wait_for(lambda m: m['type'] == 'response' and m['request_seq'] == s, timeout, command)
    if not r['success']:
        print(f'  {command} FAILED:', r.get('message'))
    return r


print('initialize'); request('initialize', {'adapterID': 'c64vice'})
print('launch'); r = request('launch', {'testSelector': program} if is_test else {'program': program})
wait_for(lambda m: m['type'] == 'event' and m['event'] == 'initialized', 30, 'initialized')
print('setBreakpoints', bp_file, bp_line)
source_dir = 'Test' if is_test else program
r = request('setBreakpoints', {'source': {'path': os.path.join(ROOT, source_dir, bp_file)}, 'breakpoints': [{'line': bp_line}]})
print('  ->', json.dumps(r['body']))
print('configurationDone'); request('configurationDone')
if bp_line == 0:
    # No breakpoint: let the program run, then pause it and continue again.
    time.sleep(5)
    # If VICE started fast the breakpoint may already have stopped it (a
    # pause is then a no-op); the stopped event will already have been read
    # while waiting for the pause response.
    print('pause'); request('pause', {'threadId': 1})
    if not any(e['event'] == 'stopped' for e in events):
        wait_for(lambda m: m['type'] == 'event' and m['event'] in ('stopped', 'terminated'), 30, 'stopped after pause')
    events.clear()
    print('  while paused:', end=' ')
    st = request('stackTrace', {'threadId': 1})['body']['stackFrames']
    print([(f['name'], f.get('instructionPointerReference')) for f in st])
    print('continue'); request('continue', {'threadId': 1})
    # Resuming from the pause runs on until the breakpoint the resolver
    # snapped to (the first executable line) is reached.
    wait_for(lambda m: m['type'] == 'event' and m['event'] in ('stopped', 'terminated'), 60, 'stopped after continue')
    print('disconnect'); request('disconnect')
    time.sleep(1)
    p.terminate()
    raise SystemExit(0)
stop = wait_for(lambda m: m['type'] == 'event' and m['event'] in ('stopped', 'terminated'), 60, 'stopped')

def inspect():
    st = request('stackTrace', {'threadId': 1})['body']['stackFrames']
    print('  stack:', [(f['name'], f.get('line'), f.get('source', {}).get('name')) for f in st])
    vs = request('variables', {'variablesReference': 1})['body']['variables']
    print('  locals:', [(v['name'], v['value']) for v in vs])
    regs = request('variables', {'variablesReference': 2})['body']['variables']
    print('  regs:', [(v['name'], v['value']) for v in regs][:5])

if stop['event'] == 'stopped':
    inspect()
    # HOLD_SECONDS=n keeps the session paused at the first stop, e.g. to take
    # a screenshot of VICE while it is stopped in the monitor.
    hold = float(os.environ.get('HOLD_SECONDS', '0'))
    if hold:
        print(f'holding the stop for {hold}s'); sys.stdout.flush()
        time.sleep(hold)
    print('evaluate i'); r = request('evaluate', {'expression': 'i'}); print('  ->', r.get('body'))
    print('continue'); request('continue', {'threadId': 1})
    wait_for(lambda m: m['type'] == 'event' and m['event'] in ('stopped', 'terminated'), 60, 'stopped2')
    inspect()
    hold2 = float(os.environ.get('HOLD_AFTER_CONTINUE', '0'))
    if hold2:
        print(f'holding the second stop for {hold2}s'); sys.stdout.flush()
        time.sleep(hold2)
    print('next'); request('next', {'threadId': 1})
    wait_for(lambda m: m['type'] == 'event' and m['event'] in ('stopped', 'terminated'), 60, 'stopped3')
    inspect()

print('disconnect'); request('disconnect')
time.sleep(1)
p.terminate()
