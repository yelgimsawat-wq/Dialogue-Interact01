using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal enum QuestLinkKind { Giver, Sender, Door, LegacyAccept, LegacyReport }

/// <summary>A quest component in a scene or prefab, read once so windows can query it cheaply.</summary>
internal sealed class QuestLink
{
    internal Component Component;
    internal QuestLinkKind Kind;
    internal QuestSet Quest;
    internal string ObjectiveId;
    internal string EventId;
    internal string TargetId;
    internal QuestSendTrigger Trigger;
    internal bool IsPrefab;

    internal bool Gives(QuestSet quest)
        => Quest == quest && (Kind == QuestLinkKind.Giver || Kind == QuestLinkKind.LegacyAccept);

    /// <summary>Mirrors the runtime: a chosen objective wins, otherwise legacy Event ID matching.</summary>
    internal bool Progresses(QuestSet quest, QuestObjective_Child objective)
    {
        if (Kind != QuestLinkKind.Sender && Kind != QuestLinkKind.LegacyReport) return false;
        if (Quest != null && !string.IsNullOrWhiteSpace(ObjectiveId))
            return Quest == quest && ObjectiveId == objective.ObjectiveId;
        return objective.MatchesEvent(EventId, TargetId);
    }

    internal bool RelatesTo(QuestSet quest)
        => Quest == quest || (quest.Objectives != null && quest.Objectives.Any(o => o != null && Progresses(quest, o)));

    internal string Describe()
    {
        string name = Component.gameObject.name + (IsPrefab ? " (prefab)" : string.Empty);
        switch (Kind)
        {
            case QuestLinkKind.Sender: return name + "  ·  " + TriggerLabel(Trigger);
            case QuestLinkKind.LegacyReport: return name + "  ·  press E (old component)";
            case QuestLinkKind.LegacyAccept: return name + "  (old component)";
            default: return name;
        }
    }

    internal static string TriggerLabel(QuestSendTrigger trigger)
    {
        switch (trigger)
        {
            case QuestSendTrigger.PlayerPressesE: return "press E";
            case QuestSendTrigger.PlayerEntersArea: return "player enters area";
            case QuestSendTrigger.ObjectDestroyed: return "when destroyed";
            default: return "manual (UnityEvent / code)";
        }
    }

    internal static bool TryCreate(Component component, bool isPrefab, out QuestLink link)
    {
        link = null;
        if (!(component is QuestGiver || component is QuestDoor || component is QuestEventSender || component is QuestInteractable))
            return false;
        var serialized = new SerializedObject(component);
        switch (component)
        {
            case QuestGiver _:
                link = new QuestLink { Kind = QuestLinkKind.Giver };
                break;
            case QuestDoor _:
                link = new QuestLink { Kind = QuestLinkKind.Door, Quest = serialized.FindProperty("targetQuest").objectReferenceValue as QuestSet };
                break;
            case QuestEventSender _:
                link = new QuestLink { Kind = QuestLinkKind.Sender, Trigger = (QuestSendTrigger)serialized.FindProperty("sendWhen").intValue };
                break;
            case QuestInteractable _:
                var action = (QuestInteractionActions)serialized.FindProperty("action").intValue;
                if (action == QuestInteractionActions.None) return false;
                link = new QuestLink { Kind = action == QuestInteractionActions.AcceptQuest ? QuestLinkKind.LegacyAccept : QuestLinkKind.LegacyReport };
                break;
            default:
                return false;
        }

        link.Component = component;
        link.IsPrefab = isPrefab;
        if (link.Kind != QuestLinkKind.Door)
            link.Quest = serialized.FindProperty("questDefinition").objectReferenceValue as QuestSet;
        if (link.Kind == QuestLinkKind.Sender || link.Kind == QuestLinkKind.LegacyReport)
        {
            link.ObjectiveId = serialized.FindProperty("objectiveId").stringValue;
            link.EventId = serialized.FindProperty("eventId").stringValue;
            link.TargetId = serialized.FindProperty("targetId").stringValue;
        }
        return true;
    }
}

/// <summary>Every quest component in the open scene (plus, optionally, in prefabs).</summary>
internal sealed class QuestSceneIndex
{
    internal readonly List<QuestLink> Links = new List<QuestLink>();

    internal static QuestSceneIndex Build(IEnumerable<QuestLink> prefabLinks = null)
    {
        var index = new QuestSceneIndex();
        foreach (MonoBehaviour behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (QuestLink.TryCreate(behaviour, false, out QuestLink link)) index.Links.Add(link);
        if (prefabLinks != null) index.Links.AddRange(prefabLinks.Where(link => link.Component != null));
        return index;
    }

    internal static List<QuestLink> ScanPrefabs()
    {
        var links = new List<QuestLink>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab == null) continue;
            foreach (MonoBehaviour behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
                if (QuestLink.TryCreate(behaviour, true, out QuestLink link)) links.Add(link);
        }
        return links;
    }
}
