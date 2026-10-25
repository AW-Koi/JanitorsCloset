# Performance review

Commit read: `1ead39f` (origin/main).

I followed the brief with no deviation. I read origin/main at commit 1ead39f in the read-only worktree, and checked the vanilla call sites by decompiling Assembly-CSharp.dll (decompiled sources are in C:/Users/Synergy/AppData/Local/Temp/claude/D--Modding-RimWorld-JanitorsCloset/0af785ee-0382-44d0-8bcb-b403a0da9da5/scratchpad/performance). I measured nothing in-game, so the costs below are estimates from code paths.

## Findings ranked by impact on a 20+ pawn colony

1. **High: cleaning is forced to `Prioritized` for every pawn.** `Patches/Patch_CleanFilthCategoryBias.cs:24-31` and `:72-91`.
   - **Frequency:** every work search that reaches Cleaning, for every pawn with Cleaning enabled.
   - **Cost:** with `Prioritized` on, `JobGiver_Work` calls `GenClosest.ClosestThing_Global_Reachable` with a priority getter. That method skips its distance pruning when a getter is present, so it runs `CanReach`, `IsForbidden`, `HasJobOnThing` (`CanReserve`, home and fog checks, plus both mod postfixes) and `GetPriority` on every filth in `listerFilthInHomeArea`. Vanilla does a region-local closest search that stops at the first valid filth, usually after tens of checks. A late game can have 500 to 2000 home filth, which I estimate at roughly 1 to 5 ms per scan, for each pawn.
   - **Wasted for tool-less pawns:** a pawn without a single-category tool gets priority `-distSq`, which picks the same filth vanilla would.
   - **Fix:** record the scanning pawn in a `JobGiver_Work.TryIssueJobPackage` prefix. Return `Prioritized = true` only when `PawnToolPreference.TryResolvePreferredCategory` gives a value for that pawn.

2. **Medium: diagnostic arguments allocate every tick even with logging off.** `Cleaning/StatPart_CleaningToolMatch.cs:60-74`, `StatPart_PollutionToolBonus.cs:52-56`, `StatPart_WeatherBuildupToolBonus.cs:47-51`.
   - **Frequency:** vanilla's clean `tickIntervalAction` calls `GetStatValue(CleaningSpeed)` uncached on every interval tick for each cleaning pawn (pollution and snow jobs use GeneralLaborSpeed the same way).
   - **Cost:** the `params object[]` array, a boxed `CleaningCategory`, and `ToStringPercent()` (two strings) are built before `Diagnostics` checks `LogStats`. That is roughly 100 to 150 bytes of garbage per cleaning pawn per tick.
   - **Fix:** check `JanitorMod.Settings?.LogStats` before building the arguments.

3. **Medium (multiplied by #1): the area restriction runs a label lookup per filth.** `Patches/Patch_CleaningAreaAssignment.cs:62-69` calling `CompCleaningAssignment.cs:39-58`.
   - **Frequency:** every `HasJobOnThing` call for a pawn whose tool has an area set.
   - **Cost:** `AreaManager.GetLabeled` compares strings across all areas. Built-in areas such as `Area_Home.Label` call `"Home".Translate()` on each comparison. The same call also does `GetComp` and `GetRoom`.
   - **Fix:** cache the resolved `Area` per map for each tick, or store the area's ID instead of its label.

4. **Low to medium: marker filth sits in the home-filth list.** `Patches/Patch_SkipCleaningMarkerFilth.cs:59-62`.
   - **Cost:** mop marks and foam keep vanilla's `ShouldSkip` false, so idle cleaners still scan. The postfix rejects a marker only after vanilla's `CanReserve`, and in the #1 sweep also after `CanReach`.
   - **Fix:** use a prefix that returns false before the original runs.

5. **Low: JobDriver tick hooks run on every pawn every tick.** `Patch_TrackCurrentJobDriver.cs:22-48` covers `DriverTick` and `DriverTickInterval` for all pawns, including animals, visitors and raiders. Four more `DriverTickInterval` postfixes add type checks (`Patch_BroomDustPuff`, `Patch_HazmatSprayerVFX`, `Patch_SpawnHazmatFoam`, `Patch_GlittervacuumCleaningGlow`).
   - **Cost:** about 10 type checks plus `[ThreadStatic]` reads and writes per pawn per tick. `[ThreadStatic]` is slow on Mono.
   - **Fix:** merge into one postfix and use a plain static.

6. **Low: the `Filth.SpawnSetup` postfix does a linear component search.** `Patch_TrackedFilthIntoCleanRoom.cs:29`.
   - **Frequency:** every filth spawn, including all filth on map load.
   - **Cost:** `Map.GetComponent<T>` loops over every map component, then `GetRoom` runs.
   - **Fix:** cache the component per map and exit early while no room is stamped.

7. **Low: `Filth.ThinFilth` allocates three `State` objects per call.** `Patch_StampFreshRoomOnClean.cs:28`, `Patch_SpawnMopMark.cs:24`, `Patch_GlittervacuumDematerialise.cs:40`.
   - **Fix:** merge the three into one patch with a struct `__state`.

8. **Low: closures on every cleaning job.** `Patch_GlittervacuumLayerCollapse.cs:41-49` wraps every toil's `initAction` in a closure for every cleaning job, whatever the tool.
   - **Fix:** wrap only the clean toil, and only when the tool has `clearsFilthStack`.

9. **Low: the fresh-room dictionary grows without limit.** `MapComponent_FreshlyCleanedRooms.cs:36`.
   - **Cost:** room IDs never repeat, so stamps on rooms that construction has rebuilt stay in the dictionary and are written to the save.
   - **Fix:** prune IDs that no longer match a live room during the 500-tick refresh.

10. **Low: gizmos allocate every frame for each selected pawn.** `CompCleaningAssignment.cs:76-103` concatenates strings and boxes the enum for `Translate()`, then calls `ContentFinder.Get` per frame. Selecting all 20 colonists multiplies this by 20.
    - **Fix:** cache labels and icons per assignment value.

11. **Low: the sound patch uses a culture-sensitive string check.** `Patch_CleaningToolSound.cs:43` calls `StartsWith` with culture rules on every sustainer created anywhere.
    - **Fix:** pass `StringComparison.Ordinal`.

12. **Debug only: diagnostic dictionaries hold `Pawn` references.** `Patch_DrawEquipmentAiming.cs:128` and `Patch_WeaponRenderDiagnostic.cs:37` keep pawns alive across save reloads, but only while their logging channels are on.

## Paths I checked that are fine

- **Per-frame draw hooks:** `Patch_DrawEquipmentAiming`, `Patch_HazmatSprayerInHandDraw`, `Patch_HazmatTankRender`, `Patch_CarryWeaponOpenly` and `Patch_WeaponRenderDiagnostic` use def reference compares or one `GetModExtension` with early exits. They cache their `Graphic` and materials and allocate nothing.
- **Other hooks:** the `EffecterDef.Spawn` postfix, the `JobOnThing` queue reorder (scratch list reused, one small closure per job), `MapComponentTick` (every 500 ticks, staggered), and `SpecialDisplayStats` (info card only) are all cheap.
- **Caches:** `FilthCategoryResolver`, `SoftFloorResolver` and `PawnToolPreference` are bounded or cached per tick. The per-pawn VFX dictionaries are bounded by pawn count.
- **Stat parts:** apart from #2, every stat part exits early, so the GeneralLaborSpeed parts add almost nothing outside cleaning jobs.
- **Ticking comps:** the mod has none.
