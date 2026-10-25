# Progress: 1.7 port, Tier 1 and test tooling

Status as of 25 October 2026. Janitor's Closet `main` is at `1447577`. Archotech Harness `main` is at `acbd010`.

## Where things stand

Janitor's Closet runs on RimWorld 1.7, and the eleven Tier 1 items from [the plan](reviews/plan.md) are done and verified in game. Gameplay changes can now be proven end to end: a C# scenario builds a colony, lets the game run, and checks the result. Four scenarios run in parallel in about 50 seconds.

## Janitor's Closet

**1.7 port**
- The mod builds for .NET 10 against RimWorld 1.7's assemblies, with Harmony 3.
- Shared defs and patches live in `mod/Common`. 1.6 support is dropped.
- The build finds RimWorld through Steam, or through `RIMWORLD_DIR`.

**Tier 1: performance**
- Only a pawn whose tool prefers a filth category runs the full filth search. Every other pawn keeps vanilla's nearest search.
- Per-tick work runs only while a tool holder is cleaning. Pawns that aren't cleaning run none of the mod's code per tick.
- Stat parts no longer allocate on every tick.

**Tier 1: correctness and quality of life**
- A forced cleaning order keeps the filth you clicked.
- Fresh-room stamps survive reloads.
- Pawns incapable of violence can equip cleaning tools.
- Hazmat foam shows for sprayers off-screen.
- Research nodes no longer overlap vanilla ones.
- Opinion penalties stack once per pawn.
- Info cards state each tool's real bonus, including the glittervacuum's per-tile speed.

**Tier 1: feel**
- A freshly cleaned room gives +2.
- Downed and bedridden pawns don't get the tracked-filth thought.
- A room made spotless gives its janitor a +3 thought, a glint, a faint chime and a short floor shimmer, at most once an hour per janitor.

**Project**
- 42 unit tests.
- Four in-game scenarios in `harness/`.
- A rewritten README and a new CONTRIBUTING guide.
- Code under Apache 2.0 and assets under CC BY 4.0, with a NOTICE that names Terra Incognita.

## Archotech Harness

A new public tool: [AW-Koi/archotech-harness](https://github.com/AW-Koi/archotech-harness). It tests RimWorld mods end to end.

- **Scenarios** are C# classes. They use fluent builders and typed defs:

  ```csharp
  Spawn.Colonist()
      .Doing(WorkTypeDefOf.Cleaning)
      .IncapableOf(WorkTags.Violent)
      .Spawn();
  ```

  A scenario can declare its own `[DefOf]` class, and the harness binds it.
- **`archo test`** finds scenarios, filters them and retries failures.
  - It boots one game per mod and DLC set, then gives each scenario a fresh colony in about a second.
  - It runs games in parallel with `--workers auto`.
- **Reports** include checks with their source text, notes, screenshots and a step timeline. Add `--video` for an mp4 per scenario.
- **Isolation:** your saves and config are never touched. Player files are backed up, restored and hash-checked after every run.
- **Mods can expose read-only probes** through an `ArchotechProbes` class.

## Not yet verified in game

- Fresh-room stamps surviving a save and reload. The unit tests cover the logic, but no scenario saves and reloads yet.
- A violence-disabling gene leaving the janitor's tool in hand.
- Performance measured in game, either as timings or as an allocation probe.

The harness can now script all three, so they make good first scenarios for the next tier.

## Next: Tier 2

[The plan](reviews/plan.md) lists Tier 2 as cheap polish in three streams.

- **Performance:**
  - cache the area restriction
  - merge the `ThinFilth` patches
  - keep marker filth out of the home-filth list
  - cache the map component
  - compare sounds by reference
  - cache gizmo labels and icons
- **Quality of life:**
  - give filtered filth a failure reason
  - correct the cleaning-area wording
  - apply area picks to every selected janitor
  - hide gizmos on non-colonists
  - show filth categories on info cards
  - show a tool's assignment in its inspect string
  - add a cleaning-tools stockpile category
  - add gameplay toggles
- **Content:**
  - classify machine bits and fleshmass correctly
  - scale the cleaning bonus with quality
  - add a cleaning inspiration
  - add janitor backstories
  - lowercase "hazmat"
  - offer a hay-free straw broom recipe
  - patch the sprayer for Progression: Education
  - give the glittervacuum its toxic and weather bonuses
- **Strings:** remove the defName string checks in the effecter, sound and filth-category code.

Each gameplay change should come with a scenario that fails before the change and passes after it.

Tier 3 holds the larger features: the closet as a resupply hub, wet-floor signs, mess detection and tool swapping. Each needs a design pass first; see [design notes](reviews/design-notes.md).
