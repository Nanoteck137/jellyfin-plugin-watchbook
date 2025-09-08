{
  description = "";

  inputs = {
    nixpkgs.url      = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url  = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        overlays = [];
        pkgs = import nixpkgs {
          inherit system overlays;
        };

        app = pkgs.python3Packages.buildPythonPackage rec {
          pname = "jprm";
          version = "1.1.1";
          format = "setuptools";

          src = pkgs.fetchFromGitHub {
              owner = "oddstr13";
              repo = "jellyfin-plugin-repository-manager";
              tag = "v${version}";
              hash = "sha256-PWgZ9K81RX+AboU8/6IGEQ8Fv/e8d2I1KH3+jIQOyj4=";
          };

          dependencies = with pkgs.python3Packages; [
            pyyaml
            click
            click-log
            python-slugify
            tabulate
          ];

          # postInstall = ''
          #   mv "$out/bin/tagopus.py" "$out/bin/tagopus"
          # '';
        };
      in
      {
        packages.default = app;

        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            (pkgs.python3.withPackages (python-pkgs: [
                python-pkgs.mutagen
            ]))
          ];
        };
      }
    );
}
