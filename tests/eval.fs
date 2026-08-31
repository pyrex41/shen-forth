\ Reader + primitive eval (no kernel load).

s" 42" kl-read-string dup obj-type ty-number = s" read int type" assert-true
kl>f 42 s>d d>f f= s" read int val" assert-true

s" hello" kl-intern constant sy-hello
s" hello" kl-read-string sy-hello = s" read symbol intern" assert-true

s\" \"hi\"" kl-read-string kl-string-bytes s" hi" compare 0= s" read string" assert-true

s\" (pos \"ABC\" 0)" kl-read-string kl-nil kl-eval
kl-string-bytes s" A" compare 0= s" eval pos 0" assert-true
s\" (pos \"ABC\" 2)" kl-read-string kl-nil kl-eval
kl-string-bytes s" C" compare 0= s" eval pos 2" assert-true

s" ()" kl-read-string kl-nil = s" read nil" assert-true

s" (+ 1 2)" kl-read-string kl-nil kl-eval
kl>f 3 s>d d>f f= s" eval +" assert-true

s" (* 4 5)" kl-read-string kl-nil kl-eval
kl>f 20 s>d d>f f= s" eval *" assert-true

s" (if true 1 2)" kl-read-string kl-nil kl-eval
kl>f 1 s>d d>f f= s" eval if true" assert-true

s" (if false 1 2)" kl-read-string kl-nil kl-eval
kl>f 2 s>d d>f f= s" eval if false" assert-true

s" (let X 3 (+ X 4))" kl-read-string kl-nil kl-eval
kl>f 7 s>d d>f f= s" eval let" assert-true

s" (let X 3 (let Y 4 (+ X Y)))" kl-read-string kl-nil kl-eval
kl>f 7 s>d d>f f= s" eval nested let" assert-true

s\" (cn (str Y) (str 1))" kl-read-string kl-nil kl-eval
kl-string-bytes s" Y1" compare 0= s" eval cn str" assert-true

s" (cons 1 (cons 2 ()))" kl-read-string kl-nil kl-eval
dup obj-type ty-cons = s" eval cons type" assert-true
dup kl-car kl>f 1 s>d d>f f= s" eval cons car" assert-true
kl-cdr kl-car kl>f 2 s>d d>f f= s" eval cons cadr" assert-true

s" (defun inc (N) (+ N 1))" kl-read-string kl-nil kl-eval drop
s" (inc 10)" kl-read-string kl-nil kl-eval
kl>f 11 s>d d>f f= s" eval defun" assert-true

s" ((lambda X (+ X X)) 6)" kl-read-string kl-nil kl-eval
kl>f 12 s>d d>f f= s" eval lambda" assert-true

s" ((lambda X (lambda Y (+ X Y))) 3 4)" kl-read-string kl-nil kl-eval
kl>f 7 s>d d>f f= s" eval nested lambda overapply" assert-true

s" ((lambda X (lambda Y (lambda Z (+ X (+ Y Z))))) 1 2 3)" kl-read-string kl-nil kl-eval
kl>f 6 s>d d>f f= s" eval triple lambda overapply" assert-true

s" (and true false)" kl-read-string kl-nil kl-eval
kl-false = s" eval and" assert-true

s" (or false true)" kl-read-string kl-nil kl-eval
kl-true = s" eval or" assert-true

s" (do 1 2)" kl-read-string kl-nil kl-eval
kl>f 2 s>d d>f f= s" eval do" assert-true

s\" (trap-error (simple-error \"boom\") (lambda E 99))" kl-read-string kl-nil kl-eval
kl>f 99 s>d d>f f= s" eval trap-error" assert-true

s" /." kl-intern sy-lambda = 0= s" slashdot distinct from lambda" assert-true

s" ((/. X (+ X 1)) 4)" kl-read-string kl-nil kl-eval
kl>f 5 s>d d>f f= s" eval slashdot" assert-true

s" ((/. X Y (+ X Y)) 3 4)" kl-read-string kl-nil kl-eval
kl>f 7 s>d d>f f= s" eval nested slashdot" assert-true

s" [1 2 3]" kl-read-string kl-nil kl-eval
dup obj-type ty-cons = s" eval [] type" assert-true
dup kl-car kl>f 1 s>d d>f f= s" eval [] 1" assert-true
dup kl-cdr kl-car kl>f 2 s>d d>f f= s" eval [] 2" assert-true
kl-cdr kl-cdr kl-car kl>f 3 s>d d>f f= s" eval [] 3" assert-true

s" [1 | 2]" kl-read-string kl-nil kl-eval
dup kl-car kl>f 1 s>d d>f f= s" eval [|] car" assert-true
kl-cdr kl>f 2 s>d d>f f= s" eval [|] cdr" assert-true

s\" (trap-error (simple-error \"boom\") (/. E 99))" kl-read-string kl-nil kl-eval
kl>f 99 s>d d>f f= s" eval trap-error slashdot" assert-true

s" (thaw (freeze 8))" kl-read-string kl-nil kl-eval
kl>f 8 s>d d>f f= s" eval freeze/thaw" assert-true

s" (set foo 5)" kl-read-string kl-nil kl-eval drop
s" (value foo)" kl-read-string kl-nil kl-eval
kl>f 5 s>d d>f f= s" eval set/value" assert-true

s" (cond (false 1) (true 2))" kl-read-string kl-nil kl-eval
kl>f 2 s>d d>f f= s" eval cond" assert-true

: printed ( obj -- c-addr u )
  s" /tmp/shen-forth-printcap" w/o bin create-file throw
  outfile-id >r
  dup to outfile-id
  swap kl-print
  dup flush-file drop
  r> to outfile-id
  close-file throw
  s" /tmp/shen-forth-printcap" slurp-file ;

s" ()" kl-read-string kl-nil kl-eval printed
s" []" compare 0= s" print nil []" assert-true

s" (cons 1 ())" kl-read-string kl-nil kl-eval printed
s" [1]" compare 0= s" print [1]" assert-true

s" (cons 1 (cons 2 ()))" kl-read-string kl-nil kl-eval printed
s" [1 2]" compare 0= s" print [1 2]" assert-true

s" (cons 1 2)" kl-read-string kl-nil kl-eval printed
s" [1 | 2]" compare 0= s" print [1 | 2]" assert-true

s\" (cn \"fo\" \"o\")" kl-read-string kl-nil kl-eval printed
s" foo" compare 0= s" print string unquoted" assert-true
