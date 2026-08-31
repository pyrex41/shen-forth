\ Symbol intern table. ALLOCATE'd buckets; names are heap strings.

128 constant intern-buckets
variable intern-table

: intern-init ( -- )
  intern-buckets cells allocate throw intern-table !
  intern-buckets 0 do
    0 intern-table @ i cells + !
  loop ;

: hash-str ( c-addr u -- u )
  5381 -rot over + swap ?do
    33 * i c@ +
  loop ;

variable intern-scan
variable intern-hash

: intern-lookup ( c-addr u hash -- sym|0 )
  intern-buckets mod cells intern-table @ + @ intern-scan !
  begin intern-scan @ while
    2dup intern-scan @ kl-symbol-name kl-string-bytes compare 0= if
      2drop intern-scan @ exit
    then
    intern-scan @ obj-payload 2 cells + @ intern-scan !
  repeat
  2drop 0 ;

: kl-sym-name ( obj -- str ) obj-payload @ ;
: kl-sym-value ( obj -- x ) obj-payload 3 cells + @ ;
: kl-sym-value! ( x obj -- )
  tuck obj-payload 3 cells + !
  dup obj-payload 5 cells + dup @ 1 or swap ! drop ;
: kl-sym-bound? ( obj -- f ) obj-payload 5 cells + @ 1 and 0<> ;
: kl-sym-fun ( obj -- x ) obj-payload 4 cells + @ ;
: kl-sym-fun! ( x obj -- ) obj-payload 4 cells + ! ;

: kl-intern ( c-addr u -- sym )
  2dup hash-str intern-hash !
  2dup intern-hash @ intern-lookup dup if nip nip exit then drop
  kl-string heap-root-push
  6 cells ty-symbol heap-alloc
  heap-root-pop over obj-payload !
  intern-hash @ over obj-payload cell+ !
  intern-hash @ intern-buckets mod intern-table @ swap cells +
  dup @ 2 pick obj-payload 2 cells + !
  2dup ! drop
  0 over obj-payload 3 cells + !
  0 over obj-payload 4 cells + !
  0 over obj-payload 5 cells + ! ;

:noname ( -- )
  kl-nil heap-mark
  kl-true heap-mark
  kl-false heap-mark
  intern-table @ 0= if exit then
  intern-buckets 0 do
    intern-table @ i cells + @
    begin dup while
      dup heap-mark
      obj-payload 2 cells + @
    repeat drop
  loop
; is intern-mark

variable heap-ready

: heap-boot ( -- )
  heap-ready @ if exit then
  heap-init
  intern-init
  0 ty-nil heap-alloc to kl-nil
  s" true" kl-intern to kl-true
  s" false" kl-intern to kl-false
  -1 heap-gc-enabled !
  -1 heap-ready ! ;
