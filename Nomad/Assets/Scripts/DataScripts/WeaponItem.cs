using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon", menuName = "NomadTeam/Weapon")]

public class WeaponItem : Item
{
    public float damage;
    [Range(1, 2)]
    public int type = 1;
    public float range = 3;
    public float actackSpeed = 3;
}
