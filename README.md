# Compendium Arcana

Get the matching Arcana bonus in Persona 4 Golden Social Links when a matching Persona is registered in your Compendium. You can leave it there without paying to summon it or freeing a stock slot.

A carried Persona takes priority. If you have no match in stock, Social Link prompts that normally name a Persona can use the first eligible Compendium entry in internal ID order. Having both grants the usual bonus once.

## Install

1. Download the ZIP from [Releases](https://github.com/Roroca-1/P4G-Compendium-Arcana/releases/latest).
2. Extract its contents into a new directory named `p4g64.compendiumArcana` inside Reloaded-II's `Mods` directory.
3. Enable **Compendium Arcana** for P4G in Reloaded-II. It requires `reloaded.sharedlib.hooks`.

The mod checks the Steam 64-bit executable before loading. The supported P4G.exe SHA-256 is `0f327c7669e6cac72aaf355181b1e51edd12fee4d71709fc1802dbf0da70546e`.

## Build on Linux

Run `fish build.fish /path/to/Reloaded-II` with a .NET SDK that supports net9.0, clang, objcopy and zip. The build script uses the portable Reloaded.Hooks.Definitions.dll bundled with Exp Share as its compile-time reference.
