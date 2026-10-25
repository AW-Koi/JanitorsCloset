# Performance proposal

Draft for review. No code has changed. Each finding below was checked against the source at `1ead39f` and the decompiled 1.7 `JobGiver_Work`, `GenClosest`, `Pawn_JobTracker` and `JobDriver`.

## Targets

- **Pawns that aren't cleaning pay nothing.** The mod adds no work to any per-tick method that every pawn runs.
- **Cleaning pawns allocate nothing per tick.** Per-job allocations are kept to the minimum and listed explicitly.
- **Pawns without a preferring tool keep vanilla's work-scan cost.** Janitors with a single-category tool pay only for their own scan.
- **Each hot path has an in-game probe** that reports bytes allocated and time per call, so a regression shows up.

## What's wrong now

| # | Cost | Where | Verified behaviour |
|---|---|---|---|
| 1 | High, per work scan for every cleaner | `Patch_CleanFilthCategoryBias.cs:24-31` | `Prioritized` is forced on for every pawn. Vanilla `JobGiver_Work` then calls `ClosestThing_Global_Reachable` with a priority getter. Once the getter is set, the distance cut-off no longer applies, so every home-area filth gets `CanReach`, the full `HasJobOnThing` validator (reservation, fog, both mod postfixes) and `GetPriority`. Without the patch, vanilla's search stops after 4 regions at the nearest valid filth. A pawn with no tool pays the full cost and gets the same answer vanilla would. |
| 2 | Medium, every tick for **every pawn with a job**, including animals and visitors | `Patch_TrackCurrentJobDriver.cs`, plus 4 postfixes on `DriverTickInterval` | `Pawn_JobTracker` calls `DriverTick` every tick and `DriverTickInterval` every interval tick for every pawn. The mod adds a prefix and postfix to both methods, plus 4 more postfixes, so up to 7 patch calls and about 12 type checks per pawn per tick. Only cleaning pawns need any of it. |
| 3 | Medium, every interval tick for each cleaning pawn | `StatPart_CleaningToolMatch.cs:60-74`, `StatPart_PollutionToolBonus.cs:52-56`, `StatPart_WeatherBuildupToolBonus.cs:47-51` | The cleaning toil reads the stat each interval. The diagnostic call builds a `params object[]`, boxes the enum and formats percent strings before `Diagnostics` checks `LogStats`. That happens on both the match and mismatch paths, so it allocates on every tick of cleaning. |
| 4 | Medium, multiplied by #1 | `CompCleaningAssignment.cs:39-58`, called from `Patch_CleaningAreaAssignment.cs:50-57` | For a pawn whose tool has an area set, each validated filth runs `GetComp`, `GetRoom` and `AreaManager.GetLabeled`. That last call compares strings over every area, and built-in area labels call `Translate()`. The room check runs before the cheaper area-grid check. |
| 5 | Low to medium, every `ThinFilth` by any cleaner | `Patch_StampFreshRoomOnClean.cs`, `Patch_SpawnMopMark.cs`, `Patch_GlittervacuumDematerialise.cs` | Three prefix and postfix pairs. Two of them allocate a `State` class on every call before checking whether a janitor tool is involved. |
| 6 | Low to medium, every scan | `Patch_SkipCleaningMarkerFilth.cs:33-36` | Mop marks and foam sit in `listerFilthInHomeArea`. That keeps `ShouldSkip` false, so idle cleaners scan with nothing real to clean. The marker rejection is a postfix, so it only runs after vanilla's reservation check (and `CanReach`, under #1). |
| 7 | Low, per cleaning job for every cleaner | `Patch_GlittervacuumLayerCollapse.cs:23-31` | Every toil's `initAction` gets wrapped in a closure, whatever tool the pawn holds. |
| 8 | Low, every filth spawn (and every filth on map load) | `Patch_TrackedFilthIntoCleanRoom.cs:29` | `Map.GetComponent<T>` does a linear search over the map components, then `GetRoom` runs. |
| 9 | Low, save size over a long game | `MapComponent_FreshlyCleanedRooms.cs:36` | Room IDs never repeat, so stamps for rooms that no longer exist stay in the dictionary and in the save. |
| 10 | Low, per sustainer creation | `Patch_CleaningToolSound.cs:35` | Culture-sensitive `StartsWith` on every sustainer in the game. |
| 11 | UI only, per frame while selected | `CompCleaningAssignment.cs:76-103`, `Patch_CleaningAreaAssignment.cs:21-33` | Gizmo labels are concatenated, the enum is boxed for `Translate()` and icons are looked up every frame. |

I didn't re-check the draw hooks myself. The agent found they allocate nothing and exit early.

## Proposed fixes

### 1. Work scan: run the full scan only for pawns that use it

Record the scanning pawn in a prefix on `JobGiver_Work.TryIssueJobPackage`, and clear it in a finaliser. The `Prioritized` getter returns true only when `PawnToolPreference` gives that pawn a single category. Every other pawn, including all pawns without a tool, goes back to vanilla's 4-region nearest search.

That still leaves janitors on a full scan. A follow-up could replace it with two early-exit searches: the nearest matching-category filth within a radius, then vanilla nearest as the fallback. That changes the rule from "a match up to 40 tiles further beats a closer mismatch" to "the nearest match within N tiles, else the nearest anything", so it needs your call. I suggest doing step 1 now, then measuring the janitor scan with the probe before deciding.

### 2. Scope every per-tick hook to the cleaning toils

Remove `Patch_TrackCurrentJobDriver` and the four `DriverTickInterval` postfixes. Instead, postfix `MakeNewToils` on `JobDriver_CleanFilth`, `JobDriver_ClearPollution` and `JobDriver_ClearSnowAndSand`. For the work toil only, wrap the tick delegates so they do three things:
- set `Current`
- run the original delegate
- run the tool's VFX

The wrapper does nothing when the pawn holds no janitor tool.

- **Result:** pawns that aren't cleaning run none of the mod's code per tick.
- **Cost:** one closure per wrapped toil per cleaning job. Vanilla already allocates the job, its toils and their closures, so this adds to an existing per-job allocation rather than creating a per-tick one. Fix #7 folds into this change.
- **Needs checking during implementation:** the effecter spawn, the sustainer creation and `ThinFilth` all have to run inside the wrapped delegates. The `Clean` effecter and the sustainer come from `WithEffect` and `PlaySustainerOrSound` helpers, and I haven't yet confirmed which toil delegate list they use.
- **Optional:** the stat parts could read `pawn.jobs.curDriver` instead of `Current`. That drops their dependence on tick timing. It also means the info card shows the real bonus while a pawn is mid-job, which is the overstated bonus the QoL review flagged. That changes behaviour, so it's your call.

### 3. Stat-part diagnostics

Check `LogStats` before building any arguments, at each call site. Apply the same pattern to the sound patch.

### 4. Area restriction

- Cache the resolved `Area` per map and invalidate it when areas change, or store the area ID instead of the label and keep resolving by label only across maps.
- Check the area grid before `GetRoom`.

### 5. Merge the `ThinFilth` patches

Make one prefix and postfix pair with a struct `__state`. It exits before capturing anything unless the current driver is a cleaning job and the tool has a `CleaningToolExtension`.

### 6. Keep marker filth out of the home-filth list

Stop mop marks and foam from entering `listerFilthInHomeArea`, so they never reach a scan and an idle colony skips the cleaning work giver entirely. If that hook proves fragile, fall back to a `HasJobOnThing` prefix that rejects markers before vanilla runs.

### 8 to 11. Small fixes

- **#8:** cache the component per map, and exit when no rooms are stamped.
- **#9:** prune dead room IDs during the existing 500-tick refresh.
- **#10:** compare `SoundDef` references against the two vanilla cleaning sounds instead of strings.
- **#11:** cache the gizmo labels and icons for each assignment value.

## How we prove it

The repo has no test project, and most of these hot paths need a live map and pawn. My proposal:

- **Allocation probe:** a dev-mode debug action. It runs each hot path 10,000 times against a real pawn and filth: the stat parts' `TransformValue`, `GetPriority`, both `HasJobOnThing` postfixes, the tick wrapper and the `ThinFilth` patch. It reports `GC.GetAllocatedBytesForCurrentThread()` and time per call. The target is 0 bytes for every path except the per-job closure in #2.
- **Scan timing:** time a scan on a seeded dev map with 1,000 filth and 20 pawns, before and after #1.
- **TDD:** for each fix, I first record the probe failing (non-zero bytes, or the "before" timing), then make the change.

## Overlap with the correctness review

The correctness review came in after I wrote this draft. Three of its findings touch the same code:

- **Correctness #3 (foam and spray depend on the camera):** `DriverTickInterval` runs every 1 to 15 ticks, so `TicksGame % N` checks miss for most pawns. The fix #2 tick wrapper has to use the `delta` argument (`GenTicks.IsTickIntervalDelta` or a per-pawn accumulator), which fixes this bug as part of the same change.
- **Correctness #7 (straight-line distance instead of path order):** fix #1 restores vanilla's search order for pawns without a preference.
- **Correctness #2 (stamps keyed by room ID don't survive a load):** this replaces fix #9. Key the stamps by a representative cell, which also removes the growing dictionary.

## Order and decisions

Order: #1, #2, #3, #5, #6, #4, then the small fixes.

Decisions for you:
1. **Janitor scan:** keep the full scan for janitors for now (step 1 only), or go to the two-search rule straight away?
2. **Per-job closure:** is one closure per cleaning job acceptable for #2?
3. **Stat parts:** should they read `curDriver`, which changes what the info card shows mid-job?
