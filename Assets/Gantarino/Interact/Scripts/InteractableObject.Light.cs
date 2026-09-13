using UnityEngine;

public partial class InteractableObject
{
    [SerializeField]
    private Light light;

    partial void LightUpdate()
    {
        if (light == null)
        {
            Debug.LogWarning("Light component is not assigned.");
            return;
        }

        light.enabled = !light.enabled;
    }

}

