# Architecture Overview

> Keep this file up to date after every session that adds or changes system structure.
> Claude reads this instead of raw source files to save tokens.

---

## Assembly Layout

8 asmdefs organized by domain. Each is `autoReferenced: false` so dependencies are explicit.

```
Game.Core ──┬── Game.VFX ──┬── Game.Combat ──┬── Game.AI ── (uses Pickups)
            │              │                  ├── Game.Player
            │              │                  └── Game.UI
            └── Game.Pickups ──────────────── (no other deps)

Game.Editor ── Game.Core                       (Editor-only platform)
```

| Asmdef | Depends on | Purpose |
|---|---|---|
| `Game.Core` | (external only) | Foundation: utilities + contracts everyone uses |
| `Game.VFX` | Game.Core | Reusable visual effects (damage opt-in) |
| `Game.Combat` | Game.Core, Game.VFX | Weapons, projectiles, buffs |
| `Game.AI` | Game.Core, Game.VFX, Game.Combat, Game.Pickups | Enemies + AI managers |
| `Game.Pickups` | Game.Core | World pickups (exp drops, future: coins/hearts/etc.) |
| `Game.UI` | Game.Core, Game.Combat | HUD/UI — event-driven; Combat ref is for buff-card rendering (`BuffSO` display data) |
| `Game.Player` | Game.Core, Game.Combat | Player controller + states |
| `Game.Editor` | Game.Core | Editor tooling (LokiInspector drawers, validators, toolbar) |

**External deps:** UniTask, UniTask.Addressables, Unity.InputSystem, Unity.Addressables, Unity.ResourceManager, PrimeTween.Runtime, TMPro, UnityEngine.UI.

## Folder Map (`Assets/_Scripts/Runtime/`)

```
Core/                              [Game.Core]
  Attributes/                      — LokiAttributes (custom inspector attrs)
  EventBus/                        — Generic event bus + IEvent definitions
  Flyweight/                       — Flyweight + FlyweightSettings + FlyweightFactory (pool infra)
  Helpers/                         — Cam/Cursor helpers + ExtensionMethods/
  Hitbox/                          — IDamageable (generic damage contract)
  Input/                           — InputReader (SO) + generated MyInputActions
  Singletons/                      — Singleton / Persistent / Regulator base classes
  StateMachine/                    — Generic FSM (Tick + FixedTick)
  Time/                            — TimeManager (Persistent) + TimeScaleChangedEvent

VFX/                               [Game.VFX]
  OneShotVfx                       — Pooled VFX with optional damaging hitbox
  OneShotVfxSettings               — SO config (dealsDamage opt-in)
  HitFlash                         — Emission flash on IDamageable.OnDamaged (MPB + UniTask)

Combat/                            [Game.Combat]
  Character/
    CharacterInfos (SO)            — per-character definition: display info, AssetReference model, default WeaponData, all base stats
    CharacterStats (plain class)   — runtime mutable stats + static Current; constructed by PlayerController from CharacterInfos via Initialize(CharacterInfos)
    CharacterEvents                — PlayerSpawnedEvent(Transform, CharacterInfos): raised by PlayerSpawner once the player prefab is loaded + instantiated + initialized. Game-start signal — listened by WeaponManager (binds player transform), EnemyManager (kicks off spawn flow), and any future Camera/UI follower
  Weapons/
    Weapon, RangedWeapon, MeleeWeapon, WeaponManager (Singleton)
    Data/                          — WeaponData / RangedWeaponData / MeleeWeaponData (SO)
    AttackAnimations/              — WeaponAttackAnimSO + Stab + Swing
  Projectiles/
    Projectile, StraightProjectile, IProjectileLaunchData
    ProjectileSettings, StraightProjectileSettings (SO)
  Buffs/
    BuffSO (Command base), BuffManager (Singleton invoker)
    ExpProgression (Singleton)     — XP + level curve; subscribes ExpPickupEvent, raises ExpProgressChangedEvent + LevelUpEvent
    BuffSelector                   — plain class: weighted sampling without replacement; active buffs get a configurable weight boost
    AddWeaponBuff                  — concrete Command: load + equip weapon (Remove raises RemoveWeaponEvent → unequip)
    CharacterStatsModifier         — concrete Command: MovementStatBlock + Melee/Ranged CombatStatBlock (% all stats). Apply/Remove raise events; PlayerController subscribes and runs the math against CharacterStats.Current

AI/                                [Game.AI]
  EnemyManager (Singleton)         — spawning + ExpDrop on death
  Enemy/
    EnemyController, EnemyData, IEnemyContext
    States/                        — Spawn/Idle/Move/Attack/Die
    Strategies/Movement/           — IMovementStrategy + Chaser/Ranged/Zigzag SOs
    Strategies/Attack/             — IAttackStrategy + StandardMelee/SpinLunge/JumpLand/BounceShoot SOs
    AttackAnimData/                — IAttackAnimation + matching anim SOs

Pickups/                           [Game.Pickups]
  Pickable, ExpDrop, PickableSettings, ExpDropSettings

UI/                                [Game.UI]
  ExpUIController                  — subscribes to ExpProgressChangedEvent + LevelUpEvent
  BuffUIController                 — subscribes to ShowBuffsEvent; binds up to N cards from payload; slide-up anim; click → BuffManager.ApplyBuff
  BuffCard                         — single-card view (icon/name/desc/button), Bind(BuffSO, onSelected)
  BlackBGController                — fade overlay (manual LerpIn/LerpOut; no event subscription)

Player/                            [Game.Player]
  PlayerSpawner                    — scene-placed MonoBehaviour; serialized `CharacterInfos` + `_spawnPoint`. `Start()` runs the async spawn flow: Addressables-loads `CharacterInfos.CharacterModelRef`, `Instantiate`s it at the spawn point (the prefab itself carries `PlayerController`), calls `PlayerController.Initialize(CharacterInfos)`, awaits `DefaultWeapon.LoadWeaponAssetsAsync()`, raises `PlayerSpawnedEvent` (the game-start signal), then raises `AddWeaponEvent` for the default weapon. Owns the model `AsyncOperationHandle` and releases it in `OnDestroy`.
  PlayerController                 — `Singleton<PlayerController>` mounted on the loaded character prefab. `Awake` only grabs `Rigidbody`. `Initialize(CharacterInfos)` (called by `PlayerSpawner` immediately after `Instantiate`) constructs `new CharacterStats(infos) + .Activate()` and initializes the state machine. Subscribes to Apply/Remove `CharacterStatsModifierEvent` in `OnEnable` and applies % math to `CharacterStats.Current` for all 9 stat fields. Delegate getters MoveSpeed/MaxHealth/CurrentHealth/PickupRadius forward to `CharacterStats.Current`. No async loading — the model **is** this GameObject, and default-weapon equip is orchestrated by `PlayerSpawner`.
  PlayerStates/                    — Idle/Move/Hurt

Editor/                            [Game.Editor]
  LokiEditorBase / LokiEditorForMono / LokiEditorForSO
  Drawer/                          — AttributesDrawer (MinMaxSlider, ReadOnly)
  Validator/                       — RequiredFieldValidator + GroupValidator + ConsoleClickHandler
  CustomToolbar/                   — Main toolbar buttons + timescale slider
  Utilitys/                        — VisualElementExtensions + LokiSpecialGUIStyle
  Windows/                         — LokiToolWindow
```

## Key Architectural Decisions

- **`IDamageable` lives in Game.Core** (not Combat) so VFX/AI/environment can deal damage without dragging Combat as a transitive dep.
- **`OneShotVfx` is one class with damage opt-in** via `OneShotVfxSettings.dealsDamage` — non-damage and damage VFX use the same primitive.
- **XP/level lives in `ExpProgression`, not `BuffManager`.** `ExpProgression` owns the curve, subscribes to `ExpPickupEvent`, and raises `ExpProgressChangedEvent` + `LevelUpEvent`. `BuffManager` subscribes to `LevelUpEvent` to pop a choice. This is the SRP split — XP, buff lifecycle, and buff selection (`BuffSelector`) are three independent classes.
- **`EnemyManager` (in AI) handles exp drop spawning** via `ExpDropSettings` — AI→Pickups is one-way and intentional.
- **`WeaponFactory` (mentioned in prior docs) does not exist as a class**; weapon instantiation lives directly in `WeaponManager.GetWeapon`.

---

## Core Systems

### 1. Flyweight / Object Pool
**Entry point:** `FlyweightFactory` (Singleton)  
**Flow:** `FlyweightSettings.Create()` → `Flyweight` instance → pooled via `IObjectPool<Flyweight>`  
**Key types:**
- `FlyweightSettings` (abstract SO) — intrinsic state + pool config + Addressable prefab ref. `OnGet` detaches from parent and activates; `OnRelease` deactivates.
- `Flyweight` (abstract MB) — base for anything pooled; calls `ReturnToPool()`
- `FlyweightFactory` — single pool dictionary keyed by settings asset

**Extends to:** Projectiles, Enemies, OneShotVfx — anything spawned frequently.

**OneShotVfx subsystem:**
- `OneShotVfxSettings` (SO) — `dealsDamage`, `baseDamage`, `hitboxLayers`, `hitboxActivateDelay`, `hitboxActiveDuration`; overrides `OnGet` to skip `SetActive` — caller must `FlyweightInit` + `OneShotVfxInit()` after getting from pool
- `OneShotVfx` (MB : Flyweight) — plays `ParticleSystem` or `VisualEffect` (behind `#if UNITY_VFX_GRAPH`) on init; UniTask tasks manage hitbox on/off window and wait-for-completion-then-despawn, both gated by `_lifetimeCts` cancelled in `OnDisable`; `OnTriggerEnter` applies `_currentDamage` to `IDamageable`
- `OneShotVfxInit(float damageOverride)` overload sets `_currentDamage` after init — used by enemy attack strategies to route per-instance `IEnemyContext.Damage` into the spawned hitbox (so buffs on the enemy flow through to the VFX, never the SO).

---

### 2. Weapon System
**Hierarchy:**
```
WeaponData (abstract SO)              — keeps only AssetReferences + _baseWeaponRange
  ├── RangedWeaponData → RangedWeapon (Weapon, IProjectileLaunchData) — adds projectileSettings, muzzleVfxSettings, _bulletSpeed, ammo, recoil tuning
  └── MeleeWeaponData  → MeleeWeapon  (Weapon)                        — AttackAnim : WeaponAttackAnimSO

Weapon (abstract MB) implements IWeapon
  ├── RangedWeapon — fires Projectiles, PrimeTween recoil on attack
  └── MeleeWeapon  — plays WeaponAttackAnimSO on attack (stab or swing)

WeaponAttackAnimSO (abstract SO)
  ├── StabAttackAnimSO   — translates _weaponModel along local Z
  └── SwingAttackAnimSO  — lunge toward target (flat XZ) + rotation swing
```

**`IWeapon` contract:**
```csharp
WeaponData WeaponData { get; }      // getter only — no setter
Transform Transform { get; }        // avoids downcasting to MonoBehaviour
void OnEquip(Transform user, Vector3 localPosition);
void OnUnequip();
bool IsEquippedWith(WeaponData data);
```

**Stat read-through (no runtime cache):** `Weapon` exposes abstract `Damage / AttackRate / RangeMul` getters that subclasses route to `CharacterStats.Current`. `MeleeWeapon` reads `MeleeDamage / MeleeAttackRate / MeleeRangeMul`; `RangedWeapon` reads `RangedDamage / RangedAttackRate / RangedRangeMul`. Effective range is `Range = WeaponData.BaseWeaponRange * RangeMul`. **All runtime reads** (CanAttack, OnEquip charging, GetClosestEnemy, MeleeWeapon AttackAnim args, RangedWeapon's IProjectileLaunchData) go through these properties — never the SO, never cached. Buffs mutate `CharacterStats.Current` directly via `PlayerController.OnApplyStatsModifier`; weapons see the new values on the next frame with zero per-weapon plumbing.

**Auto-targeting:** `Weapon.Update()` is sealed (not virtual). It calls `Physics.OverlapSphereNonAlloc` each frame (non-alloc, 20-slot buffer, Enemy layer mask) to find the closest enemy. When a target is in range, `FaceTarget()` rotates the weapon and `Attack()` fires automatically. Subclasses extend per-frame logic via `protected virtual void OnUpdate()` (Template Method).

**Attack rate:** `CanAttack` gates on elapsed time ≥ `1f / AttackRate`. On equip, pre-charged to full interval so weapon fires immediately.

**RangedWeapon specifics (`RangedWeaponData` / `RangedWeapon`):**
- `RangedWeaponData` only carries archetype data: `projectileSettings`, `muzzleVfxSettings`, `_bulletSpeed`, ammo, recoil tuning. **No damage / attackRate** — those come from `CharacterStats.Current.RangedDamage / RangedAttackRate`.
- `RangedWeapon` implements `IProjectileLaunchData` (explicit) — passes `this` to `Projectile.ShootProjectile`. `Speed` reads `RangedWeaponData.BulletSpeed`; `Damage` and `Range` route to the live character stats.
- Ammo: `AmmoPerMagazine`, `MagazineCapacity`, `ReloadTime`
- Recoil: `RecoilDistance/Duration/ReturnDuration/Ease` — `_weaponModel` pushed backward then returned via PrimeTween `Sequence`

**MeleeWeapon specifics:**
- `MeleeWeapon.Attack()` passes `_currentTarget` to `AttackAnim.Play()` so animations orient toward the enemy
- `StopAnimation()` / `OnUnequip()` snap the model back to origin

**`WeaponAttackAnimSO.Play` signature:**  
`Play(Transform weaponModel, Transform target, Vector3 originLocalPos, float range, Quaternion originLocalRot, float attackInterval)`

**Factory:** weapon instantiation lives directly in `WeaponManager.TryCreateWeapon` (uses `data.WeaponPrefab`).  
**Equip system:** `WeaponManager` (Singleton) — owns `List<IWeapon>` only. Accesses transforms via `IWeapon.Transform` (no downcasting). Orbits weapons around player at `_weaponOrbitRadius`. `RefreshWeaponPositions()` recalculates all slots after equip/unequip. Initialized by `PlayerController.Awake()`. **No modifier plumbing** — stat buffs mutate `CharacterStats.Current`; weapons read live every frame, so any weapon equipped at any time sees the current stat values automatically. `WeaponManager` only handles `AddWeaponEvent` / `RemoveWeaponEvent`.  
**Assets:** `WeaponData.LoadWeaponAssetsAsync()` loads prefab + icon via Addressables. `RangedWeaponData` override also loads `projectileSettings.LoadPrefabAsync()` and the muzzle VFX prefab.

**OCP compliance:** New weapon type = new `WeaponData` subclass + new `Weapon` subclass. New melee anim = new `WeaponAttackAnimSO` subclass. No existing code changes.

---

### 3. Projectile System
**Hierarchy:**
```
IProjectileLaunchData (interface)
  — Speed, Damage, Range
  — implemented by RangedWeapon (explicit; Damage/Range route to CharacterStats.Current via the weapon's abstract getters)

ProjectileSettings (abstract SO) : FlyweightSettings
  + dealsDamage: bool
  + onImpactVfx: OneShotVfxSettings (optional)
  └── StraightProjectileSettings  → StraightProjectile (Projectile)
                                     + collisionLayers: LayerMask

Projectile (abstract MB) : Flyweight
  + ShootProjectile(startPos, targetPos, IProjectileLaunchData) — decoupled from RangedWeaponData
  + OnHit(Collider) — deals damage via IDamageable + spawns onImpactVfx
  └── StraightProjectile — Rigidbody-driven, distance-limited, ContinuousDynamic collision
```

**Key design:** `Projectile` takes `IProjectileLaunchData` — not `RangedWeaponData`. Any system (turret, trap, enemy) can fire projectiles by implementing the interface.

**Rigidbody plumbing (base class):** `Projectile` owns the `Rigidbody` reference (moved out of `StraightProjectile`) plus the launch-time interpolation handshake. `PrepareRigidbodyForLaunch(pos, rot)` sets `interpolation = None` and teleports `_rb.position/.rotation` to the spawn pose; the base `FixedUpdate` then flips interpolation back to `Interpolate` on the next physics step. This avoids the visual snap that occurs when a pooled body is reused and interpolation lerps from its old transform.

**Trail management:** `Projectile` owns a `TrailRenderer`. `OnDisable` clears + disables the trail. `ResetTrail()` re-enables it on fire.

**Impact VFX:** `SpawnImpactVfx()` is overloaded — no-arg uses `transform.position`; `SpawnImpactVfx(Vector3 point)` uses the actual hit point so the impact effect lands on the surface, not at the projectile pivot.

**Spawn:** `RangedWeaponData.projectileSettings` → `FlyweightFactory.Spawn()` → `ShootProjectile(tip.position, tip.position + tip.forward, this)` where `this` is the `RangedWeapon`.  
**Hit detection (`StraightProjectile`):** `FixedUpdate` does a `Physics.Raycast` sweep from `_prevPosition` → current `_rb.position` filtered by `collisionLayers`. This catches tunneling on fast projectiles that `OnTriggerEnter` would miss. `OnTriggerEnter` remains as a low-speed fallback and early-outs if `!_isInitialized`. On hit or max-distance: `OnHit` → `SpawnImpactVfx(hit.point)` → `Despawn()`.  
**Despawn:** `Despawn()` flips collider/meshes off and resets `interpolation = None`, then UniTask `FadeTrailThenPool` (`WaitForSeconds(_trail.time)` → `ReturnToPool`) gated by `_lifetimeCts` cancelled in `OnDisable`.

---

### 4. Enemy System
**Controller:** `EnemyController : Flyweight, IDamageable, IEnemyContext`  
**Data:** `EnemyData : FlyweightSettings` — holds HP, `baseDamage`, speed, strategy SOs, fracture/split settings  
**Init flow:** `EnemyData.Create()` → `EnemyController.EnemyInit(data, movementStrategy, attackStrategy)`

**Damage stat (mirrors Gun's pattern):** `EnemyData.baseDamage` is the SO base value (immutable at runtime). `EnemyController` holds `public float Damage { get; private set; }` (runtime mutable), copied from `Data.baseDamage` in both `EnemyInit` and `ResetEnemy`. `ModifyDamage(float delta)` clamps to ≥ 0 — future damage buffs call this on the instance, never mutate the SO. `IEnemyContext.Damage` exposes the runtime value to strategies, which route it into spawned hitboxes (`OneShotVfxInit(damage)`) or projectiles (via a runtime `IProjectileLaunchData` wrapper).

**Damage event:** `IDamageable` exposes `event Action<float> OnDamaged` alongside `TakeDamage(float)`. Implementers (currently `EnemyController`) fire it inside `TakeDamage` before any death check. Hit-reaction components depend on `IDamageable` — never on the concrete controller.

**Hit flash (`HitFlash`):** generic, reusable on any GameObject with an `IDamageable` component (Enemy, Player, BreakableCrate, etc.). `Awake` resolves `GetComponent<IDamageable>()` and subscribes to `OnDamaged`; logs an error and disables itself if none found. On hit, drives `_EmissionColor` via a cached `MaterialPropertyBlock` (no material instancing) — lerps `Color.white * peakIntensity` down to black across `duration` using `_fadeCurve`. UniTask-based (`UniTaskVoid` + `CancellationTokenSource` cancelled on every new flash and in `OnDisable`). Inspector field: `_targetRenderer` (mesh whose emission flashes); tuning: `_peakIntensity` (default 3.5 ≈ HDR intensity stop 1.8), `_duration`, `_fadeCurve` (ease-out by default). Requires `_EMISSION` keyword enabled on the source material.

**`IEnemyContext` interface** (in `Enemy/IEnemyContext.cs`):
```csharp
Transform transform { get; }
Rigidbody Rb { get; }
Transform VisualRoot { get; }
Vector3 VrOgScale { get; }
Quaternion VrOgRotation { get; }
EnemyData Data { get; }
float Damage { get; }   // runtime mutable, mirrors EnemyData.baseDamage at spawn
```
All strategies and animations take `IEnemyContext` instead of `EnemyController`. Adding a `BossController` only requires implementing this interface — no strategy code changes.

**State machine:** Spawn → Idle → Move → Attack → Die  
**`EnemyController.Update()`** ticks `AttackStrategy.Tick(dt)` before `SM.Tick()` so cooldown timers always advance.

**On death:** `EnemyDie.OnEnter()` calls `Owner.ReturnToPool()` — the enemy is returned to the pool, NOT destroyed. `ResetEnemy()` re-enables the collider and resets HP/state for the next spawn. Fracture and split spawns use `FlyweightFactory.Spawn()`.

**Death spawn assets (`EnemyData`):**
- `fractureSettings: FlyweightSettings` — VFX or rigid-body fracture effect; spawned via pool on death
- `splitEnemyData: EnemyData` — enemy type to spawn on split death; spawned via pool with `SetTarget` forwarded
- `EnemyData.LoadPrefabAsync()` is overridden to also load `fractureSettings` and `splitEnemyData` prefabs

**Strategies (Strategy Pattern):**

| Interface | Method signature | Notes |
|---|---|---|
| `IMovementStrategy` | `Move(IEnemyContext, Transform target)` | Chaser, Ranged, Zigzag |
| `IAttackStrategy` | `Tick(float dt)` / `StartAttack(IEnemyContext, Transform, Action)` / `Interrupt(IEnemyContext)` / `IsReady` / `ShouldFaceTarget` | `Tick` advances cooldown; `IsReady` gates re-attack. `ShouldFaceTarget` is a runtime flag the anim toggles via callback. |
| `IAttackAnimation` | `Build(IEnemyContext, Transform, Action onStrike, Action onComplete, Action<bool> setFaceTarget)` / `OnInterrupt(IEnemyContext)` | StandardMeleeAnim, SpinLungeAnim, JumpLandAnim, BounceShootAnim. `setFaceTarget` toggles the strategy's `ShouldFaceTarget` flag at any phase boundary (e.g. lock direction at spin/jump start). |

**Face-target gate:** `EnemyAttack.Tick` calls `Owner.RotateToPlayer()` only when `Owner.AttackStrategy.ShouldFaceTarget` is true. Each strategy resets the flag to `true` on `StartAttack`; the anim is the authority on when to flip it off (via the `setFaceTarget` callback in its `Build`). This keeps anims reusable across fruits — the lock-direction logic lives in the anim that needs it, not on a static SO bool.

**Concrete attack strategies (one runtime class + one SO data class per pair):**

| Strategy | Anim | Behavior |
|---|---|---|
| `StandardMeleeAttack` | `StandardMeleeAnim` | Wind-up rotation tilt + strike squash + return. `SpawnAttackFlyweight` is still TODO. |
| `SpinLungeAttack` | `SpinLungeAnim` | Wind-up tilt+squat → locked spin-and-lunge (owner.transform translates along snapshot direction while VisualRoot spins N×360° via `Tween.LocalEulerAngles` with `Ease.Linear`; anim calls `setFaceTarget(false)` at spin start). Strike fires at the top of each spin → spawns `attackFlyweightSettings` (OneShotVfx) at owner position with `OneShotVfxInit(owner.Damage)`. Owner returns to snapshot world position. |
| `JumpLandAttack` | `JumpLandAnim` | Wind-up squat → owner.transform tweens to apex midpoint (height += `jumpHeight`) → falls to `target.x,owner.y,target.z` (snapshot at StartAttack). Landing fires strike, scale snaps to `landScale`, pause `landPauseDur`, then wobble back via `Ease.OutBack`. Anim locks face-target at jump start. Strike spawns OneShotVfx at landed position. |
| `BounceShootAttack` | `BounceShootAnim` | N scale-Y bounces (squat ↓ then taller ↑); strike fires at the top of each bounce-up → spawns projectile via `FlyweightFactory.Spawn(projectileSettings)` and calls `ShootProjectile(spawnPos, target.position, new RuntimeProjectileLaunchData(speed, owner.Damage, range))`. Always faces target (anim never calls `setFaceTarget`). |

**Runtime IProjectileLaunchData for enemy projectiles:** `BounceShootAttack` defines a file-private `readonly struct RuntimeProjectileLaunchData : IProjectileLaunchData` that snapshots `(speed, owner.Damage, range)` at shoot time. This is the enemy-side equivalent of how `RangedWeapon` routes `Damage`/`Range` through `CharacterStats.Current` — damage buffs on the enemy controller flow to the projectile without ever mutating an SO.

**Optional movement strategy interfaces (ISP):**

| Interface | Methods | Called from | Purpose |
|---|---|---|---|
| `IMovementLifecycle` | `OnOwnerCreated(IEnemyContext)` / `OnOwnerReset()` | `EnemyController.EnemyInit` / `ResetEnemy` | One-time setup (e.g. spawn trail child GO); phase reset on respawn |
| `IMovementStateListener` | `OnMoveEnter(IEnemyContext)` / `OnMoveExit(IEnemyContext)` | `EnemyMove.OnEnter` / `OnExit` | Activate/deactivate per-strategy visuals tied to Move state |

**`ZigzagMovement`** implements all three: `IMovementStrategy`, `IMovementLifecycle`, `IMovementStateListener`. On `OnOwnerCreated`, instantiates `ZigzagMovementData.trailPrefab` (`[Required]`) as a child of the enemy with `worldPositionStays: false`, sets its `localPosition` to `data.positionOffset`, disables the `TrailRenderer`, and caches the `TrailResetter` via `GetOrAdd<T>()` (auto-attached if missing — `TrailResetter` has `[RequireComponent(typeof(TrailRenderer))]`). `OnMoveEnter` calls `TrailResetter.Activate()`, `OnMoveExit` calls `Deactivate()`; `TrailResetter.OnDisable` auto-clears trail data when the enemy is pooled. Movement uses a 4-phase cycle: `ZigDash → ZigPause → ZagDash → ZagPause`. Pause duration formula: `max(minPause, basePause / (1 + moveSpeed * reductionFactor))`.

**`ZigzagMovementData` SO tabs (LokiInspector):**
- **Dash** — `dashMultiplier` (speed multiplier), `dashDistance`, `lateralOffset`
- **Pause** — `pauseDuration` (base), `pauseReductionFactor`, `minPauseDuration` (floor)
- **Trail** — `[Required] trailPrefab` (`GameObject` containing a configured `TrailRenderer`), `positionOffset` (local offset applied to the instantiated trail child)

**OCP compliance:** New enemy behavior = new strategy SO + strategy class. `EnemyController` untouched (interface checks are open-ended).

---

### 5. State Machine
**Generic:** `StateMachine` + `IState` + `State<TOwner>`  
**`IState` contract:** `OnEnter`, `Tick`, `FixedTick`, `GetTransition` (null = stay), `OnExit`  
**`StateMachine.Tick()`:** calls `GetTransition()` first — if non-null, transitions; otherwise calls `Current.Tick()`. Tick is skipped on the transition frame (by design).  
**`ForceTransition<T>()`** and `ForceTransition(IState)` for imperative transitions.

**Used by:**
- `PlayerController` — `Update()` → `SM.Tick()`, `FixedUpdate()` → `SM.FixedTick()`
  - `PlayerIdle` / `PlayerMove` — reads `PlayerController.InputDir` (read-only property, set via `OnMove`)
  - `PlayerHurt` — applies `PendingKnockback` decay via `FixedTick`
- `EnemyController` — Spawn/Idle/Move/Attack/Die

---

### 6. Event Bus
**Generic type-safe:** `EventBus<T>` where `T : IEvent`  
**Binding:** `EventBinding<T>` — supports `Action<T>` and no-arg `Action`  
**Init:** `EventBusUtil.InitializeAllBuses()` — reflection discovers all `IEvent` implementations  
**Current events:**
- `EnemyDeathEvent(Transform)` — raised by `EnemyDie`, consumed by `EnemyManager` (drops exp) — [Core/EventBus/Events.cs]
- `ExpPickupEvent(float)` — raised by `ExpDrop` on pickup, consumed by `BuffManager` — [Core/EventBus/Events.cs]
- `ExpProgressChangedEvent(float)` — raised by `ExpProgression` on exp gain, consumed by `ExpUIController` — [Core/EventBus/Events.cs]
- `LevelUpEvent(int)` — raised by `ExpProgression` on level threshold (loops for multi-level pickups), consumed by `BuffManager` (triggers buff selection) + `ExpUIController` (level text). Not raised at startup — UI inits via Awake defaults — [Core/EventBus/Events.cs]
- `ShowBuffsEvent(BuffSO[] Choices)` — raised by `BuffManager` on level-up, consumed by `BuffUIController` — [Combat/Buffs/BuffEvents.cs] (lives in Combat because payload references `BuffSO`; UI gained Combat asmdef ref for this)
- `AddWeaponEvent(WeaponData)` / `RemoveWeaponEvent(WeaponData)` — raised by `AddWeaponBuff.Apply` / `.Remove` and by `PlayerSpawner` for the default weapon (after `PlayerSpawnedEvent`); consumed by `WeaponManager` — [Combat/Buffs/BuffEvents.cs]
- `PlayerSpawnedEvent(Transform PlayerTransform, CharacterInfos CharacterInfos)` — raised once by `PlayerSpawner` after the player prefab loads, instantiates, and runs `Initialize`. Consumed by `WeaponManager` (binds player transform) and `EnemyManager` (kicks off spawn flow). Treat as the "game has started" signal — [Combat/Character/CharacterEvents.cs]
- `ApplyCharacterStatsModifierEvent / RemoveCharacterStatsModifierEvent(CharacterStatsModifier Modifier)` — raised by `CharacterStatsModifier`, consumed by `PlayerController` which mutates `CharacterStats.Current` for all 9 stats (movement + melee + ranged blocks). Weapons read stats live every frame, so they pick up changes automatically with zero per-weapon plumbing — [Combat/Buffs/BuffEvents.cs]

(Single unified stat-modifier event pair — buff assets carry zero scene/asset references; weapons never receive modifier dispatches.)

**Cross-assembly event placement rule:** Generic infrastructure (`EventBus<T>`, `IEvent`, `EventBinding<T>`) lives in Core. Concrete event TYPES live in whichever assembly owns their payload — never force a payload-bearing event into Core if it forces Core to depend on a downstream assembly.

---

### 7. Input System
**`InputReader`** : ScriptableObject — wraps generated `MyInputActions`  
**`GameplayActions`** — exposes `event Action<Vector2> onMove`  
Currently only Move is mapped. Input action map switching supported.

---

### 8. Buff System
**Pattern:** Command — each `BuffSO` subclass is a self-contained command with `Apply()` / `Remove()`.  
**`BuffSO` display fields** (base class): `DisplayName`, `Description`, `Icon` — used by `BuffCard` to render level-up choices. All under the `Display` LokiInspector tab.  
**Runtime description:** `BuffSO.GetRuntimeDescription()` is virtual; default returns `_description`. `CharacterStatsModifier` overrides it to append `Stat : current -> new` lines computed from `CharacterStats.Current` for all nine stats across the three blocks. If `Current` is null (e.g. inspector preview before play), the line falls back to `Stat : +X%`. Lines for stats with `percent == 0` are skipped. `BuffCard.Bind` calls `GetRuntimeDescription()`, not `Description`. Base class exposes `protected static AppendStatLine(sb, name, current, percent)` so subclasses share formatting.  
**Character stats accessor:** `CharacterStats` is a plain C# class with `static Current` set by `PlayerController.Awake → new CharacterStats(_characterInfos).Activate()` and cleared in `OnDestroy → Deactivate()`. Lives in Combat (not Player) so `CharacterStatsModifier.GetRuntimeDescription()` can read live stats without forcing a Combat→Player asmdef dep. `PlayerController` is itself a `Singleton<PlayerController>` — used by Player-asmdef code; Combat reads stats via `CharacterStats.Current`.

**SRP split (three classes, three reasons to change):**
- **`BuffManager`** (Singleton) — **buff lifecycle only**. `ApplyBuff(BuffSO)` / `RemoveBuff(BuffSO)` with null guards + dedupe (re-applying the same SO is now a no-op — `_activeBuffs.Add` short-circuits before `Apply()` runs). Owns `_allBuffs : BuffSO[]` (pool) + `_choicesPerLevelUp : int` (default 3) + `_activeBuffBoost : float` (default 0.15 = +15% weight for actives). Subscribes to `LevelUpEvent` and on each fire rebuilds `_activeSetCache : HashSet<BuffSO>` from `_activeBuffs.Keys`, then calls `_selector.Pick(_allBuffs, _activeSetCache, _activeBuffBoost, _choicesPerLevelUp)` then raises `ShowBuffsEvent`. Pre-allocated cache HashSet avoids per-level-up GC. Has `_testBuffs : BuffSO[]` applied in `Start()` for editor testing. **If you need stackable buffs of the same archetype, instantiate distinct SO instances** — reusing one asset is now a no-op.
- **`ExpProgression`** (Singleton) — **XP curve only**. `_baseExpNeeded`, `_expMultiplierPerLevel` serialized. Subscribes to `ExpPickupEvent`; `while (_currentExp >= _expNeeded)` loop handles multi-level pickups (big orbs that cross 2+ thresholds in one hit). Raises `LevelUpEvent` per crossed threshold + one final `ExpProgressChangedEvent`. **Does NOT raise `LevelUpEvent` at startup** — that would trigger a spurious buff-selection on game start. UI must init its level text via Awake defaults.
- **`BuffSelector`** (plain class, not MB / not SO) — **selection algorithm only**. `Pick(pool, active, activeBoost, count)` performs weighted sampling without replacement: actives get weight `1 + activeBoost`, others weight `1`. `active` is nullable (null = uniform weights). Flat boost regardless of stack count. If fewer than `count` candidates are available the result array is shorter — `BuffUIController` handles short payloads by hiding extra cards. Pure logic, trivially testable.

**Concrete commands:**
- `AddWeaponBuff` — `Apply` calls `LoadWeaponAssetsAsync()` (async) then raises `AddWeaponEvent` → `WeaponManager.EquipWeapon`. `Remove` raises `RemoveWeaponEvent(WeaponData)` → `WeaponManager.UnequipWeapon` (LSP-compliant — `Remove` actually reverses `Apply`).
- `CharacterStatsModifier` — holds one `MovementStatBlock` (`moveSpeedPercent`/`maxHealthPercent`/`pickupRadiusPercent`) and two `CombatStatBlock` structs (`_melee`, `_ranged`), each with `[Range(-100,100)] damagePercent / attackRatePercent / rangePercent`. `Apply()`/`Remove()` raise `ApplyCharacterStatsModifierEvent` / `RemoveCharacterStatsModifierEvent`. `PlayerController` subscribes and applies `field *= 1f + p*0.01f` (or `/=` on remove) to `CharacterStats.Current` for every non-zero percent. `MaxHealth` changes propagate to `CurrentHealth` clamped to the new max. Enables Brotato-style assets like Sharpshooter (+ranged dmg, +ranged range, −ranged attackRate) and OneWithSteel (+ranged dmg / −melee dmg) in one SO. Range percent modifies the multiplier (`MeleeRangeMul` / `RangedRangeMul`); effective weapon range = `WeaponData.BaseWeaponRange * RangeMul`.
- `CharacterInfos` — per-character definition SO (one asset per playable character). `Display` tab: name/icon/description. `Visual` tab: `AssetReference` to the character model prefab (loaded by `PlayerController.Awake`). `Loadout` tab: `WeaponData` ref for the starting weapon. `Base - Movement` / `Base - Combat` tabs: all 9 base stat values (`_baseMoveSpeed`, `_baseMaxHealth`, `_basePickupRadius`, `_baseMeleeDamage`, `_baseRangedDamage`, `_baseMeleeAttackRate`, `_baseRangedAttackRate`, `_baseMeleeRangeMul = 1`, `_baseRangedRangeMul = 1`).
- `CharacterStats` — plain C# class (not SO) holding all 9 runtime mutable stat fields + `CurrentHealth` + `static Current`. Constructor takes `CharacterInfos` and calls `ResetToBase()` to copy base values into the runtime fields. `Activate()` sets `Current = this`; `Deactivate()` clears it. Plain class chosen so there's no asset lifecycle to manage — lives as long as the `PlayerController` that owns it.

**Rarity system:**
- `BuffRarity` enum (`Combat/Buffs/BuffRarity.cs`) — `Common / Uncommon / Rare / Epic / Legendary`.
- `RarityVisualsSO` (`Combat/Buffs/RarityVisualsSO.cs`) — one SO asset per rarity. Carries `_baseWeight` (drop weight), `_accentColor`, `_frameSprite`, `_backgroundSprite`, `_cardFxPrefab` (optional particle/glow prefab spawned onto the card). Identity + Visuals tabs.
- `BuffSO` adds `[Required] _rarityVisuals : RarityVisualsSO`; exposes `RarityVisuals` (direct ref) and `Rarity` (forwarded enum, falls back to `Common` if unassigned).
- `BuffSelector` weight formula: `rarityWeight * (active ? 1 + activeBoost : 1)` where `rarityWeight = buff.RarityVisuals.BaseWeight` (fallback 1f if null). Rarer rarities ⇒ lower weight ⇒ rarer roll. Active boost still stacks multiplicatively.
- `BuffCard` reads `buff.RarityVisuals` in `Bind` and applies sprite + accent color to `_frameImage` / `_backgroundImage`, optionally tints `_nameText`, and instantiates `CardFxPrefab` under `_fxRoot`. Spawned FX is destroyed in `OnDisable` and before each re-bind.
- **OCP for rarity:** new tier = add enum entry + create new `RarityVisualsSO` asset. Zero code changes to selector, card, or manager.

**OCP compliance:** New buff type = new `BuffSO` subclass. No `BuffManager` changes.

---

### 9. Helpers

| Class | Key members |
|---|---|
| `CamHelpers` | `Cam` (cached, refreshed on scene load), `GetCamFlatForward()` (XZ-flat normalized) |
| `CursorHelpers` | `Hide(confine)`, `Show(confine)`, `Toggle(confine)` |
| `Helpers` (static) | `GetWaitForSeconds(seconds)` (cached `WaitForSeconds` via `WaitFor`), `LerpValueAsync<T>(from, to, duration, lerpFunc, onUpdate, ct)` — generic UniTask lerp; replaces the old `LerpValue` `IEnumerator`. Cancel via the passed `CancellationToken`. |

`CamHelpers` subscribes to `SceneManager.sceneLoaded` once (static ctor guard) to refresh the `Camera.main` cache across scene transitions.

The `Core.Helpers` asmdef now references the UniTask assembly (GUID `f51ebe6a0ceec4240a699833d6309b23`) so helpers can return `UniTask`/`UniTaskVoid` directly.

---

### 10. Singletons
| Class | Behavior |
|---|---|
| `Singleton<T>` | Lazy find, no DontDestroyOnLoad |
| `PersistentSingleton<T>` | DontDestroyOnLoad, destroys duplicates |
| `RegulatorSingleton<T>` | DontDestroyOnLoad, keeps newest instance |

---

### 11. Time System
**Entry point:** `TimeManager` (`PersistentSingleton<TimeManager>`) at `Core/Time/`
**Purpose:** Single owner of `UnityEngine.Time.timeScale`. Stacked, key-based modifier API so concurrent slow-mo / hit-stop / pause requests don't clobber each other.

**API:**
- `PushScale(string key, float scale)` — register or replace a modifier
- `PopScale(string key)` — remove a modifier
- `PushScaleFor(string key, float scale, float realSeconds)` — auto-pops after N unscaled seconds (hit-stop, freeze-frames)
- `ClearAll()` — wipe all modifiers, restore 1.0
- `CurrentScale`, `DeltaTime`, `UnscaledDeltaTime` getters

**Combination rule:** `min` of all active modifiers (slowest request wins). A 0 modifier always dominates — that's how pause / hit-stop work.

**Pause convention:** no dedicated API — pause = `TimeManager.Instance.PushScale("Pause", 0f)`.

**Event:** `TimeScaleChangedEvent(float NewScale)` raised on every effective change. UI / VFX / audio-pitch systems should subscribe instead of polling. The event lives in Core since the payload is a primitive.

**Caveats:**
- Long-running tweens spawned during slow-mo will run at the slowed rate unless `useUnscaledTime: true` is passed to PrimeTween. UI animations should opt into unscaled time.
- The editor toolbar timescale slider (`MainToolbarTimescaleSlider`) writes `Time.timeScale` directly and will fight TimeManager at runtime. It's editor-only debug — leave it; TimeManager overwrites on the next change.
- `OnDestroy` restores `timeScale = 1` so scene reloads don't strand the engine at 0.

---

## Key Interfaces
| Interface | Purpose |
|---|---|
| `IWeapon` | `WeaponData`, `Transform`, equip/unequip — no downcasting required; stat modifiers no longer flow through weapons (they mutate `CharacterStats.Current` directly) |
| `IProjectileLaunchData` | `Speed`, `Damage`, `Range` — implemented by `RangedWeapon`; decouples projectiles from weapon types |
| `IEnemyContext` | Minimal context passed to all enemy strategies — `transform`, `Rb`, `VisualRoot`, `VrOgScale`, `VrOgRotation`, `Data`, runtime `Damage` |
| `IDamageable` | `TakeDamage(float)` + `event Action<float> OnDamaged` — implemented by EnemyController; subscribers (HitFlash, etc.) depend on this, never the concrete controller |
| `IMovementStrategy` | `Move(IEnemyContext, Transform)` — pluggable enemy movement |
| `IAttackStrategy` | `Tick(float dt)` / `StartAttack` / `Interrupt` / `IsReady` / `ShouldFaceTarget` — pluggable enemy attack with cooldown; ShouldFaceTarget is a runtime flag toggled by the anim |
| `IAttackAnimation` | `Build(IEnemyContext, Transform, Action onStrike, Action onComplete, Action<bool> setFaceTarget)` / `OnInterrupt(IEnemyContext)` — pluggable attack animation; setFaceTarget toggles the strategy's facing flag mid-anim |
| `IState` | State machine contract (`OnEnter/Tick/FixedTick/GetTransition/OnExit`) |
| `IEvent` | Marker for event bus events |

---

## External Dependencies
| Package | Usage |
|---|---|
| Addressables | Weapon prefabs, weapon icons, Flyweight prefabs |
| UniTask | Async asset loading (`LoadPrefabAsync`, `LoadWeaponAssetsAsync`) |
| PrimeTween | Melee attack animations, gun recoil, enemy spawn/idle/attack tweens |
| Unity Input System | Player input via generated `MyInputActions` |

---

## SOLID Scorecard
- **S** — Good. Each class has a single reason to change. `WeaponData` / `RangedWeaponData` / `MeleeWeaponData` are purely data (no mutation methods). Buff system split into `BuffManager` (lifecycle) / `ExpProgression` (XP curve) / `BuffSelector` (picker) — three independent reasons to change. Spawn pipeline split: `PlayerSpawner` (load + instantiate + broadcast spawn) vs `PlayerController` (runtime behaviour + stat modifiers) — neither owns the other's reason to change.
- **O** — Good. Weapons, enemies, strategies, attack anims, buffs, projectile launchers, and now **characters** (new `CharacterInfos` asset = new playable character, zero code change to spawner/controller) all extend via new subclasses/assets.
- **L** — Good. `RangedWeapon`/`MeleeWeapon` are substitutable as `IWeapon`; all strategies substitutable via `IMovementStrategy`/`IAttackStrategy`/`IAttackAnimation`; `RangedWeapon` substitutable as `IProjectileLaunchData`. `AddWeaponBuff.Remove` now actually unequips (raises `RemoveWeaponEvent`) — `BuffSO` subtypes are behaviorally substitutable for the Command contract.
- **I** — Good. `IWeapon`, `IProjectileLaunchData`, `IEnemyContext` are small and focused. No fat interfaces.
- **D** — Good. `WeaponManager` depends on `IWeapon` (not `Weapon`) and learns the player transform via `PlayerSpawnedEvent` (no direct ref to `PlayerController`). `EnemyManager` likewise — no more `FindGameObjectWithTag("Player")` at scene load. Strategies depend on `IEnemyContext` (not `EnemyController`). `Projectile` depends on `IProjectileLaunchData` (not `RangedWeaponData`). Weapons depend on `CharacterStats.Current` as the live stat source — read-through, never cached. Remaining concrete dependency: `Weapon` base on `WeaponData` SO — acceptable Unity tradeoff.
