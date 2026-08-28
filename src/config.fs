\ Foundation config. ANS-shaped, Gforth-first.

s" 0.0.0-foundation" 2constant shen-forth-version

0 constant ty-nil
1 constant ty-symbol
2 constant ty-string
3 constant ty-number
4 constant ty-cons
5 constant ty-vector
6 constant ty-closure
7 constant ty-stream
8 constant ty-exception

$00ff constant ty-mask
$0100 constant mark-bit
$0200 constant free-bit
