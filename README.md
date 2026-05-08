# Janitor's Closet

A RimWorld 1.6 mod that turns the colony janitor from a chore into a character. Themed gear, dedicated tools, and (eventually) autonomous behaviors that make a colonist with the Cleaning priority feel like a specialist who actually owns the role.

## Vision

Four pillars guide the full mod:

1. **Make a dedicated janitor pawn feel impactful and noticeable.** When a colonist is the janitor, you should be able to tell by looking at them and by how the colony plays.
2. **Janitor-specific gear** — galoshes, rubber gloves, coveralls, mop — that grants meaningful (but not overpowered) cleaning speed buffs.
3. **Janitor-specific autonomous behaviors** — detect large messes, place wet floor signs other pawns path around, etc. The janitor *does janitor things* on their own; the player isn't placing signs by hand.
4. **A "Janitor's Closet"** — a small dedicated room with the building, supplies, and storage they need, so the janitor has a *home base*.

The janitor's primary work is cleaning; broken-component replacement is a secondary domain.

## Gear

| Item | Slot | Crafted at |
|---|---|---|
| Galoshes | OnSkin / Feet | Tailoring bench |
| Rubber gloves | OnSkin / Hands | Tailoring bench |
| Janitor's coveralls | Middle / Torso | Tailoring bench |
| Mop (melee weapon, blunt) | Equipped weapon | Crafting spot or machining table |

Each piece grants a CleaningSpeed bonus; the full kit stacks to a modest improvement — clearly faster than a generalist, doesn't trivialize cleaning. Exact values live in the defs under `1.6/Defs/`.

## Compatibility

- **RimWorld 1.6** only (current target).
- Recommended modlist companion: **Simple Sidearms** or any "multiple equipped weapons" mod, so a janitor can carry the mop alongside a real weapon. The mop is a usable melee weapon (blunt) but not a serious raid-defense tool.
- Harmony is a hard dependency (declared in `About.xml`).

## Project layout

```
JanitorsCloset/
├── About/                          mod metadata
├── 1.6/
│   ├── Defs/
│   │   ├── ThingDefs_Apparel/      apparel defs
│   │   └── ThingDefs_Misc/         non-apparel thing defs
│   ├── Patches/                    XML patches
│   └── Assemblies/                 build output for the C# DLL
├── Languages/English/Keyed/        keyed strings
├── Source/Janitor's Closet/        C# project
├── Textures/Things/                in-game textures
└── LoadFolders.xml
```

## Building

The C# project is at `Source/Janitor's Closet/JanitorsCloset.csproj`. It outputs the DLL into `1.6/Assemblies/`. References point to `Assembly-CSharp.dll` and Unity assemblies under your local RimWorld install — adjust the `HintPath` values in the csproj if your install isn't at the default Steam path.

```
dotnet build "Source/Janitor's Closet/JanitorsCloset.csproj"
```

## Dev workflow — directory junction

Source lives outside the RimWorld install (here at `D:\Modding\RimWorld\JanitorsCloset\`). To make RimWorld load it, create a **directory junction** from the install's `Mods\` folder back to the source folder. Junctions are reparse points — RimWorld walks into `Mods\JanitorsCloset` and transparently reads from the source path. No copy, no sync, no upload step.

```
mklink /J "D:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\JanitorsCloset" "D:\Modding\RimWorld\JanitorsCloset"
```

Notes:
- Run from `cmd` (or `cmd /c "..."` from PowerShell). `mklink` is a `cmd.exe` builtin.
- `/J` (junction) doesn't need admin rights and works for same-volume directories. `/D` (true symbolic link) requires admin or Windows Developer Mode and is unnecessary here.
- Adjust both paths if your RimWorld install or source folder differs.
- Edits via either path edit the same physical files. Restart RimWorld to reload changes (XML def changes always need a full restart; C# changes always need a rebuild + restart).
- To remove: `rmdir "D:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\JanitorsCloset"` — this only deletes the junction, not the source folder.
- Workshop publishing is a separate step done via the in-game mod settings later. Junctioned mods do not need to be on Workshop to be loadable.

## Iteration loop

1. Edit XML defs or C# source under `D:\Modding\RimWorld\JanitorsCloset\`.
2. If C# changed: `dotnet build` (DLL drops into `1.6\Assemblies\` and is visible in `Mods\` via the junction).
3. Restart RimWorld.
4. Use dev mode (`Options → Development mode`) to spawn items, inspect stats, and force-equip apparel for fast verification.

## Credits

- Author: Terra Incognita
- License: TBD
