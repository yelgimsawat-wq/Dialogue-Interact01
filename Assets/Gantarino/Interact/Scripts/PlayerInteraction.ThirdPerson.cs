using UnityEngine;

public partial class PlayerInteraction
{
    private void ThirdpersonHandleRaycasting()
    {
        RaycastHit hit;
        if (!Physics.SphereCast(player.transform.position + 0.6f * Vector3.up, SphereCastRadius, player.transform.forward, out hit, Range, LayerMask))
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