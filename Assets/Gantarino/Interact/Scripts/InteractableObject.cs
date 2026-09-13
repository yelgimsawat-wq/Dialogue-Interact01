using UnityEngine;

public enum InteractableType
{
    Dialogue,
    Light,
}

public partial class InteractableObject : MonoBehaviour, IInteractable
{
    
    [SerializeField] 
    private InteractableType InteractableType;
    
    private string InteractText => "Press E to interact with " + gameObject.name + ".";
    public string InteractionText { get { return InteractText; } }

    public void Interact()
    {
        switch (InteractableType)
        {
            case InteractableType.Dialogue:
                DialogueUpdate();
                break;
            case InteractableType.Light:
                LightUpdate();
                break;
        }
    }

    partial void LightUpdate();
    partial void DialogueUpdate();
}
