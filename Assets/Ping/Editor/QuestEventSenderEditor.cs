using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
        var labels = new[] { "Select an objective..." }.Concat(choices.Select(choice => choice.Label)).ToArray();
        int current = choices.FindIndex(choice =>
            choice.Quest == questProperty.objectReferenceValue &&
            choice.Objective.ObjectiveId == objectiveIdProperty.stringValue) + 1;

        int selected = EditorGUILayout.Popup("Quest Objective", current, labels);
        if (selected > 0 && selected != current)
        {
            Choice choice = choices[selected - 1];
            questProperty.objectReferenceValue = choice.Quest;
            objectiveIdProperty.stringValue = choice.Objective.ObjectiveId;
            if (legacyEventId != null) legacyEventId.stringValue = choice.Objective.EventId;
            if (legacyTargetId != null) legacyTargetId.stringValue = choice.Objective.TargetId;
        }

        if (selected > 0)
            EditorGUILayout.LabelField("Progresses", choices[selected - 1].Label, EditorStyles.miniLabel);
        return selected > 0;
    }

    internal static bool IsValid(QuestSet quest, string objectiveId)
        => quest != null && quest.Objectives != null &&
           quest.Objectives.Any(objective => objective != null && objective.ObjectiveId == objectiveId);

    private static List<Choice> BuildChoices()
    {
        var choices = new List<Choice>();
        foreach (QuestSet quest in QuestEditorValidation.FindQuests())
        {
            if (quest.Objectives == null) continue;
            foreach (QuestObjective_Child objective in quest.Objectives.Where(objective => objective != null))
            {
                string questName = string.IsNullOrWhiteSpace(quest.DisplayName) ? quest.name : quest.DisplayName;
                string objectiveName = string.IsNullOrWhiteSpace(objective.DisplayName)
                    ? "Objective " + (quest.Objectives.IndexOf(objective) + 1)
                    : objective.DisplayName;
                choices.Add(new Choice
                {
                    Quest = quest,
                    Objective = objective,
                    Label = questName + " / " + objectiveName
                });
            }
        }
        return choices.OrderBy(choice => choice.Label).ToList();
    }
}

[CustomEditor(typeof(QuestEventSender))]
public class QuestEventSenderEditor : Editor
{
    private bool showAdvanced;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty system = serializedObject.FindProperty("questSystem");
        SerializedProperty quest = serializedObject.FindProperty("questDefinition");
        SerializedProperty objectiveId = serializedObject.FindProperty("objectiveId");
        SerializedProperty eventId = serializedObject.FindProperty("eventId");
        SerializedProperty targetId = serializedObject.FindProperty("targetId");
        SerializedProperty amount = serializedObject.FindProperty("amount");
        var resolvedSystem = system.objectReferenceValue as QuestSystem ?? QuestEditorAutomation.FindSceneSystem();
        bool hasDirectObjective = QuestObjectivePicker.IsValid(quest.objectReferenceValue as QuestSet, objectiveId.stringValue);
        bool hasLegacyEvent = !string.IsNullOrWhiteSpace(eventId.stringValue);
        int issues = (resolvedSystem == null ? 1 : 0) + (!hasDirectObjective && !hasLegacyEvent ? 1 : 0) +
                     (amount.intValue <= 0 ? 1 : 0);

        QuestEditorUI.Header("Quest Event Sender", "Progress an objective without writing code",
            EditorGUIUtility.IconContent("d_Animation.EventMarker").image);
        QuestEditorUI.Status(issues, "Event sender is ready");

        QuestEditorUI.Section("Objective");
        QuestObjectivePicker.Draw(quest, objectiveId, eventId, targetId);
        QuestEditorValidation.Field(serializedObject, "amount", "Amount");

        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true, EditorStyles.foldoutHeader);
        if (showAdvanced)
        {
            QuestEditorValidation.Field(serializedObject, "questSystem", "Quest System Override");
            EditorGUILayout.PropertyField(quest, new GUIContent("Quest Set"));
            EditorGUILayout.PropertyField(objectiveId, new GUIContent("Objective ID"));
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Legacy event matching", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(eventId, new GUIContent("Event ID"));
            EditorGUILayout.PropertyField(targetId, new GUIContent("Target ID (Optional)"));
        }
        serializedObject.ApplyModifiedProperties();

        if (resolvedSystem == null)
        {
            QuestEditorUI.CompactWarning("No Quest System exists in this scene.");
            if (GUILayout.Button("Create Quest System"))
            {
                QuestEditorAutomation.SetupScene(out _);
                serializedObject.Update();
            }
        }
        else if (system.objectReferenceValue == null)
            EditorGUILayout.HelpBox("Quest System will be found automatically: " + resolvedSystem.name, MessageType.Info);
        if (!hasDirectObjective && !hasLegacyEvent) QuestEditorUI.CompactWarning("Select the Objective this object progresses.");
        if (!hasDirectObjective && hasLegacyEvent)
            EditorGUILayout.HelpBox("Using legacy Event ID matching. Select an Objective above to use the easier workflow.", MessageType.Info);
        if (amount.intValue <= 0) QuestEditorUI.CompactWarning("Amount must be greater than zero.");

        QuestEditorUI.Section("Play mode test");
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
            if (GUILayout.Button(new GUIContent(" Send Objective", EditorGUIUtility.IconContent("PlayButton").image), GUILayout.Height(30f)))
                ((QuestEventSender)target).SendEvent();
    }
}
