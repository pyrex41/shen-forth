# Kernel provenance

## ShenOSKernel 43.1

The vendored kernel sources are byte-identical files from the community
ShenOSKernel 43.1 release archive.

Canonical source: https://github.com/Shen-Language/shen-sources/releases/tag/shen-43.1
Archive: `ShenOSKernel-43.1.tar.gz`
Archive SHA-256: `900eedef44f440af78f9395ed06e689ce1604546fc309f5bc7c8bc84cc651920`
Kernel commit: `715824a52b55e9c81a9501df8548ff3edd8411f5`

The 43.1 core load includes `toplevel`, `core`, `sys`, `dict`, `sequent`,
`yacc`, `reader`, `prolog`, `track`, `load`, `writer`, `macros`,
`declarations`, `types`, `t-star`, and `init`. Extension and stlib files are
vendored for audit and remain outside the bounded Forth qualification scope.
There is no KLambda-to-Forth AOT.
