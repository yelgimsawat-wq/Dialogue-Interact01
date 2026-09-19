using UnityEngine;

public class QuestDoor : BaseDoor
{
    [Header("Quest Integration")]
    [SerializeField]
    private QuestSet targetQuest;

    [SerializeField]
    private string targetQuestId;

    [SerializeField]
    private QuestSystem questSystem;

    [Header("Behavior Settings")]
    [SerializeField]
    private bool autoOpenOnComplete = false;

    [SerializeField]
    private string lockedPrompt = "Locked. Complete quest to open.";

    [SerializeField]
    private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    public string TargetQuestId
    {
        get
        {
            if (targetQuest != null && !string.IsNullOrEmpty(targetQuest.QuestId))
            {
                return targetQuest.QuestId;
            }
            return targetQuestId;
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitializeQuestListener();
    }

    private void OnEnable()
    {
        if (questSystem != null && questSystem.Manager != null)
        {
            questSystem.Manager.onQuestCompleted -= HandleQuestCompleted;
            questSystem.Manager.onQuestCompleted += HandleQuestCompleted;
        }
    }

    private void OnDisable()
    {
        if (questSystem != null && questSystem.Manager != null)
        {
            questSystem.Manager.onQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void InitializeQuestListener()
    {
        if (questSystem == null)
        {
            questSystem = FindFirstObjectByType<QuestSystem>();
        }

        if (questSystem != null && questSystem.Manager != null)
        {
            questSystem.Manager.onQuestCompleted -= HandleQuestCompleted;
            questSystem.Manager.onQuestCompleted += HandleQuestCompleted;

            // Check if quest was already completed
            string qId = TargetQuestId;
            if (!string.IsNullOrEmpty(qId))
            {
                QuestInstance instance = questSystem.Manager.GetQuest(qId);
                if (instance != null && instance.IsCompleted())
                {
                    UnlockDoor(autoOpen: false);
                }
            }
        }
    }

    private void HandleQuestCompleted(QuestInstance completedQuest)
    {
        if (completedQuest == null || completedQuest.quest == null) return;

        string activeTargetId = TargetQuestId;
        if (!string.IsNullOrEmpty(activeTargetId) && completedQuest.quest.QuestId == activeTargetId)
        {
            Debug.Log($"[QuestDoor] Quest '{completedQuest.quest.QuestId}' completed! Unlocking door '{gameObject.name}'.");
            UnlockDoor(autoOpen: autoOpenOnComplete);
        }
    }

    public void UnlockDoor(bool autoOpen = false)
    {
        isUnlocked = true;
        if (autoOpen && !IsOpen)
        {
            Open(transform.position);
        }
    }

    public void LockDoor()
    {
        isUnlocked = false;
        if (IsOpen)
        {
            Close();
        }
    }

    public override bool CanInteract => isUnlocked;

    public override string InteractionText
    {
        get
        {
            if (!isUnlocked)
            {
                if (targetQuest != null && !string.IsNullOrEmpty(targetQuest.DisplayName))
                {
                    return $"Locked. Required quest: {targetQuest.DisplayName}";
                }
                return lockedPrompt;
            }
            return base.InteractionText;
        }
    }

    public override void Interact()
    {
        if (!isUnlocked)
        {
            Debug.Log($"[QuestDoor] Door '{gameObject.name}' is locked. Requires quest '{TargetQuestId}'.");
            return;
        }

        base.Interact();
    }
}
