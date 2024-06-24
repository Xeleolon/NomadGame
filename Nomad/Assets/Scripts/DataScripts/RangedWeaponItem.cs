using UnityEngine;
[CreateAssetMenu(fileName = "New Range Weapon", menuName = "NomadTeam/RangeWeapon")]

public class RangedWeaponItem : Item
{
    public float damage = 1;
    public float maxPower = 5;
    public float minPower = 3;
    public float drawSpeed = 0.2f;
}
