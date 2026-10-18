\ Interpreter bootstrap of the 16 canonical modules; (version) from the kernel.

kl-ensure-boot
kl-booted @ s" kernel booted" assert-true

s" (version)" kl-read-string kl-nil kl-eval
dup obj-type ty-string = s" version type" assert-true
kl-string-bytes s" 43.1" compare 0= s" version 43.1" assert-true

s" (+ 1 2)" kl-read-string kl-nil kl-eval
kl>f 3 s>d d>f f= s" boot still +" assert-true

s" (value *version*)" kl-read-string kl-nil kl-eval
kl-string-bytes s" 43.1" compare 0= s" *version* 43.1" assert-true

s" (package? shen)" kl-read-string kl-nil kl-eval
kl-true = s" shen package metadata" assert-true

s" (fn append)" kl-read-string kl-nil kl-eval
obj-type ty-closure = s" append lambda metadata" assert-true

s" (let V (absvector 2) (do (address-> V 1 99) (<-address V 1)))" kl-read-string kl-nil kl-eval
kl>f 99 s>d d>f f= s" vector store/read" assert-true

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


\ Source loading needs the package host hook; deferred for bounded port scope.
