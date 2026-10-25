# Contributing to Janitor's Closet

Thanks for helping. This guide covers setting up, the conventions the code follows, and how to show a change works.

## Setting up

1. Install RimWorld 1.7, the .NET 10 SDK and the [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) mod.
2. Clone this repo. The build finds RimWorld through Steam; for other installs, set `RIMWORLD_DIR`.
3. Clone [Archotech Harness](https://github.com/AW-Koi/archotech-harness) next to this repo, or set `ARCHO_HOME` to it, for the in-game scenarios.
4. Build and test:

   ```
   dotnet build Source/JanitorsCloset
   dotnet test Source/JanitorsCloset.Tests
   ```

5. Link `mod/` into RimWorld's `Mods` folder to play with your build, as the [README](README.md#running-it-in-rimworld) describes.

Open the repo folder in your editor, so it loads `JanitorsCloset.sln` with the mod, the tests and the scenarios.

## Showing a change works

- **Logic** that runs without the game gets a unit test in `Source/JanitorsCloset.Tests`. Write the failing test first.
- **Gameplay** gets a scenario in `harness/` that sets up the situation, lets the game run and checks the outcome. Put it in the folder for its feature, such as `harness/Cleaning/`, and start each block with a one-line comment saying what it sets up or checks. `Scenarios.csproj` declares the mod once, so scenarios need no `[Mod]` attribute. A scenario that checks compatibility with another mod goes in `harness/Compatibility/` with `[RequiresMod("package.id")]`; it skips on machines without that mod. Run them with `<archotech harness>/archo test harness --workers auto`. For a bug fix, show that the scenario fails without the fix.
- **Warnings**: clear every ReSharper InspectCode warning in the files you touch. `dotnet build` doesn't report them:

  ```
  jb inspectcode JanitorsCloset.sln --no-build --severity=WARNING --output=inspect.sarif
  ```

## Code conventions

- One top-level type per file, named after the type.
- Reference defs through `DefOf` classes (`ThingDefOf.Filth_Blood`, `JanitorDefOf.Janitor_Mop`), never by defName string. Add a field to a `[DefOf]` class when one is missing.
- Keep per-tick work off pawns that aren't cleaning, and allocate nothing per tick.
- A comment labels a block in one short line. Design reasoning belongs in the commit message.
- Player-facing text lives in `mod/Languages/English/Keyed/` or in def fields, never in C# strings.
- Don't hard-code paths or anything specific to your machine.

## Defs and content

- Defs and patches go in `mod/Common/`. DLC-only content goes in that DLC's folder, such as `mod/Common/Biotech/` or `mod/Common/Anomaly/`, which `LoadFolders.xml` loads only with the DLC. `MayRequire` on a patch operation doesn't stop it running.
- Thought labels are lowercase, and memory descriptions are in the past tense, as in vanilla.
- Art and sound sources go next to the code in `Source/JanitorsCloset/Art` and `Source/JanitorsCloset/Sound`. Shipped textures and sounds go under `mod/`.

## Writing

Write prose in British English, in the active voice. That covers commit messages, comments and docs. Commit subjects are short imperatives, such as "Keep the clicked filth in a forced cleaning order", with the reasoning in the body.

## Sending a change

1. Fork the repo and create a branch from `main`.
2. Open a pull request against `main` and fill in the template.
3. The maintainer reviews it. Resolve every review conversation and push fixes to the same branch.
4. The maintainer squash-merges the pull request, so its title becomes the commit subject on `main`.

Nobody pushes to `main` directly.

## Licence

By contributing, you agree your code is released under [Apache 2.0](LICENSE), and your art, audio and written text under [CC BY 4.0](LICENSE-ASSETS), as the [NOTICE](NOTICE) describes.
