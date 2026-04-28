using UnityEngine;

public abstract class Flyweight : MonoBehaviour
{
    [HideInInspector]
    public FlyweightSettings settings; // Intrinsic state
    public void FlyweightInitialize(Vector3 position, Quaternion? rotation = null, Transform parent = null)
    {
        transform.position = position;
        if(rotation.HasValue)
            transform.rotation = rotation.Value;
        else
            transform.rotation = Quaternion.identity;

        if (parent != null)
            transform.SetParent(parent);
    }
    public void ReturnToPool()
    {
        if(gameObject.activeSelf)
        FlyweightFactory.ReturnToPool(this);
    }
    
}
