# 🪵 Potato Hammer

**Sorts the build hammer by mod.** Mods put their build pieces wherever they like: spread over several of the game's categories, in none, in the wrong one, or under each other's tabs. Potato Hammer moves every mod's hammer pieces to **a tab named after the mod**, and leaves the game's own pieces in the game's tabs.

- Works with any mod, Jotunn-based or not: Jotunn keeps track of which mod added each piece.
- **One hammer for everything:** pieces from other mods' own hammers (like Odin's Hammer from Odin Architect) are added to the hammer too, on their mod's tab. Their own hammer still works. The hoe, the cultivator and the serving tray are left alone.
- Each piece shows on its mod's tab only, not also under the game's categories (Walls, Building...) or under other mods' tags.
- Client-side: it only changes your own build menu. The server and other players don't need it.

Part of the **PotatoClient** pack for the Potato Server, alongside [Potato Tweaks](https://github.com/TheRealRel/PotatoTweaks). Bug reports: this repository's Issues.

## Settings

In `BepInEx/config/potato.hammer.cfg`, or in game with Configuration Manager. Changes take effect the next time you take out the hammer.

| Section | Setting | What it does |
|---|---|---|
| **1 - General** | Sort the hammer (on) | Switch the sorting off to put every piece back where its mod put it. |
| | Include other hammers (on) | Add the pieces from other mods' own hammers to the hammer, each on its mod's tab. |
| | Mods to leave alone | Mods whose pieces stay where the mod put them (and whose own hammer stays separate), separated by commas. |
| | Extra pieces | Pieces whose mod can't be told, put on a tab by name: `piece=Tab`, separated by commas. The log lists those pieces. |
| **2 - Tabs** | One line per mod | The tab name for that mod's pieces (the mod's name with spaces by default, like "Boat Additions"). Give two mods the same name to share a tab; leave it empty to leave that mod's pieces where they were. The lines appear after the first time you take out the hammer in a world. |

## Requirements

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
- [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/)

## Building

- Source: `src/`. Thunderstore files: `package/`.
- References come from Valheim's `valheim_Data/Managed` folder and a BepInEx profile (BepInEx and Harmony from `BepInEx/core`, Jotunn from `BepInEx/plugins/ValheimModding-Jotunn`).
- `build.sh` compiles with the .NET SDK's Roslyn compiler into `out/PotatoHammer.dll`. Point it at your files with `VALHEIM_MANAGED`, `BEPINEX` and, optionally, `CSC`:

```
VALHEIM_MANAGED=".../Valheim/valheim_Data/Managed" BEPINEX=".../profiles/MyProfile/BepInEx" ./build.sh
```

- To package, zip `package/manifest.json`, `README.md`, `CHANGELOG.md`, `LICENSE` and `icon.png` at the root, with the DLL at `plugins/PotatoHammer.dll`.
