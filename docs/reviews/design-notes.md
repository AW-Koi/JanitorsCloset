# Tier 3 design

Agreed on 2026-10-25. The janitor's closet becomes a resupply hub: the janitor has to return to it to keep working well. Each loop follows vanilla patterns, so it feels like part of the base game and gives the player new things to build and manage.

## Janitor's cart

The cart is a resupply vehicle for the mop. The janitor wheels it around the colony, refills the mop from it, and takes it back to the closet to change the water. Sanitation (industrial) unlocks it.

### What the cart is

- An item, not a building. Vanilla can't carry a building without minifying it, and carrying an item is well-tested code.
- `alwaysHaulable` is off, so `EverHaulable` is false (`ThingDef.cs:446`) and haulers leave it alone.
- It has a path cost, like a chunk, so pawns route around a parked cart. It never makes a cell impassable.
- It's crafted at a bench, so it isn't stuff-tinted.
- Its art has north, east and south views (`Janitor_Cart_*.svg`), and west mirrors east. When parked, it takes the janitor's facing. Check that rotation draws correctly for an item.
- Each cart is assigned to one janitor, like bed ownership. A janitor holding a mop claims an unassigned cart.
- Raiders can steal it.

### Carrying only during a push

The cart is on the ground unless a "push cart" job is running. During that job it sits in the vanilla carry tracker. Vanilla already drops a carried thing when the job ends (`Pawn_JobTracker.cs:510`), when the pawn is drafted (`Pawn_DraftController.cs:57`), or when the pawn is downed or dies (`Pawn.cs:2909`). An interrupted push therefore leaves the cart parked where the janitor stood, and there's no state to recover.

- Pushing applies -15% move speed.
- Vanilla hides the equipped weapon while a pawn carries something, so the mop isn't drawn during a push. `Patch_CarryWeaponOpenly` must keep allowing this.
- A draw patch puts the cart on the ground in front of the pawn, and behind them when they face north. Vanilla would draw it at hand height.

### A cleaning run

Vanilla hands out one clean job per cluster: up to 15 filth within about 5.6 cells of the first target, all in the same room (`WorkGiver_CleanFilth.JobOnThing`).

1. If the janitor holds a mop and their cart is outside the cluster's room, or more than about 12 cells away, a push comes before the clean. This is decided once, when the job is given out, so the cart has no tick cost.
2. Push: the janitor walks to the cart, takes hold of it, wheels it to a parking cell and sets it down.
3. Clean: the vanilla clean job runs unchanged. When the mop is spent, the janitor steps back to the cart to dunk it.
4. The next nearby cluster reuses the parked cart.

Forced orders (the player's right-click "Clean") skip the push.

### Choosing a parking cell

This is pure logic and gets unit tests.

- Never a door cell: an item in a door cell holds the door open (`Building_Door.BlockedOpenMomentary`).
- Never on an item, a building or a stockpile cell.
- Prefer a cell next to a wall, close to the cluster, on the side the janitor came in from.
- If no valid cell is reachable, skip the push and clean without the cart.

### Mop charge and water

- The mop holds a charge, which goes down with each wet filth it cleans. A spent mop cleans as if the janitor had no tool.
- The cart holds a set number of dunks. Each dunk refills the mop and dirties the water.
- When the cart is spent, the next job is to push it home to the closet sink to empty and refill it.
- Without a cart, the janitor walks to the sink to wet the mop.
- The numbers get tuned in game with the harness.

### Closet sink

- A utility sink that refills carts and mops.
- Without Dubs Bad Hygiene, it always works.
- With Dubs Bad Hygiene, it needs plumbing and draws water from the network. DBH's API isn't checked yet.

### Bringing the cart home

A cart left unused for 4 in-game hours away from home gets a low-priority cleaning job to return it. Any cleaner can take it, so carts whose owner died or stopped cleaning still go home.

### Failure modes

| Event | Result |
| --- | --- |
| Drafted, downed or killed mid-push | Vanilla drops the cart. It stays parked there |
| Push path blocked mid-route | The cart is dropped where the janitor stands |
| Cart unreachable (locked door, fire) | No push. The janitor cleans with the mop's remaining charge |
| Janitor swaps the mop for a broom | The cart stays parked until the idle return |
| Raid | The cart can burn, be smashed or be stolen |

## Glittervacuum dock

- The vacuum fills as it cleans. When full, the janitor empties it at a powered 1x1 dock whenever they can. If they can't, they clean as if they had no tool.
- The dock produces vanilla wastepacks, which the player manages like any others. Neglected wastepacks pollute, and the hazmat sprayer cleans pollution.
- Without Biotech there are no wastepacks, so the vacuum has no capacity and works forever.
- The 50% per-tile penalty stays in all games, because it's the price of the 3x3 clear. Reword the specialty text around when to use the tool: "Best on heavy mess: clears up to nine tiles per pass, but a single spot cleans at half speed."

## Simple Sidearms

Checked against the installed copy (workshop 927155256, v1.6 assembly).

- Already works: SS counts anything with `equippedStatOffsets` as a tool, so janitors, pacifists included, can carry a cleaning tool as a sidearm.
- Doesn't work: auto-switching. SS takes its stats from the job's `RecipeDef.workSpeedStat` or the toil's active skill. Cleaning has neither, so SS never switches for it.
- Fix: a compat assembly in an `IfModActive` folder. When a cleaning toil starts, it picks the tool with `PawnToolPreference`, calls SS's `equipSpecificWeaponFromInventory`, and sets SS's auto-tool memory so SS swaps back afterwards.
- SS declares support up to 1.6 only, so 1.7 support may have to wait for an SS update.

## Parked

- Wet floor signs and slipping. The idea is a rare stumble with a bruise, worse for older pawns, which raiders don't avoid. No design for the signs works yet.

## Dropped

- Large-mess detection: it's too intrusive in normal play.
- Auto tool swapping in vanilla: a pawn holds either a weapon or a tool.
- A hazmat sprayer refill: too much micromanagement for a niche tool.
- A mop bucket building: the cart replaces it.
