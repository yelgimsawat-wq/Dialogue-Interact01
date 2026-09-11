using UnityEngine;
using TMPro;

public enum InteractionType
{
    FirstPerson,
    ThirdPerson
}

public partial class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float Range = 3f;
    [SerializeField] private float SphereCastRadius = 0.5f;
    [SerializeField] private LayerMask LayerMask;
    [SerializeField] private TMP_Text InteractionText;
    [SerializeField] private InteractionType InteractionType;

    private Camera mainCamera;
    private GameObject player;
    
    private void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");
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

}


