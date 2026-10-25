# Correctness review

Commit read: `1ead39f` (origin/main). The paths `mod/1.6/...` below are now under `mod/Common/...` on the `port/rimworld-1.7` branch.

I didn't deviate from the brief. I read origin/main at commit 1ead39f in the read-only worktree, and decompiled Assembly-CSharp (plus the Holsters and Progression: Education assemblies) into the scratch folder `C:/Users/Synergy/AppData/Local/Temp/claude/D--Modding-RimWorld-JanitorsCloset/0af785ee-0382-44d0-8bcb-b403a0da9da5/scratchpad/correctness`. I found no crash-class bugs. The three mod-compat paths are null-safe when the other mod is absent, the Holsters parameter name `weapon` matches its `DrawWeapon`, and all Harmony targets and argument names match 1.6.

## Findings, most severe first

**1. A forced cleaning order can drop the filth the player clicked.**
- **Where:** `Source/JanitorsCloset/Patches/Patch_CleanFilthQueueCategoryOrder.cs:35-81`
- **Defect:** The postfix runs on forced `JobOnThing` too, and it removes the clicked target from the queue whenever a nearby filth matches the tool's preference.
- **Trigger:** Give a pawn a push broom, right-click a blood pool with dirt nearby, and pick "Prioritize cleaning". The queue becomes dirt only, so the pawn sweeps the dirt and leaves the blood.
- **Fix:** Return early when `forced` is true, or always keep `queue[0]`.
- **Confidence:** Confirmed by tracing `FloatMenuOptionProvider_WorkGivers` → `JobOnThing(forced: true)`.

**2. Freshly-cleaned stamps don't survive save/load.**
- **Where:** `Source/JanitorsCloset/Cleaning/MapComponent_FreshlyCleanedRooms.cs:36, 62, 139`
- **Defect:** The stamps are saved under `Room.ID`, which comes from a static counter that is never saved, and rooms are rebuilt with new IDs on load (`Map.FinalizeLoading` → `RebuildAllRegionsAndRooms`).
- **Trigger:** Stamp a room, save, and reload. The stamp is lost. After a fresh game launch the IDs restart at 0, so an old key can land on an unrelated room, which then gives the moodlet without being cleaned. The patch on `Filth.SpawnSetup` also runs during load, so a matching ID can expire a stamp and give "tracked filth" to a pawn just because it loaded on that cell.
- **Side effect:** Every remodel leaves an orphan entry that is saved forever.
- **Fix:** Save a representative cell per stamp and resolve it with `cell.GetRoom()`, or don't save the dictionary at all. Skip the spawn patch when `respawningAfterLoad` is true.
- **Confidence:** Confirmed.

**3. Hazmat foam, doused thoughts and spray puffs depend on the camera.**
- **Where:** `Source/JanitorsCloset/Patches/Patch_SpawnHazmatFoam.cs:47-49` and `Patch_HazmatSprayerVFX.cs:49-51`
- **Defect:** Both check `(TicksGame + phase) % 30` (or `% 10`) inside `DriverTickInterval`. In 1.6 that method runs only every 1–15 ticks, at a per-pawn hash offset (`Thing.DoTick`, `GenTicks.GetCameraUpdateRate`), so for most pawns the condition never comes true.
- **Trigger:** A pawn sprays pollution off-screen. With a 15-tick interval, about 14 of 15 pawns never lay foam and never douse bystanders. Zoomed out, about half of pawns get no foam or puffs.
- **Fix:** Use the `delta` argument with `GenTicks.IsTickIntervalDelta(offset, period, delta)`, or keep a per-pawn tick accumulator.
- **Confidence:** Confirmed.

**4. Research nodes overlap vanilla nodes.**
- **Where:** `mod/1.6/Defs/ResearchProjectDefs/ResearchProjects_Janitor.xml:14-15, 56-57` and `mod/1.6/Biotech/Defs/ResearchProjectDefs/ResearchProjects_HazmatSprayer.xml:60-61`
- **Defect:** Nodes are 140×50 px, placed at x·190, y·100. Housekeeping (0.5, 1.2) overlaps Stonecutting (0, 1.4) and Pemmican (1, 1.4). AtomicSanitation (9, 1.2) overlaps TubeTelevision (9, 0.9) and PackagedSurvivalMeal (9, 1.5). ToxicCleanup (5.5, 2.2) overlaps AirConditioning (6, 2.1).
- **Trigger:** Open the Main research tab. Overlapping nodes hide or block clicks on vanilla projects.
- **Fix:** Move the nodes to free coordinates.
- **Confidence:** Confirmed against Core defs and `MainTabWindow_Research`.

**5. The area picker applies to only one tool when several janitors are selected.**
- **Where:** `Source/JanitorsCloset/Cleaning/CompCleaningAssignment.cs:175-204`
- **Defect:** Grouped gizmos each open a `FloatMenu`, and `onlyOneOfTypeAllowed` keeps only the last one, so the choice reaches a single tool.
- **Trigger:** Select two janitors, click Area, and pick Home. Only one tool is restricted.
- **Fix:** Override `ProcessGroupInput`, or capture every comp in the group.
- **Confidence:** Confirmed.

**6. Non-colonists show the cleaning-assignment gizmos.**
- **Where:** `Source/JanitorsCloset/Patches/Patch_CleaningAreaAssignment.cs:33-44`
- **Defect:** The `GetGizmos` postfix doesn't check the pawn's faction, and `Pawn.GetGizmos` calls `equipment.GetGizmos()` for every pawn.
- **Trigger:** Select a raider or trader holding a mop. The assignment gizmos appear and work.
- **Fix:** Only yield when `__instance.pawn.IsColonistPlayerControlled`.
- **Confidence:** Confirmed. The effect is harmless.

**7. Vanilla cleaning target choice changes for every pawn.**
- **Where:** `Source/JanitorsCloset/Patches/Patch_CleanFilthCategoryBias.cs:24-31, 80-85`
- **Defect:** Forcing `Prioritized` sends all cleaners, toolless pawns included, through `ClosestThing_Global_Reachable` scored by straight-line distance. Vanilla's search goes region by region, nearest along the path first, for 4 regions.
- **Trigger:** A pawn picks filth just behind a wall over filth that is genuinely nearer by path.
- **Fix:** Set `Prioritized` only when the pawn has a preferred category, or score by path distance.
- **Confidence:** Plausible.

**8. A second mopper's splash refreshes the first mopper's opinion penalty.**
- **Where:** `mod/1.6/Defs/ThoughtDefs/Thoughts_Janitor.xml:150-161` (the Biotech decon-foam social thought behaves the same)
- **Defect:** `stackLimit` 1 on a social memory, combined with the shared label, makes `TryMergeWithExistingMemory` group across different `otherPawn`s.
- **Trigger:** Mopper A splashes a bystander, then mopper B splashes the same bystander. The penalty against A is renewed and B gets nothing.
- **Fix:** Use `stackLimitForSameOtherPawn`.
- **Confidence:** Confirmed.

**9. Progression: Education still gates the HAZMAT sprayer.**
- **Where:** `mod/1.6/ProgressionEducation/Patches/Weapons.xml`
- **Defect:** Only the push broom and mop get the LowTech proficiency override. The sprayer is Industrial too, so by the patch's own premise it needs Firearm proficiency.
- **Trigger:** With PE and Biotech active, a non-shooter can't equip the sprayer.
- **Fix:** Add a Biotech+PE gated patch for the sprayer.
- **Confidence:** Plausible. I didn't trace PE's enforcement path.

## Minor items

- The Glittervacuum declares Toxic and WeatherBuildup but has no `CleaningSpeed` offset, so both bonus StatParts add 0. Its "weather buildup" stat text promises a bonus that never applies.
- `Janitor_TrackedFilthIntoCleanRoom` goes to any pawn on the filth's cell. That includes a colonist shot while standing in a clean room and a surgery patient.
