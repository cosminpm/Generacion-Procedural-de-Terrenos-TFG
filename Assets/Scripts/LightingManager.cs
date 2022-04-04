
using System;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;
    [SerializeField, Range(0,24)] private float TimeOfDay;
    
    public float speedTimePass = 0.01f;

    private void Update()
    {
        if (Preset == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            UpdateLighting(TimeOfDay / 24f );
        }
        else
        {
            UpdateLighting(TimeOfDay/24f);
        }
    }

    private void UpdateLighting(float time)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(time);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(time);

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(time);
            DirectionalLight.transform.rotation = Quaternion.Euler(new Vector3((time*360f)-90f,-170f,0));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    DirectionalLight = l;
                }
            }
        }
    }
}
