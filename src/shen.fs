\ Runtime load. Interpreter bootstrap of vendored S42 KLambda; no AOT.
\ `included` is relative to this file's directory (src/).

warnings off
s" config.fs" included
s" heap.fs" included
s" object.fs" included
s" intern.fs" included
s" reader.fs" included
s" eval.fs" included
s" prims.fs" included
s" boot.fs" included
s" print.fs" included
s" launcher.fs" included

heap-boot
prims-init
