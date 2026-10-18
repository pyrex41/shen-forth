\ Tagged KL objects on the ALLOCATE heap.

0 value kl-nil
0 value kl-true
0 value kl-false

: payload! ( x obj -- ) obj-payload ! ;
: payload@ ( obj -- x ) obj-payload @ ;

variable str-src
variable str-len

: kl-string ( c-addr u -- obj )
  dup 65536 u> if ." str-len=" . cr abort" string too large" then
  str-len ! str-src !
  str-src @ 0= abort" string: null src"
  str-len @ aligned-bytes 1 cells + ty-string heap-alloc
  str-len @ over obj-payload !
  str-src @ over obj-payload cell+ str-len @ cmove ;

: kl-string-bytes ( obj -- c-addr u )
  obj-payload dup cell+ swap @ ;

: kl-int ( n -- obj )
  8 ty-number heap-alloc
  swap s>d d>f dup obj-payload df! ;

: kl-fnum ( -- obj ) ( F: r -- )
  8 ty-number heap-alloc
  dup obj-payload df! ;

: kl>f ( obj -- ) ( F: -- r )
  obj-payload df@ ;

: kl-bool ( f -- obj )
  if kl-true else kl-false then ;

: kl-true? ( obj -- f )
  dup kl-true = if drop -1 exit then
  dup kl-false = if drop 0 exit then
  drop 0 ;

: kl-car ( obj -- x ) obj-payload @ ;
: kl-cdr ( obj -- x ) obj-payload cell+ @ ;
: kl-car! ( x obj -- ) obj-payload ! ;
: kl-cdr! ( x obj -- ) obj-payload cell+ ! ;

: kl-cons ( a b -- obj )
  swap heap-root-push heap-root-push
  2 cells ty-cons heap-alloc
  heap-root-pop over kl-cdr!
  heap-root-pop over kl-car! ;

: kl-vector ( n -- obj )
  dup 0< abort" negative vector"
  dup 1+ cells ty-vector heap-alloc
  2dup obj-payload !
  swap 0 ?do
    kl-nil over obj-payload cell+ i cells + !
  loop ;

: kl-vlen ( obj -- n ) obj-payload @ ;

variable vset-val
variable vset-vec
variable vset-idx

: kl-vref ( vec i -- obj )
  over kl-vlen over <= abort" vector bounds"
  dup 0< abort" vector bounds"
  cells swap obj-payload cell+ + @ ;

: kl-vset ( val vec i -- )
  vset-idx ! vset-vec ! vset-val !
  vset-idx @ 0< abort" vector bounds"
  vset-idx @ vset-vec @ kl-vlen >= abort" vector bounds"
  vset-val @ vset-vec @ obj-payload cell+ vset-idx @ cells + ! ;

: kl-symbol-name ( obj -- str ) obj-payload @ ;

: kl-closure ( xt arity env -- obj )
  heap-root-push
  6 cells ty-closure heap-alloc
  heap-root-pop over obj-payload 2 cells + !
  tuck obj-payload cell+ !
  tuck obj-payload !
  kl-nil over obj-payload 3 cells + !
  kl-nil over obj-payload 4 cells + !
  kl-nil over obj-payload 5 cells + ! ;

: kl-closure-xt ( obj -- xt ) obj-payload @ ;
: kl-closure-arity ( obj -- n ) obj-payload cell+ @ ;
: kl-closure-env ( obj -- env ) obj-payload 2 cells + @ ;
: kl-closure-params ( obj -- obj ) obj-payload 3 cells + @ ;
: kl-closure-body ( obj -- obj ) obj-payload 4 cells + @ ;
: kl-closure-partial ( obj -- obj ) obj-payload 5 cells + @ ;
: kl-closure-params! ( x obj -- ) obj-payload 3 cells + ! ;
: kl-closure-body! ( x obj -- ) obj-payload 4 cells + ! ;
: kl-closure-partial! ( x obj -- ) obj-payload 5 cells + ! ;
: kl-closure-env! ( x obj -- ) obj-payload 2 cells + ! ;

: kl-stream ( kind handle -- obj )
  2 cells ty-stream heap-alloc
  tuck obj-payload cell+ !
  tuck obj-payload ! ;

: kl-stream-kind ( obj -- n ) obj-payload @ ;
: kl-stream-handle ( obj -- n ) obj-payload cell+ @ ;

: kl-exception ( msg -- obj )
  heap-root-push
  1 cells ty-exception heap-alloc
  heap-root-pop over payload! ;

: kl-exception-msg ( obj -- obj ) payload@ ;

: mark-cons ( obj -- )
  dup kl-car heap-mark kl-cdr heap-mark ;

: mark-vector ( obj -- )
  dup kl-vlen 0 ?do
    dup i kl-vref heap-mark
  loop drop ;

: mark-symbol ( obj -- )
  dup obj-payload @ heap-mark
  dup obj-payload 3 cells + @ heap-mark
  obj-payload 4 cells + @ heap-mark ;

: mark-closure ( obj -- )
  dup kl-closure-env heap-mark
  dup kl-closure-params heap-mark
  dup kl-closure-body heap-mark
  kl-closure-partial heap-mark ;

: kl-list-rev ( list -- list )
  kl-nil swap
  begin dup 0= over kl-nil = or 0= while
    dup obj-type ty-cons <> if drop exit then
    dup kl-car rot kl-cons swap kl-cdr
  repeat drop ;

: kl-list-len ( list -- n )
  0 swap
  begin dup kl-nil <> while
    swap 1+ swap kl-cdr
  repeat drop ;

: kl-equal ( a b -- f )
  begin
    2dup = if 2drop -1 exit then
    dup 0= if 2drop 0 exit then
    over 0= if 2drop 0 exit then
    2dup obj-type swap obj-type <> if 2drop 0 exit then
    dup obj-type case
      ty-symbol of
        2drop 0 exit
      endof
      ty-nil of
        2drop 0 exit
      endof
      ty-number of
        kl>f kl>f f= exit
      endof
      ty-string of
        kl-string-bytes rot kl-string-bytes compare 0= exit
      endof
      ty-cons of
        2dup kl-car swap kl-car recurse 0= if 2drop 0 exit then
        kl-cdr swap kl-cdr
      endof
      ty-vector of
        2dup kl-vlen swap kl-vlen <> if 2drop 0 exit then
        dup kl-vlen 0 ?do
          2dup i kl-vref swap i kl-vref recurse 0= if 2drop unloop 0 exit then
        loop
        2drop -1 exit
      endof
      ty-exception of
        kl-exception-msg swap kl-exception-msg recurse exit
      endof
      drop drop 0 exit
    endcase
  again ;

: mark-exception ( obj -- ) kl-exception-msg heap-mark ;

:noname ( obj -- )
  dup obj-type case
    ty-nil       of drop endof
    ty-symbol    of mark-symbol endof
    ty-string    of drop endof
    ty-number    of drop endof
    ty-cons      of mark-cons endof
    ty-vector    of mark-vector endof
    ty-closure   of mark-closure endof
    ty-stream    of drop endof
    ty-exception of mark-exception endof
    nip
  endcase
; is obj-mark-payload
