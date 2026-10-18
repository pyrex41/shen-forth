#!/usr/bin/env python3
"""Bounded Shen 43.1 certification runner and receipt parser."""
import argparse, json, os, re, selectors, signal, subprocess, sys, time
from pathlib import Path

REPORT = re.compile(r"^passed \.\.\. (\d+)\s*$")
FAILED = re.compile(r"^failed \.\.\. (\d+)\s*$")

def rss_tree(root):
    try:
        rows = subprocess.check_output(["ps", "-axo", "pid=,ppid=,rss="], text=True)
    except (OSError, subprocess.CalledProcessError):
        return []
    children, rss = {}, {}
    for line in rows.splitlines():
        bits = line.split()
        if len(bits) == 3:
            try:
                pid, ppid, kb = map(int, bits)
                children.setdefault(ppid, []).append(pid); rss[pid] = kb
            except ValueError: pass
    todo, seen = [root], set()
    while todo:
        pid = todo.pop()
        if pid in seen: continue
        seen.add(pid); todo.extend(children.get(pid, []))
    return [rss[p] for p in seen if p in rss]

def kill_group(proc):
    try: os.killpg(proc.pid, signal.SIGKILL)
    except ProcessLookupError: pass

def parse(text):
    reports, pending = [], None
    individual = sum(1 for line in text.splitlines() if line.strip() == "passed")
    for line in text.splitlines():
        m = REPORT.match(line.strip())
        if m: pending = int(m.group(1)); continue
        m = FAILED.match(line.strip())
        if m and pending is not None:
            reports.append({"passed": pending, "failed": int(m.group(1))}); pending = None
    return reports, individual

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--timeout", type=float, default=240)
    ap.add_argument("--rss-mib", type=int, default=1536)
    ap.add_argument("--output-mib", type=int, default=4)
    ap.add_argument("--launcher", default=None)
    ap.add_argument("--receipt-dir", default=None)
    args = ap.parse_args()
    root = Path(__file__).resolve().parents[1]
    launcher = Path(args.launcher or root / "bin/shen").resolve()
    outdir = Path(args.receipt_dir or root / "evidence/certification"); outdir.mkdir(parents=True, exist_ok=True)
    stamp = time.strftime("%Y%m%d-%H%M%S") + f"-{os.getpid()}"
    log_path, json_path = outdir / f"canonical-43.1-{stamp}.log", outdir / f"canonical-43.1-{stamp}.json"
    cmd = [str(launcher), "script", "runme.shen"]; start = time.monotonic(); reason = None; output = bytearray()
    proc = subprocess.Popen(cmd, cwd=root / "kernel/tests", stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                            stderr=subprocess.STDOUT, start_new_session=True)
    try: proc.stdin.write(b"y\n" * 512); proc.stdin.close()
    except BrokenPipeError: pass
    sel = selectors.DefaultSelector(); sel.register(proc.stdout, selectors.EVENT_READ); cap = args.output_mib * 1024 * 1024
    with log_path.open("wb") as log:
        while True:
            if time.monotonic() - start > args.timeout: reason = "timeout"; kill_group(proc); break
            peak = max(rss_tree(proc.pid) or [0])
            if peak > args.rss_mib * 1024: reason = f"rss-limit ({peak} KiB > {args.rss_mib} MiB)"; kill_group(proc); break
            for key, _ in sel.select(0.2):
                chunk = os.read(key.fd, 65536)
                if not chunk: sel.unregister(key.fileobj); continue
                keep = min(len(chunk), cap - len(output)); output.extend(chunk[:keep]); log.write(chunk[:keep])
                if keep < len(chunk): reason = "output-limit"; kill_group(proc); break
            if reason: break
            if proc.poll() is not None and not sel.get_map(): break
    proc.wait(); text = output.decode("utf-8", "replace"); reports, individual = parse(text)
    valid = reason is None and proc.returncode == 0 and len(reports) == 40 and reports[-1:] == [{"passed": 154, "failed": 0}] and individual == 154
    status = "PASS" if valid else ("UNKNOWN" if reason or proc.returncode != 0 else "FAIL")
    if not valid and reason is None and proc.returncode == 0: reason = "receipt did not satisfy 40 reports, final 154/0, and 154 passed lines"
    receipt = {"status": status, "reason": reason, "command": cmd, "cwd": str(root / "kernel/tests"), "log": str(log_path),
               "reports": reports, "passed_lines": individual, "exit_code": proc.returncode, "timeout_s": args.timeout,
               "rss_mib": args.rss_mib, "output_mib": args.output_mib, "duration_s": round(time.monotonic()-start, 3)}
    json_path.write_text(json.dumps(receipt, indent=2) + "\n"); receipt["receipt"] = str(json_path); print(json.dumps(receipt, indent=2))
    return 0 if status == "PASS" else 1

if __name__ == "__main__": sys.exit(main())
