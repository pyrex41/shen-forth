\ Tree-walking KLambda evaluator. Tail positions trampoline.

variable ev-expr
variable ev-env
variable call-vec
variable kl-err-obj
variable trap-expr
variable trap-env
variable ea-head
variable ea-args
variable ea-env

0 value kl-fail
0 value sy-if
0 value sy-let
0 value sy-lambda
0 value sy-slashdot
0 value sy-defun
0 value sy-cond
0 value sy-do
0 value sy-and
0 value sy-or
0 value sy-freeze
0 value sy-thaw
0 value sy-trap
0 value sy-type
0 value sy-quote

: kl-error ( c-addr u -- )
  kl-string kl-exception dup kl-err-obj !
  kl-err-throw throw ;

: kl-expect-bool ( obj -- f )
  dup kl-true = if drop -1 exit then
  dup kl-false = if drop 0 exit then
  drop s" not a boolean" kl-error ;

: env-get ( key env -- val|0 )
  begin
    dup kl-nil = if 2drop 0 exit then
    dup kl-car kl-car 2 pick = if
      kl-car kl-cdr nip exit
    then
    kl-cdr
  again ;

: env-bind ( val sym env -- env )
  >r swap kl-cons r> kl-cons ;

: intern-specials ( -- )
  s" if" kl-intern to sy-if
  s" let" kl-intern to sy-let
  s" lambda" kl-intern to sy-lambda
  s" /." kl-intern to sy-slashdot
  s" defun" kl-intern to sy-defun
  s" cond" kl-intern to sy-cond
  s" do" kl-intern to sy-do
  s" and" kl-intern to sy-and
  s" or" kl-intern to sy-or
  s" freeze" kl-intern to sy-freeze
  s" thaw" kl-intern to sy-thaw
  s" trap-error" kl-intern to sy-trap
  s" type" kl-intern to sy-type
  s" quote" kl-intern to sy-quote
  s" shen.fail!" kl-intern to kl-fail ;

defer kl-eval
defer kl-apply

: eval-atom ( expr env -- value )
  over obj-type ty-symbol = if
    2dup env-get dup if nip nip exit then drop drop
  else drop then ;

variable ev-args-env
: eval-args ( list env -- list )
  ev-args-env @ >r
  ev-args-env !
  kl-nil swap
  begin dup 0= over kl-nil = or 0= while
    dup obj-type ty-cons <> if drop r> ev-args-env ! kl-list-rev exit then
    swap heap-root-push
    dup heap-root-push
    kl-car
    ev-args-env @ kl-eval
    heap-root-pop
    heap-root-pop
    rot swap kl-cons swap kl-cdr
  repeat
  drop
  r> ev-args-env !
  kl-list-rev ;

: bind-params ( params args env -- env )
  begin
    2 pick kl-nil = if nip nip exit then
    over kl-nil = if s" too few arguments" kl-error then
    2 pick kl-car heap-root-push
    over kl-car heap-root-pop rot env-bind
    >r kl-cdr swap kl-cdr swap r>
  again ;

: list-append ( a b -- list )
  over dup 0= swap kl-nil = or if nip exit then
  swap kl-list-rev
  begin dup 0= over kl-nil = or 0= while
    dup kl-car rot kl-cons swap kl-cdr
  repeat drop ;

: take-n ( list n -- taken rest )
  >r kl-nil swap
  r> 0 ?do
    dup kl-nil = if s" too few arguments" kl-error then
    dup kl-car rot kl-cons swap kl-cdr
  loop
  swap kl-list-rev swap ;

: args-to-vec ( list -- vec )
  dup kl-list-len kl-vector heap-root-push
  0 >r
  begin dup kl-nil <> while
    dup kl-car heap-root-pop dup heap-root-push r@ kl-vset
    kl-cdr r> 1+ >r
  repeat drop r> drop
  heap-root-pop ;

: lambda-clos ( param body env -- clos )
  rot heap-root-push swap heap-root-push heap-root-push
  0 1 heap-root-pop kl-closure
  heap-root-pop over kl-closure-body!
  heap-root-pop kl-nil kl-cons over kl-closure-params! ;

\ Shen (/. X Y body) is nested lambda; KLambda lambda stays 1-param.
: slashdot-clos ( args env -- clos )
  heap-root-push
  dup kl-car swap kl-cdr
  dup kl-cdr dup 0= swap kl-nil = or if
    kl-car
  else
    sy-slashdot swap kl-cons
  then
  heap-root-pop lambda-clos ;

: freeze-clos ( body env -- clos )
  swap heap-root-push heap-root-push
  0 0 heap-root-pop kl-closure
  heap-root-pop over kl-closure-body! ;

: defun-clos ( params body -- clos )
  over heap-root-push dup heap-root-push
  over kl-list-len
  0 swap kl-nil kl-closure
  heap-root-pop over kl-closure-body!
  heap-root-pop over kl-closure-params!
  nip nip ;

: do-defun ( name params body -- name )
  rot dup heap-root-push
  -rot defun-clos
  2dup swap kl-sym-fun! drop
  heap-root-pop drop ;

: arg@ ( i -- obj ) call-vec @ swap kl-vref ;

: call-native ( clos vec -- value )
  call-vec @ >r
  call-vec !
  kl-closure-xt execute
  r> call-vec ! ;

: ensure-clos ( obj -- clos )
  dup obj-type ty-closure = if exit then
  dup obj-type ty-symbol = if
    dup kl-sym-fun dup if
      nip dup obj-type ty-closure = if exit then
      drop s" not a function" kl-error
    then drop
    drop s" undefined function" kl-error
  then
  drop s" not a function" kl-error ;

: clos-with-partial ( clos args -- clos )
  >r
  dup kl-closure-xt over kl-closure-arity 2 pick kl-closure-env kl-closure
  over kl-closure-params over kl-closure-params!
  over kl-closure-body over kl-closure-body!
  nip r> over kl-closure-partial! ;

: apply-exact ( clos args -- value )
  over kl-closure-xt if
    args-to-vec call-native
  else
    dup heap-root-push over heap-root-push
    over kl-closure-params swap rot kl-closure-env bind-params
    heap-root-pop kl-closure-body swap
    heap-root-pop drop
    kl-eval
  then ;

\ Over-apply: ( clos args arity -- value ). arity is already on the stack.
: apply-over ( clos args arity -- value )
  rot >r take-n r> -rot
  >r apply-exact r> kl-apply ;

: full-apply ( clos args -- value )
  over kl-closure-partial swap list-append
  over kl-closure-arity over kl-list-len
  2dup > if 2drop clos-with-partial exit then
  2dup = if 2drop apply-exact exit then
  drop apply-over ;

: interp-enter ( clos args -- )
  over heap-root-push
  over kl-closure-params swap rot kl-closure-env bind-params
  heap-root-pop kl-closure-body swap
  ev-env ! ev-expr ! ;

: tail-apply ( clos args -- value 0 | -1 )
  over obj-type ty-closure <> if
    drop drop s" not a function" kl-error
  then
  over kl-closure-partial swap list-append
  over kl-closure-arity over kl-list-len
  2dup > if 2drop clos-with-partial 0 exit then
  2dup = if
    2drop
    over kl-closure-xt if
      args-to-vec call-native 0
    else
      interp-enter -1
    then
    exit
  then
  drop apply-over 0 ;

: lookup-call ( sym env -- obj )
  2dup env-get dup if nip nip ensure-clos exit then drop
  drop dup kl-sym-fun dup if nip ensure-clos exit then drop
  drop s" undefined function" kl-error ;

: (eval1) trap-expr @ trap-env @ kl-eval ;

: do-trap ( expr env handler -- value )
  heap-root-n @ >r
  call-vec @ >r
  trap-expr @ >r trap-env @ >r
  >r trap-env ! trap-expr !
  ['] (eval1) catch
  r> swap
  dup 0= if
    drop drop
    r> trap-env ! r> trap-expr ! r> call-vec ! r> heap-root-n !
    exit
  then
  dup kl-err-throw = if
    drop
    r> trap-env ! r> trap-expr ! r> call-vec ! r> heap-root-n !
    kl-err-obj @ kl-nil kl-cons
    kl-apply
  else
    r> trap-env ! r> trap-expr ! r> call-vec ! r> heap-root-n !
    swap drop throw
  then ;

: eval-app ( expr env -- value 0 | -1 )
  ea-env ! dup kl-cdr ea-args ! kl-car ea-head !
  ea-head @ obj-type ty-symbol <> if
    ea-head @ ea-env @ kl-eval ensure-clos
    heap-root-push
    ea-args @ ea-env @ eval-args
    heap-root-pop swap
    tail-apply exit
  then
  ea-head @ sy-if = if
    ea-args @ kl-car ea-env @ kl-eval kl-expect-bool
    if ea-args @ kl-cdr kl-car else ea-args @ kl-cdr kl-cdr kl-car then
    ev-expr ! -1 exit
  then
  ea-head @ sy-let = if
    ea-args @ heap-root-push
    ea-env @ heap-root-push
    ea-args @ kl-car heap-root-push
    ea-args @ kl-cdr kl-car ea-env @ kl-eval
    heap-root-pop swap heap-root-pop rot swap env-bind ev-env !
    heap-root-pop kl-cdr kl-cdr kl-car ev-expr !
    -1 exit
  then
  ea-head @ sy-lambda = if
    ea-args @ kl-car ea-args @ kl-cdr kl-car ea-env @ lambda-clos 0 exit
  then
  ea-head @ sy-slashdot = if
    ea-args @ ea-env @ slashdot-clos 0 exit
  then
  ea-head @ sy-defun = if
    ea-args @ kl-car
    ea-args @ kl-cdr kl-car
    ea-args @ kl-cdr kl-cdr kl-car
    do-defun 0 exit
  then
  ea-head @ sy-cond = if
    begin
      ea-args @ dup kl-nil = if s" cond: no clause matched" kl-error then
      kl-car dup heap-root-push
      kl-car ea-env @ kl-eval kl-expect-bool if
        heap-root-pop kl-cdr kl-car ev-expr ! -1 exit
      then
      heap-root-pop drop
      ea-args @ kl-cdr ea-args !
    again
  then
  ea-head @ sy-do = if
    begin
      ea-args @ dup kl-cdr kl-nil = if
        kl-car ev-expr ! -1 exit
      then
      kl-car ea-env @ kl-eval drop
      ea-args @ kl-cdr ea-args !
    again
  then
  ea-head @ sy-and = if
    ea-args @ kl-car ea-env @ kl-eval dup kl-expect-bool 0= if 0 exit then drop
    ea-args @ kl-cdr kl-car ev-expr ! -1 exit
  then
  ea-head @ sy-or = if
    ea-args @ kl-car ea-env @ kl-eval dup kl-expect-bool if 0 exit then drop
    ea-args @ kl-cdr kl-car ev-expr ! -1 exit
  then
  ea-head @ sy-freeze = if
    ea-args @ kl-car ea-env @ freeze-clos 0 exit
  then
  ea-head @ sy-thaw = if
    ea-args @ kl-car ea-env @ kl-eval kl-nil kl-apply 0 exit
  then
  ea-head @ sy-trap = if
    ea-args @ kl-cdr kl-car ea-env @ kl-eval >r
    ea-args @ kl-car ea-env @ r> do-trap 0 exit
  then
  ea-head @ sy-type = if
    ea-args @ kl-car ev-expr ! -1 exit
  then
  ea-head @ sy-quote = if
    ea-args @ kl-car 0 exit
  then
  ea-head @ ea-env @ lookup-call
  heap-root-push
  ea-args @ ea-env @ eval-args
  heap-root-pop swap
  tail-apply ;

:noname ( -- )
  kl-nil heap-mark
  kl-true heap-mark
  kl-false heap-mark
  sy-if heap-mark
  sy-let heap-mark
  sy-lambda heap-mark
  sy-slashdot heap-mark
  sy-defun heap-mark
  sy-cond heap-mark
  sy-do heap-mark
  sy-and heap-mark
  sy-or heap-mark
  sy-freeze heap-mark
  sy-thaw heap-mark
  sy-trap heap-mark
  sy-type heap-mark
  sy-quote heap-mark
  kl-fail heap-mark
  ev-expr @ heap-mark
  ev-env @ heap-mark
  call-vec @ heap-mark
  kl-err-obj @ heap-mark
  trap-expr @ heap-mark
  trap-env @ heap-mark
  ea-head @ heap-mark
  ea-args @ heap-mark
  ea-env @ heap-mark
  ev-args-env @ heap-mark
; is extra-mark

:noname ( expr env -- value )
  ea-head @ ea-args @ ea-env @ 2>r >r
  ev-expr @ ev-env @ 2>r
  ev-env ! ev-expr !
  begin
    ev-expr @ dup obj-type ty-cons <> if
      ev-env @ eval-atom
      2r> ev-env ! ev-expr !
      r> 2r> ea-env ! ea-args ! ea-head ! exit
    then drop
    ev-expr @ ev-env @ eval-app
    0= if
      2r> ev-env ! ev-expr !
      r> 2r> ea-env ! ea-args ! ea-head ! exit
    then
  again
; is kl-eval

:noname ( fn args -- value )
  swap ensure-clos swap full-apply
; is kl-apply

: kl-eval-top ( expr -- value )
  kl-nil kl-eval ;
