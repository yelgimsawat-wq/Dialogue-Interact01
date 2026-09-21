using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class QuestAcceptResultEvent : UnityEvent<QuestAcceptResult> { }

public class QuestGiver : MonoBehaviour, IInteractable
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private bool canGiveQuest = true;
    [SerializeField] private QuestAcceptResultEvent onQuestResult = new QuestAcceptResultEvent();
    [SerializeField] private UnityEvent onAccepted = new UnityEvent();

    public QuestSet QuestDefinition => questDefinition;
    public bool CanGiveQuest => canGiveQuest;
    public string InteractionText => canGiveQuest ? "Talk to " + gameObject.name + "." : "Quest unavailable.";

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
        if (questSystem == null) questSystem = FindFirstObjectByType<QuestSystem>();
        if (questSystem == null || questSystem.Manager == null)
        {
            Debug.LogError("No ready Quest System was found in the scene.", this);
            onQuestResult?.Invoke(QuestAcceptResult.SystemUnavailable);
            return;
        }
        QuestAcceptResult result = questSystem.Manager.TryAcceptQuest(questDefinition);
        onQuestResult?.Invoke(result);
        if (result == QuestAcceptResult.Success) onAccepted?.Invoke();
        Debug.Log("Quest result: " + result, this);
    }
}
