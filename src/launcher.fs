\ Launcher: eval / script / repl / --version.

: shen-version-print ( -- )
  ." shen-forth " shen-forth-version type cr ;

: shen-help-print ( -- )
  ." usage: shen [eval|script|repl|--version|--help] [--verbose]" cr
  ."   eval [-q] [-e FORM] [-l FILE]   evaluate after booting the S42 kernel" cr
  ."   script FILE                     (load FILE) via the kernel reader" cr
  ."   repl                            boot kernel and enter Shen" cr
  ."   --verbose                       boot/launcher diagnostics (stderr)" cr
  ."   --version" cr
  ." env: SHEN_FORTH_ROOT (kernel files), SHEN_FORTH_QUIET, SHEN_FORTH_VERBOSE" cr ;

: s= ( c-addr1 u1 c-addr2 u2 -- f ) compare 0= ;

: looks-like-cmd ( c-addr u -- f )
  dup 0= if 2drop 0 exit then
  2dup s" eval" s= >r
  2dup s" script" s= r> or >r
  2dup s" repl" s= r> or >r
  2dup s" --version" s= r> or >r
  2dup s" -V" s= r> or >r
  2dup s" --help" s= r> or >r
  2dup s" -h" s= r> or
  nip nip ;

variable launch-i
variable launch-done
variable arg-i
variable eval-want-repl
create form-buf 4096 allot
variable form-len

: diag-err-msg ( obj -- )
  dup obj-type ty-string = if kl-string-bytes diag-type else drop then ;

: catch-kl ( obj -- value )
  kl-nil ['] kl-eval catch
  dup 0= if drop exit then
  dup kl-err-throw = if
    drop s" error: " diag-type
    kl-err-obj @ kl-exception-msg diag-err-msg
    diag-cr 1 (bye)
  then
  throw ;

: hush-stdout? ( -- f )
  s" *hush*" kl-intern
  dup kl-sym-bound? 0= if drop 0 exit then
  kl-sym-value kl-true = ;

: eval-print-value ( obj -- )
  hush-stdout? if drop exit then
  kl-print cr ;

: eval-one ( c-addr u -- )
  dup 0= if 2drop shen-help-print 2 (bye) then
  dup 4095 > if drop 4095 then
  dup form-len !
  form-buf swap move
  form-buf form-len @ kl-read-string catch-kl
  eval-print-value ;

: next-arg ( -- c-addr u )
  arg-i @ 1+ dup arg-i !
  arg ;

: require-arg ( -- c-addr u )
  next-arg dup 0= if
    2drop s" error: missing argument" diag-type diag-cr 2 (bye)
  then ;

: set-hush ( -- )
  kl-true s" *hush*" kl-intern kl-sym-value! ;

: shen-load-file ( c-addr u -- )
  kl-string heap-root-push
  s" load" kl-intern heap-root-push
  heap-root-pop heap-root-pop kl-nil kl-cons kl-cons
  catch-kl drop ;

: flag-starts? ( c-addr u -- f )
  dup 0= if 2drop 0 exit then
  drop c@ [char] - = ;

: shen-repl ( -- )
  kl-ensure-boot
  s" shen.shen" kl-intern dup kl-sym-fun if
    kl-nil kl-apply drop
  else
    drop
    shen-quiet @ 0= if
      s" shen-forth REPL (kernel booted)" diag-type diag-cr
    then
    begin
      ." > " pad 256 accept dup 0= if drop 0 (bye) then
      pad swap kl-read-string kl-nil ['] kl-eval catch
      dup 0= if drop kl-print cr else
        dup kl-err-throw = if
          drop s" error: " diag-type
          kl-err-obj @ kl-exception-msg diag-err-msg
          diag-cr
        else throw then
      then
    again
  then
  0 (bye) ;

: shen-eval ( -- )
  launch-i @ arg-i !
  0 eval-want-repl !
  kl-ensure-boot
  begin
    next-arg
    dup
  while
    2dup s" -e" s= >r 2dup s" --eval" s= r> or if
      2drop require-arg eval-one
    else 2dup s" -l" s= >r 2dup s" --load" s= r> or if
      2drop require-arg shen-load-file
    else 2dup s" -q" s= >r 2dup s" --quiet" s= r> or if
      2drop set-hush
    else 2dup s" -r" s= >r 2dup s" --repl" s= r> or if
      2drop -1 eval-want-repl !
    else 2dup s" --verbose" s= if
      2drop -1 shen-verbose !
    else 2dup flag-starts? if
      s" error: invalid eval argument: " diag-type diag-type diag-cr 2 (bye)
    else
      eval-one
    then then then then then then
  repeat 2drop
  eval-want-repl @ if shen-repl then
  0 (bye) ;

: shen-script ( -- )
  launch-i @ 1+ arg
  dup 0= if 2drop shen-help-print 2 (bye) then
  dup 4095 > if drop 4095 then
  dup form-len ! form-buf swap move
  kl-ensure-boot
  form-buf form-len @
  2dup file-ok? 0= if diag-type s" : not found" diag-type diag-cr 1 (bye) then
  2drop
  form-buf form-len @ shen-load-file
  0 (bye) ;

: shen-dispatch ( c-addr u -- )
  2dup s" --version" s= >r 2dup s" -V" s= r> or if 2drop shen-version-print 0 (bye) then
  2dup s" --help" s= >r 2dup s" -h" s= r> or if 2drop shen-help-print 0 (bye) then
  2dup s" eval" s= if 2drop shen-eval then
  2dup s" script" s= if 2drop shen-script then
  2dup s" repl" s= if 2drop shen-repl then
  2drop shen-help-print 2 (bye) ;

: shen-launch ( -- )
  0 launch-done !
  32 0 do
    i arg dup 0= if 2drop leave then
    2dup s" --verbose" s= if
      2drop -1 shen-verbose ! 0 shen-quiet !
    else 2dup looks-like-cmd if
      i launch-i !
      shen-dispatch
    else 2drop then then
  loop
  launch-done @ 0= if shen-repl then ;
