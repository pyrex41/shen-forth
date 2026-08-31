\ Print KL values for eval/REPL output.

: kl-print-num ( obj -- )
  kl>f fdup fdup floor f= if
    fdup f0< >r fabs f>d drop 0 <# #s r> sign #> type
  else
    f.
  then ;

defer kl-print
:noname ( obj -- )
  dup obj-type case
    ty-nil of drop ." []" endof
    ty-symbol of kl-symbol-name kl-string-bytes type endof
    ty-string of
      \ CLI/REPL print matches Go shen.a (unquoted). KLambda `str` still quotes.
      kl-string-bytes type
    endof
    ty-number of kl-print-num endof
    ty-cons of
      ." ["
      begin
        dup obj-type ty-cons =
      while
        dup kl-car kl-print
        kl-cdr
        dup kl-nil = if drop ." ]" exit then
        dup obj-type ty-cons = if space else s"  | " type kl-print ." ]" exit then
      repeat
      drop
    endof
    ty-vector of drop ." #<absvector>" endof
    ty-closure of drop ." #<closure>" endof
    ty-stream of drop ." #<stream>" endof
    ty-exception of
      ." #<error " kl-exception-msg dup obj-type ty-string = if kl-string-bytes type else drop then ." >"
    endof
    drop ." #<unknown>"
  endcase
; is kl-print
