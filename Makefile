# shen-forth — Gforth-first S42 interpreter bootstrap. No KLambda-to-Forth AOT.
#
#   make          same as make test
#   make test     port harness + launcher eval (boots kernel)
#   make version  bin/shen --version

GFORTH ?= gforth
# Nested cons in declarations.kl needs a deep Forth return stack (same as bin/shen).
GFORTHFLAGS ?= -r 8M -d 4M -l 1M -m 256M
ROOT   := $(abspath .)
export SHEN_FORTH_ROOT := $(ROOT)

.PHONY: all test test-forth test-launcher test-klambda corpus certify version help clean

all: test

help:
	@echo "make test      - run the harness and launcher (nix develop -c make)"
	@echo "make corpus    - run kernel/tests/runme.shen (record the exact report)"
	@echo "make certify   - corpus and require passed 134 / failed 0"
	@echo "make version   - print the launcher version"
	@echo "make clean     - remove editor junk"

test: test-klambda test-forth test-launcher

test-klambda:
	@test -f $(ROOT)/kernel/klambda/core.kl
	@test -f $(ROOT)/kernel/klambda/PROVENANCE.md
	@test ! -e $(ROOT)/kernel/klambda/extension-features.kl
	@n=$$(find $(ROOT)/kernel/klambda -maxdepth 1 -name '*.kl' | wc -l | tr -d ' '); \
	  test "$$n" = "15"
	@test -f $(ROOT)/kernel/tests/runme.shen
	@test -f $(ROOT)/kernel/tests/harness.shen
	@test -f $(ROOT)/kernel/tests/kerneltests.shen

test-forth:
	cd $(ROOT) && $(GFORTH) $(GFORTHFLAGS) src/shen.fs tests/run.fs

test-launcher:
	cd $(ROOT) && ./bin/shen --version | grep -q shen-forth
	cd $(ROOT) && ./bin/shen --help >/dev/null
	cd $(ROOT) && ./bin/shen eval -e "(+ 1 2)" | grep -q 3
	cd $(ROOT) && ./bin/shen eval -e "(version)" | grep -qx 42
	cd /tmp && $(ROOT)/bin/shen eval -e "(+ 1 2)" | grep -qx 3
	cd $(ROOT) && ./bin/shen eval -e '(@s "foo" "bar")' | grep -Fqx foobar
	cd $(ROOT) && ./bin/shen eval -e '(tlstr "hello")' | grep -Fqx ello
	cd $(ROOT) && ./bin/shen eval -e '(n->string 65)' | grep -Fqx A
	cd $(ROOT) && ./bin/shen eval -e '(trap-error (simple-error "boom") (/. E (error-to-string E)))' | grep -Fqx boom
	cd $(ROOT) && ./bin/shen eval -e "((/. X (+ X 1)) 4)" | grep -qx 5
	cd $(ROOT) && ./bin/shen eval -e '(trap-error (simple-error "boom") (/. E 99))' | grep -qx 99
	cd $(ROOT) && ./bin/shen eval -e "(cons 1 ())" | grep -Fqx '[1]'
	cd $(ROOT) && ./bin/shen eval -e "()" | grep -Fqx '[]'
	cd $(ROOT) && ./bin/shen eval -e "(map (+ 1) [1 2 3])" | grep -Fqx '[2 3 4]'
	cd $(ROOT) && ./bin/shen eval -e "(thaw (freeze 8))" | grep -qx 8
	cd $(ROOT) && ./bin/shen eval -e "(defun fact (N) (if (= N 0) 1 (* N (fact (- N 1)))))" | grep -q fact
	cd $(ROOT) && ./bin/shen eval -e "(declare foo [number --> number])" | grep -qx foo
	cd $(ROOT) && ./bin/shen eval -e '(do (cd "kernel/tests") (do (load "cartprod.shen") (cartesian-product [1 2] [1 2])))' | grep -F -q '[1 1]'
	cd $(ROOT)/kernel/tests && $(ROOT)/bin/shen eval -e '(load "cartprod.shen")' | grep -q loaded
	@d=$$(mktemp -d) && \
	  printf '%s\n' "(let S (open \"$$d/out\" out) (do (pr \"hello\" S) (close S)))" > "$$d/w.shen" && \
	  $(ROOT)/bin/shen eval -q -l "$$d/w.shen" && \
	  test -s "$$d/out" && \
	  rm -rf "$$d"

# Canonical S42 suite (harness.shen + kerneltests.shen). Do not skip groups.
# Not part of `make test` (~12 min on this interpreter).
corpus:
	cd $(ROOT) && ./bin/shen eval -e '(do (cd "kernel/tests") (load "runme.shen"))'

# Tarver kernel/tests/runme.shen. Fail unless the live report is passed 134 / failed 0.
certify:
	cd $(ROOT) && ./bin/shen eval -e '(do (cd "kernel/tests") (load "runme.shen"))' | tee $(ROOT)/evidence/certify.log
	@tail -n 30 $(ROOT)/evidence/certify.log | grep -F 'passed ... 134'
	@tail -n 30 $(ROOT)/evidence/certify.log | grep -F 'failed ... 0'

version:
	cd $(ROOT) && ./bin/shen --version

clean:
	@find $(ROOT) -name '*~' -delete
