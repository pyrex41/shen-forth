\ Foundation runtime load. Interpreter bootstrap only; no KLambda load.
\ `included` is relative to this file's directory (src/).

warnings off
s" config.fs" included
s" heap.fs" included
s" object.fs" included
s" intern.fs" included
s" launcher.fs" included

heap-boot
