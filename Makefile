# shen-forth — Gforth-first foundation. Does not boot the S42 kernel.
#
#   make          same as make test
#   make test     port harness (objects, intern, mark/sweep, launcher stub)
#   make version  bin/shen --version

GFORTH ?= gforth
ROOT   := $(abspath .)
export SHEN_FORTH_ROOT := $(ROOT)

.PHONY: all test test-forth test-launcher test-klambda version help clean

all: test

help:
	@echo "make test      - run the foundation harness (nix develop -c make)"
	@echo "make version   - print the launcher stub version"
	@echo "make clean     - remove editor junk"

test: test-klambda test-forth test-launcher

test-klambda:
	@test -f $(ROOT)/kernel/klambda/core.kl
	@test -f $(ROOT)/kernel/klambda/PROVENANCE.md
	@test ! -e $(ROOT)/kernel/klambda/extension-features.kl
	@n=$$(find $(ROOT)/kernel/klambda -maxdepth 1 -name '*.kl' | wc -l | tr -d ' '); \
	  test "$$n" = "15"

test-forth:
	cd $(ROOT) && $(GFORTH) src/shen.fs tests/run.fs

test-launcher:
	cd $(ROOT) && ./bin/shen --version | grep -q shen-forth
	cd $(ROOT) && ./bin/shen --help >/dev/null
	cd $(ROOT) && ./bin/shen repl | grep -q foundation
	@if cd $(ROOT) && ./bin/shen eval -e "(+ 1 2)"; then \
	  echo "eval stub must not boot the kernel" >&2; exit 1; \
	fi
	@if cd $(ROOT) && ./bin/shen script /dev/null; then \
	  echo "script stub must not boot the kernel" >&2; exit 1; \
	fi

version:
	cd $(ROOT) && ./bin/shen --version

clean:
	@find $(ROOT) -name '*~' -delete
