# RimWorld Mod Template

A minimal, reusable skeleton for a RimWorld 1.6 mod with Harmony, ModSettings, and a build that drops the DLL straight into `1.6/Assemblies/`.

## Quick start

1. **Copy this folder** to a sibling location, e.g. `../MyNewMod/`.
2. **Rename** the source folder and files inside `Source/`:
   - `Source/__ASSEMBLY__/`              → `Source/MyNewMod/`
   - `Source/MyNewMod/__ASSEMBLY__.csproj` → `MyNewMod.csproj`
   - `Source/MyNewMod/__ASSEMBLY__.cs`     → `MyNewMod.cs`
3. **Find-and-replace across the whole folder** (Rider/VS Code: replace-in-files):

   | Sentinel          | Replace with                                  | Example                  |
   | ----------------- | --------------------------------------------- | ------------------------ |
   | `__MODNAME__`     | display name (spaces & punctuation OK)        | `Janitor's Closet`       |
   | `__AUTHOR__`      | your handle                                   | `Synergy`                |
   | `__PACKAGEID__`   | unique id, lowercase, dot-separated, no spaces | `synergy.janitorscloset` |
   | `__DEFPREFIX__`   | short uppercase prefix for every defName      | `JANITOR`                |
   | `__ASSEMBLY__`    | C# identifier; matches the renamed folder/files (no spaces, no punctuation) | `JanitorsCloset`         |

   The replace is case-sensitive and content-only: file/folder names use `__ASSEMBLY__` so step 2 handles those manually.
4. **Add art** to `About/`:
   - `Preview.png` (640×360 recommended)
   - `ModIcon.png` (32×32 recommended)
5. **Build** the project (`dotnet build` from `Source/MyNewMod/`, or via Rider/VS). The DLL outputs directly to `1.6/Assemblies/MyNewMod.dll`.
6. **Symlink or copy** the mod folder into `RimWorld/Mods/` (or your local Steam Workshop dir) and enable it in the in-game mod list.

## Folder layout

```
__ModTemplate__/
├── About/
│   ├── About.xml              metadata (name, author, packageId, deps)
│   └── (Preview.png, ModIcon.png — add your own)
├── 1.6/
│   ├── Assemblies/            built DLL lands here (gitignored)
│   ├── Defs/Things.xml        ThingDefs, RecipeDefs, etc.
│   └── Patches/Patches.xml    XML PatchOperations
├── Languages/English/Keyed/Keys.xml
├── Source/__ASSEMBLY__/
│   ├── __ASSEMBLY__.cs        Mod, ModSettings, Harmony bootstrapper
│   └── __ASSEMBLY__.csproj    net472, refs Assembly-CSharp + UnityEngine, Harmony 2.3.3
├── Textures/                  PNGs referenced by texPath in Defs
├── LoadFolders.xml            tells RimWorld to load both / and 1.6/
├── .gitignore
└── README.md
```

## Conventions

- **Every defName** starts with `__DEFPREFIX___` (prefix + underscore) to avoid collisions with vanilla and other mods.
- **Every translation key** in `Languages/.../Keys.xml` also starts with `__DEFPREFIX___`.
- **Harmony id** uses the packageId (`__PACKAGEID__`), so it's automatically unique.
- **DLL output** goes straight to `1.6/Assemblies/` — no manual copy step.

## Adjusting the Steam path

The `.csproj` references RimWorld DLLs at `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\`. If your install lives elsewhere, update the five `<HintPath>` entries in `__ASSEMBLY__.csproj` (or set up a `Directory.Build.props` if you want one source of truth across mods).

## Targeting other RimWorld versions

This template targets 1.6. To support 1.5 alongside:
- Add a `1.5/` folder mirroring `1.6/`.
- Add a `<v1.5>` block to `LoadFolders.xml`.
- Add `<li>1.5</li>` to `<supportedVersions>` in `About.xml`.
- Either build twice with different references, or use `MayRequire`/version checks in code.
