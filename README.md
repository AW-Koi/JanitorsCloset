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

# TO-DOs
- [x] Mop
- [x] Mopping behaviour + wet floors
- [x] Janitor's Coveralls
- [ ] Janitor's Galoshes
- [ ] Janitor's Rubber gloves
- [ ] Janitor's Cap

## Compatibility

- **RimWorld 1.6** only (current target).
- Harmony is a hard dependency (declared in `About.xml`).

## Building

The C# project is at `Source/Janitor's Closet/JanitorsCloset.csproj`. It outputs the DLL into `1.6/Assemblies/`. References point to `Assembly-CSharp.dll` and Unity assemblies under your local RimWorld install — adjust the `HintPath` values in the csproj if your install isn't at the default Steam path.

```
dotnet build "Source/Janitor's Closet/JanitorsCloset.csproj"
```

## Dev workflow

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
