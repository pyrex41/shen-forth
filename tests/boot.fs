\ Interpreter bootstrap of the 15 canonical modules; (version) from the kernel.

kl-ensure-boot
kl-booted @ s" kernel booted" assert-true

s" (version)" kl-read-string kl-nil kl-eval
dup obj-type ty-string = s" version type" assert-true
kl-string-bytes s" 42" compare 0= s" version 42" assert-true

s" (+ 1 2)" kl-read-string kl-nil kl-eval
kl>f 3 s>d d>f f= s" boot still +" assert-true

s" (value *version*)" kl-read-string kl-nil kl-eval
kl-string-bytes s" 42" compare 0= s" *version* 42" assert-true

s" (map (+ 1) [1 2 3])" kl-read-string kl-nil kl-eval
dup obj-type ty-cons = s" map type" assert-true
dup kl-car kl>f 2 s>d d>f f= s" map 2" assert-true
dup kl-cdr kl-car kl>f 3 s>d d>f f= s" map 3" assert-true
kl-cdr kl-cdr kl-car kl>f 4 s>d d>f f= s" map 4" assert-true

s" (thaw (freeze 8))" kl-read-string kl-nil kl-eval
kl>f 8 s>d d>f f= s" boot freeze/thaw" assert-true

s" (defun fact (N) (if (= N 0) 1 (* N (fact (- N 1)))))" kl-read-string kl-nil kl-eval drop
s" (fact 5)" kl-read-string kl-nil kl-eval
kl>f 120 s>d d>f f= s" recursive fact" assert-true

s" (declare foo [number --> number])" kl-read-string kl-nil kl-eval
dup obj-type ty-symbol = s" declare result symbol" assert-true
kl-symbol-name kl-string-bytes s" foo" compare 0= s" declare foo" assert-true

s" (trap-error (value shen.*lambdatable*) (lambda E ()))" kl-read-string kl-nil kl-eval
dup kl-nil = 0= s" lambda table set" assert-true
drop

s\" (cd \"kernel/tests\")" kl-read-string kl-nil kl-eval drop
s\" (load \"cartprod.shen\")" kl-read-string kl-nil kl-eval
dup obj-type ty-symbol = s" load cartprod symbol" assert-true
kl-symbol-name kl-string-bytes s" loaded" compare 0= s" load cartprod" assert-true
s" (cartesian-product [1 2] [1 2])" kl-read-string kl-nil kl-eval
dup obj-type ty-cons = s" cartesian type" assert-true
drop
