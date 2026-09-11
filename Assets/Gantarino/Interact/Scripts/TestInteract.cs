using UnityEngine;

public class TestInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private string InteractText = "Press E to interact with this object.";

    public void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }

    public string InteractionText { get { return InteractText; } }
}