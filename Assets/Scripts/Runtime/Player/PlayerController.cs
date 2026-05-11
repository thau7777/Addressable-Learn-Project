using LokiInspector;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, TabGroup("References")] private InputReader _inputReader;

    [TabGroup("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    public Rigidbody Rb { get; private set; }
    public float MoveSpeed => _moveSpeed;
    [HideInInspector] public Vector3 inputDir;
    public Vector3 PendingKnockback { get; private set; }
    public StateMachine SM { get; private set; }

    #region Initialization
    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        StatesInit();
        WeaponManager.Instance.Initialize(transform);
    }

    private void OnEnable() => _inputReader.gameplayActions.onMove += OnMove;
    private void OnDisable() => _inputReader.gameplayActions.onMove -= OnMove;

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
    private void Update() => SM.Tick();
    private void FixedUpdate() => SM.FixedTick();
    #endregion

    #region Inputs
    private void OnMove(Vector2 dir)
    {
        inputDir.x = dir.x;
        inputDir.z = dir.y;
        inputDir = inputDir.normalized;
    }
    #endregion

    public void ApplyKnockback(Vector3 force)
    {
        PendingKnockback = force;
        SM.ForceTransition<PlayerHurt>();
    }
}