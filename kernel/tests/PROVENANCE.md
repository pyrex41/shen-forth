# Kernel tests provenance

Canonical Shen 43.1 suite from Mark Tarver: `runme.shen` loads `harness.shen`
then `kerneltests.shen`.

Source: `pyrex41/shen-upstream@s42-pristine-20260825` (`Test Programs/`),
archive SHA-256
`30abdc7e5a1e27b7a20109c1ed141e4712885e31f24d9710d16415fbbd4dfb23`.
Vendored byte-identical with `shen-go-shen42/kernel/tests` at that pin
(CRLF as in the S42 zip).

The 43.1 qualification target is 40 reports ending `passed ... 154` and
`failed ... 0`, with 154 individual `passed` lines. `tools/certify.py` applies
240 seconds, 1536 MiB RSS, and 4 MiB output bounds and retains a log and JSON
receipt. A limit or incomplete receipt is UNKNOWN and exits nonzero; groups
must not be skipped to fake a green run.
