\ Vendored S42 files are present and not loaded as Forth.

s" kernel/klambda/backend.kl" file-exists s" backend.kl" assert-true
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
s" kernel/klambda/extension-features.kl" file-exists 0= s" no extension kl" assert-true

\ Loading KLambda as Forth must not have happened.
s" shen.klambda-booted" forth-wordlist search-wordlist 0= s" kernel not booted" assert-true
