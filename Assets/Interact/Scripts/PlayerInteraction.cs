using UnityEngine;
using TMPro;

public enum InteractionType
{
    FirstPerson,
    ThirdPerson
}

public partial class PlayerInteraction : MonoBehaviour
{
    
    [SerializeField]
    private InteractionType InteractionType;
    [SerializeField] [Tooltip("Define which layers can be interacted with.")]
    private LayerMask LayerMask;
    [SerializeField] [Tooltip("Add a TextMeshProUGUI component.")]
    private TMP_Text InteractionText;
    [SerializeField] [Tooltip("Add the player object.")]
    private GameObject player;
    [SerializeField] [Tooltip("Add the player’s main camera.")]
    private Camera mainCamera;
    [SerializeField] [Range(0.1f, 10f)] [Tooltip("Set the object detection range.")]
    private float Range = 3f;
    
    public InteractionType CurrentInteractionType => InteractionType;
    
    
    private void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");;
    }

    

    private void Update()
    {
        HandleRaycasting();
    }

    private void HandleRaycasting()
    {
        switch (InteractionType)
        {
            case InteractionType.FirstPerson:
                FirstpersonHandleRaycasting();
                break;
            case InteractionType.ThirdPerson:
                ThirdpersonHandleRaycasting();
                break;
        }
    }

    private static IInteractable[] GetInteractables(Collider hitCollider)
    {
        var components = hitCollider.GetComponents<MonoBehaviour>();
        var interactables = new System.Collections.Generic.List<IInteractable>();
        foreach (MonoBehaviour component in components)
            if (component is IInteractable interactable)
                interactables.Add(interactable);
        return interactables.ToArray();
    }

    partial void FirstpersonHandleRaycasting();
    partial void ThirdpersonHandleRaycasting();
    partial void OnDrawGizmosSelected();

}
