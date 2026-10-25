# Progress: 1.7 port, Tiers 1 and 2, and test tooling

Status as of 25 October 2026. Janitor's Closet `main` is at `06f7246`. Archotech Harness `main` is at `35fadd9`.

## Where things stand

Janitor's Closet runs on RimWorld 1.7, and Tiers 1 and 2 from [the plan](reviews/plan.md) are done. Every gameplay change has an in-game scenario. The 23 scenarios run in parallel in about a minute, and each new one failed before its change.

Tier 3 is next, and each feature needs a design pass first; see [design notes](reviews/design-notes.md).

## Tier 2

**Performance**
- One `Filth.ThinFilth` patch, with a struct state, runs the glittervacuum's field collapse, mop marks and the fresh-room stamp. It replaces three patches that each allocated per call.
- Mop marks and foam stay off the home-area filth list, so idle cleaners never scan them.
- The fresh-room component is looked up through a per-map table. The filth-spawn hook exits early while no room is stamped.
- A tool's cleaning area resolves once per tick rather than once per filth. Gizmo labels, descriptions and icons are cached.
- Cleaning sounds and effecters compare by def reference rather than by name.
- A scenario reads a cleaning janitor's work-speed stats a thousand times and requires zero bytes allocated.

**Quality of life**
- Selected janitors share one area gizmo, and picking an area sets every selected tool.
- A forced order outside a tool's assignment fails with the reason "outside the tool's cleaning assignment".
- Only colonists show the assignment gizmos.
- A tool's inspect pane names its assignment, such as "Cleans indoors, in Home".
- The area gizmo says cleaning stays inside the home area.
- A filth's info card shows whether it's wet or dry.
- Cleaning tools have their own stockpile category under weapons.
- The settings page holds only diagnostics, and it appears only in development mode. There are no gameplay toggles, by design.
- Stat explanations use keyed format strings, and the dead translation keys are gone.

**Content**
- Tool bonuses scale with quality, from 0.8x for awful to 1.5x for legendary.
- The glittervacuum gains +100% for pollution and weather buildup. Filth still cleans at half speed per tile.
- Machine bits count as dry filth, and Anomaly's fleshmass debris as wet.
- A cleaning frenzy inspiration doubles cleaning speed for four days.
- Four backstories: janitor's kid, custodian, sanitation worker and ship's swab.
- A straw broom can be made from wood alone.
- "Hazmat" is lowercase in labels and stat lines.
- With Progression: Education and Biotech, the sprayer needs low-tech proficiency. A one-off run with PE and VE Framework from the workshop confirmed it.
- The galoshes keep their grip claim for the Tier 3 slip mechanic.

**Verified from Tier 1**
- Fresh-room stamps survive a save and reload.
- A violence-disabling gene drops a club but leaves a mop in hand.

## Tier 1

- **Performance:** only pawns whose tool prefers a filth category run the full filth search. Per-tick work runs only while a tool holder cleans, and stat parts allocate nothing per tick.
- **Correctness and quality of life:** forced orders keep their target, fresh-room stamps survive reloads, pacifists can hold tools, foam shows off-screen, research nodes don't overlap, opinion penalties stack once, and info cards show each tool's real bonus.
- **Feel:** a freshly cleaned room gives +2, downed pawns don't get the tracked-filth thought, and a spotless room gives its janitor +3 with a glint, chime and shimmer.

## Scenarios

Scenarios live in feature folders under `harness/`: `Assignments`, `Cleaning`, `Crafting`, `FreshRooms`, `Inspirations`, `Performance`, `Settings` and `Tools`. `Scenarios.csproj` declares the mod once, and each block of a scenario starts with a one-line comment. Run them with `<archotech harness>/archo test harness --workers auto`.

## Archotech Harness

- **Project-level mods:** a scenarios project declares its mod with an `ArchoMod` item, so scenarios need no `[Mod]` and can sit at any folder depth.
- **Disk use:** before each run, the harness keeps the newest 20 runs in each folder, capped at 1 GB. `archo clean` prunes by hand; `ARCHO_KEEP_RUNS` and `ARCHO_KEEP_MB` change the limits.
- **`Reload()`** saves the game, loads it back and waits until the map is playable.
- **`Require`** is marked as an assertion, so null analysis trusts it.

## Repositories

Both repos protect `main`: nobody can force-push, delete it or merge non-linear history. Outside contributors open pull requests with a template, and the maintainer reviews and squash-merges. The maintainer can push to `main` directly.

## Not yet verified in game

- The glittervacuum's pollution bonus. Its snow bonus is verified, and both use the same stat part.
- Backstories have no scenario. They load without errors.

## Next: Tier 3

Tier 3 holds the larger features: the closet as a resupply hub, wet-floor signs with slipping, large-mess detection, and tool swapping. Each needs a design pass first; see [design notes](reviews/design-notes.md). The `CleaningTools` stockpile category and the shared `Janitor_CleaningToolBase` are ready for the closet to build on.
