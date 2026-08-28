\ Minimal pass/fail harness.

variable #ok
variable #bad

: pass ( -- ) 1 #ok +! ;

: fail ( c-addr u -- )
  1 #bad +!
  ." FAIL: " type cr ;

: assert-true ( f c-addr u -- )
  2 pick if 2drop drop pass else rot drop fail then ;

: assert-eq ( n1 n2 c-addr u -- )
  2over = if 2drop 2drop pass else
    ." FAIL: " type ."  (got " rot . ." want " swap . ." )" cr
    1 #bad +!
  then ;

: file-exists ( c-addr u -- f )
  r/o open-file if drop 0 else close-file drop -1 then ;

: test-summary ( -- )
  ." passed " #ok @ . ." failed " #bad @ . cr
  #bad @ if 1 (bye) else 0 (bye) then ;
