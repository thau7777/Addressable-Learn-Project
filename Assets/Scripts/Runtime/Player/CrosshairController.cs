using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup _targetGroup;
    [SerializeField]
    private GameObject _crosshairWorldObjectInstance;
    private Crosshair _currentCrosshair;
    private float _range = 10f;

    private void Start()
    {
        CursorHelpers.Hide(true);
        _targetGroup.AddMember(transform, 1f, 0f);
        _targetGroup.AddMember(_crosshairWorldObjectInstance.transform, .25f, 0f);
    }
    public void InitCrosshair(Crosshair crosshair)
    {
        _currentCrosshair = crosshair;
    }
    private void Update()
    {
        if (!_currentCrosshair || !CursorHelpers.GetCursorWorldPositionOnFlatSurface(Mouse.current.position.ReadValue(), out Vector3 worldPos)) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 playerXZ = new Vector3(transform.position.x, worldPos.y, transform.position.z);
        Vector3 offset = worldPos - playerXZ;

        if (offset.magnitude > _range)
            offset = offset.normalized * _range;

        _crosshairWorldObjectInstance.transform.position = playerXZ + offset;


        _currentCrosshair.transform.position = screenPos;
    }
    public void OnAttack()
    {
        _currentCrosshair.OnExecute();
    }
}