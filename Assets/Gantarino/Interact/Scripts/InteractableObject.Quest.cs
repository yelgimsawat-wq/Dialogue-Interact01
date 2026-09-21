using UnityEngine;

public partial class InteractableObject
{
    [Header("Quest Config")]
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestInteractionActions questActions = QuestInteractionActions.AcceptQuest;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private string questObjectiveId;
    [SerializeField] private string questEventId;
    [SerializeField] private string questTargetId;
    [SerializeField, Min(1)] private int questEventAmount = 1;

    partial void QuestUpdate()
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
            if (questEventAmount <= 0)
            {
                Debug.LogWarning("Quest Event Amount must be greater than zero.", this);
                return;
            }

            if (questDefinition != null && !string.IsNullOrWhiteSpace(questObjectiveId))
            {
                if (!questSystem.Manager.ReportObjective(questDefinition, questObjectiveId, questEventAmount))
                    Debug.LogWarning("The selected quest objective could not be progressed.", this);
                return;
            }
            if (string.IsNullOrWhiteSpace(questEventId))
            {
                Debug.LogWarning("Select a Quest Objective in the Inspector.", this);
                return;
            }
            questSystem.Manager.ProcessEvent(questEventId, questTargetId, questEventAmount);
        }
    }
}
