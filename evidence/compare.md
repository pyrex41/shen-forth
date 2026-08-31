# Compare: shen-go vs shen-c vs shen-forth

Live run 2026-08-29T19:16:51Z–19:36:52Z. Host Darwin arm64.
Reference is **shen-go S41.2** (`/Users/reuben/projects/shen-go/.bin/shen-go`), not `shen-go-shen42`.
Bifrost adapters stay **experimental** for shen-c and shen-forth. Not production.

cwd for Bifrost: `/Users/reuben/projects/bifrost`
command: `python3 bifrost.py --impls shen-go,shen-c,shen-forth`
`SHEN_C_HOME` unset. gforth from Nix 0.7.3 on PATH.
Bifrost does **not** record wall time; walls below are outer `perf_counter` / `/usr/bin/time -p`.

## Table

| impl | bifrost | bifrost wall s | canonical runme | canonical wall s |
|------|---------|----------------|-----------------|------------------|
| shen-go (S41.2 ref) | **pass** 31/31 | 5.879 | **pass** 134/0 | 5.97 |
| shen-c (S42 exp) | **fail** 25/31 | 3.569 | **fail** 129/5 | 85.22 |
| shen-forth (S42 exp) | **fail** 25/31 | 57.063 | **pass** 134/0 | 595.22 |

Combined `--impls shen-go,shen-c,shen-forth`: **fail** 21 pass / 10 FAIL, wall 63.22s, exit 1.
Agreement cases that pass solo can fail combined (go vs S42 string/float/hush).

## SHAs / launchers

| tree | HEAD | launcher |
|------|------|----------|
| bifrost | 7d2f9bbcf745bfb90aa95a7e101fb59e9c4eccfa | python3 bifrost.py |
| shen-go | 54c0afd24ab37a6b7aca1c3e3d9f5132dea5bda9 | ../shen-go/.bin/shen-go |
| shen-go-shen42 (not used) | b95bfeffe1c25708754ab11f7d944da3f7b84390 | — |
| shen-c | e1c8073050fbe7a8843f46ca2207de48e2a91c41 | ../shen-c/bin/shen-c (not rlwrap wrapper) |
| shen-forth | 1eb3d11699c10f12828539585e480ac51583233f | ../shen-forth/bin/shen |

## Evals (not Bifrost corpus)

Expr: `(version)`, `(+ 1 1)`, recursive countdown 20 via `(let C (/. F (/. N (if (= N 0) done (F F (- N 1))))) (C C 20))`.

| impl | --version | (version) | (+ 1 1) | countdown20 | walls s |
|------|-----------|-----------|---------|-------------|---------|
| shen-go | 41.2 (port Go …) | 41.2 | 2 | done | 0.186 / 0.144 / 0.145 / 0.150 |
| shen-c | Shen-C 0.2.3 | "42" | 2 | done | 0.005 / 0.093 / 0.090 / 0.100 |
| shen-forth | shen-forth 0.1.0-s42 | "42" | 2 | done | 0.017 / 1.528 / 1.699 / 1.756 |

## Canonical runme

All four trees have `runme.shen` = `(load "harness.shen")` then `(load "kerneltests.shen")`.

- shen-go: `cd kernel/tests && yes \| .bin/shen-go script runme.shen` → passed 134 failed 0, real 5.97, EXIT:0
- shen-c: `cd shen/test/s42 && yes \| env -u SHEN_C_HOME ../../../bin/shen-c script runme.shen` → passed 129 failed 5 (4 yacc `Function list is undefined`, 1 montague `Function t is undefined`), real 85.22, harness EXIT:0. **Not corpus green.**
- shen-forth: `./bin/shen eval -e '(do (cd "kernel/tests") (load "runme.shen"))'` → passed 134 failed 0, real 595.22, EXIT:0. Interpreter only; port-owned mark/sweep; no AOT.

## Bifrost solo FAIL (S42 vs 41.2 goldens)

Shared by shen-c and shen-forth (6): `version-expr-41-2`, `version-banner-contains`, `trap-error-message` (`"boom"` vs `boom`), `string-concat` / `tlstr` / `n-to-string` (quoted strings).

Combined extra FAILs: `float-add-imprecise` (go `0.30000000000000004` vs c/forth `0.3`), `hush-file-write` (go writes, c/forth empty), `load-toplevel-echo` (forth `error: open: cannot open`; go/c agree), `error-message-head` (quoted+newline on c/forth).

`tail-call-countdown` (100000) PASS on all three. `recursion-fib-file` PASS on all three.

Logs: `evidence/compare.log`, `compare-evals.log`, `compare-runme-shen-{go,c,forth}.log`.
