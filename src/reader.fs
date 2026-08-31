\ KLambda s-expression reader. No AOT. Strings have no backslash escapes.

variable rd-buf
variable rd-len
variable rd-pos

: rd-set ( c-addr u -- )
  rd-len ! rd-buf ! 0 rd-pos ! ;

: rd-at-end? ( -- f ) rd-pos @ rd-len @ >= ;

: rd-c ( -- c )
  rd-at-end? if 0 else rd-buf @ rd-pos @ + c@ then ;

: rd-next ( -- ) rd-pos @ 1+ rd-pos ! ;

: rd-ws? ( c -- f )
  dup bl = over 9 = or over 10 = or over 13 = or swap 12 = or ;

: rd-skip ( -- )
  begin
    rd-at-end? if exit then
    rd-c rd-ws? if rd-next else
      rd-c [char] \ = if
        rd-pos @ 1+ rd-len @ < if
          rd-buf @ rd-pos @ 1+ + c@ [char] * = if
            2 rd-pos +!
            begin
              rd-at-end? if exit then
              rd-c [char] * = if
                rd-pos @ 1+ rd-len @ < if
                  rd-buf @ rd-pos @ 1+ + c@ [char] \ = if
                    2 rd-pos +! 0
                  else rd-next -1 then
                else rd-next -1 then
              else rd-next -1 then
            0= until
          else
            rd-buf @ rd-pos @ 1+ + c@ [char] \ = if
              begin
                rd-at-end? 0= if rd-c 10 <> else 0 then
              while rd-next repeat
              rd-at-end? 0= if rd-next then
            else exit then
          then
        else exit then
      else exit then
    then
  again ;

: rd-more? ( -- f ) rd-skip rd-at-end? 0= ;

: rd-delim? ( c -- f )
  dup rd-ws? over [char] ( = or over [char] ) = or
  over [char] [ = or over [char] ] = or
  over [char] { = or over [char] } = or
  swap [char] " = or ;

: rd-has-digit ( c-addr u -- f )
  0 ?do
    dup i + c@ dup [char] 0 >= swap [char] 9 <= and if drop -1 unloop exit then
  loop drop 0 ;

: rd-atom-number ( c-addr u -- obj|0 )
  2dup rd-has-digit 0= if 2drop 0 exit then
  2dup >float if
    2drop kl-fnum
  else 2drop 0 then ;

defer rd-expr

: rd-string ( -- obj )
  rd-next
  rd-pos @
  begin
    rd-at-end? if s" unterminated string" abort then
    rd-c [char] " <>
  while rd-next repeat
  rd-buf @ over +  rd-pos @ rot -
  kl-string
  rd-next ;

: rd-atom ( -- obj )
  rd-pos @
  begin
    rd-at-end? 0= if rd-c rd-delim? 0= else 0 then
  while rd-next repeat
  rd-buf @ over +  rd-pos @ rot -
  2dup rd-atom-number dup if nip nip exit then drop
  2dup s" true" compare 0= if 2drop kl-true exit then
  2dup s" false" compare 0= if 2drop kl-false exit then
  kl-intern ;

: rd-list ( -- obj )
  rd-next
  kl-nil
  begin
    rd-skip
    rd-at-end? if s" unclosed '('" abort then
    rd-c [char] ) = if rd-next kl-list-rev exit then
    rd-expr swap kl-cons
  again ;

\ Shen [a b | c] is cons-form so KLambda eval constructs the list.
: rd-cons-form ( list -- expr )
  dup kl-nil = if exit then
  dup obj-type ty-cons <> if exit then
  dup kl-car swap kl-cdr recurse
  heap-root-push heap-root-push
  s" cons" kl-intern
  heap-root-pop heap-root-pop
  kl-nil kl-cons kl-cons kl-cons ;

: rd-brackets ( -- obj )
  rd-next
  kl-nil
  begin
    rd-skip
    rd-at-end? if s" unclosed '['" abort then
    rd-c [char] ] = if rd-next kl-list-rev rd-cons-form exit then
    rd-c [char] | = if
      rd-next rd-skip rd-expr
      rd-skip rd-c [char] ] <> if s" expected ] after |" abort then
      rd-next
      swap kl-list-rev
      begin dup kl-nil <> while
        dup kl-car rot kl-cons swap kl-cdr
      repeat drop rd-cons-form exit
    then
    rd-expr swap kl-cons
  again ;

:noname ( -- obj )
  rd-skip
  rd-at-end? if kl-nil exit then
  rd-c case
    [char] ( of rd-list endof
    [char] [ of rd-brackets endof
    [char] " of rd-string endof
    [char] { of rd-next s" {" kl-intern endof
    [char] } of rd-next s" }" kl-intern endof
    [char] ) of s" unexpected ')'" abort endof
    [char] ] of s" unexpected ']'" abort endof
    drop rd-atom exit
  endcase
; is rd-expr

: kl-read-string ( c-addr u -- obj )
  rd-set rd-expr ;

: kl-read-all ( c-addr u -- list )
  rd-set kl-nil
  begin rd-more? while
    rd-expr swap kl-cons
  repeat
  kl-list-rev ;
