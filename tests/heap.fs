\ Mark/sweep reclaims unrooted objects; rooted objects survive.

heap-collect
heap-n-live @ constant live0

1 kl-int 2 kl-int kl-cons drop
heap-n-live @ live0 - 3 = s" live grew by cons tree" assert-true
heap-collect
heap-n-live @ live0 = s" unrooted cons swept" assert-true

variable held
1 kl-int 2 kl-int kl-cons
dup heap-root-push
heap-collect
dup obj-free? 0= s" rooted cons live" assert-true
dup kl-car obj-free? 0= s" rooted car live" assert-true
heap-root-pop drop
dup held ! drop
heap-collect
held @ obj-free? s" unrooted after pop swept" assert-true

\ Reuse: a new object can occupy a swept slot without moving live ones.
s" keep" kl-intern constant kept
kept heap-root-push
1 kl-int drop
heap-collect
2 kl-int
dup obj-type ty-number = s" reuse alloc" assert-true
drop
heap-root-pop drop
kept s" keep" kl-intern = s" intern survives gc" assert-true
