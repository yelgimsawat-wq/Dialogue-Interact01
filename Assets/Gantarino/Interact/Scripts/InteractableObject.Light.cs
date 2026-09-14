using UnityEngine;

public partial class InteractableObject
{
    [Header("Light Config")]
    [SerializeField]
    private Light targetLight;

    partial void LightUpdate()
    {
        if (targetLight == null)
        {
            Debug.LogWarning($"Light component is not assigned on {gameObject.name}.");
            return;
        }

        targetLight.enabled = !targetLight.enabled;
    }
}