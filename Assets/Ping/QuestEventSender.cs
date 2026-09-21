using UnityEngine;

public class QuestEventSender : MonoBehaviour
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private string eventId;
    [SerializeField] private string targetId;
    [SerializeField, Min(1)] private int amount = 1;

    [ContextMenu("Send Quest Event")]
    public void SendEvent()
    {
        if (questSystem == null) questSystem = FindFirstObjectByType<QuestSystem>();
        if (questSystem == null || questSystem.Manager == null)
        {
            Debug.LogError("No ready Quest System was found in the scene.", this);
            return;
        }
        if (string.IsNullOrWhiteSpace(eventId) || amount <= 0)
        {
            Debug.LogError("Set a valid Event ID and an Amount greater than zero.", this);
            return;
        }
        questSystem.Manager.ProcessEvent(eventId, targetId, amount);
    }
}
