using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestEventSender))]
public class QuestEventSenderEditor : Editor
{
    private sealed class EventChoice
    {
        internal string Label;
        internal string EventId;
        internal string TargetId;
    }

    private bool showAdvanced;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var system = serializedObject.FindProperty("questSystem");
        var eventId = serializedObject.FindProperty("eventId");
        var targetId = serializedObject.FindProperty("targetId");
        var amount = serializedObject.FindProperty("amount");
        var resolvedSystem = system.objectReferenceValue as QuestSystem ?? QuestEditorAutomation.FindSceneSystem();
        int issues = (resolvedSystem == null ? 1 : 0) +
            (string.IsNullOrWhiteSpace(eventId.stringValue) ? 1 : 0) + (amount.intValue <= 0 ? 1 : 0);

        QuestEditorUI.Header("Quest Event Sender", "Report an action to active quests",
            EditorGUIUtility.IconContent("d_Animation.EventMarker").image);
        QuestEditorUI.Status(issues, "Event sender is ready");

        QuestEditorUI.Section("Event setup");
        DrawEventPicker(eventId, targetId);
        QuestEditorValidation.Field(serializedObject, "amount", "Amount");

        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true, EditorStyles.foldoutHeader);
        if (showAdvanced)
        {
            QuestEditorValidation.Field(serializedObject, "questSystem", "Quest System Override");
            QuestEditorValidation.Field(serializedObject, "eventId", "Event ID");
            QuestEditorValidation.Field(serializedObject, "targetId", "Target ID (Optional)");
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
        if (string.IsNullOrWhiteSpace(eventId.stringValue)) QuestEditorUI.CompactWarning("Enter the Event ID expected by an objective.");
        if (amount.intValue <= 0) QuestEditorUI.CompactWarning("Amount must be greater than zero.");

        QuestEditorUI.Section("Play mode test");
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
            if (GUILayout.Button(new GUIContent(" Send Event", EditorGUIUtility.IconContent("PlayButton").image), GUILayout.Height(30f)))
                ((QuestEventSender)target).SendEvent();
    }

    private static void DrawEventPicker(SerializedProperty eventId, SerializedProperty targetId)
    {
        var choices = new List<EventChoice>();
        foreach (var quest in QuestEditorValidation.FindQuests())
        {
            if (quest.Objectives == null) continue;
            foreach (var objective in quest.Objectives.Where(o => o != null && !string.IsNullOrWhiteSpace(o.EventId)))
            {
                string target = objective.TargetId ?? string.Empty;
                if (choices.Any(choice => choice.EventId == objective.EventId && choice.TargetId == target)) continue;
                string questName = string.IsNullOrWhiteSpace(quest.DisplayName) ? quest.name : quest.DisplayName;
                string objectiveName = string.IsNullOrWhiteSpace(objective.DisplayName) ? objective.EventId : objective.DisplayName;
                choices.Add(new EventChoice
                {
                    Label = questName + " / " + objectiveName,
                    EventId = objective.EventId,
                    TargetId = target
                });
            }
        }

        var labels = new[] { "Custom event..." }.Concat(choices.Select(choice => choice.Label)).ToArray();
        int current = choices.FindIndex(choice => choice.EventId == eventId.stringValue && choice.TargetId == targetId.stringValue) + 1;
        int selected = EditorGUILayout.Popup("Quest Objective", current, labels);
        if (selected > 0 && selected != current)
        {
            eventId.stringValue = choices[selected - 1].EventId;
            targetId.stringValue = choices[selected - 1].TargetId;
        }
        if (selected > 0)
            EditorGUILayout.LabelField("Sends", eventId.stringValue + (string.IsNullOrWhiteSpace(targetId.stringValue) ? string.Empty : " / " + targetId.stringValue), EditorStyles.miniLabel);
    }
}