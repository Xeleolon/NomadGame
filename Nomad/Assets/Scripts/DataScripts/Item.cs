using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "NomadTeam/Item")]
public class Item : ScriptableObject
{
    new public string name = "New Item";
    public Sprite icon = null;

}
