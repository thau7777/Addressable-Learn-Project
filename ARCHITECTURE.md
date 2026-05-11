# Architecture Overview

> Keep this file up to date after every session that adds or changes system structure.
> Claude reads this instead of raw source files to save tokens.

---

## Folder Map (`Assets/Scripts/Runtime/`)

```
EventBus/         — Generic type-safe event system
Singletons/       — Three singleton base classes
Attributes/       — Custom inspector attributes (LokiAttributes)
StateMachine/     — Generic state machine
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
  PlayerStates/   — Player state machine states
Managers/
  EnemyManager
  WeaponManager   — equip/unequip weapons, orbital positioning; initialized by PlayerController
  Buff/
    BuffSO.cs     — abstract Command base (Apply / Remove)
    BuffManager.cs — invoker; holds List<BuffSO> active buffs
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
- `FlyweightSettings` (abstract SO) — intrinsic state + pool config + Addressable prefab ref
- `Flyweight` (abstract MB) — base for anything pooled; calls `ReturnToPool()`
- `FlyweightFactory` — single pool dictionary keyed by settings asset

**Extends to:** Projectiles, Enemies, attack VFX — anything spawned frequently.

---

### 2. Weapon System
**Hierarchy:**
```
WeaponData (abstract SO)
  ├── GunData      → Gun (Weapon)    — shake anim on shoot (ShakeMagnitude/Duration/Frequency)
  └── MeleeData    → Melee (Weapon)  — AttackAnim : WeaponAttackAnimSO

Weapon (abstract MB) implements IWeapon
  ├── Gun          — fires Projectiles, handles ammo/reload/spread, PrimeTween shake on attack
  └── Melee        — plays WeaponAttackAnimSO on attack (stab or swing)

WeaponAttackAnimSO (abstract SO)
  ├── StabAttackAnimSO   — translates _weaponModel along local Z then returns
  └── SwingAttackAnimSO  — lunge (local Z) + rotation around configurable axis, then returns
```
**Factory:** `WeaponFactory` (Singleton) — `GetWeapon(WeaponData)` instantiates and initializes.  
**Equip system:** `WeaponManager` (Singleton) — owns `List<IWeapon>`, orbits weapons around player. Initialized by `PlayerController.Awake()` via `Initialize(transform)`.  
**Assets:** `WeaponData.LoadWeaponAssets()` (callback) + `LoadWeaponAssetsAsync()` (UniTask) — async version used by `EquipWeaponBuffSO`.

**OCP compliance:** New weapon type = new `WeaponData` subclass + new `Weapon` subclass. New melee anim = new `WeaponAttackAnimSO` subclass. No existing code changes.

---

### 3. Projectile System
**Hierarchy:**
```
ProjectileSettings (abstract SO) : FlyweightSettings
  └── StraightProjectileSettings  → StraightProjectile (Projectile)

Projectile (abstract MB) : Flyweight
  └── StraightProjectile          — Rigidbody-driven, distance-limited
```
**Spawn:** `GunData.projectileSettings` → `FlyweightFactory.Spawn()` → `ShootProjectile(start, target, gunData)`  
**Despawn:** On trigger enter or max distance → `Despawn()` → `ReturnToPool()`

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
**Used by:** `PlayerController` (Idle/Move/Hurt) and `EnemyController` (Spawn/Idle/Move/Attack/Die)  
**Transition:** Each state's `GetTransition()` returns next `IState` or null to stay.

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
**Invoker:** `BuffManager` (Singleton) — `ApplyBuff(BuffSO)` / `RemoveBuff(BuffSO)`, tracks `List<BuffSO> _activeBuffs`. Same SO reference stored N times — removal takes the first match (correct stacking behavior).  
**Concrete commands:**
- `EquipWeaponBuffSO` — loads weapon assets async, then calls `WeaponManager.EquipWeapon`. `Remove()` calls `UnequipWeapon`.  

**OCP compliance:** New buff type = new `BuffSO` subclass in the appropriate `Buff/<Category>/` folder. No `BuffManager` changes.

---

### 9. Singletons
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
| `IState` | State machine contract |
| `IEvent` | Marker for event bus events |

---

## External Dependencies
| Package | Usage |
|---|---|
| Addressables | Weapon prefabs, weapon icons, Flyweight prefabs |
| UniTask | Async asset loading (`LoadPrefabAsync`, `LoadWeaponAssets`) |
| PrimeTween | Melee attack animation, enemy spawn/idle tweens |
| Unity Input System | Player input via generated `MyInputActions` |

---

## SOLID Scorecard (current state)
- **S** — Good. Each class has a clear responsibility.
- **O** — Good. Weapons, enemies, strategies all extend via new subclasses.
- **L** — Good. `Gun`/`Melee` are substitutable as `IWeapon`; strategies are substitutable.
- **I** — Good. Interfaces are small and focused.
- **D** — Mostly good. `Weapon` base class depends on `WeaponData` (concrete SO) — acceptable Unity tradeoff. `Gun` casts to `GunData` internally.
