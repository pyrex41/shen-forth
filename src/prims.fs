\ Required KLambda primitives. Native xts read arguments from call-vec.

create io-buf 8 allot
4096 constant /path-buf
create path-buf /path-buf allot
create cn-pad 65536 allot
2variable uptime0

0 value st-in
0 value st-out
0 value st-err

: defprim ( xt arity c-addr u -- )
  kl-intern dup heap-root-push
  -rot kl-nil kl-closure
  swap kl-sym-fun!
  heap-root-pop drop ;

: num@ ( i -- ) ( F: -- r )
  arg@ dup obj-type ty-number = if kl>f else drop s" not a number" kl-error then ;

: expect-str ( obj -- c-addr u )
  dup obj-type ty-string = if kl-string-bytes else drop s" not a string" kl-error then ;

: prim-+ 0 num@ 1 num@ f+ kl-fnum ;
: prim-- 0 num@ 1 num@ f- kl-fnum ;
: prim-* 0 num@ 1 num@ f* kl-fnum ;
: prim-/
  1 num@ fdup f0= if fdrop s" division by zero" kl-error then
  0 num@ fswap f/ kl-fnum ;
: prim-> 0 num@ 1 num@ f> kl-bool ;
: prim-< 0 num@ 1 num@ f< kl-bool ;
: prim->= 0 num@ 1 num@ f>= kl-bool ;
: prim-<= 0 num@ 1 num@ f<= kl-bool ;
: prim-= 0 arg@ 1 arg@ kl-equal kl-bool ;

: prim-cons 0 arg@ 1 arg@ kl-cons ;
: prim-hd
  0 arg@ dup obj-type ty-cons = if kl-car else drop s" hd: not a cons" kl-error then ;
: prim-tl
  0 arg@ dup obj-type ty-cons = if kl-cdr else drop s" tl: not a cons" kl-error then ;
: prim-cons? 0 arg@ obj-type ty-cons = kl-bool ;

: prim-number? 0 arg@ obj-type ty-number = kl-bool ;
: prim-string? 0 arg@ obj-type ty-string = kl-bool ;
: prim-symbol? 0 arg@ obj-type ty-symbol = kl-bool ;
: prim-absvector? 0 arg@ obj-type ty-vector = kl-bool ;
: prim-empty? 0 arg@ kl-nil = kl-bool ;

: prim-intern 0 arg@ expect-str kl-intern ;

create num-pad 128 allot
: copy-to-num-pad ( c-addr u -- c-addr u )
  dup 127 > if drop 127 then
  tuck num-pad swap move num-pad swap ;

: prim-str
  0 arg@ dup obj-type case
    ty-nil of drop s" str: cannot convert ()" kl-error endof
    ty-symbol of
      kl-symbol-name dup obj-type ty-string <> if drop s" str: bad symbol" kl-error then
      kl-string-bytes copy-to-num-pad kl-string
    endof
    ty-string of
      kl-string-bytes dup 2 + >r
      [char] " cn-pad c!
      cn-pad 1+ swap 2dup + >r cmove
      [char] " r> c!
      cn-pad r> kl-string
    endof
    ty-number of
      kl>f fdup fdup floor f= if
        fdup f0< >r fabs f>d drop 0 <# #s r> sign #> copy-to-num-pad kl-string
      else
        fdrop 0 arg@ kl>f 16 6 0 f>str-rdp copy-to-num-pad kl-string
      then
    endof
    ty-closure of drop s" #<closure x>" kl-string endof
    ty-vector of drop s" #<absvector x>" kl-string endof
    ty-stream of drop s" #<stream x>" kl-string endof
    ty-exception of drop s" #<error x>" kl-string endof
    drop s" str: cannot convert" kl-error
  endcase ;

variable cn-n
: prim-cn
  0 arg@ expect-str
  dup cn-n !
  cn-pad swap move
  1 arg@ expect-str
  dup >r
  cn-pad cn-n @ + swap move
  cn-pad cn-n @ r> + kl-string ;

variable pos-addr
variable pos-u
: prim-pos
  0 arg@ expect-str pos-u ! pos-addr !
  1 arg@ dup obj-type ty-number <> if drop s" pos: bad index" kl-error then
  kl>f f>s
  dup 0< if drop s" pos: index out of range" kl-error then
  dup pos-u @ >= if drop s" pos: index out of range" kl-error then
  pos-addr @ + c@ io-buf c!
  io-buf 1 kl-string ;

: prim-tlstr
  0 arg@ expect-str
  dup 0= if s" tlstr: empty string" kl-error then
  1 /string kl-string ;

: prim-string-n
  0 arg@ expect-str
  dup 0= if s" string->n: empty string" kl-error then
  drop c@ kl-int ;

: prim-n-string
  0 arg@ dup obj-type ty-number = 0= if drop s" n->string: not a number" kl-error then
  kl>f f>d drop
  io-buf c! io-buf 1 kl-string ;

: prim-set
  0 arg@ dup obj-type ty-symbol = 0= if drop s" set: not a symbol" kl-error then
  1 arg@ swap 2dup kl-sym-value! drop ;

: prim-value
  0 arg@ dup obj-type ty-symbol = 0= if drop s" value: not a symbol" kl-error then
  dup kl-sym-bound? 0= if
    dup kl-symbol-name dup obj-type ty-string = if kl-string-bytes type cr else drop then
    drop s" variable has no value" kl-error
  then
  kl-sym-value ;

: prim-absvector
  0 arg@ dup obj-type ty-number = 0= if drop s" absvector: not a number" kl-error then
  kl>f f>d drop
  dup 0< if s" absvector: negative" kl-error then
  dup 1 20 lshift > if s" absvector: too large" kl-error then
  kl-vector
  dup kl-vlen 0 ?do kl-fail over i kl-vset loop ;

: prim-addr-get
  0 arg@ dup obj-type ty-vector = 0= if drop s" <-address: not a vector" kl-error then
  1 arg@ dup obj-type ty-number = 0= if drop drop s" <-address: bad index" kl-error then
  kl>f f>d drop
  dup 0< if s" <-address: out of range" kl-error then
  over kl-vlen over <= if s" <-address: out of range" kl-error then
  kl-vref ;

: prim-addr-set
  0 arg@ dup obj-type ty-vector = 0= if drop s" address->: not a vector" kl-error then
  1 arg@ dup obj-type ty-number = 0= if drop drop s" address->: bad index" kl-error then
  kl>f f>d drop
  dup 0< if s" address->: out of range" kl-error then
  over kl-vlen over <= if s" address->: out of range" kl-error then
  2 arg@ -rot kl-vset
  0 arg@ ;

: prim-simple-error 0 arg@ expect-str kl-error ;

: prim-error-to-string
  0 arg@ dup obj-type ty-exception = if
    kl-exception-msg
  else drop s" error-to-string: not an error" kl-error then ;

: prim-eval-kl 0 arg@ kl-nil kl-eval ;

: prim-type 0 arg@ ;

: prim-get-time
  0 arg@ dup obj-type ty-symbol = 0= if drop s" get-time: not a symbol" kl-error then
  kl-symbol-name kl-string-bytes
  2dup s" unix" compare 0= if 2drop utime d>f 1e6 f/ kl-fnum exit then
  2dup s" run" compare 0= if 2drop utime uptime0 2@ d- d>f 1e6 f/ kl-fnum exit then
  2drop s" get-time does not understand the parameter" kl-error ;

: home-prefix ( -- c-addr u )
  s" *home-directory*" kl-intern
  dup kl-sym-bound? 0= if drop s" " exit then
  kl-sym-value
  dup obj-type ty-string = if kl-string-bytes else drop s" " then ;

\ Kernel `open` concatenates *home-directory* and the path literally.
: resolve-path ( c-addr u -- c-addr u )
  2>r home-prefix
  dup 0= if 2drop 2r> exit then
  dup r@ + /path-buf 1- > if 2drop 2r> 2drop s" open: path too long" kl-error then
  tuck path-buf swap move
  path-buf over + 2r> >r swap r@ move r> +
  path-buf swap ;

: prim-open
  0 arg@ expect-str resolve-path
  1 arg@ dup obj-type ty-symbol = 0= if drop s" open: bad direction" kl-error then
  kl-symbol-name kl-string-bytes
  2dup s" in" compare 0= if
    2drop r/o bin open-file if drop s" open: cannot open" kl-error then
    stream-in swap kl-stream
  else
    2dup s" out" compare 0= if
      2drop w/o bin create-file if drop s" open: cannot open" kl-error then
      stream-out swap kl-stream
    else 2drop s" open: invalid direction" kl-error then
  then ;

: prim-close
  0 arg@ dup obj-type ty-stream = 0= if drop s" close: not a stream" kl-error then
  dup kl-stream-kind stream-std = if drop kl-nil exit then
  dup kl-stream-kind stream-out = if dup kl-stream-handle flush-file drop then
  kl-stream-handle close-file drop kl-nil ;

: prim-read-byte
  0 arg@ dup obj-type ty-stream = 0= if drop s" read-byte: not a stream" kl-error then
  dup kl-stream-handle
  io-buf 1 rot read-file if drop drop s" read-byte failed" kl-error then
  0= if
    kl-stream-kind stream-std = if 0 (bye) then
    -1 kl-int
  else
    nip io-buf c@ kl-int
  then ;

: prim-y-or-n? kl-true ;

: prim-write-byte
  0 arg@ dup obj-type ty-number = 0= if drop s" write-byte: not a number" kl-error then
  kl>f f>d drop io-buf c!
  1 arg@ dup obj-type ty-stream = 0= if drop s" write-byte: not a stream" kl-error then
  kl-stream-handle
  io-buf 1 rot write-file if s" write-byte failed" kl-error then
  0 arg@ ;

: prim-write-string
  0 arg@ expect-str
  1 arg@ dup obj-type ty-stream = 0= if drop s" write-string: not a stream" kl-error then
  kl-stream-handle write-file if s" write-string failed" kl-error then
  0 arg@ ;

\ S43.1 public stream protocol. Gforth uses byte streams for the bounded
\ port, so char-stream? is false and read-unit-string is retained as the
\ required one-unit entry point for ports that select the character path.
: prim-char-stream? drop kl-false ;

: prim-read-unit-string
  0 arg@ dup obj-type ty-stream = 0= if drop s" read-unit-string: not a stream" kl-error then
  dup kl-stream-kind stream-std = 0= if drop s" read-unit-string: character streams unsupported" kl-error then
  \ The bounded Forth port advertises byte streams only. Keep this guard
  \ before the host read so an unsupported call is catchable without consuming
  \ the next byte (including malformed UTF-8 or EOF sentinels).
  drop s" read-unit-string: character streams unsupported" kl-error ;
\ unreachable byte implementation retained for the future char-stream port
\  dup kl-stream-handle
\  io-buf 1 rot read-file if drop drop s" read-unit-string failed" kl-error then
\  0= if 2drop s" " kl-string exit then
\  2drop io-buf c@ kl-int kl>f f>d drop io-buf c! io-buf 1 kl-string ;

: prim-char-out
  0 arg@ dup st-out = swap st-err = or kl-bool ;

: prim-char-in kl-false ;

: prim-thaw 0 arg@ kl-nil kl-apply ;

: prim-fn
  0 arg@ dup obj-type ty-symbol = 0= if drop s" fn: not a symbol" kl-error then
  dup kl-sym-fun dup if nip else drop s" fn: undefined" kl-error then ;

: prim-apply 0 arg@ 1 arg@ kl-apply ;

: prim-exit
  0 arg@ dup obj-type ty-number = if kl>f f>d drop else drop 0 then
  (bye) ;

: prim-integer?
  0 arg@ dup obj-type ty-number = if
    kl>f fdup floor f= kl-bool
  else drop kl-false then ;

: prim-element?
  0 arg@ 1 arg@
  begin dup kl-nil <> while
    dup obj-type ty-cons <> if drop drop s" element?: not a list" kl-error then
    2dup kl-car kl-equal if 2drop kl-true exit then
    kl-cdr
  repeat 2drop kl-false ;

: prim-length
  0 arg@ 0 swap
  begin dup kl-nil <> while
    dup obj-type ty-cons <> if drop drop s" length: not a list" kl-error then
    swap 1+ swap kl-cdr
  repeat drop kl-int ;

: obj-hashval ( obj -- u )
  dup obj-type ty-symbol = if kl-symbol-name then
  dup obj-type ty-string = if kl-string-bytes hash-str exit then
  drop abs ;

: prim-hash
  1 arg@ dup obj-type ty-number = 0= if drop s" hash: not a number" kl-error then
  kl>f f>d drop
  dup 1 < if drop 1 then
  0 arg@ obj-hashval
  over mod
  dup 0= if drop 1 then
  nip kl-int ;

: prim-not 0 arg@ kl-expect-bool 0= kl-bool ;

: set-global-str ( val-addr val-u name-addr name-u -- )
  kl-intern >r kl-string r@ kl-sym-value! r> drop ;

: init-streams ( -- )
  stream-std stdin kl-stream to st-in
  stream-std stdout kl-stream to st-out
  stream-std stderr kl-stream to st-err
  st-in s" *stinput*" kl-intern kl-sym-value!
  st-out s" *stoutput*" kl-intern kl-sym-value!
  st-err s" *sterror*" kl-intern kl-sym-value! ;

: init-home ( -- )
  \ Empty unless kernel `cd`. SHEN_FORTH_ROOT is boot.fs module-path only.
  s" " kl-string
  s" *home-directory*" kl-intern kl-sym-value! ;

: register-prims ( -- )
  ['] prim-+ 2 s" +" defprim
  ['] prim-- 2 s" -" defprim
  ['] prim-* 2 s" *" defprim
  ['] prim-/ 2 s" /" defprim
  ['] prim-> 2 s" >" defprim
  ['] prim-< 2 s" <" defprim
  ['] prim->= 2 s" >=" defprim
  ['] prim-<= 2 s" <=" defprim
  ['] prim-= 2 s" =" defprim
  ['] prim-cons 2 s" cons" defprim
  ['] prim-hd 1 s" hd" defprim
  ['] prim-tl 1 s" tl" defprim
  ['] prim-cons? 1 s" cons?" defprim
  ['] prim-number? 1 s" number?" defprim
  ['] prim-string? 1 s" string?" defprim
  ['] prim-symbol? 1 s" symbol?" defprim
  ['] prim-absvector? 1 s" absvector?" defprim
  ['] prim-empty? 1 s" empty?" defprim
  ['] prim-intern 1 s" intern" defprim
  ['] prim-str 1 s" str" defprim
  ['] prim-cn 2 s" cn" defprim
  ['] prim-pos 2 s" pos" defprim
  ['] prim-tlstr 1 s" tlstr" defprim
  ['] prim-string-n 1 s" string->n" defprim
  ['] prim-n-string 1 s" n->string" defprim
  ['] prim-set 2 s" set" defprim
  ['] prim-value 1 s" value" defprim
  ['] prim-absvector 1 s" absvector" defprim
  ['] prim-addr-get 2 s" <-address" defprim
  ['] prim-addr-set 3 s" address->" defprim
  ['] prim-simple-error 1 s" simple-error" defprim
  ['] prim-error-to-string 1 s" error-to-string" defprim
  ['] prim-eval-kl 1 s" eval-kl" defprim
  ['] prim-type 2 s" type" defprim
  ['] prim-get-time 1 s" get-time" defprim
  ['] prim-open 2 s" open" defprim
  ['] prim-close 1 s" close" defprim
  ['] prim-read-byte 1 s" read-byte" defprim
  ['] prim-write-byte 2 s" write-byte" defprim
  ['] prim-write-string 2 s" shen.write-string" defprim
  ['] prim-write-string 2 s" write-string" defprim
  ['] prim-read-unit-string 1 s" read-unit-string" defprim
  ['] prim-char-stream? 1 s" char-stream?" defprim
  ['] prim-char-out 1 s" shen.char-stoutput?" defprim
  ['] prim-char-in 1 s" shen.char-stinput?" defprim
  ['] prim-thaw 1 s" thaw" defprim
  ['] prim-fn 1 s" fn" defprim
  ['] prim-apply 2 s" apply" defprim
  ['] prim-exit 1 s" exit" defprim
  ['] prim-integer? 1 s" integer?" defprim
  ['] prim-not 1 s" not" defprim
  ['] prim-element? 2 s" element?" defprim
  ['] prim-length 1 s" length" defprim
  ['] prim-hash 2 s" hash" defprim
  ['] prim-y-or-n? 1 s" y-or-n?" defprim ;

: override-hot ( -- )
  ['] prim-integer? 1 s" integer?" defprim
  ['] prim-element? 2 s" element?" defprim
  ['] prim-length 1 s" length" defprim
  ['] prim-empty? 1 s" empty?" defprim
  ['] prim-hash 2 s" hash" defprim
  ['] prim-y-or-n? 1 s" y-or-n?" defprim ;

: pin-globals ( -- )
  kl-nil heap-root-push
  kl-true heap-root-push
  kl-false heap-root-push
  st-in heap-root-push
  st-out heap-root-push
  st-err heap-root-push ;

: init-diag-flags ( -- )
  0 shen-quiet !
  0 shen-verbose !
  s" SHEN_FORTH_QUIET" getenv nip if -1 shen-quiet ! then
  s" SHEN_FORTH_VERBOSE" getenv nip if -1 shen-verbose ! then ;

: prims-init ( -- )
  intern-specials
  register-prims
  utime uptime0 2!
  init-streams
  pin-globals
  init-home
  init-diag-flags
  s" Forth" s" *language*" set-global-str
  s" Gforth" s" *implementation*" set-global-str
  s" 0.1.0" s" *release*" set-global-str
  s" shen-forth" s" *port*" set-global-str
  s" Reuben Brooks" s" *porters*" set-global-str
  s" Unix" s" *os*" set-global-str
  s" 43.1" s" *version*" set-global-str
  s" 200" s" *maximum-print-sequence-size*" set-global-str
  kl-nil s" *macros*" kl-intern kl-sym-value!
  kl-false s" *hush*" kl-intern kl-sym-value! ;
