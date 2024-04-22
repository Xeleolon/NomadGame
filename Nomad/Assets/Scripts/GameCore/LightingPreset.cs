using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Lighting Preset", menuName = "NomadTeam/Lighting Preset")]
public class LightingPreset : ScriptableObject
{
    public Gradient ambientColor;
    public Gradient directionalColor;
    public Gradient fogColor;
    public Gradient moonColor;
}
