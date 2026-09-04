{
  description = "shen-forth development environment";
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
  outputs = { nixpkgs, ... }:
    let
      systems = [ "aarch64-darwin" "aarch64-linux" "x86_64-linux" ];
      each = f: nixpkgs.lib.genAttrs systems (system: f nixpkgs.legacyPackages.${system});
      # nixpkgs gforth is marked broken on Darwin (gforthmi segfault in 0.7.9 snapshots).
      # GNU Gforth 0.7.3 is the last stable tarball and builds on aarch64-darwin.
      gforthPkg = pkgs: pkgs.stdenv.mkDerivation rec {
        pname = "gforth";
        version = "0.7.3";
        src = pkgs.fetchurl {
          url = "mirror://gnu/gforth/gforth-${version}.tar.gz";
          sha256 = "2f62f2233bf022c23d01c920b1556aa13eab168e3236b13352ac5e9f18542bb0";
        };
        nativeBuildInputs = [
          pkgs.updateAutotoolsGnuConfigScriptsHook
          pkgs.m4
        ];
        buildInputs = [ pkgs.libffi ];
        enableParallelBuilding = false;
        env.NIX_CFLAGS_COMPILE = "-std=gnu99 -fcommon -Wno-error=incompatible-pointer-types -Wno-incompatible-pointer-types -Wno-implicit-function-declaration -Wno-implicit-int";
        preConfigure = ''
          mkdir -p "$out/share/emacs/site-lisp"
        '';
        configureFlags = [
          "--with-lispdir=${placeholder "out"}/share/emacs/site-lisp"
        ];
        makeFlags = [ "EMACS=true" ];
        preInstall = ''
          touch gforth.elc
        '';
        doCheck = false;
        meta = {
          mainProgram = "gforth";
          description = "GNU Forth 0.7.3";
        };
      };
    in {
      packages = each (pkgs:
        let gforth = gforthPkg pkgs;
        in {
          inherit gforth;
          toolchain = pkgs.buildEnv {
            name = "shen-forth-toolchain";
            paths = [ gforth pkgs.gnumake ];
          };
          default = pkgs.buildEnv {
            name = "shen-forth-toolchain";
            paths = [ gforth pkgs.gnumake ];
          };
        });
      devShells = each (pkgs: {
        default = pkgs.mkShell {
          packages = [ (gforthPkg pkgs) pkgs.gnumake ];
        };
      });
    };
}
