using UnityEngine;

public enum QuestInteractionActions
{
    None = 0,
    AcceptQuest = 1,
    ReportEvent = 2
}

/// <summary>Independent quest interaction component, usable alone or beside other interactables.</summary>
public class QuestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestInteractionActions action = QuestInteractionActions.AcceptQuest;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private string objectiveId;
    [SerializeField] private string eventId;
    [SerializeField] private string targetId;
    [SerializeField, Min(1)] private int amount = 1;

    public string InteractionText => "Press E to interact with " + gameObject.name + ".";

    public void Interact()
    {
        if (action == QuestInteractionActions.None) return;
        if (questSystem == null) questSystem = FindFirstObjectByType<QuestSystem>();
        if (questSystem == null || questSystem.Manager == null)
        {
            Debug.LogWarning("No ready Quest System was found in the scene.", this);
            return;
        }

        switch (action)
        {
            case QuestInteractionActions.AcceptQuest:
                QuestAcceptResult result = questSystem.Manager.TryAcceptQuest(questDefinition);
                if (result != QuestAcceptResult.Success && result != QuestAcceptResult.AlreadyActive &&
                    result != QuestAcceptResult.AlreadyCompleted)
                    Debug.LogWarning("Quest could not be accepted: " + result, this);
                break;
            case QuestInteractionActions.ReportEvent:
                ReportProgress();
                break;
        }
    }

    private void ReportProgress()
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Quest Event Amount must be greater than zero.", this);
            return;
        }

        if (questDefinition != null && !string.IsNullOrWhiteSpace(objectiveId))
        {
            if (!questSystem.Manager.ReportObjective(questDefinition, objectiveId, amount))
                Debug.LogWarning("The selected quest objective could not be progressed.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(eventId))
        {
            Debug.LogWarning("Select a Quest Objective in the Inspector.", this);
            return;
        }
        questSystem.Manager.ProcessEvent(eventId, targetId, amount);
    }
}
