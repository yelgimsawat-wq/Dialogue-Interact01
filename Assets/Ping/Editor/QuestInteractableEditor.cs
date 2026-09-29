using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestInteractable))]
public class QuestInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty system = serializedObject.FindProperty("questSystem");
        SerializedProperty action = serializedObject.FindProperty("action");
        SerializedProperty quest = serializedObject.FindProperty("questDefinition");
        SerializedProperty objective = serializedObject.FindProperty("objectiveId");
        SerializedProperty eventId = serializedObject.FindProperty("eventId");
        SerializedProperty targetId = serializedObject.FindProperty("targetId");
        SerializedProperty amount = serializedObject.FindProperty("amount");

        EditorGUILayout.PropertyField(system, new GUIContent("Quest System (optional)"));
        EditorGUILayout.PropertyField(action, new GUIContent("Action"));
        QuestInteractionActions selected = (QuestInteractionActions)action.enumValueIndex;
        if (selected == QuestInteractionActions.AcceptQuest)
            EditorGUILayout.PropertyField(quest, new GUIContent("Quest To Accept"));
        else if (selected == QuestInteractionActions.ReportEvent)
        {
            EditorGUILayout.PropertyField(quest, new GUIContent("Quest Set"));
            QuestObjectivePicker.Draw(quest, objective, eventId, targetId);
            EditorGUILayout.PropertyField(amount);
            EditorGUILayout.HelpBox("The quest must be active before an objective can progress.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
