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
    [SerializeField] [Tooltip("กำหนด Layer ของวัตถุที่สามารถตรวจจับได้")]
    private LayerMask LayerMask;
    [SerializeField] [Tooltip("นำ TextMeshProUGUI มาใส่")]
    private TMP_Text InteractionText;
    [SerializeField] [Tooltip("นำตัว player มาใส่")]
    private GameObject player;
    [SerializeField] [Tooltip("นำกล้องหลักของ player มาใส่")]
    private Camera mainCamera;
    [SerializeField] [Range(0.1f, 10f)] [Tooltip("กำหนดระยะทางของการตรวจจับวัตถุ")]
    private float Range = 3f;
    [SerializeField] [Range(0.1f, 5f)] [Tooltip("กำหนดรัสมีของการตรวจจับวัตถุ")]
    private float SphereCastRadius = 0.5f;
    
    
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

}