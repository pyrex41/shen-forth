\ Launcher stub: eval / script / repl / --version. Does not boot KLambda.

: shen-version-print ( -- )
  ." shen-forth " shen-forth-version type cr ;

: shen-help-print ( -- )
  ." usage: shen [eval|script|repl|--version|--help]" cr
  ."   eval     recognized; kernel is not booted at foundation" cr
  ."   script   recognized; kernel is not booted at foundation" cr
  ."   repl     stub notice; kernel is not booted" cr
  ."   --version" cr ;

: shen-repl-stub ( -- )
  ." shen-forth foundation" cr
  ." kernel/klambda is vendored (S42) and not booted" cr
  0 (bye) ;

: shen-eval-stub ( -- )
  ." shen-forth: eval recognized; S42 kernel is not booted at foundation" cr
  2 (bye) ;

: shen-script-stub ( -- )
  ." shen-forth: script recognized; S42 kernel is not booted at foundation" cr
  2 (bye) ;

: s= ( c-addr1 u1 c-addr2 u2 -- f ) compare 0= ;

: looks-like-cmd ( c-addr u -- f )
  2dup s" eval" s= >r
  2dup s" script" s= r> or >r
  2dup s" repl" s= r> or >r
  2dup s" --version" s= r> or >r
  2dup s" -V" s= r> or >r
  2dup s" --help" s= r> or >r
  2dup s" -h" s= r> or
  nip nip ;

: shen-dispatch ( c-addr u -- )
  2dup s" --version" s= >r 2dup s" -V" s= r> or if 2drop shen-version-print 0 (bye) then
  2dup s" --help" s= >r 2dup s" -h" s= r> or if 2drop shen-help-print 0 (bye) then
  2dup s" eval" s= if 2drop shen-eval-stub then
  2dup s" script" s= if 2drop shen-script-stub then
  2dup s" repl" s= if 2drop shen-repl-stub then
  2drop shen-help-print 2 (bye) ;

: shen-launch ( -- )
  argc 0 do
    i arg looks-like-cmd if i arg shen-dispatch then
  loop
  shen-repl-stub ;
