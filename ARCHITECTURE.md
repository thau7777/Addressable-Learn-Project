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
  Flyweight/      — Object pool core + Projectile types + OneShotVfx
  Weapon/
    AttackAnimation/ — WeaponAttackAnimSO (abstract) + StabAttackAnimSO + SwingAttackAnimSO
    WeaponFactory, Weapon, Gun, Melee, WeaponData, GunData, MeleeData
Enemy/
  IEnemyContext.cs — minimal context interface passed to all strategies
  States/          — Enemy state machine states
  Strategies/
    Movement/      — IMovementStrategy + data ScriptableObjects
    Attack/        — IAttackStrategy + data ScriptableObjects
  AttackAnimData/  — IAttackAnimation + data ScriptableObjects
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

**Extends to:** Projectiles, Enemies, OneShotVfx — anything spawned frequently.

**OneShotVfx subsystem:**
- `OneShotVfxSettings` (SO) — `dealsDamage`, `baseDamage`, `hitboxLayers`, `hitboxActivateDelay`, `hitboxActiveDuration`; overrides `OnGet` to skip `SetActive` — caller must `FlyweightInit` + `OneShotVfxInit()` after getting from pool
- `OneShotVfx` (MB : Flyweight) — plays `ParticleSystem` or `VisualEffect` (behind `#if UNITY_VFX_GRAPH`) on init; coroutine manages hitbox on/off window; auto-returns to pool when effect ends; `OnTriggerEnter` applies `_currentDamage` to `IDamageable`

---

### 2. Weapon System
**Hierarchy:**
```
WeaponData (abstract SO)
  ├── GunData      → Gun (Weapon, IProjectileLaunchData) — ammo/reload, recoil animation on shoot
  └── MeleeData    → Melee (Weapon)                      — AttackAnim : WeaponAttackAnimSO

Weapon (abstract MB) implements IWeapon
  ├── Gun          — fires Projectiles, tracks ammo/reload, PrimeTween recoil on attack
  └── Melee        — plays WeaponAttackAnimSO on attack (stab or swing)

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

**Auto-targeting:** `Weapon.Update()` is sealed (not virtual). It calls `Physics.OverlapSphereNonAlloc` each frame (non-alloc, 20-slot buffer, Enemy layer mask) to find the closest enemy. When a target is in range, `FaceTarget()` rotates the weapon and `Attack()` fires automatically. Subclasses extend per-frame logic via `protected virtual void OnUpdate()` (Template Method).

**Attack rate:** `CanAttack` gates on elapsed time ≥ `1f / AttackRate`. On equip, pre-charged to full interval so weapon fires immediately.

**Gun specifics (`GunData` / `Gun`):**
- `GunData` is **immutable at runtime** — `BaseBulletDamage` is the SO base value. Never mutate SO fields directly.
- `Gun` holds `_runtimeBulletDamage` (copied from `BaseBulletDamage` in `SetWeaponData`). Damage buffs call `Gun.ModifyDamage(delta)` on the instance.
- `Gun` implements `IProjectileLaunchData` (explicit) — passes `this` to `Projectile.ShootProjectile`, so the projectile always reads the runtime damage, not the SO.
- Ammo: `AmmoPerMagazine`, `MagazineCapacity`, `ReloadTime`
- Recoil: `RecoilDistance/Duration/ReturnDuration/Ease` — `_weaponModel` pushed backward then returned via PrimeTween `Sequence`

**Melee specifics:**
- `Melee.Attack()` passes `_currentTarget` to `AttackAnim.Play()` so animations orient toward the enemy
- `StopAnimation()` / `OnUnequip()` snap the model back to origin

**`WeaponAttackAnimSO.Play` signature:**  
`Play(Transform weaponModel, Transform target, Vector3 originLocalPos, float range, Quaternion originLocalRot, float attackInterval)`

**Factory:** `WeaponFactory` (Singleton) — `GetWeapon(WeaponData)` instantiates from `data.WeaponPrefab`.  
**Equip system:** `WeaponManager` (Singleton) — owns `List<IWeapon>`, accesses transforms via `IWeapon.Transform` (no downcasting). Orbits weapons around player at `_weaponOrbitRadius`. `RefreshWeaponPositions()` recalculates all slots after equip/unequip. Initialized by `PlayerController.Awake()`.  
**Assets:** `WeaponData.LoadWeaponAssetsAsync()` loads prefab + icon via Addressables. `GunData` override also loads `projectileSettings.LoadPrefabAsync()` and the impact VFX prefab.

**OCP compliance:** New weapon type = new `WeaponData` subclass + new `Weapon` subclass. New melee anim = new `WeaponAttackAnimSO` subclass. No existing code changes.

---

### 3. Projectile System
**Hierarchy:**
```
IProjectileLaunchData (interface)
  — BulletSpeed, BulletDamage, WeaponRange
  — implemented by Gun (explicit, using _runtimeBulletDamage)

ProjectileSettings (abstract SO) : FlyweightSettings
  + dealsDamage: bool
  + onImpactVfx: OneShotVfxSettings (optional)
  └── StraightProjectileSettings  → StraightProjectile (Projectile)
                                     + collisionLayers: LayerMask

Projectile (abstract MB) : Flyweight
  + ShootProjectile(startPos, targetPos, IProjectileLaunchData) — decoupled from GunData
  + OnHit(Collider) — deals damage via IDamageable + spawns onImpactVfx
  └── StraightProjectile — Rigidbody-driven, distance-limited, ContinuousDynamic collision
```

**Key design:** `Projectile` takes `IProjectileLaunchData` — not `GunData`. Any system (turret, trap, enemy) can fire projectiles by implementing the interface.

**Trail management:** `Projectile` owns a `TrailRenderer`. `OnDisable` clears + disables the trail. `ResetTrail()` re-enables it on fire.

**Spawn:** `GunData.projectileSettings` → `FlyweightFactory.Spawn()` → `ShootProjectile(tip.position, tip.position + tip.forward, this)` where `this` is the `Gun`.  
**Despawn:** `StraightProjectile.OnTriggerEnter` filters by `collisionLayers`; on match or max distance → `Despawn()` → trail fade coroutine → `ReturnToPool()`.

---

### 4. Enemy System
**Controller:** `EnemyController : Flyweight, IDamageable, IEnemyContext`  
**Data:** `EnemyData : FlyweightSettings` — holds HP, speed, strategy SOs, fracture/split settings  
**Init flow:** `EnemyData.Create()` → `EnemyController.EnemyInit(data, movementStrategy, attackStrategy)`

**`IEnemyContext` interface** (in `Enemy/IEnemyContext.cs`):
```csharp
Transform transform { get; }
Rigidbody Rb { get; }
Transform VisualRoot { get; }
Vector3 VrOgScale { get; }
Quaternion VrOgRotation { get; }
EnemyData Data { get; }
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
| `IAttackStrategy` | `Tick(float dt)` / `StartAttack(IEnemyContext, Transform, Action)` / `Interrupt(IEnemyContext)` | `Tick` must be called each frame; `IsReady` gates re-attack |
| `IAttackAnimation` | `Build(IEnemyContext, Transform, Action onStrike, Action onComplete)` / `OnInterrupt(IEnemyContext)` | StandardMeleeAnim |

**OCP compliance:** New enemy behavior = new strategy SO + strategy class. `EnemyController` untouched.

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

**Damage buffs (future):** call `Gun.ModifyDamage(delta)` on the weapon instance — never mutate `GunData` SO fields.

**OCP compliance:** New buff type = new `BuffSO` subclass. No `BuffManager` changes.

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
| `IWeapon` | `WeaponData`, `Transform`, equip/unequip contract — no downcasting required |
| `IProjectileLaunchData` | `BulletSpeed`, `BulletDamage`, `WeaponRange` — implemented by `Gun`; decouples projectiles from weapon types |
| `IEnemyContext` | Minimal context passed to all enemy strategies — `transform`, `Rb`, `VisualRoot`, `VrOgScale`, `VrOgRotation`, `Data` |
| `IDamageable` | `TakeDamage(float)` — implemented by EnemyController |
| `IMovementStrategy` | `Move(IEnemyContext, Transform)` — pluggable enemy movement |
| `IAttackStrategy` | `Tick(float dt)` / `StartAttack` / `Interrupt` / `IsReady` — pluggable enemy attack with cooldown |
| `IAttackAnimation` | `Build(IEnemyContext, ...)` / `OnInterrupt(IEnemyContext)` — pluggable attack animation |
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
- **S** — Good. Each class has a single reason to change. `GunData` is now purely data (no mutation methods).
- **O** — Good. Weapons, enemies, strategies, attack anims, buffs, and projectile launchers all extend via new subclasses/implementations.
- **L** — Good. `Gun`/`Melee` are substitutable as `IWeapon`; all strategies substitutable via `IMovementStrategy`/`IAttackStrategy`/`IAttackAnimation`; `Gun` substitutable as `IProjectileLaunchData`.
- **I** — Good. `IWeapon`, `IProjectileLaunchData`, `IEnemyContext` are small and focused. No fat interfaces.
- **D** — Good. `WeaponManager` depends on `IWeapon` (not `Weapon`). Strategies depend on `IEnemyContext` (not `EnemyController`). `Projectile` depends on `IProjectileLaunchData` (not `GunData`). Remaining concrete dependency: `Weapon` base on `WeaponData` SO — acceptable Unity tradeoff.
