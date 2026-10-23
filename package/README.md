Potato Hammer sorts Valheim's build hammer by mod and fixes snap points other mods left out or named badly. It only changes your own game, so the server and other players don't need it.

**Every mod gets its own tab.** Mods put their build pieces wherever they like: spread over the game's categories, in none, in the wrong one, or under each other's tabs. Potato Hammer moves each mod's hammer pieces to a tab named after the mod and leaves the game's own pieces where they are. It works with any mod, Jötunn-based or not, and each piece shows on its mod's tab only.

**One hammer for everything.** Pieces from other mods' own hammers (like Odin's Hammer from Odin Architect) are added to the hammer too, on their mod's tab. Their own hammer still works. The hoe, the cultivator and the serving tray are left alone.

**Snap points where pieces need them.** The game snaps pieces point to point, and plenty of mods' pieces have only one or two points, or none. Potato Hammer works out extra points from a piece's shape: its corners, the middle of its top and bottom edges, and its top and bottom centres.

- Mods' pieces with only one or two points of their own get them automatically. Modular Ladders' ladders, for example, can then sit flush against a wall or end level with a floor so you can step off.
- Any other piece, the game's own included: take out the hammer, look at a built one and press Ctrl + K (or type `snappoints` in the console). Every piece of that kind gets points; press it again to take them away.
- Nothing is saved into your world. Removing the mod puts every piece back as it was.

**Snap points named by where they sit.** When you cycle through snap points while building, the game shows each one's name, and many mods leave those as "_snappoint (5)" or "Left 1". Potato Hammer names them by their place on the piece instead: "Top front left corner", "Bottom back edge", "Left centre".

It's part of the PotatoClient pack for the Potato Server.

## Settings

In `BepInEx/config/potato.hammer.cfg`, or in game with Configuration Manager. Tab changes take effect the next time you take out the hammer.

| Setting | Default | What it does |
|---|---|---|
| Sort the hammer | On | Off puts every piece back where its mod put it. |
| Include other hammers | On | Add the pieces from other mods' own hammers to the hammer, each on its mod's tab. |
| Mods to leave alone | Empty | Mods whose pieces stay where the mod put them (and whose own hammer stays separate), separated by commas. |
| Extra pieces | Empty | Pieces whose mod can't be told, put on a tab by name: `piece=Tab`, separated by commas. The log lists those pieces. |
| Tabs (one line per mod) | The mod's name | The tab name for that mod's pieces. Give two mods the same name to share a tab; leave it empty to leave that mod's pieces where they were. |
| Add snap points | On | Give pieces that lack them snap points from their shape. |
| Automatic for mod pieces | On | Mods' pieces with only a few points of their own get them without asking. |
| Automatic up to | 2 | How few points a mod's piece has before it gets them automatically (1-8). |
| Plain snap point names | On | Name the snap point you pick by where it sits on the piece. |
| Pick key | Ctrl + K | Look at a built piece with the hammer out and press this to give its kind snap points, or take them away. |

Bug reports go to [GitHub Issues](https://github.com/TheRealRel/PotatoHammer/issues), where the source lives too. Questions are welcome on Discord: [relanebeis](https://discord.com/users/373574370267889674).

Needs [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) and [Jötunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).

The code is under the MIT License: use it, change it and share it freely. The icon is © TheRealRel under [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/): free to share and adapt with credit, but not for commercial use. See LICENSE.
