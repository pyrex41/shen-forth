# shen-forth

Gforth-first port of [Shen](https://shenlanguage.org) (43.1). Tagged heap
objects, intern, a port-owned non-moving mark/sweep collector, a `.kl`
reader, KLambda primitives (including `trap-error`), and **interpreter
bootstrap** of the 16 canonical kernel files in Shen/CL `boot.lsp` order.
There is no KLambda-to-Forth AOT.

Shen programs compile to KLambda. This port implements the primitive set
and hosts those files on the ALLOCATE heap.

## Status

Interpreter bootstrap. Experimental. `bin/shen eval -e "(version)"`
prints `43.1` after loading the 16 boot modules (`backend.kl` is
vendored, not booted). Every kernel form is evaluated, including
load-time `(declare ...)` and `(shen.build-lambda-table ...)`. `make
corpus` / `make certify` runs the 43.1 `kernel/tests/runme.shen`
(`harness.shen` + `kerneltests.shen`) with no groups skipped. Certification
requires 40 reports ending in 154 passed and 0 failed, plus 154 individual
passed lines. The certifier bounds execution at 240 seconds, 1536 MiB RSS,
and 4 MiB output; a limit produces UNKNOWN and a retained log/JSON receipt.
The current clean canonical run reached the 1536 MiB RSS bound after 15.583
seconds, so it is UNKNOWN rather than a qualification PASS. Mark/sweep is implemented but not
collected after kernel boot (Gforth VALUE/local roots). `bin/shen`
does not chdir; `*hush*` gates stdout only.

Pinned kernel: ShenOSKernel 43.1 commit `715824a52b55e9c81a9501df8548ff3edd8411f5`, archive
SHA-256 `900eedef44f440af78f9395ed06e689ce1604546fc309f5bc7c8bc84cc651920`.
See [`kernel/klambda/PROVENANCE.md`](kernel/klambda/PROVENANCE.md).

## How to run

```sh
nix develop -c make
```

That is the supported command. It runs the bounded Forth harness and checks the 16
canonical `.kl` files, checks
`kernel/tests/runme.shen` is present, and runs `bin/shen eval` (which
boots the kernel). Full certification is `nix develop -c make certify`.

```sh
nix develop -c make version
nix develop -c ./bin/shen --version
nix develop -c ./bin/shen --help
nix develop -c ./bin/shen eval -e "(+ 1 2)"
nix develop -c ./bin/shen eval -e "(version)"
nix develop -c ./bin/shen eval -e "(map (+ 1) [1 2 3])"
nix develop -c ./bin/shen eval -e "(declare foo [number --> number])"
nix develop -c ./bin/shen eval -e '(do (cd "kernel/tests") (load "runme.shen"))'
nix develop -c make corpus
nix develop -c make certify
nix develop -c ./bin/shen repl
```

Without Nix: GNU Gforth 0.7.3+ and GNU Make on `PATH`, then `make` from the
repo root. The flake builds GNU Gforth 0.7.3 (nixpkgs `gforth` is marked
broken on Darwin) and `gnumake`.

## Layout

| Path | Role |
|------|------|
| `bin/shen` | launcher: `eval` / `script` / `repl` / `--version` |
| `src/` | tagged objects, intern, mark/sweep, reader, eval, primitives |
| `tests/` | Gforth harness (not `runme.shen`) |
| `kernel/klambda/` | 43.1 canonical `.kl` files |
| `kernel/tests/` | 43.1 `runme.shen` / `harness.shen` / `kerneltests.shen` |
| `flake.nix` | Nix dev shell |

REPL and kernel objects live on the **ALLOCATE** heap, not Forth dictionary
space (`HERE`/`ALLOT`). Collection is mark/sweep and does not move live
objects. `backend.kl` is vendored for audit and is not booted.

## Non-goals (this stage)

- KLambda-to-Forth AOT
- C17 / Boehm GC (that is `shen-c`, via Nix + pkg-config, not Homebrew)
- Chicken-style C-stack continuations
- Lifetime-inserted non-GC C (rung 2)

## License

BSD-3. Port code is Copyright (c) 2026 Reuben Brooks. Vendored KLambda is
Copyright (c) 2010-2022 Mark Tarver. See `LICENSE`.
