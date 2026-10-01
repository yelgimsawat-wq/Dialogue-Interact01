using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Dropdown listing every objective of every Quest Set, grouped by quest.</summary>
internal static class QuestObjectivePicker
{
    private sealed class Choice
    {
        internal QuestSet Quest;
        internal QuestObjective_Child Objective;
        internal string Label;
    }

    internal static bool Draw(SerializedProperty questProperty, SerializedProperty objectiveIdProperty,
        SerializedProperty legacyEventId = null, SerializedProperty legacyTargetId = null)
    {
        List<Choice> choices = BuildChoices();
        if (choices.Count == 0)
        {
            EditorGUILayout.HelpBox("No objectives exist yet. Create a Quest Set first.", MessageType.Info);
            if (GUILayout.Button("Open Quest System window")) QuestSystemWindow.Open();
            return false;
        }

        var labels = new[] { "Select an objective..." }.Concat(choices.Select(choice => choice.Label)).ToArray();
        int current = choices.FindIndex(choice =>
            choice.Quest == questProperty.objectReferenceValue &&
            choice.Objective.ObjectiveId == objectiveIdProperty.stringValue) + 1;

        int selected = EditorGUILayout.Popup(new GUIContent("Quest Objective", "Which objective this object progresses."), current,
            labels.Select(label => new GUIContent(label)).ToArray());
        if (selected > 0 && selected != current)
        {
            Choice choice = choices[selected - 1];
            questProperty.objectReferenceValue = choice.Quest;
            objectiveIdProperty.stringValue = choice.Objective.ObjectiveId;
            if (legacyEventId != null) legacyEventId.stringValue = choice.Objective.EventId;
            if (legacyTargetId != null) legacyTargetId.stringValue = choice.Objective.TargetId;
        }

        if (selected > 0)
            EditorGUILayout.LabelField(" ", choices[selected - 1].Label.Replace("/", "›"), EditorStyles.miniLabel);
        return selected > 0;
    }

    internal static bool IsValid(QuestSet quest, string objectiveId)
        => quest != null && quest.FindObjective(objectiveId) != null;

    internal static string Label(QuestSet quest, QuestObjective_Child objective)
    {
        string objectiveName = string.IsNullOrWhiteSpace(objective.DisplayName)
            ? "Objective " + (quest.Objectives.IndexOf(objective) + 1)
            : objective.DisplayName;
        return quest.Title + " / " + objectiveName;
    }

    private static List<Choice> BuildChoices()
    {
        var choices = new List<Choice>();
        foreach (QuestSet quest in QuestEditorValidation.FindQuests())
        {
            if (quest.Objectives == null) continue;
            foreach (QuestObjective_Child objective in quest.Objectives.Where(objective => objective != null))
                choices.Add(new Choice { Quest = quest, Objective = objective, Label = Label(quest, objective) });
        }
        return choices.OrderBy(choice => choice.Label).ToList();
    }
}
