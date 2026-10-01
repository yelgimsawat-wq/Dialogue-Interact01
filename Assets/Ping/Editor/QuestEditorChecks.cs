using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>A setup problem, optionally with a one-click fix.</summary>
internal sealed class QuestIssue
{
    internal readonly Object Context;
    internal readonly string Message;
    internal readonly string FixLabel;
    internal readonly Action Fix;
    /// <summary>Informational only; the quest still works.</summary>
    internal readonly bool Advisory;

    internal QuestIssue(Object context, string message, string fixLabel = null, Action fix = null, bool advisory = false)
    {
        Context = context;
        Message = message;
        FixLabel = fixLabel;
        Fix = fix;
        Advisory = advisory;
    }
}

/// <summary>Setup checks for quest components in scenes and prefabs.</summary>
internal static class QuestEditorChecks
{
    internal static List<QuestIssue> For(Component component)
    {
        switch (component)
        {
            case QuestGiver giver: return Giver(giver).ToList();
            case QuestEventSender sender: return Sender(sender).ToList();
            case QuestDoor door: return Door(door).ToList();
            case QuestInteractable legacy: return Legacy(legacy).ToList();
            default: return new List<QuestIssue>();
        }
    }

    internal static IEnumerable<QuestIssue> Giver(QuestGiver giver)
    {
        if (new SerializedObject(giver).FindProperty("questDefinition").objectReferenceValue == null)
            yield return new QuestIssue(giver, "Choose the Quest Set this object gives.");
        foreach (QuestIssue issue in PressE(giver)) yield return issue;
    }

    internal static IEnumerable<QuestIssue> Sender(QuestEventSender sender)
    {
        var serialized = new SerializedObject(sender);
        foreach (QuestIssue issue in Objective(sender, serialized)) yield return issue;
        if (serialized.FindProperty("amount").intValue <= 0)
            yield return new QuestIssue(sender, "Amount must be greater than zero.", "Set to 1",
                () => SetInt(sender, "amount", 1));

        switch ((QuestSendTrigger)serialized.FindProperty("sendWhen").intValue)
        {
            case QuestSendTrigger.PlayerPressesE:
                foreach (QuestIssue issue in PressE(sender)) yield return issue;
                break;
            case QuestSendTrigger.PlayerEntersArea:
                foreach (QuestIssue issue in Area(sender)) yield return issue;
                break;
        }
    }

    internal static IEnumerable<QuestIssue> Door(QuestDoor door)
    {
        if (new SerializedObject(door).FindProperty("targetQuest").objectReferenceValue == null)
            yield return new QuestIssue(door, "Choose the quest that unlocks this door.");
        foreach (QuestIssue issue in PressE(door)) yield return issue;
    }

    internal static IEnumerable<QuestIssue> Legacy(QuestInteractable legacy)
    {
        var serialized = new SerializedObject(legacy);
        var action = (QuestInteractionActions)serialized.FindProperty("action").intValue;
        bool canConvert = action != QuestInteractionActions.None && !EditorUtility.IsPersistent(legacy);
        yield return new QuestIssue(legacy, "Older component. Convert it to a Quest Giver / Quest Event Sender (settings are kept).",
            "Convert", canConvert ? () => QuestEditorAutomation.ConvertLegacy(legacy) : (Action)null, advisory: true);

        if (action == QuestInteractionActions.AcceptQuest &&
            serialized.FindProperty("questDefinition").objectReferenceValue == null)
            yield return new QuestIssue(legacy, "Choose the Quest Set to accept.");
        if (action == QuestInteractionActions.ReportEvent)
            foreach (QuestIssue issue in Objective(legacy, serialized)) yield return issue;
        if (action != QuestInteractionActions.None)
            foreach (QuestIssue issue in PressE(legacy)) yield return issue;
    }

    /// <summary>The player aims at the object's own collider, on a layer the Player Interaction can see.</summary>
    internal static IEnumerable<QuestIssue> PressE(Component component)
    {
        GameObject gameObject = component.gameObject;
        bool inScene = !EditorUtility.IsPersistent(gameObject);
        if (gameObject.GetComponent<Collider>() == null)
        {
            yield return new QuestIssue(component, "Needs a Collider on this object so the player can aim at it.",
                "Add Box Collider", inScene ? () => QuestEditorAutomation.AddCollider(gameObject, false) : (Action)null);
            yield break;
        }
        if (!inScene) yield break;

        foreach (PlayerInteraction player in Object.FindObjectsByType<PlayerInteraction>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            int mask = new SerializedObject(player).FindProperty("LayerMask").intValue;
            if ((mask & (1 << gameObject.layer)) != 0) continue;
            string message = "Layer \"" + LayerMask.LayerToName(gameObject.layer) + "\" is not in the Layer Mask of " +
                             player.name + ", so the player can't interact with this object.";
            int layer = FirstLayer(mask);
            yield return layer >= 0
                ? new QuestIssue(component, message, "Use layer \"" + LayerMask.LayerToName(layer) + "\"",
                    () => QuestEditorAutomation.SetLayer(gameObject, layer))
                : new QuestIssue(component, message + " Its Layer Mask is set to Nothing.", "Select player",
                    () => Selection.activeObject = player);
            yield break;
        }
    }

    /// <summary>The object marks an area with a trigger collider.</summary>
    internal static IEnumerable<QuestIssue> Area(Component component)
    {
        GameObject gameObject = component.gameObject;
        bool inScene = !EditorUtility.IsPersistent(gameObject);
        Collider trigger = gameObject.GetComponents<Collider>().FirstOrDefault(collider => collider.isTrigger);
        if (trigger == null)
            yield return new QuestIssue(component, "Needs a trigger Collider that marks the area.",
                "Add trigger area", inScene ? () => QuestEditorAutomation.AddCollider(gameObject, true) : (Action)null);
        else if (trigger is MeshCollider mesh && !mesh.convex)
            yield return new QuestIssue(component, "A Mesh Collider used as a trigger must be Convex.", "Make convex",
                inScene ? () => { Undo.RecordObject(mesh, "Make Collider Convex"); mesh.convex = true; } : (Action)null);
    }

    private static IEnumerable<QuestIssue> Objective(Component component, SerializedObject serialized)
    {
        var quest = serialized.FindProperty("questDefinition").objectReferenceValue as QuestSet;
        string objectiveId = serialized.FindProperty("objectiveId").stringValue;
        string eventId = serialized.FindProperty("eventId").stringValue;
        string targetId = serialized.FindProperty("targetId").stringValue;
        if (QuestObjectivePicker.IsValid(quest, objectiveId)) yield break;

        if (quest != null && !string.IsNullOrWhiteSpace(objectiveId))
            yield return new QuestIssue(component, "The selected objective no longer exists in \"" + quest.Title + "\". Choose another.");
        else if (string.IsNullOrWhiteSpace(eventId))
            yield return new QuestIssue(component, "Choose the Objective this object progresses.");
        else if (!QuestEditorValidation.FindQuests().Where(q => q.Objectives != null).SelectMany(q => q.Objectives)
                     .Any(objective => objective != null && objective.MatchesEvent(eventId, targetId)))
            yield return new QuestIssue(component, "No objective listens for Event ID \"" + eventId + "\". Choose an Objective instead.");
    }

    private static int FirstLayer(int mask)
    {
        for (int layer = 0; layer < 32; layer++)
            if ((mask & (1 << layer)) != 0 && !string.IsNullOrEmpty(LayerMask.LayerToName(layer))) return layer;
        return -1;
    }

    private static void SetInt(Object target, string property, int value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).intValue = value;
        serialized.ApplyModifiedProperties();
    }
}
