using UnityEngine;

public abstract class Flyweight : MonoBehaviour
{
    [HideInInspector]
    public FlyweightSettings settings; // Intrinsic state
    public void FlyweightInit(Vector3 position, Quaternion? rotation = null, Transform parent = null)
    {
        transform.SetPositionAndRotation(position, rotation ?? Quaternion.identity);
        if (parent != null)
            transform.SetParent(parent);
    }
    public void ReturnToPool()
    {
        if(gameObject.activeSelf)
        FlyweightFactory.ReturnToPool(this);
    }
    
}
