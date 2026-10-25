# Janitor's Closet

> Anyone can scrub a floor. A janitor cleans.

Janitor's Closet is a RimWorld mod that turns cleaning into a profession. Give a colonist the right tools and they become the colony's janitor: they work faster on the mess their tools suit, keep rooms spotless, and leave a mark on how the colony feels.

## What it adds

- **Cleaning tools**, each suited to a kind of mess: the straw broom and push broom for dry filth, the mop for wet filth, the glittervacuum for everything at once, and, with Biotech, the hazmat sprayer for pollution.
- **Janitor apparel**: coveralls, galoshes and rubber gloves.
- **Research** that unlocks the tools as the colony advances.
- **Specialist behaviour**: a janitor heads for the filth their tool suits, and you can limit each tool's holder to indoors, outdoors or an area.
- **Spotless rooms**: when a janitor clears a room's last filth, they take pride in it and everyone inside enjoys the fresh room until someone tracks mess back in.

## Requirements

- RimWorld 1.7.
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077).
- Biotech is optional and adds the hazmat sprayer.

## Building

You need the .NET 10 SDK and RimWorld 1.7 installed. The build finds RimWorld through Steam; for other installs, set `RIMWORLD_DIR` to the folder that holds `Assembly-CSharp.dll`.

```
dotnet build Source/JanitorsCloset
```

The mod's DLL lands in `mod/1.7/Assemblies/`.

## Running it in RimWorld

Only `mod/` is the mod; the rest of the repo is source. Link `mod/` into RimWorld's `Mods` folder once, then rebuild and restart the game to pick up changes:

```
mklink /J "<RimWorld install>\Mods\JanitorsCloset" "<this repo>\mod"
```

Run `mklink` from `cmd`. Removing the link with `rmdir` leaves the repo untouched.

## Testing

Unit tests cover the logic that runs without the game:

```
dotnet test Source/JanitorsCloset.Tests
```

Gameplay is tested end to end with [Archotech Harness](https://github.com/AW-Koi/archotech-harness). The scenarios in `harness/` build a room, give a colonist a tool and check what happens in game. With the harness cloned (set `ARCHO_HOME` if it isn't next to this repo), run them all with:

```
<archotech harness>/archo test harness --workers auto
```

## Repository layout

| Path | Contents |
| --- | --- |
| `mod/` | The mod as RimWorld loads it, and the only folder to publish. |
| `mod/Common/` | Defs and patches. |
| `mod/1.7/` | The built DLL. |
| `Source/JanitorsCloset/` | The mod's C# source, plus art and sound sources. |
| `Source/JanitorsCloset.Tests/` | Unit tests. |
| `harness/` | In-game scenarios for Archotech Harness. |
| `docs/` | Reviews and plans. |

## Where it's heading

- A janitor should be noticeable: you can tell who it is by looking, and by how the colony plays.
- Gear should give meaningful, specialised speed bonuses.
- Janitors should act on their own: spot big messes, put out wet-floor signs.
- A janitor's closet should give them a home base to resupply from.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Janitor's Closet is released under the [MIT licence](LICENSE).
