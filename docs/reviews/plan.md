# Plan across all six reviews

Each finding is ranked by how much it adds to the playthrough and to pillar 1 ("a janitor feels impactful and noticeable") for its effort. Tier 1 is the 20% that delivers most of the value, and it's what I'd finish before you start playing.

## Product lens on the performance work

The performance changes must not mute the janitor. Where cost and visibility pull against each other, visibility wins as long as the cost is bounded by the number of janitors rather than by colony size.

- **Janitors keep their full search.** The behaviour players notice is a mop crossing half the base to reach the blood. Only pawns holding a single-category tool pay for that search, usually one to three pawns. Everyone else goes back to vanilla's search. (Decision 1: keep the 40-tile rule.)
- **One small allocation per cleaning job is fine.** Moving the hooks into the cleaning job costs one closure per job and none per tick. (Decision 2: yes.)
- **The info card shows what the tool actually does.** Outside a job, the card shows each bonus with its condition, for example "Mop: +60% (wet filth)". During a job, it shows the bonus that's live. (Decision 3: yes, together with QoL #3.)
- **Two performance fixes make janitors more visible.**
  - The tick-cadence fix (correctness #3) means hazmat foam and spray appear for every sprayer, on-screen or off.
  - Taking mop marks out of the filth list keeps the marks on the floor, but other pawns stop treating them as work.

## Tier 1: before you play

| # | Item | Streams | Size | Why it's here |
|---|---|---|---|---|
| 1 | Run the full search only for pawns whose tool prefers a category | Perf 1, Correctness 7 | S | It's the largest cost in a late game. It also restores vanilla's search for everyone else. |
| 2 | Move the per-tick hooks into the cleaning job and use real tick deltas | Perf 2 and 7, Correctness 3 | M | Non-cleaning pawns run none of the mod's code per tick. It also fixes foam and spray disappearing off-screen. |
| 3 | Check the log setting before building log arguments | Perf 3 | S | Removes the per-tick allocations. |
| 4 | Keep the clicked filth in a forced cleaning order | Correctness 1 | S | A player order the mod currently overrides. |
| 5 | Key clean-room stamps by cell, and skip the filth-spawn patch on load | Correctness 2, Perf 9 | S-M | Without it, the core feedback loop breaks on every reload. |
| 6 | Let pacifists equip cleaning tools | QoL 1 | S-M | The colonists most likely to be janitors can't hold a mop. |
| 7 | Rebalance the mood thoughts so a janitor helps colony mood overall | Joy flat 1, Joy 2 | S | Today a dedicated janitor can make the colony unhappier, which contradicts the vision. Numbers below. |
| 8 | Celebrate a spotless room: a "Spotless!" mote and glint over the janitor, plus a pride thought for the janitor | Joy 1 | S | The cheapest way to make a janitor noticeable in a busy colony. |
| 9 | Show the glittervacuum's real speed: correct the text and add an explanation line | Joy 3, QoL 4 | S | The endgame tool currently misleads the player. |
| 10 | Move the research nodes so they don't overlap vanilla ones | QoL 7, Correctness 4 | S | Visible on day one. |
| 11 | Give opinion penalties a per-pawn stack limit | Correctness 8 | S | A one-line XML fix. |

## Tier 2: soon after, cheap polish

- **Performance:**
  - #4: cache the area restriction.
  - #5: merge the `ThinFilth` patches.
  - #6: keep marker filth out of the filth list.
  - #8: cache the map component.
  - #10: compare cleaning sounds by reference instead of by name.
  - #11: cache the gizmo labels and icons.
  - Add the allocation probe as a regression guard.
- **QoL:**
  - Give filtered filth a failure reason.
  - Make the area text say "home area".
  - Apply the area picker to every selected janitor.
  - Hide the cleaning gizmos on non-colonists.
  - Show each filth's category on its info card.
  - Show a tool's assignment in its inspect string.
  - Add a `CleaningTools` stockpile category.
  - Add gameplay toggles, and show the debug options only in dev mode.
  - Remove the dead translation keys.
- **Content (small XML and stat work):**
  - Fix the misclassified filth: machine bits and fleshmass.
  - Scale the cleaning bonus with tool quality.
  - Add a cleaning inspiration.
  - Add janitor backstories.
  - Lowercase "hazmat" in labels and keys.
  - Offer a straw broom recipe that doesn't need hay.
  - Add a Progression: Education patch for the sprayer.
  - Fix the glittervacuum's missing bonus for toxic and weather work.
  - Rewrite the galoshes description, or keep the grip claim for a later slip mechanic.

## Tier 3: build order

The design is agreed in `design-notes.md`. Build in this order:

1. **Janitor's cart (done):** the item, ownership, the push job, parking cell choice, drawing it in front of the pawn and -15% push speed. Art is in `Source/JanitorsCloset/Art/Janitor_Cart_*.svg`.
2. **Mop charge and the closet sink (done, except Dubs Bad Hygiene):** dunking at the cart, refilling at the sink, Dubs Bad Hygiene plumbing, and the 4-hour return home to the sink.
3. **Glittervacuum dock:** capacity, the empty job, wastepacks under Biotech, and the reworded specialty text.
4. **Simple Sidearms compat:** auto-switch to the matching tool for cleaning toils.
5. **Audio and art:** broom and mop sounds, a janitor cap, the sprayer masks.

Wet floor signs and slipping are parked until a sign design works. Large-mess detection is dropped.

## Decisions for you on Tier 1

1. **Mood numbers (item 7).** My proposal:
   - Raise the freshly cleaned room thought from +1 to +2.
   - Stop giving the -2 tracked-filth thought to downed pawns or pawns in bed.
   - Keep the -2 for everyone else, so it's still a small joke at the tracker's expense.
   - Give the janitor a +3 pride thought for a spotless room (item 8).
2. **Pacifist tools (item 6).** Vanilla already stops pawns incapable of violence from attacking, so the patch only has to let them equip things that carry a `CleaningToolExtension`. No other weapon is affected.
