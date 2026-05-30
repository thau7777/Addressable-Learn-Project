using System;
using UnityEngine;

public interface IDamageable
{
    event Action<float> OnDamaged;
    void TakeDamage(float damage);
}
