using UnityEngine;

public class Melee : Weapon
{
    protected override void Attack()
    {
        base.Attack();
        Debug.Log("Melee attack!");
    }
}
