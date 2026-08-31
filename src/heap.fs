\ Port-owned non-moving mark/sweep heap.
\ Object storage is ALLOCATE'd. Never HERE/ALLOT for KL objects.

4 cells constant /obj-header
0 constant obj-flags
1 cells constant obj-psize
2 cells constant obj-all
3 cells constant obj-free

256 1024 * constant arena-default
16384 constant root-max
100000 constant heap-default-cap

variable heap-arenas
variable heap-arena
variable heap-all
variable heap-free
variable heap-n-live
variable heap-cap
variable heap-gc-enabled
variable heap-roots
variable heap-root-n
variable alloc-tmp
variable scan-addr

defer obj-mark-payload
:noname drop ; is obj-mark-payload

defer intern-mark
:noname ; is intern-mark

defer extra-mark
:noname ; is extra-mark

: obj-payload ( obj -- addr ) /obj-header + ;

: obj-type ( obj -- n )
  dup 0= if drop -1 exit then
  obj-flags + @ ty-mask and ;

: obj-mark? ( obj -- f ) obj-flags + @ mark-bit and 0<> ;

: obj-free? ( obj -- f ) obj-flags + @ free-bit and 0<> ;

: obj-mark! ( obj -- )
  dup obj-flags + @ mark-bit or swap obj-flags + ! ;

: obj-unmark! ( obj -- )
  dup obj-flags + @ mark-bit invert and swap obj-flags + ! ;

: obj-set-type ( obj n -- )
  over obj-flags + @ ty-mask invert and or swap obj-flags + ! ;

: obj-set-free ( obj -- )
  dup obj-flags + @ free-bit or swap obj-flags + ! ;

: obj-clear-free ( obj -- )
  dup obj-flags + @ free-bit invert and swap obj-flags + ! ;

: aligned-bytes ( n -- n )
  1 cells 1- + 1 cells negate and
  1 cells max ;

0 constant arena-next
1 cells constant arena-cap
2 cells constant arena-bump
3 cells constant /arena-header

: arena-data ( arena -- addr ) /arena-header + ;

variable arena-tmp

: arena-new ( bytes -- arena )
  aligned-bytes
  dup arena-default < if drop arena-default then
  dup /arena-header + allocate throw arena-tmp !
  0 arena-tmp @ arena-next + !
  arena-tmp @ arena-cap + !
  0 arena-tmp @ arena-bump + !
  heap-arenas @ arena-tmp @ arena-next + !
  arena-tmp @ heap-arenas !
  arena-tmp @ dup heap-arena ! ;

: heap-init ( -- )
  0 heap-arenas !
  0 heap-arena !
  0 heap-all !
  0 heap-free !
  0 heap-n-live !
  heap-default-cap heap-cap !
  0 heap-gc-enabled !
  0 heap-root-n !
  root-max cells allocate throw heap-roots !
  arena-default arena-new drop ;

: heap-root-push ( obj -- )
  heap-root-n @ root-max >= abort" heap root overflow"
  heap-roots @ heap-root-n @ cells + !
  1 heap-root-n +! ;

: heap-root-pop ( -- obj )
  heap-root-n @ 0= abort" heap root underflow"
  -1 heap-root-n +!
  heap-roots @ heap-root-n @ cells + @ ;

: heap-root-peek ( -- obj )
  heap-root-n @ 0= abort" heap root underflow"
  heap-roots @ heap-root-n @ 1- cells + @ ;

: heap-mark ( obj -- )
  dup 0= if drop exit then
  dup obj-free? if drop exit then
  dup obj-mark? if drop exit then
  dup obj-mark!
  obj-mark-payload ;

: heap-mark-roots ( -- )
  heap-root-n @ 0 ?do
    heap-roots @ i cells + @ heap-mark
  loop ;

: heap-reclaim ( obj -- )
  dup obj-set-free
  heap-free @ over obj-free + !
  heap-free !
  -1 heap-n-live +! ;

: heap-sweep ( -- )
  heap-all @
  begin dup while
    dup obj-free? 0= if
      dup obj-mark? if
        dup obj-unmark!
      else
        dup heap-reclaim
      then
    then
    obj-all + @
  repeat drop ;

: in-heap-range? ( addr -- f )
  scan-addr !
  heap-arenas @
  begin dup while
    dup dup arena-data swap arena-bump + @ +
    scan-addr @ -rot within
    if drop -1 exit then
    arena-next + @
  repeat drop 0 ;

: obj-looks-valid? ( addr -- f )
  dup 0= if drop 0 exit then
  dup aligned over <> if drop 0 exit then
  dup in-heap-range? 0= if drop 0 exit then
  dup obj-free? if drop 0 exit then
  obj-flags + @ obj-magic and obj-magic = ;

: maybe-mark ( addr -- )
  dup obj-looks-valid? if heap-mark else drop then ;

: scan-cells ( lo hi -- )
  2dup u< 0= if 2drop exit then
  swap ?do i @ maybe-mark cell +loop ;

: scan-range ( a b -- )
  2dup u< if scan-cells else swap scan-cells then ;

: heap-mark-stacks ( -- )
  sp@ sp0 scan-range
  rp@ rp0 scan-range ;

: heap-collect ( -- )
  intern-mark
  extra-mark
  heap-mark-roots
  heap-sweep
  heap-n-live @ 2* heap-cap @ max heap-cap ! ;

: heap-maybe-gc ( -- )
  heap-gc-enabled @ 0= if exit then
  heap-n-live @ heap-cap @ < if exit then
  heap-collect ;

: arena-room ( n -- f )
  heap-arena @ dup 0= if drop drop 0 exit then
  dup arena-cap + @ swap arena-bump + @ - swap >= ;

: bump-from-arena ( total -- obj )
  heap-arena @ arena-data heap-arena @ arena-bump + @ +
  swap heap-arena @ arena-bump + +! ;

\ LIFO free list: take the head if it is large enough.
: take-free ( need -- obj|0 )
  heap-free @ dup 0= if nip exit then
  dup obj-psize + @ 2 pick < if 2drop 0 exit then
  dup obj-free + @ heap-free !
  nip dup obj-clear-free
  0 over obj-free + ! ;

: heap-bump ( total -- obj )
  dup arena-room 0= if
    dup /arena-header + arena-new drop
  then
  bump-from-arena ;

: heap-alloc ( n type -- obj )
  heap-maybe-gc
  swap aligned-bytes swap
  over take-free dup 0= if
    drop over /obj-header + heap-bump
    0 over obj-flags + !
    2 pick over obj-psize + !
    heap-all @ over obj-all + !
    dup heap-all !
    0 over obj-free + !
  then
  alloc-tmp !
  alloc-tmp @ swap obj-set-type drop
  alloc-tmp @ obj-flags + dup @ obj-magic or swap !
  1 heap-n-live +!
  alloc-tmp @ ;
