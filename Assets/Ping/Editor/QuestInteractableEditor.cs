using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestInteractable))]
public class QuestInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var legacy = (QuestInteractable)target;
        SerializedProperty action = serializedObject.FindProperty("action");
        SerializedProperty quest = serializedObject.FindProperty("questDefinition");

        QuestEditorUI.Header("Quest Interactable (old)", "Replaced by Quest Giver and Quest Event Sender",
            EditorGUIUtility.IconContent("console.infoicon").image);
        foreach (QuestIssue issue in QuestEditorChecks.Legacy(legacy)) QuestEditorUI.Issue(issue);

        QuestEditorUI.Section("Current settings");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questSystem"), new GUIContent("Quest System (optional)"));
        EditorGUILayout.PropertyField(action, new GUIContent("Action"));
        var selected = (QuestInteractionActions)action.intValue;
        if (selected == QuestInteractionActions.AcceptQuest)
        {
            EditorGUILayout.PropertyField(quest, new GUIContent("Quest To Accept"));
        }
        else if (selected == QuestInteractionActions.ReportEvent)
        {
            QuestObjectivePicker.Draw(quest, serializedObject.FindProperty("objectiveId"),
                serializedObject.FindProperty("eventId"), serializedObject.FindProperty("targetId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("amount"));
        }
        serializedObject.ApplyModifiedProperties();
    }
}
