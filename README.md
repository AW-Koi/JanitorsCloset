# Janitor's Closet

A RimWorld 1.6 mod that turns the colony janitor from a chore into a character. Themed gear, dedicated tools, and (eventually) autonomous behaviors that make a colonist with the Cleaning priority feel like a specialist who actually owns the role.

## Vision

Four pillars guide the full mod:

1. **Make a dedicated janitor pawn feel impactful and noticeable.** When a colonist is the janitor, you should be able to tell by looking at them and by how the colony plays.
2. **Janitor-specific gear** — galoshes, rubber gloves, coveralls, mop — that grants meaningful (but not overpowered) cleaning speed buffs.
3. **Janitor-specific autonomous behaviors** — detect large messes, place wet floor signs other pawns path around, etc. The janitor *does janitor things* on their own; the player isn't placing signs by hand.
4. **A "Janitor's Closet"** — a small dedicated room with the building, supplies, and storage they need, so the janitor has a *home base*.

The janitor's primary work is cleaning, with broken-component replacement as a secondary skill in later versions.

## v0.1 — Gear (this release)

This first slice ships **XML-only** to prove the modding pipeline end-to-end. No Harmony patches yet. Behaviors, signs, and the closet building come in v0.2+.

| Item | Slot | CleaningSpeed bonus | Crafted at |
|---|---|---|---|
| Galoshes | OnSkin / Feet | +10% | Tailoring bench |
| Rubber gloves | OnSkin / Hands | +10% | Tailoring bench |
| Janitor's coveralls | Middle / Torso | +10% | Tailoring bench |
| Mop (melee weapon, blunt) | Equipped weapon | +20% | Crafting spot or machining table |

Full kit: **+50% cleaning speed**. Modest by design — clearly faster than a generalist, doesn't trivialize cleaning.

No research is required; recipes are available from game start.

> **Note:** v0.1 ships placeholder programmer-art textures (solid-color squares with the item's initial). Real art lands with v0.2.

## Roadmap

- **v0.2 — Wet Floor Signs (the marquee feature).** Janitors autonomously detect filth being cleaned and place a sign that gives surrounding tiles high path cost so other pawns route around the wet area. Custom `WorkGiver` + `JobDriver` + `Building` subclass + a Harmony pathfinding patch.
- **v0.2/v0.3 — Janitor's Closet buildables.** Dedicated objects used to house the janitor's gear, replenish supplies, and provide a home base for the janitor.
- **v0.3 — Component-replacement work.** Janitors find broken-down buildings and repair them with Components.
- **Later — Research project, work-priority bias, real art pass.**

## Compatibility

- **RimWorld 1.6** only (current target).
- Recommended modlist companion: **Simple Sidearms** or any "multiple equipped weapons" mod, so a janitor can carry the mop alongside a real weapon. The mop is a usable melee weapon (blunt) but not a serious raid-defense tool.
- Harmony is a hard dependency (already declared in `About.xml`), but v0.1 doesn't actually patch anything yet.

## Project layout

```
JanitorsCloset/
├── About/                          mod metadata
├── 1.6/
│   ├── Defs/
│   │   ├── ThingDefs_Apparel/      janitor apparel set
│   │   └── ThingDefs_Misc/         mop weapon
│   ├── Patches/                    XML patches (empty for v0.1)
│   └── Assemblies/                 build output for the C# DLL
├── Languages/English/Keyed/        keyed strings (empty for v0.1)
├── Source/Janitor's Closet/        C# project (Harmony bootstrap only in v0.1)
├── Textures/Things/                placeholder textures
└── LoadFolders.xml
```

## Building

The C# project is at `Source/Janitor's Closet/JanitorsCloset.csproj`. It outputs the DLL into `1.6/Assemblies/`. References point to `Assembly-CSharp.dll` and Unity assemblies under your local RimWorld install — adjust the `HintPath` values in the csproj if your install isn't at the default Steam path.

```
dotnet build "Source/Janitor's Closet/JanitorsCloset.csproj"
```

## Credits

- Author: Terra Incognita
- License: TBD
