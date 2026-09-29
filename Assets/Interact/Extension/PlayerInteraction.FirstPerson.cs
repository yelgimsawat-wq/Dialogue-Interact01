using UnityEngine;

public partial class PlayerInteraction
{
    partial void FirstpersonHandleRaycasting()
    {
        RaycastHit hit;
        if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, Range, LayerMask))
        {
        InteractionText.text = "";
        return;
        }
        
        IInteractable[] interactables = GetInteractables(hit.collider);
        if (interactables.Length == 0)
        {
            InteractionText.text = "";
            return;
        }

        InteractionText.text = interactables[0].InteractionText;
        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (IInteractable interactable in interactables)
                interactable.Interact();
        }
    }
}
