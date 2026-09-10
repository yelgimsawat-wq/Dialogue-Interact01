using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float Range = 3f;
    [SerializeField] private LayerMask LayerMask;
    [SerializeField] private TMP_Text InteractionText;

    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleRaycasting();
    }

    private void HandleRaycasting()
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
