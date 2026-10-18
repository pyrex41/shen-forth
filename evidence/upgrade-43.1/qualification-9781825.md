# Shen-Forth 43.1 qualification receipt

Candidate: `9781825b630aae56886e4f1be4a362e4d58dc6ca` on
`upgrade/kernel-43.1`.

The port embeds the community ShenOSKernel 43.1 sources at commit
`715824a52b55e9c81a9501df8548ff3edd8411f5` (archive SHA-256
`900eedef44f440af78f9395ed06e689ce1604546fc309f5bc7c8bc84cc651920`). The
official 23 `.kl` source files were preserved byte-for-byte during this fix.
The earlier 24-file count mistakenly included Shen/CL's separate compiler
backend, subsequently removed because it is not in the release archive.

## Verified

The bounded Forth harness passed `112` checks with `0` failures using GNU
Gforth 0.7.3 from `/nix/store/01cnlgiyrlgalxlkxk141kswh71l0swr-gforth-0.7.3/bin/gforth`.
The launcher checks in `make test` also completed successfully, including
version, evaluation, file loading, closures, list mapping, error trapping, and
declaration checks.

The boot regressions cover these live boundaries:

- `(version)` returns `43.1`.
- `(package? shen)` returns true after boot.
- `(fn append)` resolves to a callable closure.
- `(let V (absvector 2) (do (address-> V 1 99) (<-address V 1)))` returns `99`.
- `shen.dict` symbol-key insertion and lookup return the inserted association.
- The public character stream contract remains byte-port scoped: `char-stream?`
  is false and the character input/output entry points remain catchable
  unsupported operations. Byte-oriented `read-byte`/`write-byte` are retained.

## Fix boundary

`kl-vset` previously mixed the value/vector/index stack with its bounds checks,
so vector writes used by Shen dictionaries could address the wrong cell. The
implementation now captures the three operands before checking and storing.

The boot path also keeps the kernel's interpreted Shen hash installed after
initialization. The property vector is populated with that hash; replacing it
afterward with the port's byte-oriented native hash made existing keys
unreachable. The three `shen.initialise` helpers are evaluated as separate
top-level forms because the Forth tree-walker loses the property-vector state
when the generated deeply nested `do` wrapper is evaluated as one call. The
three helper calls preserve the official helper order and produce the same
environment, lambda metadata, and signed-function metadata without changing
the vendored kernel forms.

## Qualification boundary

The current 43.1 canonical `kernel/tests/runme.shen` run was interrupted during
host cleanup after stale unbounded Forth probes saturated the machine. Its
result is `UNKNOWN`, not a pass or failure receipt. The older
`evidence/compare-runme-shen-forth.log` records an S42-era 134/0 run and is
historical evidence only; it does not qualify this 43.1 candidate.

This candidate remains an interpreter-only, experimental Forth port. It has no
KLambda-to-Forth AOT, and post-boot mark/sweep collection remains disabled
because Gforth VALUE/local roots are outside the conservative scan. Those are
the supported scope limits for this upgrade.
