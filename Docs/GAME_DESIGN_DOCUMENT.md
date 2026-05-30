# Game Design Document
**Working title:** *Fruit Market Frenzy* (placeholder — see §16)
**Genre:** Auto-attack survival roguelite (Vampire Survivors-like)
**Platforms:** PC (Steam, primary) + Mobile (iOS/Android, parallel)
**Engine:** Unity 6 LTS, URP
**Document owner:** thau7777
**Version:** 0.1 — Living document. Update after every design-affecting change.

> **Reading order for new contributors:** §1 → §2 → §4 → §13. Skip implementation detail (§11) unless you're touching code.

---

## 1. Executive Summary

You are a lone shopper trapped in a possessed produce market after closing time. Fruit has come to life — bananas, watermelons, apples, pineapples — and they want you out. You can't outrun them forever, but every level-up grants you a new weapon or perk. Build your kit on the fly, push deeper into the market, and survive until dawn.

**Elevator pitch:** *Vampire Survivors meets Saturday-morning produce-aisle chaos. Five minutes per run on mobile, twenty on PC, infinitely replayable on both.*

The hook is **cross-platform parity** — same builds, same balance, same fun in either a 5-minute commute session or a 20-minute deep-dive on a monitor.

---

## 2. Design Pillars

Every feature is measured against these. If a feature doesn't serve a pillar, cut it.

| # | Pillar | What it means in practice |
|---|---|---|
| **P1** | **Readable Chaos** | Hundreds of fruit on screen, but the player always knows what's killing them and what to do next. Bright silhouettes, color-coded threats, telegraphed boss attacks. |
| **P2** | **One More Run** | A run is short enough to fit a coffee break (5 min mobile / 20 min PC) and ends with a tangible meta-progress drop. Death is never wasted time. |
| **P3** | **Build Variety Over Depth** | 8 weapons × 20 perks = many viable builds. We don't need a 100-skill tree; we need 50 small choices that reshape a run. |
| **P4** | **Touch-First, Mouse-Native** | The game is designed with thumbs in mind. Auto-attack means no aim button. Movement is the only input. Anything else is menu navigation. |
| **P5** | **Performance Is a Feature** | 200+ active entities on a mid-tier phone at 60 fps. Object pooling, Addressables, and frame-budget discipline are non-negotiable. |

---

## 3. Target Audience

| Segment | Why they'll play |
|---|---|
| **Vampire Survivors / Brotato veterans** (PC) | Familiar loop, fresh theme, deeper builds than mobile clones. |
| **Mobile arcade players** (Hades-clickers, Archero) | Short-session fix; auto-attack respects their commute time. |
| **Streamers/content creators** | High visual chaos = great clip moments. Fruit explosions are inherently funny. |

**Demographic:** 16–40, both genders, casual-to-mid-core. Plays in 5–30 minute windows.

---

## 4. Core Loops

### 4.1 Moment-to-moment loop (≤ 1 second)
```
Move thumb/stick → reposition → weapons auto-target nearest enemy → enemies die → exp orbs fly to player → repeat
```
This is the **dopamine engine.** It must feel *juicy*: hit-flash, knockback, fruit splatter VFX, satisfying orb-vacuum on level-up.

### 4.2 Per-run loop (5–20 minutes)
```
Spawn → kill → exp → level-up → pick 1 of 3 buff cards (weapon or perk)
                                       ↓
                              repeat until death or boss timer
                                       ↓
                              run summary → meta currency drops → menu
```

### 4.3 Meta loop (sessions over weeks)
```
Run → earn Seeds (soft currency) + Stamps (rare unlock currency)
                                       ↓
              spend on: weapon unlocks · character unlocks · biome unlocks · starting perks
                                       ↓
              new options appear in next run → deeper build variety → harder difficulties open
```

---

## 5. Game Mechanics — Confirmed (Already Built)

| System | Status | Notes |
|---|---|---|
| Auto-targeting weapons (Gun, Melee) | ✅ Shipped | Orbit player, find nearest enemy, fire at attack rate. See ARCHITECTURE.md §2. |
| Projectile system (straight, layered hit) | ✅ Shipped | Raycast sweep prevents tunneling. Impact VFX optional. |
| Enemy AI with strategy SOs | ✅ Shipped | Chaser/Ranged/Zigzag movement × Standard/SpinLunge/JumpLand/BounceShoot attack. |
| Pooling (Flyweight) + Addressables | ✅ Shipped | All spawned entities pooled; assets loaded async. |
| Exp/level/buff loop | ✅ Shipped | `BuffManager` raises events, UI subscribes. |
| Hit flash on `IDamageable` | ✅ Shipped | Generic, drops onto any damageable. |
| Buff cards UI (basic) | 🟡 WIP | `BuffUIController` exists; card selection flow not wired. |
| Black BG fade on level-up | ✅ Shipped | Listens for `ShowBuffsEvent`. |

---

## 6. Game Mechanics — Proposed (Roadmap)

### 6.1 Boss Fights
- **Cadence:** every 5 minutes of run time, a **King Fruit** spawns (Watermelon King, Pineapple Don, Coconut Champ).
- **Structure:** 3 phases, telegraphed attacks (1-second wind-up tells with floor decals), 1 unique mechanic per boss.
- **Reward:** guaranteed rare buff card + meta-currency drop on kill.
- **Implementation hook:** new `BossController : IEnemyContext` with a phase state machine. Reuses the existing strategy system; phase switches swap the strategy SO at runtime.

### 6.2 Meta Progression
- **Currencies:**
  - **Seeds** (soft) — earned every run, spent on minor permanent perks (+1% HP, +1% pickup radius, etc.). Diminishing returns past tier 5.
  - **Stamps** (hard) — earned only on milestones (first boss kill, full clear, etc.). Spent on unlocking weapons, characters, biomes.
- **Persistence:** local `PlayerPrefs` for v1; migrate to a save file (JSON in `Application.persistentDataPath`) before launch.

### 6.3 Multiple Playable Characters
- **Starting roster:** 3 characters at launch, 5 more unlockable.
- **Each character defines:**
  - Starting weapon (locked, can't be re-rolled)
  - 1 passive trait (e.g. "+20% movement speed but −10% HP")
  - Cosmetic: model + portrait + voice grunts
- **Tech:** `CharacterData : ScriptableObject` with starting `WeaponData` ref + `List<BuffSO>` applied on spawn. Slots into existing buff system — zero changes to `BuffManager`.

### 6.4 Maps / Biomes
- **Launch set:** 3 biomes — Produce Aisle (tutorial), Backroom Freezer (frost mechanic), Loading Dock (verticality, conveyor hazards).
- **Each biome defines:**
  - Enemy pool (4–6 enemy types eligible to spawn)
  - Environment hazards (slippery floors, conveyor lanes, etc.)
  - Boss roster (1 mid-boss, 1 final boss)
  - Music track + ambient palette
- **Unlock:** Stamps required, gated by previous biome completion.

### 6.5 Daily Challenge (post-launch)
- Seeded run with a forced character/weapon combo. Leaderboard. Cosmetic reward.
- Cheap to add once meta progression and runs are stable.

---

## 7. Content Catalog (target counts at v1.0 launch)

| Category | At launch | Stretch |
|---|---|---|
| Weapons (Gun + Melee + future) | 8 | 12 |
| Perks (passive buffs) | 12 | 20 |
| Enemy types | 12 | 18 |
| Bosses | 3 | 6 |
| Characters | 3 unlock-locked + 1 starter = 4 | 8 |
| Biomes | 3 | 5 |

See `Balance.csv` for stat tables.

---

## 8. Progression Design

### 8.1 Exp curve
Current: `_expNeeded` increments by a flat amount per level (see `BuffManager`).
**Proposed:** geometric curve — `expNeeded(n) = 10 * 1.25^n`. Levels 1–10 quick (build a kit), 10–25 slows (mastery), 25+ near-impossible (cap incentive).

### 8.2 Buff card draw
- Pool: weapons not yet equipped + perks. 3 random cards. Re-roll once per level (paid by Seeds at higher difficulties).
- **Rarity bands:** Common (white), Rare (blue), Epic (purple), Legendary (gold). Higher rarity = stronger but rarer in pool.
- **Synergies:** some perks unlock only if a specific weapon is equipped (e.g. "Banana Peel Trail" requires Banana Boomerang). This is the build-variety lever.

### 8.3 Difficulty curve
- Enemy HP/damage scales with `runTime` minutes (cap multiplier at 10× to prevent runaway).
- Spawn rate scales with `(runTime, currentLevel)`.
- Boss timer is fixed (every 5 minutes). Boss HP scales with run difficulty tier.

---

## 9. Control Schemes

### 9.1 PC
- **WASD** — move (current via Input System)
- **Mouse drag / no input** — nothing; auto-attack handles combat
- **Space** — pause
- **1/2/3** — select buff card on level-up
- **Esc** — menu

### 9.2 Mobile
- **Left-thumb floating joystick** — move
- **Tap buff card** — select
- **Top-right pause button** — menu
- *No second thumb required.* This is a design constraint, not a limitation.

### 9.3 Controller (PC)
- **Left stick** — move
- **A/X** — select card
- **Start** — menu

---

## 10. UI/UX

| Screen | Notes |
|---|---|
| Main Menu | Play · Characters · Meta Shop · Settings · Quit. Mobile-first layout (large touch targets). |
| Character Select | Carousel of unlocked characters, portrait + starting weapon + passive description. |
| Run HUD | Top: timer + level + exp bar. Bottom-left: HP. Bottom-right: equipped weapons (small icons). |
| Level-Up Card Picker | Pauses game. 3 cards centered, large touch targets. Re-roll button (Seeds cost) bottom-center. |
| Pause Menu | Resume · Restart · Quit to menu. |
| Run Summary | Time survived · enemies killed · build (weapons + perks) · Seeds earned · Stamps earned · "Play again" CTA. |
| Meta Shop | Tabs: Perks · Characters · Weapons · Biomes. Currency display top-right. |

**Style:** Flat-shaded, high-contrast, palette inspired by produce stickers and chalkboard menu boards. UI font: friendly geometric sans (Nunito or similar).

---

## 11. Technical Architecture

See `ARCHITECTURE.md` for the source-of-truth code-level breakdown. Key points relevant to design:

- **All spawned entities are pooled.** No `Instantiate`/`Destroy` in gameplay code.
- **All assets are Addressable.** Enables mobile asset budget control + future content drops.
- **Event-driven UI.** Designers can wire new HUD elements by subscribing to `IEvent` types — no controller code changes.
- **SOLID strategy pattern for AI.** A new enemy behavior = new SO + new strategy class. Zero changes to `EnemyController`.
- **Buffs are commands.** A new buff = new `BuffSO` subclass. Zero changes to `BuffManager`.

**Mobile performance budget (target):**
- 60 fps sustained, mid-tier device (Pixel 5 / iPhone 11 class)
- ≤ 60 MB RAM for gameplay scene
- ≤ 2 ms GC alloc per frame
- ≤ 4 ms CPU for AI tick at 200 active enemies

---

## 12. Art & Audio Direction

### 12.1 Art
- **3D, low-poly, stylized** — leveraging the existing Fruit Market asset pack.
- **Outline shader** (URP Renderer Feature) for enemy silhouettes — readability under chaos (Pillar P1).
- **Particle-heavy splatter** on fruit kills — colored to fruit type (red strawberry, yellow banana, etc.).
- **Color-coded threat tiers:** white = trash, yellow = elite, red = boss. This is non-negotiable for readability.

### 12.2 Audio
- **Music:** funky/uplifting per biome — Produce Aisle (bossa nova), Freezer (chillwave), Loading Dock (industrial funk).
- **SFX:** every hit is a wet "splat"; every level-up is a satisfying chime + low rumble.
- **Voice:** stylized "huh!" "hah!" character grunts. No dialogue.
- **Mute-friendly:** game must be fully playable with sound off (mobile context).

---

## 13. Feature Dependency Tree

This is the **shipping order**. A node cannot start until all its parents are stable.

```
                            ┌──────────────────────────┐
                            │   Core Gameplay Loop      │  ✅ DONE
                            │  (move + auto-attack +    │
                            │   pool + exp + buffs)     │
                            └────────────┬──────────────┘
                                         │
              ┌──────────────────────────┼──────────────────────────┐
              │                          │                          │
   ┌──────────▼─────────┐    ┌──────────▼──────────┐    ┌──────────▼──────────┐
   │  Buff Card UI flow │    │  Run Summary screen │    │  Pause Menu          │
   │  (pick 1 of 3)     │    │                     │    │                      │
   │  🟡 WIP            │    │  ❌ TODO            │    │  ❌ TODO             │
   └──────────┬─────────┘    └──────────┬──────────┘    └──────────────────────┘
              │                          │
              │                          │
   ┌──────────▼──────────────────────────▼──────────┐
   │   Run Lifecycle (start/end/timer/death)        │
   │   ❌ TODO — gate for everything below          │
   └──────────────────────┬──────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┬─────────────────┐
        │                 │                 │                 │
┌───────▼────────┐ ┌──────▼───────┐ ┌──────▼───────┐ ┌──────▼────────┐
│ Boss Spawning  │ │ Meta Currency │ │  Save System  │ │ Difficulty    │
│ (timer-based)  │ │ (Seeds/Stamps)│ │ (JSON file)   │ │ Scaling       │
│ ❌ TODO        │ │ ❌ TODO       │ │ ❌ TODO       │ │ ❌ TODO       │
└───────┬────────┘ └──────┬────────┘ └──────┬────────┘ └───────────────┘
        │                 │                  │
        │                 │                  │
        │      ┌──────────┴──────────┐       │
        │      │                     │       │
┌───────▼──────▼────┐      ┌────────▼───────▼────┐
│  Boss Controller   │      │     Meta Shop UI    │
│  (phase FSM)       │      │  (perks/char/biome) │
│  ❌ TODO           │      │  ❌ TODO            │
└───────┬────────────┘      └──────────┬──────────┘
        │                              │
        │                              │
┌───────▼────────────┐      ┌─────────▼──────────┐      ┌───────────────────┐
│  Boss Content      │      │  Character System  │      │  Biome System     │
│  (3 unique bosses) │      │  (CharacterData SO,│      │  (BiomeData SO,   │
│  ❌ TODO           │      │   3 starters)      │      │   3 biomes)       │
└────────────────────┘      │  ❌ TODO           │      │  ❌ TODO          │
                            └─────────┬──────────┘      └─────────┬─────────┘
                                      │                            │
                                      │                            │
                                      │       ┌────────────────────┘
                                      │       │
                                      │   ┌───▼─────────────────────┐
                                      │   │  Per-Biome Enemy Pools  │
                                      │   │  + Environment Hazards  │
                                      │   │  ❌ TODO                │
                                      └──►└─────────────────────────┘

                                                  │
                                         ┌────────▼─────────┐
                                         │  Daily Challenge │
                                         │  (post-launch)   │
                                         │  ❌ TODO         │
                                         └──────────────────┘
```

**Critical path to v0.5 (Vertical Slice):**
`Buff Card UI flow` → `Run Lifecycle` → `Boss Spawning` → `Boss Controller` → `1 Boss Content` → playable end-to-end run.

**Critical path to v1.0 (Launch):**
Above + `Save System` → `Meta Currency` → `Meta Shop UI` → `Character System (3)` → `Biome System (3)` → `Difficulty Scaling` → polish.

---

## 14. Milestones

| Version | Date target | Scope |
|---|---|---|
| **v0.1 (current)** | — | Architecture stable. Core loop playable. No win/lose state. |
| **v0.3 — Loop Closure** | +4 weeks | Run Lifecycle done. Buff card UI complete. Death → summary → restart works end-to-end. |
| **v0.5 — Vertical Slice** | +8 weeks | 1 biome, 1 character, 1 boss, 6 enemies, 6 weapons, 10 perks. Mobile + PC build runs at 60 fps. |
| **v0.8 — Content Complete** | +16 weeks | All v1.0 content in (3 biomes, 3 chars, 3 bosses, 8 weapons, 12 perks, 12 enemies). Meta progression functional. |
| **v1.0 — Launch Candidate** | +24 weeks | Polish, balance pass, localization (EN/ES/JP/KO/zh-CN), platform certification. |
| **v1.1+ (post-launch)** | +28 weeks | Daily Challenge, +1 character, +1 biome, +1 boss. |

---

## 15. Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| **Mobile perf shortfall** at 200 entities | Medium | High | Profiling gate at v0.5. Burst-compile AI tick if needed. Reduce target to 150 entities on low-end. |
| **Content burnout** (8 weapons × 12 perks = a lot to design and balance) | High | Medium | Start with 4 weapons, 6 perks. Add via patches. Don't gate launch on full catalog. |
| **Build variety feels shallow** | Medium | High | Synergy perks (weapon-specific) are the lever. Reserve 30% of perk pool for synergy perks. |
| **Mobile monetization** undefined | High | Medium | Launch premium (paid, no ads, no IAP). Revisit F2P only if data demands it. |
| **Cross-platform UX** divergence | Medium | Medium | Single UI built mobile-first; PC inherits with hover affordances added. Never the reverse. |
| **Theme limits audience** (fruit may read as childish) | Low | Medium | Lean into slapstick — gore-as-fruit-juice. Trailers should sell the comedy. |

---

## 16. Open Questions

These need a decision before v0.5:

1. **Final title.** "Fruit Market Frenzy" is a working name. Need a memorable, searchable title before any marketing.
2. **Monetization model.** Premium paid? F2P with cosmetics? Ad-supported on mobile only? Recommend: premium on both platforms, $4.99 mobile / $9.99 PC.
3. **Online features.** Leaderboards? Cloud save? Recommend: leaderboards (Steam + Game Center + Play Games) only — no real-time multiplayer in v1.0.
4. **Narrative framing.** Is there a story? Recommend: 30-second intro animation, no in-game dialogue. Lore in unlock blurbs.
5. **Accessibility.** Colorblind mode? One-handed mobile mode? Recommend: both, post-launch patch.

---

## 17. Appendix: Document Map

| File | Purpose |
|---|---|
| `ARCHITECTURE.md` | Code architecture — read before touching source. |
| `Docs/GAME_DESIGN_DOCUMENT.md` | This file — design intent + roadmap. |
| `Docs/Balance.csv` | Content tables — weapons, enemies, characters, biomes, bosses. Open in Excel/Sheets. |

---

*End of document. Update version + date in the header whenever a design decision changes.*
