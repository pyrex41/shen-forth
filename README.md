# shen-forth

Gforth-first port of [Shen](https://shenlanguage.org) (S42). This repository
is at the **foundation** stage: tagged heap objects, intern, and a
port-owned non-moving mark/sweep collector. The vendored KLambda kernel is
**not** booted.

Shen programs compile to KLambda. A port implements the primitive set and
hosts the 15 canonical `.kl` files. This port does that later as an
**interpreter bootstrap** (ANS-shaped Forth, Gforth first). There is no
KLambda-to-Forth AOT.

## Status

Foundation scaffold only. Not certified. Do not treat this as a production
Shen.

Pinned kernel: `pyrex41/shen-upstream@s42-pristine-20260825`, archive
SHA-256 `30abdc7e5a1e27b7a20109c1ed141e4712885e31f24d9710d16415fbbd4dfb23`.
See [`kernel/klambda/PROVENANCE.md`](kernel/klambda/PROVENANCE.md).

## How to run

```sh
nix develop -c make
```

That is the supported command. It runs the Forth harness, checks the 15
canonical `.kl` files (and that `extension-*.kl` are absent), and exercises
the `bin/shen` stub.

```sh
nix develop -c make version
nix develop -c ./bin/shen --version
nix develop -c ./bin/shen --help
nix develop -c ./bin/shen repl     # stub; does not boot the kernel
```

`eval` and `script` are recognized launcher verbs and refuse to run until a
later stage loads KLambda.

Without Nix: GNU Gforth 0.7.3+ and GNU Make on `PATH`, then `make` from the
repo root. The flake builds GNU Gforth 0.7.3 (nixpkgs `gforth` is marked
broken on Darwin) and `gnumake`.

## Layout

| Path | Role |
|------|------|
| `bin/shen` | launcher stub: `eval` / `script` / `repl` / `--version` |
| `src/` | tagged objects, intern, mark/sweep heap |
| `tests/` | Gforth harness |
| `kernel/klambda/` | 15 canonical S42 `.kl` files (not loaded) |
| `flake.nix` | Nix dev shell |

REPL and kernel objects live on the **ALLOCATE** heap, not Forth dictionary
space (`HERE`/`ALLOT`). Collection is mark/sweep and does not move live
objects.

## Non-goals (this stage)

- Booting or loading `kernel/klambda/*.kl`
- KLambda-to-Forth AOT
- C17 / Boehm GC (that is `shen-c`, via Nix + pkg-config, not Homebrew)
- Chicken-style C-stack continuations

## License

BSD-3. Port code is Copyright (c) 2026 Reuben Brooks. Vendored KLambda is
Copyright (c) 2010-2022 Mark Tarver. See `LICENSE`.
