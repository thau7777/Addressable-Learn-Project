using LokiInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField,TabGroup("References")] private InputReader _inputReader;
    [SerializeField,TabGroup("References")] private WeaponData _testMeleeWeaponData;
    [SerializeField,TabGroup("References")] private WeaponData _testWeaponData;

    [TabGroup("Movement")]
    [SerializeField] private float _moveSpeed = 5f;



    public Rigidbody Rb { get; private set; }
    public float MoveSpeed => _moveSpeed;
    [HideInInspector]
    public Vector3 inputDir;
    public Vector3 PendingKnockback { get; private set; }
    public StateMachine SM { get; private set; }


    private CrosshairController _crosshairController;
    private IWeapon _currentWeapon;

    #region Initialization
    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        _crosshairController = GetComponent<CrosshairController>();
        StatesInit();

        _testMeleeWeaponData.OnWeaponChose();
        _testWeaponData.OnWeaponChose();
    }
    private void Start()
    {
        //EquipWeapon(_testWeaponData);
    }
    private void OnEnable()
    {
        _inputReader.gameplayActions.onMove += OnMove;
        _inputReader.gameplayActions.onEquipWeapon += TestEquipWeapon;
        _inputReader.gameplayActions.onShoot += OnShoot;
        _inputReader.gameplayActions.onReload += OnReload;
    }
    private void OnDisable() 
    { 
        _inputReader.gameplayActions.onMove -= OnMove; 
        _inputReader.gameplayActions.onEquipWeapon -= TestEquipWeapon;
        _inputReader.gameplayActions.onShoot -= OnShoot;
        _inputReader.gameplayActions.onReload -= OnReload;
    }
    private void StatesInit()
    {
        SM = new StateMachine();
        SM.RegisterState(new PlayerIdle(this));
        SM.RegisterState(new PlayerMove(this));
        SM.RegisterState(new PlayerHurt(this));
        SM.Init<PlayerIdle>();
    }
    #endregion

    #region Loops
    private void Update()
    {
        SM.Tick();
        PlayerRotateHandler();
    }
    private void FixedUpdate() => SM.FixedTick(); // states handle everything
    #endregion

    #region Inputs
    private void OnMove(Vector2 dir)
    {
        inputDir.x = dir.x;
        inputDir.z = dir.y;
        inputDir = inputDir.normalized;
    }

    private void TestEquipWeapon(uint weaponIndex)
    {
        if (weaponIndex == 2)
            EquipWeapon(_testMeleeWeaponData);
        else if (weaponIndex == 1)
            EquipWeapon(_testWeaponData);
    }

    private void OnShoot(bool isShooting)
    {
        if (_currentWeapon != null)
            _currentWeapon.OnAttackTrigger(isShooting);
    }

    private void OnReload()
    {
        if (_currentWeapon == null) return;

        if (_currentWeapon is Gun gun)
        {
            gun.Reload();
        }
    }

    #endregion

    private void PlayerRotateHandler()
    {
        if (!CursorHelpers.GetCursorWorldPositionOnFlatSurface(Mouse.current.position.ReadValue(), out Vector3 worldPos)) return;

        Vector3 lookDir = worldPos - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 100f);
        }
    } 
    private void EquipWeapon(WeaponData weaponData)
    {
        if(_currentWeapon != null && _currentWeapon.IsEquals(weaponData)) return; 

        IWeapon weapon = WeaponFactory.Instance.GetWeapon(weaponData);
        if(weapon != null)
        {
            _currentWeapon?.OnUnequip();
            _currentWeapon = weapon;

            if (!_currentWeapon.IsInitialized) 
                _currentWeapon.RegisterActionOnAttack(() => _crosshairController.OnAttack());

            _currentWeapon.OnEquip(transform);
        }
        else
        {
            _currentWeapon = null;
        }

        ICrosshair crosshair = CrosshairFactory.Instance.GetCrosshair(weaponData);
        if(crosshair != null)
            _crosshairController.InitCrosshair(crosshair as Crosshair);
        else
            Debug.LogWarning($"No crosshair found for weapon, not even default: {weaponData.name}");
    }
    public void ApplyKnockback(Vector3 force)
    {
        PendingKnockback = force;
        SM.ForceTransition<PlayerHurt>();
    }
}