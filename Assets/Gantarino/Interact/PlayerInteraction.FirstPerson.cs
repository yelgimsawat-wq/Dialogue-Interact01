using UnityEngine;

public partial class PlayerInteraction
{
    private void FirstpersonHandleRaycasting()
    {
        RaycastHit hit;
        if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, Range, LayerMask))
        {
        InteractionText.text = "";
        return;
        }
        
        if (!hit.collider.TryGetComponent(out IInteractable interactable))
        {
            InteractionText.text = "";
            return;
        }

        InteractionText.text = interactable.InteractionText;
        if (Input.GetKeyDown(KeyCode.E))
        {
            interactable.Interact();
        }
    }
}
