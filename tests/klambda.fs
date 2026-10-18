\ Vendored S42 files are present and not loaded as Forth.

s" kernel/klambda/core.kl" file-exists s" core.kl" assert-true
s" kernel/klambda/declarations.kl" file-exists s" declarations.kl" assert-true
s" kernel/klambda/load.kl" file-exists s" load.kl" assert-true
s" kernel/klambda/macros.kl" file-exists s" macros.kl" assert-true
s" kernel/klambda/prolog.kl" file-exists s" prolog.kl" assert-true
s" kernel/klambda/reader.kl" file-exists s" reader.kl" assert-true
s" kernel/klambda/sequent.kl" file-exists s" sequent.kl" assert-true
s" kernel/klambda/sys.kl" file-exists s" sys.kl" assert-true
s" kernel/klambda/t-star.kl" file-exists s" t-star.kl" assert-true
s" kernel/klambda/toplevel.kl" file-exists s" toplevel.kl" assert-true
s" kernel/klambda/track.kl" file-exists s" track.kl" assert-true
s" kernel/klambda/types.kl" file-exists s" types.kl" assert-true
s" kernel/klambda/writer.kl" file-exists s" writer.kl" assert-true
s" kernel/klambda/yacc.kl" file-exists s" yacc.kl" assert-true
s" kernel/klambda/PROVENANCE.md" file-exists s" PROVENANCE.md" assert-true
s" kernel/tests/runme.shen" file-exists s" runme.shen" assert-true
s" kernel/tests/harness.shen" file-exists s" harness.shen" assert-true
s" kernel/tests/kerneltests.shen" file-exists s" kerneltests.shen" assert-true

\ Kernel files are vendored; interpreter boot is lazy until eval/repl.
kl-booted @ 0= s" kernel lazy until eval" assert-true
s" kernel/klambda/dict.kl" file-exists s" dict.kl" assert-true
s" kernel/klambda/init.kl" file-exists s" init.kl" assert-true
