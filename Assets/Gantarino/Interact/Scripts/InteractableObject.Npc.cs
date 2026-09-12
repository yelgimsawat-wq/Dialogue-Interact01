using UnityEngine;

public partial class InteractableObject
{
    private void NpcUpdate()
    {
        Debug.Log("Interacted with NPC: " + gameObject.name);
    }
}