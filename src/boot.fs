\ Incremental ShenOSKernel 43.1 bootstrap. Port order follows Shen/CL boot.lsp.

variable kl-booted
create boot-path 1024 allot
variable mp-off

: diag-type ( c-addr u -- )
  shen-quiet @ if 2drop exit then
  stderr write-file drop
  stderr flush-file drop ;

: diag-cr ( -- ) s\" \n" diag-type ;

: diag-n ( n -- )
  0 <# #s #> diag-type ;

: kernel-root ( -- c-addr u )
  s" SHEN_FORTH_ROOT" getenv dup if exit then
  2drop s" ." ;

: file-ok? ( c-addr u -- f )
  r/o open-file if drop 0 else close-file drop -1 then ;

: module-path ( c-addr u -- c-addr u )
  2>r
  kernel-root
  dup mp-off !
  boot-path swap move
  boot-path mp-off @ + [char] / swap c!
  1 mp-off +!
  s" kernel/klambda/"
  dup mp-off +!
  boot-path mp-off @ + over - swap move
  2r>
  2dup boot-path mp-off @ + swap move
  mp-off @ + boot-path swap ;

variable boot-form
: kl-load-buffer ( c-addr u -- )
  rd-set 0 boot-form !
  begin rd-more? while
    1 boot-form +!
    rd-expr
    kl-nil ['] kl-eval catch
    dup if
      dup kl-err-throw = if
        drop s" error: " diag-type
        kl-err-obj @ dup obj-type ty-exception = if
          kl-exception-msg dup obj-type ty-string = if kl-string-bytes diag-type else drop then
        else drop then
        diag-cr s" form " diag-type boot-form @ diag-n diag-cr 1 throw
      then
      s" throw " diag-type diag-n s"  form " diag-type boot-form @ diag-n diag-cr throw
    then
    drop drop
  repeat ;

: boot-module ( c-addr u -- )
  shen-verbose @ if 2dup diag-type diag-cr then
  module-path
  2dup file-ok? 0= if
    diag-type s"  missing" diag-type diag-cr s" missing kernel file" kl-error
  then
  slurp-file kl-load-buffer ;

: initialise-kernel ( -- )
  \ init.kl defines the environment builder; ports must invoke it after
  \ loading the complete 43.1 core and after registering host globals.
  \ Keep its three phases as separate top-level evaluations. Gforth's
  \ tree-walker restores the caller frame after a deeply nested `do`; the
  \ generated Shen initializer otherwise loses the property-vector state
  \ before the lambda table is visible to subsequent calls.
  s" (shen.initialise-environment)" kl-read-string kl-nil kl-eval drop
  s" (shen.initialise-lambda-forms)" kl-read-string kl-nil kl-eval drop
  s" (shen.initialise-signedfuncs)" kl-read-string kl-nil kl-eval drop ;

: seed-property-vector ( -- )
  \ declarations.kl contains a top-level external-symbols PUT.  Shen/CL
  \ has this global from its host port before loading declarations; create
  \ the same pre-init boundary once dict.kl has supplied shen.dict.
  s" (set *property-vector* (shen.dict 20000))" kl-read-string kl-nil kl-eval drop ;

\ *hush* gates stdout only — not stderr, not file streams.
\ Do not use shen.char-stoutput? (true for st-out and st-err).
: override-pr-hush ( -- )
  s\" (defun pr (V S) (if (and (value *hush*) (= S (value *stoutput*))) V (shen.write-string V S)))"
  kl-read-string kl-nil ['] kl-eval catch
  dup if
    s" error: pr hush override failed" diag-type diag-cr throw
  then drop drop ;

: kl-boot-kernel ( -- )
  0 heap-gc-enabled !
  2000000 heap-cap !
  s" toplevel.kl" boot-module
  s" core.kl" boot-module
  s" sys.kl" boot-module
  s" dict.kl" boot-module
  seed-property-vector
  s" sequent.kl" boot-module
  s" yacc.kl" boot-module
  s" reader.kl" boot-module
  s" prolog.kl" boot-module
  s" track.kl" boot-module
  s" load.kl" boot-module
  s" writer.kl" boot-module
  s" macros.kl" boot-module
  s" declarations.kl" boot-module
  s" types.kl" boot-module
  s" t-star.kl" boot-module
  s" init.kl" boot-module
  initialise-kernel
\ Extensions and stlib are loaded only after the core is bootable.
\ The current Forth port does not yet implement their required host hooks.
\ Keep the bounded kernel scope explicit until those hooks are ported.
\
\ Legacy order retained below only as a reference for old evidence.
\ s" sys.kl" boot-module
  override-hot
  override-pr-hush
  heap-n-live @ 2* 2000000 max heap-cap !
  \ After the kernel exists, a full collect is not safe yet: Gforth VALUE
  \ cells and locals are not in the conservative sp/rp scan, and reusing
  \ unmarked conses corrupts load of kerneltests.shen (shen.map-h). Keep
  \ the mark/sweep implementation, but do not collect post-boot.
  0 heap-gc-enabled ! ;

: kl-ensure-boot ( -- )
  kl-booted @ if exit then
  kl-boot-kernel
  -1 kl-booted !
  clearstack ;

: shen.klambda-booted ( -- ) ;
