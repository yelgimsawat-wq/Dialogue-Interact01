using UnityEngine;
using System.Collections.Generic;


public partial class InteractableObject
{
    [Header("Light Config")]
    [SerializeField]
    private List<Light> targetlights = new List<Light>();

    partial void LightUpdate()
    {
        foreach(Light targetLight in targetlights)
        {
            if (targetLight != null)
            {
                targetLight.enabled = !targetLight.enabled;
            }
        }
    }
}