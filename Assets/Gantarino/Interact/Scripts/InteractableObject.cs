using System;
using UnityEngine;

// Keep the original numeric values for existing scenes and prefabs.
public enum InteractableType
{
    Dialogue,
    Light,
    Door,
}

[Flags]
public enum InteractionActions
{
    None = 0,
    Dialogue = 1,
    Light = 2,
    Door = 4,
    Quest = 8,
}

[Flags]
public enum QuestInteractionActions
{
    None = 0,
    AcceptQuest = 1,
    ReportEvent = 2,
}

public partial class InteractableObject : MonoBehaviour, IInteractable, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private InteractableType InteractableType;
    [SerializeField, HideInInspector]
    private bool interactionTypesMigrated;
    [SerializeField]
    private InteractionActions interactionTypes = InteractionActions.Dialogue;

    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestInteractionActions questActions = QuestInteractionActions.AcceptQuest;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private string questEventId;
    [SerializeField] private string questTargetId;
    [SerializeField, Min(1)] private int questEventAmount = 1;

    public string InteractionText => "Press E to interact with " + gameObject.name + ".";

    public void OnBeforeSerialize() => MigrateInteractionTypes();
    public void OnAfterDeserialize() => MigrateInteractionTypes();

    private void MigrateInteractionTypes()
    {
        if (interactionTypesMigrated) return;
        switch (InteractableType)
        {
            case InteractableType.Light: interactionTypes = InteractionActions.Light; break;
            case InteractableType.Door: interactionTypes = InteractionActions.Door; break;
            default: interactionTypes = InteractionActions.Dialogue; break;
        }
        interactionTypesMigrated = true;
    }

    public void Interact()
    {
        MigrateInteractionTypes();
        // Resolve the selection once, so callbacks cannot change this interaction halfway through.
        InteractionActions actions = interactionTypes;
        if ((actions & InteractionActions.Quest) != 0) QuestUpdate();
        if ((actions & InteractionActions.Dialogue) != 0) DialogueUpdate();
        if ((actions & InteractionActions.Light) != 0) LightUpdate();
        if ((actions & InteractionActions.Door) != 0) DoorUpdate();
    }

    private void QuestUpdate()
    {
        if (questActions == QuestInteractionActions.None) return;
        if (questSystem == null) questSystem = FindFirstObjectByType<QuestSystem>();
        if (questSystem == null || questSystem.Manager == null)
        {
            Debug.LogWarning("ไม่พบ QuestSystem ที่พร้อมใช้งาน", this);
            return;
        }

        if ((questActions & QuestInteractionActions.AcceptQuest) != 0)
        {
            QuestAcceptResult result = questSystem.Manager.AcceptQuest(questDefinition);
            if (result != QuestAcceptResult.Success && result != QuestAcceptResult.AlreadyActive &&
                result != QuestAcceptResult.AlreadyCompleted)
            {
                Debug.LogWarning("รับเควสต์ไม่สำเร็จ: " + result, this);
            }
        }

        if ((questActions & QuestInteractionActions.ReportEvent) != 0)
        {
            if (string.IsNullOrWhiteSpace(questEventId) || questEventAmount <= 0)
            {
                Debug.LogWarning("กรุณากำหนด Quest Event ID และ Amount มากกว่า 0", this);
                return;
            }
            questSystem.Manager.ProcessEvent(questEventId, questTargetId, questEventAmount);
        }
    }

    partial void LightUpdate();
    partial void DialogueUpdate();
    partial void DoorUpdate();
    partial void Open(Vector3 UserPosition);
    partial void Close();
}
