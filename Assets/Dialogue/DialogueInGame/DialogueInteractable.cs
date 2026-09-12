using UnityEngine;

public class DialogueInteractable : MonoBehaviour,IInteractable
{
    [SerializeField] private DialogueContainer Dialogue;

    public void Interact()
    {
        Debug.Log("ใช้งานได้แล้วเย้");
    }

    public string InteractionText => "Press [E] to talk";
}
