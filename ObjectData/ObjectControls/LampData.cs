using UnityEngine;

public class LampData : MonoBehaviour
{
    public bool isOn;
    public GameObject lightObject;

    [Range(0.0f, 10.0f)]
    public float onEmissiveIntensity = 5;

    [Range(0.0f, 10.0f)]
    public float offEmissiveIntensity = 0;

    private Renderer[] lightBodies;
    private Light light;
    private Color emissiveColor;

    private void Awake()
    {
        lightBodies = GetComponentsInChildren<Renderer>();
        light = GetComponentInChildren<Light>();

        if (light == null)
        {
            Debug.LogWarning("No Light component found on the lamp's children.");
            return;
        }

        emissiveColor = light.color;
    }

    public void Switch()
    {
        if (light == null || lightBodies.Length == 0)
        {
            Debug.LogWarning("LampData requires a Light component and Renderer components to function.");
            return;
        }

        // Set light enabled state
        light.enabled = isOn;

        // Determine emissive intensity
        float currentEmissiveIntensity = isOn ? onEmissiveIntensity : offEmissiveIntensity;

        // Update emissive color for each renderer
        foreach (var renderer in lightBodies)
        {
            // Check if the material supports emissive property
            if (renderer.material.HasProperty("_EmissiveColor"))
            {
                renderer.material.SetColor("_EmissiveColor", emissiveColor * currentEmissiveIntensity);
            }
            else
            {
                Debug.LogWarning("The material does not support an emissive color property.");
            }
        }
    }
}