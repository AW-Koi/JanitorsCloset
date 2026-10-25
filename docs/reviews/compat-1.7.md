# Janitor's Closet on RimWorld 1.7: compatibility audit

**Deviations from the brief:** none in scope. I compiled a copy of the source in scratch (`C:/Users/Synergy/AppData/Local/Temp/claude/D--Modding-RimWorld-JanitorsCloset/0af785ee-0382-44d0-8bcb-b403a0da9da5/scratchpad/compat/build`) against the 1.7 binaries. That build doesn't touch the repo or the worktree. I also read the existing 1.7 Player.log and ModsConfig.xml from LocalLow, read-only. **Commit read:** origin/main 1ead39f.

**Main finding:** the mod already loads and patches on 1.7. Player.log from 2026-10-13 shows the current 1.6 net472 DLL running on build 1.7.4976 rev1043 with Biotech, Holsters and Progression: Education active:
- "[Janitor's Closet] Harmony patches applied."
- "Holsters compatibility patch applied."
- Both Progression: Education overrides applied.

The log has no errors or warnings that name Janitor, and a save loaded in that session. That session was on build 4976, though, and the install is now **1.7.4985 rev1269**. Nothing has been run on 4985 yet.

## 1.7 baseline

- **Runtime:** CoreCLR .NET 10 (`RimWorld.runtimeconfig.json`: tfm net10.0, "10.0.13-waylithclr.dev"). Assembly-CSharp itself targets net10.0 and references `UnityStubs`.
- **Unity types:** they live on in **UnityStubs.dll**, not Waylith.UnityCompat (that one only holds 5 internal Waylith types). The runtime config redirects `UnityEngine*=UnityStubs` and sets `LaxAssemblyBinding`, `LaxMemberSignatures` and similar options. That shim is why the old net472 DLL binds.
- **References for a native build:** `Assembly-CSharp.dll`, `UnityStubs.dll` and `Waylith.dll` from the install root, plus 0Harmony.
- **Harmony:** the brrainz.harmony workshop mod 3.0.1.0 has a `1.7/` load folder with 0Harmony **3.0.0-rc.3**, net10.0, namespace still `HarmonyLib`. NuGet has `Lib.Harmony 3.0.0-rc.3`. Every Harmony API the mod uses exists in v3 with the same signature (`Harmony(string)`, `PatchAll()`, `Patch(...)`, `HarmonyMethod(MethodInfo)`, `AccessTools.*`, the `HarmonyPatch` constructors, `MethodType.Getter`, `Priority.Low`).
- **Compile check:** the unchanged source compiles against net10.0 + 1.7 Assembly-CSharp + UnityStubs + Harmony 3 with **0 errors and 0 warnings**. So every engine-facing API resolves with a compatible signature.
- **What I can't establish:**
  - Runtime behaviour on build 4985, because I didn't launch the game.
  - Whether the Waylith lax-binding shims stay in future builds. They're "dev" runtime flags with no documented guarantee.

## Findings, most severe first

| Severity | Where | 1.7 evidence | Minimal fix |
|---|---|---|---|
| Blocks building | `Source/JanitorsCloset/JanitorsCloset.csproj:3,14-33,37` | `RimWorldWin64_Data/Managed` doesn't exist. Assembly-CSharp is net10.0. Lib.Harmony 2.3.3 targets net472. | Target net10.0. Reference `Assembly-CSharp.dll`, `UnityStubs.dll` and `Waylith.dll` from the install root. Use `Lib.Harmony 3.0.0-rc.3` with `ExcludeAssets="runtime"`. Output to `mod/1.7/Assemblies`. |
| Load risk (rests on shims) | the built `mod/1.6/Assemblies/JanitorsCloset.dll` | It loads today only through `WaylithClr.AssemblyNameRedirects` and the Lax* options. It referenced 0Harmony 2.3.3.0, and 3.0.0.0 was the one loaded. | Ship a net10 build for 1.7. Keep the net472 build for 1.6. |
| Cosmetic (mod list) | `mod/About/About.xml:7-9` | `ModMetaData.VersionCompatible` checks `supportedVersions`. Without 1.7 the mod shows as incompatible and `Page_ModsConfig:875` includes it in "unsubscribe incompatible". | Add `<li>1.7</li>`. |
| Works (by fallback) | `mod/LoadFolders.xml` | `ModContentPack.InitLoadFolders` falls back to the highest defined version at or below the current one, so v1.6 loads. | Add a `<v1.7>` block (`/`, `1.6`, `1.7`, plus the gated Biotech and PE folders) so the net10 DLL loads from `1.7/Assemblies`. |
| Works, unverified at runtime | `Cleaning/CompCleaningAssignment.cs:139-170` (`Graphics.Blit`, `RenderTexture.GetTemporary/active`, `Texture2D.ReadPixels/GetPixels/SetPixels/Apply`) | The UnityStubs versions are real code, not throwing stubs. `ReadPixels` falls back to a single-pixel read when `CameraTextureRenderer.TryReadPixels` fails. | Check the tinted area gizmo background in game. If it renders black, tint with `GUI.color` instead of baking a texture. |

**Harmony patch targets, all present in 1.7 with the same signatures and parameter names:**
- `JobDriver.DriverTickInterval(int)` and `DriverTick()`
- `PawnRenderUtility.DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)`, `DrawEquipmentAndApparelExtras(Pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags)`, `CarryWeaponOpenly(Pawn)` and `AltitudeForLayer`
- `WorkGiver_Scanner.Prioritized` (getter) and `GetPriority(Pawn, TargetInfo)`
- `WorkGiver_CleanFilth.HasJobOnThing` and `JobOnThing` (`pawn`, `t`)
- `Pawn_EquipmentTracker.GetGizmos`
- `FloatMenuOptionProvider_CleanRoom.GetRoomFilthCleanableByPawn(Room, Pawn pawn)` (private static)
- `Sustainer(SoundDef, SoundInfo)`, still called from `SoundStarter:106`
- `Filth.ThinFilth()` and `SpawnSetup`
- `JobDriver_CleanFilth.MakeNewToils`
- `EffecterDef.Spawn`, `SpawnMaintained` and `SpawnAttached`
- `Thing.SpecialDisplayStats`

The vanilla bodies the patches depend on are unchanged:
- `ThinFilth` destroys the filth at thickness 0.
- The clean, pollution and snow jobs still use `PlaySustainerOrSound(Interact_CleanFilth)` and `EffecterDefOf.Clean`.
- `ClearPollution` and `ClearSnowAndSand` still scale by `GeneralLaborSpeed`.

Tick lists aren't threaded, so the `[ThreadStatic] Current` driver tracking stays valid. The Harmony runtime config sets `JitPatchableAssemblies: Assembly-CSharp`, so small targets such as `CarryWeaponOpenly` and the `Prioritized` getter stay patchable.

**Engine APIs, all present in UnityStubs and Assembly-CSharp:**
- `Graphics.DrawMesh(mesh, Matrix4x4, Material, int)` and `(mesh, Vector3, Quaternion, Material, int)`
- `Matrix4x4.TRS`, `Quaternion.AngleAxis`, `MeshPool.plane10/plane10Flip`
- `ShaderDatabase.CutoutComplex`, `GraphicDatabase.Get<Graphic_Single/Graphic_Multi>`, `MatSingleFor/MatAt/MeshAt`
- `FleckMaker.ThrowDustPuff/ThrowDustPuffThick/Static/GetDataStatic`, `map.flecks.CreateFleck`
- `Effecter.children`, `Command.BGTex`, virtual `BGTexture` and `BGTextureShrunk`
- `Color32`, `Mathf`, `Listing_Standard`, `Text.Font`
- `Scribe_Values` and `Scribe_Collections`, `TryGainMemoryFast` and `TryGainMemory`, `FilthMaker.TryMakeFilth`

The compile check backs all of these.

**XML side:** everything the defs reference exists in the 1.7 assembly or core data:
- Classes: `Graphic_Single`, `Graphic_Random`, `Graphic_Fleck`, `FleckSystemThrown` and `FleckSystemStatic`, `Thought_MemorySocial`, `AudioGrain_Clip`, and the `PatchOperation*` classes.
- Shader types: `ShaderTypeDef` Cutout, CutoutComplex and MoteGlow in Data/Core.
- SoundDef: `sustainFadeoutTime` (SoundDef.cs:49).
- Altitude layers: MoteLow and MoteOverhead.
- Parent defs: `ApparelMakeableBase`, `BaseFilth` and `BaseMeleeWeapon_Blunt_Quality` in Core.
- Patched stats: `CleaningSpeed` and `GeneralLaborSpeed`.
- Vanilla sounds: `Interact_CleanFilth_Dirt` and `_Fluid`.
- Asset formats: the loader still accepts .png and .ogg.

**Not my lens, but noticed:** the worktree has no `mod/1.6/Assemblies`. Build output is gitignored, so the published layout depends on a local build.

## Migration checklist

1. Retarget a 1.7 build: net10.0, references to `Assembly-CSharp.dll`, `UnityStubs.dll` and `Waylith.dll` in the install root, `Lib.Harmony 3.0.0-rc.3` (runtime excluded), output to `mod/1.7/Assemblies`. Keep a net472 configuration for 1.6 if you still support it.
2. Add `<v1.7>` to LoadFolders.xml (`/`, `1.6`, `1.7`, plus the gated `1.6/Biotech` and `1.6/ProgressionEducation`).
3. Add `<li>1.7</li>` to `supportedVersions` in About.xml.
4. Launch on 1.7.4985 with Harmony 3.0.1. Confirm the "patches applied" log line, then check the five places where engine behaviour could differ:
   - the tool swing animation
   - the Hazmat wand and tank draw
   - the dust, glitter and spray flecks
   - the swapped cleaning sustainer
   - the tinted area gizmo background
5. Then run InspectCode as usual.
