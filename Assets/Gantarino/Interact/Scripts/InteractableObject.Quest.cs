using UnityEngine;

public partial class InteractableObject
{
    [Header("Quest Config")]
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestInteractionActions questActions = QuestInteractionActions.AcceptQuest;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private string questEventId;
    [SerializeField] private string questTargetId;
    [SerializeField, Min(1)] private int questEventAmount = 1;

    private void QuestUpdate()
    {
        if (questActions == QuestInteractionActions.None) return;
        if (questSystem == null) questSystem = FindFirstObjectByType<QuestSystem>();
        if (questSystem == null || questSystem.Manager == null)
        {
            Debug.LogWarning("No ready Quest System was found in the scene.", this);
            return;
        }

        if ((questActions & QuestInteractionActions.AcceptQuest) != 0)
        {
            QuestAcceptResult result = questSystem.Manager.TryAcceptQuest(questDefinition);
            if (result != QuestAcceptResult.Success && result != QuestAcceptResult.AlreadyActive &&
                result != QuestAcceptResult.AlreadyCompleted)
                Debug.LogWarning("Quest could not be accepted: " + result, this);
        }

        if ((questActions & QuestInteractionActions.ReportEvent) != 0)
        {
            if (string.IsNullOrWhiteSpace(questEventId) || questEventAmount <= 0)
            {
                Debug.LogWarning("Set a valid Quest Event ID and Amount.", this);
                return;
            }
            questSystem.Manager.ProcessEvent(questEventId, questTargetId, questEventAmount);
        }
    }
}
