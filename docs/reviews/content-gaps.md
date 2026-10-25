# Content gaps review

Commit read: `1ead39f` (origin/main).

**Deviation:** I didn't deviate from the brief. I read origin/main at commit 1ead39f in the read-only checkout and edited nothing. I checked vanilla and DLC defs under the Steam `Data` folder. Nothing was tested in-game, so the two items marked "verify" are code-reading conclusions.

## Pillar inventory

**1. A noticeable janitor (partly built)**
- Coveralls, galoshes and rubber gloves, with worn art for every body type.
- Per-tool sweep animations and custom sounds for the glittervacuum and sprayer.
- Visible marks while working: mop damp marks, decontaminant foam and the glittervacuum glow.
- Thoughts: freshly cleaned room (+1), splashed by mop (-1 mood, -3 opinion), doused in foam (-3, -5), tracked filth into a clean room (-2).
- Nothing identifies "the janitor" as a role. There's no headgear, backstory, inspiration or title.

**2. Gear with cleaning buffs (strongest pillar)**
- Tiers run straw broom (neolithic, dry) to push broom and mop (industrial, dry and wet) to the HAZMAT sprayer (Biotech, pollution, x3) to the glittervacuum (spacer, all categories, 3x3 clear).
- Wet or dry is inferred from each filth's cleaning sound. Weather buildup and pollution use separate stat parts.
- Three research projects: Housekeeping, Sanitation and Atomic Sanitation, plus Toxic Cleanup with a VAE reroute.

**3. Autonomous behaviour (scaffolding only)**
- A janitor holding a tool prefers filth its tool is suited to and reorders the queue to match.
- Each tool has indoor/outdoor and area-restriction gizmos.
- Wet-floor signs and large-mess detection aren't built. The mop-mark comment says it was defined for them.

**4. Buildables (absent)**
- `Things.xml` is an empty TODO. There are no buildings at all, although the mod is named after one.

## Gaps (effort, then pillar fit)

1. Wet-floor sign: a building that raises path cost, then a WorkGiver that places it on mop marks automatically. Medium to large. Fit: very high (P3 names it).
2. Janitor's closet: a storage building filtered to janitor gear, with a tool-rack graphic. Small to medium (mostly art). Fit: very high (P4 and the mod's name).
3. Cleaning inspiration: copy vanilla `Frenzy_Work` with `statFactors CleaningSpeed` and require the Cleaning work type. Small (XML only). Fit: high (P1).
4. Janitor backstories (custodian, sanitation worker, ship's swab, cleaned by default). Small (XML). Fit: high (P1).
5. Headgear such as a work cap or hairnet, so the janitor reads at a glance. Small (XML and art). Fit: high (P1).
6. Quality doesn't change the cleaning bonus (verify). The tools use flat `equippedStatOffsets`, which I believe vanilla doesn't scale by quality, so quality only affects melee. Small (one StatPart). Fit: high (P2).
7. Filth misclassified by sound. `Filth_MachineBits` uses the Fluid sound, so the mop speeds it up. `Filth_Fleshmass` uses the Dirt sound, so brooms speed it up. Small (`FilthCategoryExtension` patches). Fit: medium (P2 correctness).
8. Large-mess detection: after a raid or butchering, raise the priority of a large blood or flesh cluster near home and show a message. Medium. Fit: high (P3).
9. Doormat or boot scraper that cuts tracked-in filth, built on the existing tracked-filth patch. Medium. Fit: high (P4).
10. Mop bucket or wash station, a refill point for the mop with a small buff. Medium. Fit: medium to high (P4).
11. Anomaly tool for flesh and dark filth, such as scrapers or bleach. `Filth_RevenantBloodPool` takes 2,000 work to clean. Medium. Fit: medium.
12. Galoshes promise grip in their description, but the mod has no slip mechanic. The real effect is FilthRate -50%. Small (rewrite the text) or large (add slipping). Fit: low.
13. Unmasked CutoutComplex art: the sprayer combo, wand and tank have no `_m` masks, so the Metallic stuff colour likely tints the whole sprite (verify). `Janitor_Capsule.psd` and `HazmatBackpack.ai` in Source aren't shipped. Small. Fit: polish.
14. Odyssey: in vacuum, mop marks should freeze or not spawn, and grav-ship floors have a `CleaningTimeFactor`. Small. Fit: low to medium.
15. Ideology: a custodian role or a cleanliness precept with a work bonus. Medium. Fit: medium.
16. Biotech: the Cleansweeper can't use tools. A mech cleaning module or gene is possible. Medium. Fit: low.
17. Royalty: titles from acolyte up disable Cleaning, which leaves little to hook into. Fit: low.
18. Traders: the tools inherit the `WeaponMelee` trade tag, so only weapon traders sell mops. Adding them to general goods is small. Fit: low.
19. No medieval or ultratech tier, such as a rag and bucket. Small. Fit: low.

## Top five picks

1. **Janitor's closet:** it's the namesake and all of P4, and it's mostly a storage def plus art.
2. **Cleaning inspiration:** about 15 lines of XML, and it makes a janitor memorable during a playthrough.
3. **Wet-floor sign, manual first:** it starts the headline P3 feature, mop marks already give it a hook, and autonomy can come next.
4. **Quality-scaled cleaning bonus:** one StatPart makes every quality roll on a mop matter.
5. **Backstories and a cap together:** both are small XML, and they're the fastest way to make a janitor recognisable in your playthrough.
