using UnityEngine;

public interface IEnemyContext
{
    Transform transform { get; }
    Rigidbody Rb { get; }
    Transform VisualRoot { get; }
    Vector3 VrOgScale { get; }
    Quaternion VrOgRotation { get; }
    EnemyData Data { get; }
    float Damage { get; }
}
