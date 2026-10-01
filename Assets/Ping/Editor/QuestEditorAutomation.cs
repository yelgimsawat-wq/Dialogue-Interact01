using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>One-click setup actions used by the quest inspectors, the Quest System window and menus.</summary>
internal static class QuestEditorAutomation
{
    internal const int IgnoreRaycastLayer = 2;

    [MenuItem("GameObject/Quest/Quest Area Trigger", false, 11)]
    private static void CreateAreaFromMenu() => CreateAreaTrigger(null, null);

    /// <summary>Creates an invisible trigger box that progresses the objective when the player walks in.</summary>
    internal static GameObject CreateAreaTrigger(QuestSet quest, QuestObjective_Child objective)
    {
        string name = objective != null && !string.IsNullOrWhiteSpace(objective.DisplayName)
            ? "Quest Area - " + objective.DisplayName
            : "Quest Area";
        var gameObject = new GameObject(name);
        gameObject.layer = IgnoreRaycastLayer; // so the area doesn't block aiming at objects inside it
        if (SceneView.lastActiveSceneView != null) gameObject.transform.position = SceneView.lastActiveSceneView.pivot;
        var box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 1f, 0f);
        box.size = new Vector3(4f, 2f, 4f);
        Configure(gameObject.AddComponent<QuestEventSender>(), quest, objective, QuestSendTrigger.PlayerEntersArea);
        Undo.RegisterCreatedObjectUndo(gameObject, "Create Quest Area");
        Selection.activeGameObject = gameObject;
        return gameObject;
    }

    internal static void AddGivers(IEnumerable<GameObject> targets, QuestSet quest)
    {
        foreach (GameObject target in targets.Where(target => !EditorUtility.IsPersistent(target)))
        {
            QuestGiver giver = target.GetComponents<QuestGiver>().FirstOrDefault(existing => existing.QuestDefinition == quest || existing.QuestDefinition == null);
            if (giver == null) giver = Undo.AddComponent<QuestGiver>(target);
            var serialized = new SerializedObject(giver);
            serialized.FindProperty("questDefinition").objectReferenceValue = quest;
            serialized.ApplyModifiedProperties();
            if (target.GetComponent<Collider>() == null) AddCollider(target, false);
        }
    }

    internal static void AddSenders(IEnumerable<GameObject> targets, QuestSet quest, QuestObjective_Child objective, QuestSendTrigger trigger)
    {
        foreach (GameObject target in targets.Where(target => !EditorUtility.IsPersistent(target)))
        {
            QuestEventSender sender = target.GetComponents<QuestEventSender>()
                .FirstOrDefault(existing => existing.QuestDefinition == quest && existing.ObjectiveId == objective.ObjectiveId);
            if (sender == null) sender = Undo.AddComponent<QuestEventSender>(target);
            Configure(sender, quest, objective, trigger);

            if (trigger == QuestSendTrigger.PlayerPressesE && target.GetComponent<Collider>() == null)
                AddCollider(target, false);
            else if (trigger == QuestSendTrigger.PlayerEntersArea && !target.GetComponents<Collider>().Any(collider => collider.isTrigger))
                AddCollider(target, true);
        }
    }

    internal static void Configure(QuestEventSender sender, QuestSet quest, QuestObjective_Child objective, QuestSendTrigger trigger)
    {
        var serialized = new SerializedObject(sender);
        serialized.FindProperty("sendWhen").intValue = (int)trigger;
        serialized.FindProperty("onlyOnce").boolValue = trigger == QuestSendTrigger.PlayerPressesE || trigger == QuestSendTrigger.PlayerEntersArea;
        if (quest != null && objective != null)
        {
            serialized.FindProperty("questDefinition").objectReferenceValue = quest;
            serialized.FindProperty("objectiveId").stringValue = objective.ObjectiveId;
            serialized.FindProperty("eventId").stringValue = objective.EventId;
            serialized.FindProperty("targetId").stringValue = objective.TargetId;
        }
        serialized.ApplyModifiedProperties();
    }

    /// <summary>Adds a Box Collider fitted to the object. A trigger is made larger than any solid collider so the player can enter it.</summary>
    internal static void AddCollider(GameObject target, bool trigger)
    {
        bool hasSolid = target.GetComponents<Collider>().Any(collider => !collider.isTrigger);
        var box = Undo.AddComponent<BoxCollider>(target);
        if (!trigger) return;
        Undo.RecordObject(box, "Configure Trigger");
        box.isTrigger = true;
        if (target.GetComponent<Renderer>() == null)
        {
            box.center = new Vector3(0f, 1f, 0f);
            box.size = new Vector3(4f, 2f, 4f);
        }
        else if (hasSolid)
        {
            box.size *= 1.5f;
        }
    }

    internal static void SetLayer(GameObject target, int layer)
    {
        Undo.RecordObject(target, "Change Layer");
        target.layer = layer;
    }

    internal static void AddObjective(QuestSet quest)
    {
        QuestEditorValidation.Edit(quest, "Add Objective", () =>
        {
            string objectiveId = QuestSet.CreateGeneratedId("objective");
            quest.Objectives.Add(new QuestObjective_Child
            {
                ObjectiveId = objectiveId,
                DisplayName = "Objective " + (quest.Objectives.Count + 1),
                EventId = objectiveId,
                requiredAmount = 1
            });
        });
    }

    /// <summary>Replaces an old Quest Interactable with the equivalent Quest Giver or Quest Event Sender.</summary>
    internal static void ConvertLegacy(QuestInteractable legacy)
    {
        var old = new SerializedObject(legacy);
        var action = (QuestInteractionActions)old.FindProperty("action").intValue;
        if (action == QuestInteractionActions.None) return;

        GameObject target = legacy.gameObject;
        Undo.SetCurrentGroupName("Convert Quest Interactable");
        int group = Undo.GetCurrentGroup();
        Component replacement = action == QuestInteractionActions.AcceptQuest
            ? (Component)Undo.AddComponent<QuestGiver>(target)
            : Undo.AddComponent<QuestEventSender>(target);
        var serialized = new SerializedObject(replacement);
        serialized.FindProperty("questSystem").objectReferenceValue = old.FindProperty("questSystem").objectReferenceValue;
        serialized.FindProperty("questDefinition").objectReferenceValue = old.FindProperty("questDefinition").objectReferenceValue;
        if (action == QuestInteractionActions.ReportEvent)
        {
            foreach (string property in new[] { "objectiveId", "eventId", "targetId" })
                serialized.FindProperty(property).stringValue = old.FindProperty(property).stringValue;
            serialized.FindProperty("amount").intValue = old.FindProperty("amount").intValue;
            serialized.FindProperty("sendWhen").intValue = (int)QuestSendTrigger.PlayerPressesE;
        }
        serialized.ApplyModifiedProperties();
        Undo.DestroyObjectImmediate(legacy);
        Undo.CollapseUndoOperations(group);
    }

    /// <summary>A readable Quest ID from the asset name that no other Quest Set uses.</summary>
    internal static string UniqueQuestId(QuestSet quest, IEnumerable<QuestSet> all = null)
        => UniqueQuestId(quest != null ? quest.name : null, quest, all);

    internal static string UniqueQuestId(string name, QuestSet exclude, IEnumerable<QuestSet> all = null)
    {
        var taken = new HashSet<string>((all ?? QuestEditorValidation.FindQuests())
            .Where(other => other != null && other != exclude).Select(other => other.QuestId));
        string baseId = MakeId(name, "quest");
        string id = baseId;
        for (int number = 2; taken.Contains(id); number++) id = baseId + "_" + number;
        return id;
    }

    internal static string MakeId(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) value = fallback;
        var result = new System.Text.StringBuilder();
        bool separator = false;
        foreach (char character in value.Trim())
        {
            if (char.IsLetterOrDigit(character))
            {
                if (separator && result.Length > 0) result.Append('_');
                result.Append(char.ToLowerInvariant(character));
                separator = false;
            }
            else separator = true;
        }
        return result.Length == 0 ? fallback : result.ToString();
    }

    internal static void Ping(Object target)
    {
        if (target == null) return;
        Object selection = target is Component component ? component.gameObject : target;
        Selection.activeObject = selection;
        EditorGUIUtility.PingObject(selection);
    }
}
