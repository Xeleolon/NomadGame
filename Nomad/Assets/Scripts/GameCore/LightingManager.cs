using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    //Refences
    [SerializeField] private Light directionLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private LightingPreset preset;
    [SerializeField] private float cycleSpeed = 1;
    [SerializeField] private bool pause;

    //Variables
    [SerializeField, Range(0, 24)] private float dayCycle;

    private void Update()
    {
        if (preset == null)
            return;

        if (Application.isPlaying && !pause)
        {
            dayCycle += Time.deltaTime * cycleSpeed;
            dayCycle %= 24; //clamos between 0 to 24
            UpdateLighting(dayCycle/24f);
        } 
        else
        {
            UpdateLighting(dayCycle/24f);
        }
    }
    

    private void UpdateLighting(float dayPercent)
    {
        
        RenderSettings.ambientLight = preset.ambientColor.Evaluate(dayPercent);
        RenderSettings.fogColor = preset.fogColor.Evaluate(dayPercent);
        
        if (directionLight != null)
        {
            directionLight.color = preset.directionalColor.Evaluate(dayPercent);
            directionLight.transform.localRotation = Quaternion.Euler(new Vector3((dayPercent * 360f) - 90f, - 170, 0));
        }
        if (moonLight != null)
        {
            moonLight.color = preset.moonColor.Evaluate(dayPercent);
            moonLight.transform.localRotation = Quaternion.Euler(new Vector3((dayPercent * 360f) + 90f, - 170, 0));
        }
    }

    private void OnValidate()
    {
        if (directionLight != null)
            return;
        
        if (RenderSettings.sun != null)
        {
            directionLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionLight = light;
                    return;
                }
            }
        }
    }
}
