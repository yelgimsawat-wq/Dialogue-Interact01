using UnityEngine;

public enum InteractableType
{
    Dialogue,
}

public partial class InteractableObject : MonoBehaviour, IInteractable
{
    
    [SerializeField] 
    private InteractableType InteractableType;
    [SerializeField]
    private DialogueCanvas dialogueCanvas;
    [SerializeField]
    private DialogueContainer dialogueContainer;
    private string InteractText => "Press E to interact with " + gameObject.name + ".";

    public void Interact()
    {
        switch (InteractableType)
        {
            case InteractableType.Dialogue:
                DialogueUpdate();
                break;
        }
    }

    public string InteractionText { get { return InteractText; } }
}
