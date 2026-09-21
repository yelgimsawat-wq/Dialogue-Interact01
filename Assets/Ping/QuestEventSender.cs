using UnityEngine;

public class QuestEventSender : MonoBehaviour
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private string objectiveId;
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
        if (amount <= 0)
        {
            Debug.LogError("Amount must be greater than zero.", this);
            return;
        }

        if (questDefinition != null && !string.IsNullOrWhiteSpace(objectiveId))
        {
            if (!questSystem.Manager.ReportObjective(questDefinition, objectiveId, amount))
                Debug.LogWarning("The selected quest is not active, the objective is already complete, or the objective no longer exists.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(eventId))
        {
            Debug.LogError("Select a Quest Objective in the Inspector.", this);
            return;
        }
        questSystem.Manager.ProcessEvent(eventId, targetId, amount);
    }
}
