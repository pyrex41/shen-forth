\ Tagged object constructors and accessors.

here
s" hello" kl-string
swap here = s" string not in dictionary" assert-true
dup obj-type ty-string = s" string type" assert-true
kl-string-bytes s" hello" compare 0= s" string bytes" assert-true

42 kl-int
dup obj-type ty-number = s" number type" assert-true
kl>f 42 s>d d>f f= s" number value" assert-true

s" abc" kl-intern
dup obj-type ty-symbol = s" symbol type" assert-true
s" abc" kl-intern = s" intern unique" assert-true

kl-nil obj-type ty-nil = s" nil type" assert-true

1 kl-int 2 kl-int kl-cons
dup obj-type ty-cons = s" cons type" assert-true
dup kl-car kl>f 1 s>d d>f f= s" car" assert-true
kl-cdr kl>f 2 s>d d>f f= s" cdr" assert-true

3 kl-vector
dup obj-type ty-vector = s" vector type" assert-true
dup kl-vlen 3 = s" vector len" assert-true
dup 0 kl-vref kl-nil = s" vector init nil" assert-true
dup kl-true swap 1 kl-vset
dup 1 kl-vref kl-true = s" vector set" assert-true
drop

0 2 kl-nil kl-closure
dup obj-type ty-closure = s" closure type" assert-true
dup kl-closure-arity 2 = s" closure arity" assert-true
kl-closure-env kl-nil = s" closure env" assert-true

0 stdin kl-stream
dup obj-type ty-stream = s" stream type" assert-true
dup kl-stream-kind 0 = s" stream kind" assert-true
kl-stream-handle stdin = s" stream handle" assert-true

s" boom" kl-string kl-exception
dup obj-type ty-exception = s" exception type" assert-true
kl-exception-msg kl-string-bytes s" boom" compare 0= s" exception msg" assert-true

kl-true obj-type ty-symbol = s" true symbol" assert-true
kl-false obj-type ty-symbol = s" false symbol" assert-true
