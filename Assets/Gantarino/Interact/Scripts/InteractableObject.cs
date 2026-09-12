using UnityEngine;

public enum InteractableType
{
    Npc
}

public partial class InteractableObject : MonoBehaviour, IInteractable
{
    
    [SerializeField] 
    private InteractableType InteractableType;
    private string InteractText => "Press E to interact with " + gameObject.name + ".";

    public void Interact()
    {
        switch (InteractableType)
        {
            case InteractableType.Npc:
                NpcUpdate();
                break;
        }
    }

    public string InteractionText { get { return InteractText; } }
}
