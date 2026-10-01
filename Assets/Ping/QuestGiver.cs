using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class QuestAcceptResultEvent : UnityEvent<QuestAcceptResult> { }

/// <summary>Gives a quest when the player presses E on this object.</summary>
[AddComponentMenu("Quest/Quest Giver")]
public class QuestGiver : MonoBehaviour, IInteractable
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private bool canGiveQuest = true;
    [SerializeField] private UnityEvent onAccepted = new UnityEvent();
    [SerializeField] private UnityEvent onQuestCompleted = new UnityEvent();
    [SerializeField] private QuestAcceptResultEvent onQuestResult = new QuestAcceptResultEvent();

    public QuestSet QuestDefinition => questDefinition;
    public bool CanGiveQuest => canGiveQuest;

    public string InteractionText
    {
        get
        {
            QuestSystem system = canGiveQuest && questDefinition != null ? QuestSystem.Resolve(questSystem) : null;
            if (system == null) return QuestInteractionUtility.OtherPrompt(this);

            QuestState state = system.Manager.GetState(questDefinition);
            if (state == QuestState.Available) return "Press E: Accept quest \"" + questDefinition.Title + "\"";
            if (state == QuestState.Locked) return LockedPrompt(system.Manager);

            // Already accepted: let a dialogue on the same object speak instead.
            string other = QuestInteractionUtility.OtherPrompt(this);
            if (!string.IsNullOrEmpty(other)) return other;
            return (state == QuestState.Active ? "Quest in progress: " : "Quest complete: ") + questDefinition.Title;
        }
    }

    public void Interact() => GiveQuest();

    [ContextMenu("Give Quest")]
    public void GiveQuest()
    {
        if (!canGiveQuest)
        {
            onQuestResult?.Invoke(QuestAcceptResult.GiverDisabled);
            return;
        }
        if (questDefinition == null)
        {
            Debug.LogError("Quest Giver has no Quest Set assigned.", this);
            onQuestResult?.Invoke(QuestAcceptResult.NullDefinition);
            return;
        }
        QuestSystem system = QuestSystem.Resolve(questSystem);
        if (system == null)
        {
            Debug.LogError("No ready Quest System was found in the scene.", this);
            onQuestResult?.Invoke(QuestAcceptResult.SystemUnavailable);
            return;
        }

        QuestAcceptResult result = system.Manager.TryAcceptQuest(questDefinition);
        if (result == QuestAcceptResult.Success)
        {
            QuestInstance quest = system.Manager.GetQuest(questDefinition.QuestId);
            if (quest != null) quest.onCompleted += HandleQuestCompleted;
            onAccepted?.Invoke();
        }
        else if (result != QuestAcceptResult.AlreadyAccepted && result != QuestAcceptResult.AlreadyCompleted &&
                 result != QuestAcceptResult.RequirementNotMet)
        {
            Debug.LogWarning("Quest could not be accepted: " + result, this);
        }
        onQuestResult?.Invoke(result);
    }

    private string LockedPrompt(QuestManager manager)
    {
        if (questDefinition.RequiredQuests != null)
            foreach (QuestSet required in questDefinition.RequiredQuests)
                if (required != null && required != questDefinition && !manager.IsQuestCompleted(required.QuestId))
                    return "Complete \"" + required.Title + "\" first.";
        return "Quest unavailable.";
    }

    private void HandleQuestCompleted(QuestInstance quest)
    {
        quest.onCompleted -= HandleQuestCompleted;
        if (this != null) onQuestCompleted?.Invoke();
    }
}
