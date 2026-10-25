# Quality-of-life review

Commit read: `1ead39f` (origin/main).

I followed the brief with no deviations. I read commit 1ead39f (origin/main, the scratchpad worktree). To check vanilla behaviour, I decompiled `FloatMenuOptionProvider_Equip`, `Pawn_EquipmentTracker` and `PawnAttackGizmoUtility` from the installed Assembly-CSharp.dll.

The mod does have ModSettings, but they contain only diagnostic logging toggles (`JanitorsCloset.cs:18-56`).

## Ranked QoL issues

1. **Pawns incapable of violence can't hold any tool.** All tools inherit `BaseMeleeWeapon_*` (`Weapons_Janitor.xml:5,82,163,247`; also the hazmat sprayer). Vanilla `FloatMenuOptionProvider_Equip:26` refuses any `IsWeapon` thing for these pawns, and they're the colonists players most often make cleaners.
   - Fix: patch that check (and the Equip job) to allow things with a `CleaningToolExtension`.
   - Size: small to medium.

2. **The tool takes the only weapon slot, so a janitor must swap by hand between combat and cleaning.** Every bonus reads `equipment.Primary` (`StatPart_CleaningToolMatch.cs:43`, `PawnToolPreference.cs:41`). The mod has no swap logic.
   - Fix: auto-swap from inventory when a Clean, ClearPollution or ClearSnow job starts and swap back when drafted, or add Simple Sidearms compatibility. At minimum, add a one-click "swap to tool" gizmo.
   - Size: large.

3. **Cleaning speed overstates the bonus outside a job.** The suppression applies only while a job is active (`StatPart_CleaningToolMatch.cs:51-52`). A mop shows +60% at all times, and the hazmat sprayer shows +300% Cleaning speed (`Weapons_HazmatSprayer.xml:47`) even though it never speeds up filth cleaning.
   - Fix: when no job is active, have `ExplanationPart` return a conditional line such as "Mop: +60% (wet filth only)".
   - Size: small.

4. **The glittervacuum text contradicts a hidden penalty.** `SpecialtyExplanationStackClear` says "per-tile cleaning speed is unchanged", but `StatPart_AreaCleanerSilentSpeed.cs:19-25` subtracts 50% and shows nothing. The tool also has no Cleaning speed line at all, so players can't compare it with other tools.
   - Fix: reword the key and give the penalty an explanation line.
   - Size: small.

5. **The area gizmo text is wrong, and picking an area outside Home fails silently.** `CleaningArea.Desc.None` says "anywhere on the map", but vanilla only cleans inside the Home area, and the picker lists only allowed-areas (`CompCleaningAssignment.cs:189`). A janitor restricted to an area outside Home cleans nothing.
   - Fix: change the text to "anywhere in the home area", and warn in the tooltip when the chosen area doesn't overlap Home.
   - Size: small.

6. **Filtered filth disappears from right-click with no reason.** The postfix sets `__result = false` without a `JobFailReason` (`Patch_CleaningAreaAssignment.cs:68`), and the "Clean room" option drops filth silently (`:102`).
   - Fix: call `JobFailReason.Is("JanitorsCloset.OutsideAssignment".Translate())`.
   - Size: small.

7. **Research nodes overlap vanilla nodes** (vanilla nodes are 140x50 px, spaced 190/100 px per unit):
   - Housekeeping (0.5, 1.2) overlaps Stonecutting and Pemmican.
   - AtomicSanitation (9, 1.2) overlaps TubeTelevision and PackagedSurvivalMeal.
   - ToxicCleanup (5.5, 2.2) overlaps AirConditioning.

   Fix: move the nodes to free cells in `ResearchProjects_Janitor.xml:14-15,57-58` and `ResearchProjects_HazmatSprayer.xml:23-24`. Size: small.

8. **The filth category is invisible.** The player can't tell whether a filth counts as Wet or Dry (`FilthCategoryResolver.cs`).
   - Fix: add a `SpecialDisplayStats` entry "Cleaning category: Wet/Dry" for filth defs, reusing `Patch_AddCleaningSpecialtyEntry`.
   - Size: small.

9. **The assignment lives on the tool and only shows while it's equipped.** Players can't see it on a tool lying in a stockpile, and it changes silently when the pawn swaps tools (`CompCleaningAssignment.cs:70`, `Patch_CleaningAreaAssignment.cs:40`).
   - Fix: add `CompInspectStringExtra` (and optionally a label suffix such as "mop (indoors)").
   - Size: small.

10. **There are no gameplay settings, and players see the debug toggles.** Possible toggles:
    - the mop splash mood and opinion thoughts (-3 opinion of the janitor, `Thoughts_Janitor.xml:113-124`)
    - the freshly cleaned and tracked-filth thoughts
    - mop marks and VFX

    Fix: add those toggles and show the diagnostics only when `Prefs.DevMode` is on. Size: medium.

11. **Tools share the stockpile filter with combat weapons.** They inherit `WeaponsMelee` (vanilla `BaseWeapons.xml:118-120`).
    - Fix: add a `CleaningTools` ThingCategoryDef so players can stock them by the cleaning room.
    - Size: small.

12. **The mod page doesn't explain how to use the mod.** `About.xml:10-15` names no tools, research, gizmos or add/remove safety. Adding the mod mid-save is fine (the MapComponent is created on load). Removing it leaves missing-def and missing-class errors for tools, apparel, marker filth, thoughts and `MapComponent_FreshlyCleanedRooms`.
    - Fix: add a short "How it works" section and "Safe to add; removing logs errors".
    - Size: small.

13. **Translation leftovers are minor.** The `Thought.FreshlyCleanedRoom.*` keys in `Keys.xml:27-28` are never used, because the thought text lives in the def. Stat explanations build "label: -X%" by concatenation (`StatPart_CleaningToolMatch.cs:32-33`), which fixes the word order for translators. Everything else player-facing is keyed.
    - Fix: delete the dead keys and use a keyed format string with `{0}`.
    - Size: small.

I left the gizmos themselves alone: the cycle and area tooltips are clear, they're keyed, and the area preview on hover matches vanilla.
