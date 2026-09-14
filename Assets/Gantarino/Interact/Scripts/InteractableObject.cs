using UnityEngine;

public enum InteractableType
{
    Dialogue,
    Light,
    Door,
}

public partial class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interactable Settings")]
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
            case InteractableType.Door:
                DoorUpdate();
                break;
        }
    }

    partial void LightUpdate();
    partial void DialogueUpdate();
    partial void DoorUpdate();
    partial void Open(Vector3 UserPosition);
    partial void Close();
}