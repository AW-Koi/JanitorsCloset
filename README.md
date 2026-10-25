# Janitor's Closet

> Anyone can scrub a floor. A janitor cleans.
> 
> Janitor's Closet turns cleaning from an everyday chore into a profession. Put the right colonists on the job and the colony quietly reshapes itself around their work. Think kitchens where the floor isn't ending up in the food, hospitals that don't reek of the casualties after a raid, a rec room that's somehow presentable when visitors arrive. They're the colonists you'll find yourself watching while they work, and the ones you'll miss when they're not around.
>
> Cleanliness isn't cosmetic. A janitor is a shield against chaos—and you only see their real work once they stop cleaning.

Janitor's Closet is a RimWorld mod that turns the colony janitor from a chore into a character. Themed gear, dedicated tools, and (eventually) autonomous behaviors that make a colonist with the Cleaning priority feel like a specialist who actually owns the role.

## Design Pillars

1. **Make a dedicated janitor pawn feel impactful and noticeable.** When a colonist is the janitor, you should be able to tell by looking at them and by how the colony plays.
2. **Janitor-specific gear** — equipment that grants meaningful cleaning speed buffs.
3. **Janitor-specific autonomous cleaning behaviors** — detect large messes, place wet floor signs other pawns path around, etc. The janitor *does janitor things* on their own; the player isn't placing signs by hand.
4. **Janitor-specific buildables** — enable the construction of a home base for janitors with the supplies and storage they need to do their work.

## Compatibility

- **RimWorld 1.7** only.
- Harmony is a hard dependency (declared in `About.xml`).

## Repo layout

The repo root is **not** the mod folder. Runtime mod content lives under `mod/`, and only that subdirectory should ever be uploaded to Steam Workshop:

```
JanitorsCloset/                    <- repo root (.git, Source, README, .sln)
├── Source/                        <- C# project + art binaries (not shipped)
└── mod/                           <- the actual mod (point Steam/RimWorld here)
    ├── About/
    ├── Languages/
    ├── Textures/
    ├── Common/                    <- defs and patches shared by every game version
    ├── 1.7/                       <- 1.7 DLL only
    └── LoadFolders.xml
```

This keeps `.git/` and `Source/` (`.ai`/`.psd` design files) out of the Workshop upload.

## Building

The C# project is at `Source/JanitorsCloset/JanitorsCloset.csproj`. It targets .NET 10 and outputs the DLL into `mod/1.7/Assemblies/`. It references `Assembly-CSharp.dll`, `UnityStubs.dll` and `Waylith.dll` from your RimWorld install, which the build finds through Steam's library folders. For a non-Steam install, set the `RIMWORLD_DIR` environment variable to the folder that holds `Assembly-CSharp.dll`.

```
dotnet build "Source/JanitorsCloset/JanitorsCloset.csproj"
```

## Dev workflow

The repo lives outside the RimWorld install. To make RimWorld load it, create a **directory junction** from the install's `Mods\` folder to the `mod\` subdirectory. Junctions are reparse points — RimWorld walks into `Mods\JanitorsCloset` and transparently reads from the source path. No copy, no sync, no upload step.

```
mklink /J "<RimWorld install>\Mods\JanitorsCloset" "<this repo>\mod"
```

Notes:
- Run from `cmd` (or `cmd /c "..."` from PowerShell). `mklink` is a `cmd.exe` builtin.
- `/J` (junction) doesn't need admin rights and works for same-volume directories. `/D` (true symbolic link) requires admin or Windows Developer Mode and is unnecessary here.
- Edits via either path edit the same physical files. Restart RimWorld to reload changes (XML def changes always need a full restart; C# changes always need a rebuild + restart).
- To remove: `rmdir "<RimWorld install>\Mods\JanitorsCloset"`. This deletes only the junction, not the source folder.
- Workshop publishing is a separate step done via the in-game mod settings later. Junctioned mods do not need to be on Workshop to be loadable.
