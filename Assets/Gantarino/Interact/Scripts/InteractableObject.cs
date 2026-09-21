using System;
using UnityEngine;

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
    [SerializeField, HideInInspector] private InteractableType InteractableType;
    [SerializeField, HideInInspector] private bool interactionTypesMigrated;
    [SerializeField] private InteractionActions interactionTypes = InteractionActions.Dialogue;

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
        InteractionActions actions = interactionTypes;
        if ((actions & InteractionActions.Quest) != 0) QuestUpdate();
        if ((actions & InteractionActions.Dialogue) != 0) DialogueUpdate();
        if ((actions & InteractionActions.Light) != 0) LightUpdate();
        if ((actions & InteractionActions.Door) != 0) DoorUpdate();
    }

    partial void LightUpdate();
    partial void DialogueUpdate();
    partial void DoorUpdate();
    partial void QuestUpdate();
    partial void Open(Vector3 userPosition);
    partial void Close();
}
