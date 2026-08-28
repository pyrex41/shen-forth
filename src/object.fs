\ Tagged KL objects on the ALLOCATE heap.

0 value kl-nil
0 value kl-true
0 value kl-false

: payload! ( x obj -- ) obj-payload ! ;
: payload@ ( obj -- x ) obj-payload @ ;

variable str-src
variable str-len

: kl-string ( c-addr u -- obj )
  str-len ! str-src !
  str-len @ aligned-bytes 1 cells + ty-string heap-alloc
  str-len @ over obj-payload !
  str-src @ over obj-payload cell+ str-len @ cmove ;

: kl-string-bytes ( obj -- c-addr u )
  obj-payload dup cell+ swap @ ;

: kl-int ( n -- obj )
  8 ty-number heap-alloc
  swap s>d d>f dup obj-payload df! ;

: kl>f ( obj -- ) ( F: -- r )
  obj-payload df@ ;

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

: kl-vref ( vec i -- obj )
  over kl-vlen over <= abort" vector bounds"
  dup 0< abort" vector bounds"
  cells swap obj-payload cell+ + @ ;

: kl-vset ( val vec i -- )
  over kl-vlen over <= abort" vector bounds"
  dup 0< abort" vector bounds"
  cells swap obj-payload cell+ + ! ;

: kl-symbol-name ( obj -- str ) obj-payload @ ;

: kl-closure ( xt arity env -- obj )
  heap-root-push
  3 cells ty-closure heap-alloc
  heap-root-pop over obj-payload 2 cells + !
  tuck obj-payload cell+ !
  tuck obj-payload ! ;

: kl-closure-xt ( obj -- xt ) obj-payload @ ;
: kl-closure-arity ( obj -- n ) obj-payload cell+ @ ;
: kl-closure-env ( obj -- env ) obj-payload 2 cells + @ ;

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

: mark-symbol ( obj -- ) kl-symbol-name heap-mark ;

: mark-closure ( obj -- ) kl-closure-env heap-mark ;

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
