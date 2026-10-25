# Joy and feel review

Commit read: `1ead39f` (origin/main).

**Deviations:** none. I read origin/main at commit 1ead39f in the read-only scratchpad worktree, and made no edits and no commits.

## What already sparks joy

- **Each tool has its own fantasy.** The straw broom's 18° wobble differs from the push broom's long 0.22-tile slide, and the mop has a deep speed swing (Weapons_Janitor.xml animProfiles). Brooms throw a dust puff on each stroke (Patch_BroomDustPuff.cs), and the mop leaves short-lived damp patches.
- **The glittervacuum is a real endgame toy.** It has a breathing cyan glow, cyan puffs that float off as it clears a 3x3 patch, and a custom sound. The description ("Despite the name, it doesn't suck") and the "atomic sanitation" research text match vanilla's dry humour.
- **The HAZMAT sprayer reads clearly from across the map.** It shows a tank on the pawn's back, foam that lingers for hours and spray puffs. Its +300% bonus applies only to pollution work, which makes it a specialist and never a must-have.
- **Splash and decontaminant thoughts are good comedy.** A small mood hit plus an opinion hit against the janitor gives the janitor a social presence, and the -1/-3 sizes fit vanilla's scale.
- **The flavour text is consistently good.** "Puddles of unspecified origin" and "swing the dirty end at the problem" are two examples.
- **The info card explains the specialties** on hover (Patch_AddCleaningSpecialtyEntry.cs).

## What falls flat

1. **The good moments are mostly hidden, and the negative ones show.** The only positive thought is Janitor_FreshlyCleanedRoom at +1 (Thoughts_Janitor.xml). For scale, vanilla's AteFineMeal is +5. It only appears once a room has zero filth, and any new filth removes it (MapComponent_FreshlyCleanedRooms.cs). Busy rooms get dirt tracked in constantly, so the bonus rarely lasts.
   - Every time that happens, Patch_TrackedFilthIntoCleanRoom.cs gives -2 to whoever stands on the cell, including a patient bleeding in a hospital bed.
   - All the janitor's own social footprint is negative (-3/-5 opinion), so a dedicated janitor can lower colony mood overall. That works against pillar 1.
2. **The janitor gets nothing personally.** TryAward skips anyone holding a tool, and no thought, tale or notification credits their work. The README's line "you'll miss them when they're not around" has no mechanic behind it.
3. **The fresh-room moment has no on-map payoff.** A room becoming spotless produces no mote, sparkle or sound, so the main feedback loop happens only inside the needs tab.
4. **The glittervacuum card contradicts its behaviour.** The specialty text (Keys.xml, SpecialtyExplanationStackClear) says "The per-tile cleaning speed is unchanged". StatPart_AreaCleanerSilentSpeed.cs actually halves it, and the card doesn't show this. On a sparse floor the endgame tool feels slower than bare hands, with no explanation.
5. **Pillars 3 and 4 don't ship.** There are no wet floor signs, mess detection, slipping or buildables, and Things.xml is still a TODO. Mop marks last about 25 seconds at 1x and have no effect on anything.
6. **The speed gains are real but hard to feel.** The full kit plus a mop gives about 2x on matching filth. Cleaning jobs are short, so players notice where a janitor works far more than how fast.
7. **Brooms and the mop reuse vanilla sounds**, so only the two late-game tools have their own audio.
8. **Some smaller tone and friction issues:**
   - The "HAZMAT sprayer" label and the "HAZMAT cleaning tool" key are in capitals, which breaks vanilla's lowercase labels.
   - The straw broom needs 5 hay, and a tribal start often has none on day one.

## Ranked joy improvements

1. **(Small) Celebrate the spotless moment.** When a room is stamped clean, show a text mote over the janitor ("Spotless!") and a few glint flecks, and give the janitor a "pride in my work" thought (+3, refreshed while rooms stay clean). This makes pillar 1 visible.
2. **(Small) Swap the tracked-filth guilt for janitor grumbling.**
   - Drop the -2 on the tracker.
   - Give the janitor a small social "tracked mud on my clean floor" opinion against them.
   - Raise the fresh-room thought to +2 or +3.
   - Skip downed or bedridden pawns.

   Galoshes reducing FilthRate then pays off as a joke too.
3. **(Small) Make the glittervacuum honest.** Show the per-tile penalty on the card, or fix the text so it matches. A visible trade-off is more satisfying than a hidden one.
4. **(Medium) Wet floor signs.** Damp-floor patches already exist. A janitor carrying a mop drops a sign on a cluster of them, other pawns path around it, and anyone who walks across risks a comic slip (a brief stumble and "slipped on a wet floor"). This is the README's signature promise and the strongest source of stories.
5. **(Medium) A janitor's closet buildable.** A small locker where tools are stored, which adds a short speed boost after restocking and a "well-stocked closet" thought for the janitor. It gives pillar 4 a home base that players will build around.
6. **(Small) Mess-cleared tale and art.** Add a tale when a janitor clears a large mess after a raid, so sculptures can show "Mira heroically mopping the hospital". This is cheap and memorable.
7. **(Small) Unique swish and squelch sounds for the brooms and mop**, so the early game has audio identity as well.
8. **(Large) Detect large messes after raids.** The janitor flags the area, raises its cleaning priority and announces it with a letter or message. This delivers "you only see their real work once they stop".
9. **(Small) Tone fixes.** Use lowercase "hazmat" in the labels, and offer a hay-free recipe for the straw broom.
