# Architecture Overview

> Keep this file up to date after every session that adds or changes system structure.
> Claude reads this instead of raw source files to save tokens.

---

## Folder Map (`Assets/Scripts/Runtime/`)

```
EventBus/         — Generic type-safe event system
Singletons/       — Three singleton base classes
Attributes/       — Custom inspector attributes (LokiAttributes)
StateMachine/     — Generic state machine (Tick + FixedTick)
InputSystem/      — Input reader (ScriptableObject-based)
HitboxStuff/      — IDamageable interface
Factory/
  Flyweight/      — Object pool core + Projectile types
  Weapon/
    AttackAnimation/ — WeaponAttackAnimSO (abstract) + StabAttackAnimSO + SwingAttackAnimSO
    WeaponFactory, Weapon, Gun, Melee, WeaponData, GunData, MeleeData
Enemy/
  States/         — Enemy state machine states
  Strategies/
    Movement/     — IMovementStrategy + data ScriptableObjects
    Attack/       — IAttackStrategy + data ScriptableObjects
  AttackAnimData/ — IAttackAnimation + data ScriptableObjects
Player/
  PlayerStates/   — Player state machine states (Idle/Move/Hurt)
Managers/
  EnemyManager
  WeaponManager   — equip/unequip weapons, orbital positioning; initialized by PlayerController
  Buff/
    BuffSO.cs     — abstract Command base (Apply / Remove)
    BuffManager.cs — invoker; holds List<BuffSO> active buffs + _testBuffs for editor testing
    Weapon/       — EquipWeaponBuffSO
    Stat/         — (future)
    Ability/      — (future)
    Status/       — (future)
Helpers/          — Static helpers + extension methods
```

---

## Core Systems

### 1. Flyweight / Object Pool
**Entry point:** `FlyweightFactory` (Singleton)  
**Flow:** `FlyweightSettings.Create()` → `Flyweight` instance → pooled via `IObjectPool<Flyweight>`  
**Key types:**
- `FlyweightSettings` (abstract SO) — intrinsic state + pool config + Addressable prefab ref. `OnGet` detaches from parent and activates; `OnRelease` deactivates.
- `Flyweight` (abstract MB) — base for anything pooled; calls `ReturnToPool()`
- `FlyweightFactory` — single pool dictionary keyed by settings asset

**Extends to:** Projectiles, Enemies, attack VFX — anything spawned frequently.

---

### 2. Weapon System
**Hierarchy:**
```
WeaponData (abstract SO)
  ├── GunData      → Gun (Weapon)    — ammo/reload, recoil animation on shoot
  └── MeleeData    → Melee (Weapon)  — AttackAnim : WeaponAttackAnimSO

Weapon (abstract MB) implements IWeapon
  ├── Gun          — fires Projectiles, tracks ammo/reload, PrimeTween recoil on attack
  └── Melee        — plays WeaponAttackAnimSO on attack (stab or swing)

WeaponAttackAnimSO (abstract SO)
  ├── StabAttackAnimSO   — translates _weaponModel along local Z (stabPos = origin + forward * range) then returns
  └── SwingAttackAnimSO  — lunge toward target (flat XZ, distance-clamped) + X-axis rotation, durations scaled by attackInterval
```

**Auto-targeting:** `Weapon.Update()` calls `Physics.OverlapSphereNonAlloc` each frame (non-alloc, 20-slot buffer, Enemy layer mask) to find the closest enemy. When a target is in range, `FaceTarget()` rotates the weapon (XZ-flat look) and `Attack()` fires automatically. No player input required — weapons are autonomous agents.

**Attack rate:** `CanAttack` gates attack on elapsed time ≥ `1f / AttackRate`. On equip, `_attackRateElapsedTime` is pre-charged to the full interval so the weapon fires immediately.

**Gun specifics (`GunData`):**
- Ammo: `AmmoPerMagazine`, `MagazineCapacity`, `ReloadTime`
- Ballistics: `BulletSpeed`, `BulletDamage`
- Spread curves: `SpreadOnShoot`, `ReturnDuration`, `MaxSpreadThreshold`, `SpreadDuration`, `SpreadCurve`, `ReturnCurve` (data defined, not yet consumed by Gun)
- Recoil: `RecoilDistance`, `RecoilDuration`, `RecoilReturnDuration`, `RecoilEase`, `RecoilReturnEase` — `_weaponModel` pushed backward then returned via PrimeTween `Sequence`

**Melee specifics:**
- `Melee.Attack()` passes `_currentTarget` to `AttackAnim.Play()` so animations can orient toward the enemy
- `StopAnimation()` / `OnUnequip()` snap the model back to `_originLocalPos`/`_originLocalRot`

**`WeaponAttackAnimSO.Play` signature:**  
`Play(Transform weaponModel, Transform target, Vector3 originLocalPos, float range, Quaternion originLocalRot, float attackInterval)`  
All concrete implementations scale their tween durations by `attackInterval` (= `1f / AttackRate`).

**Factory:** `WeaponFactory` (Singleton) — `GetWeapon(WeaponData)` instantiates from `data.WeaponPrefab`.  
**Equip system:** `WeaponManager` (Singleton) — owns `List<IWeapon>`, orbits weapons around player at `_weaponOrbitRadius`. `RefreshWeaponPositions()` recalculates all slots after any equip/unequip. Initialized by `PlayerController.Awake()`.  
**Assets:** `WeaponData.LoadWeaponAssetsAsync()` loads prefab + icon via Addressables. `GunData` override also loads `projectileSettings.LoadPrefabAsync()`.

**OCP compliance:** New weapon type = new `WeaponData` subclass + new `Weapon` subclass. New melee anim = new `WeaponAttackAnimSO` subclass. No existing code changes.

---

### 3. Projectile System
**Hierarchy:**
```
ProjectileSettings (abstract SO) : FlyweightSettings
  └── StraightProjectileSettings  → StraightProjectile (Projectile)
                                     + collisionLayers: LayerMask (filters OnTriggerEnter)

Projectile (abstract MB) : Flyweight
  └── StraightProjectile          — Rigidbody-driven, distance-limited, ContinuousDynamic collision
```

**Trail management:** `Projectile` owns a `TrailRenderer`. `OnDisable` clears + disables the trail (prevents teleport artifact). `ResetTrail()` re-enables it on fire. `StraightProjectile.ShootProjectile()` calls `ResetTrail()` after positioning.

**Spawn:** `GunData.projectileSettings` → `FlyweightFactory.Spawn()` → `ShootProjectile(tip.position, tip.position + tip.forward, gunData)`  
**Despawn:** `StraightProjectile.OnTriggerEnter` filters by `collisionLayers` bitmask; on match or max distance → `Despawn()` → `ReturnToPool()`

---

### 4. Enemy System
**Controller:** `EnemyController : Flyweight, IDamageable`  
**Data:** `EnemyData : FlyweightSettings` — holds HP, speed, strategy SOs, split/fracture prefabs  
**Init flow:** `EnemyData.Create()` → `EnemyController.EnemyInit(data, movementStrategy, attackStrategy)`

**State machine:** Spawn → Idle → Move → Attack → Die  
**Strategies (Strategy Pattern):**
- `IMovementStrategy` — `Move(owner, target)` — Chaser, Ranged, Zigzag
- `IAttackStrategy` — `StartAttack / Interrupt / IsReady` — StandardMeleeAttack
- `IAttackAnimation` — `Build / OnInterrupt` — StandardMeleeAnim

**OCP compliance:** New enemy behavior = new strategy SO + strategy class. EnemyController untouched.

---

### 5. State Machine
**Generic:** `StateMachine` + `IState` + `State<TOwner>`  
**`IState` contract:** `OnEnter`, `Tick`, `FixedTick`, `GetTransition` (null = stay), `OnExit`  
**`State<TOwner>`:** `FixedTick()` is virtual (no-op default) — override only when physics is needed.  
**StateMachine:** `Tick()` checks `GetTransition()` first, then calls `Current.Tick()`. `FixedTick()` delegates directly to current state. `ForceTransition<T>()` and `ForceTransition(IState)` for imperative transitions.

**Used by:**
- `PlayerController` — `Update()` → `SM.Tick()`, `FixedUpdate()` → `SM.FixedTick()`
  - `PlayerIdle` / `PlayerMove` — movement physics in `PlayerMove.FixedTick()` via `Rb.MovePosition`
  - `PlayerHurt` — applies `PendingKnockback`, triggered via `SM.ForceTransition<PlayerHurt>()`
- `EnemyController` — Spawn/Idle/Move/Attack/Die

---

### 6. Event Bus
**Generic type-safe:** `EventBus<T>` where `T : IEvent`  
**Binding:** `EventBinding<T>` — supports `Action<T>` and no-arg `Action`  
**Init:** `EventBusUtil.InitializeAllBuses()` — reflection discovers all `IEvent` implementations  
**Current events:** `TestEvent`, `PlayerExampleEvent` (placeholder — system ready for real events)

---

### 7. Input System
**`InputReader`** : ScriptableObject — wraps generated `MyInputActions`  
**`GameplayActions`** — exposes `event Action<Vector2> onMove`  
Currently only Move is mapped. Input action map switching supported.

---

### 8. Buff System
**Pattern:** Command — each `BuffSO` subclass is a self-contained command with `Apply()` / `Remove()`.  
**Invoker:** `BuffManager` (Singleton) — `ApplyBuff(BuffSO)` / `RemoveBuff(BuffSO)`, tracks `List<BuffSO> _activeBuffs`. Has `_testBuffs` (SerializeField) applied in `Start()` for editor testing. Same SO reference stored N times — removal takes the first match (correct stacking behavior).  
**Concrete commands:**
- `EquipWeaponBuffSO` — calls `LoadWeaponAssetsAsync()` (async), then `WeaponManager.EquipWeapon`. `Remove()` calls `UnequipWeapon`.

**OCP compliance:** New buff type = new `BuffSO` subclass in the appropriate `Buff/<Category>/` folder. No `BuffManager` changes.

---

### 9. Helpers

| Class | Key members |
|---|---|
| `CamHelpers` | `Cam` (cached, refreshed on scene load), `GetCamFlatForward()` (XZ-flat normalized) |
| `CursorHelpers` | `Hide(confine)`, `Show(confine)`, `Toggle(confine)` |

`CamHelpers` subscribes to `SceneManager.sceneLoaded` once (static ctor guard) to refresh the `Camera.main` cache across scene transitions.

---

### 10. Singletons
| Class | Behavior |
|---|---|
| `Singleton<T>` | Lazy find, no DontDestroyOnLoad |
| `PersistentSingleton<T>` | DontDestroyOnLoad, destroys duplicates |
| `RegulatorSingleton<T>` | DontDestroyOnLoad, keeps newest instance |

---

## Key Interfaces
| Interface | Purpose |
|---|---|
| `IWeapon` | Equip/unequip contract for all weapons |
| `IDamageable` | `TakeDamage(float)` — implemented by EnemyController |
| `IMovementStrategy` | `Move(owner, target)` — pluggable enemy movement |
| `IAttackStrategy` | `StartAttack / Interrupt / IsReady` — pluggable enemy attack |
| `IAttackAnimation` | `Build / OnInterrupt` — pluggable attack animation |
| `IState` | State machine contract (`OnEnter/Tick/FixedTick/GetTransition/OnExit`) |
| `IEvent` | Marker for event bus events |

---

## External Dependencies
| Package | Usage |
|---|---|
| Addressables | Weapon prefabs, weapon icons, Flyweight prefabs |
| UniTask | Async asset loading (`LoadPrefabAsync`, `LoadWeaponAssetsAsync`) |
| PrimeTween | Melee attack animations, gun recoil, enemy spawn/idle tweens |
| Unity Input System | Player input via generated `MyInputActions` |

---

## SOLID Scorecard (current state)
- **S** — Good. Each class has a clear responsibility.
- **O** — Good. Weapons, enemies, strategies, attack anims, and buffs all extend via new subclasses.
- **L** — Good. `Gun`/`Melee` are substitutable as `IWeapon`; strategies are substitutable.
- **I** — Good. Interfaces are small and focused. `IState` now includes `FixedTick` — still cohesive.
- **D** — Mostly good. `Weapon` base depends on `WeaponData` (concrete SO) — acceptable Unity tradeoff. `Gun` and `Melee` downcast to their typed data internally.
