using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string InteractText = "Press E to interact";

    public void Interact()
    {
        Debug.Log("Interacted with cube");
    }

    public string InteractionText { get { return InteractText; } }
}