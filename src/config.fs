\ Port config. ANS-shaped, Gforth-first.

s" 0.1.0-s42" 2constant shen-forth-version

variable shen-quiet
variable shen-verbose

0 constant ty-nil
1 constant ty-symbol
2 constant ty-string
3 constant ty-number
4 constant ty-cons
5 constant ty-vector
6 constant ty-closure
7 constant ty-stream
8 constant ty-exception
8 constant ty-last

$00ff constant ty-mask
$0100 constant mark-bit
$0200 constant free-bit
$00AA0000 constant obj-magic

\ Distinct from ALLOCATE ior and ABORT throw codes.
-4096 constant kl-err-throw

0 constant stream-in
1 constant stream-out
2 constant stream-std

16 constant kl-max-args
