using UnityEngine;

public partial class PlayerInteraction
{
    [SerializeField] [Range(0.1f, 5f)] [Tooltip("Set the object detection radius.")]
    private float SphereCastRadius = 0.5f;

    partial void ThirdpersonHandleRaycasting()
    {
        RaycastHit hit;
        if (!Physics.SphereCast(player.transform.position + 0.6f * Vector3.up, SphereCastRadius, player.transform.forward, out hit, Range, LayerMask))
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
