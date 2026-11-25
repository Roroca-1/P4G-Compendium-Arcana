#!/usr/bin/env fish
# Run from the source directory with the Reloaded-II directory as the argument.
if test (count $argv) -ne 1
    echo 'Usage: fish build.fish /path/to/Reloaded-II' >&2
    exit 1
end
cd (dirname (status filename)); or exit 1
set -l loader (realpath "$argv[1]")
set -l hooks "$loader/Mods/p4g64.expShare/Reloaded.Hooks.Definitions.dll"
set -l obj (mktemp); or exit 1
for variant in preview points display
    clang -c -D(string upper $variant) src/lookup.S -o "$obj"; or exit 1
    objcopy -O binary -j .text "$obj" "src/$variant.bin"; or exit 1
end
rm "$obj"
dotnet build src -c Release "-p:ReloadedDir=$loader" "-p:HooksDll=$hooks" -p:NuGetAudit=false; or exit 1
set -l package p4g64.compendiumArcana
mkdir -p "$package"; or exit 1
cp ModConfig.json src/bin/Release/net9.0/CompendiumArcana.{dll,deps.json} "$package/"; or exit 1
# Use the portable definitions assembly when packaging for the managed loader.
# The controller itself is provided by reloaded.sharedlib.hooks.
cp "$hooks" "$package/"; or exit 1
zip -q -j CompendiumArcana-0.1.1.zip "$package"/*
