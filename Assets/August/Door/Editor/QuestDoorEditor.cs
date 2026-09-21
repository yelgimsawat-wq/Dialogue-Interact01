using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestDoor))]
public class QuestDoorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Door", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isRotatingDoor"), new GUIContent("Rotating Door"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), new GUIContent("Opening Speed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAmount"), new GUIContent("Rotation Amount"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("forwardDirection"), new GUIContent("Forward Direction"));

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Quest Lock", EditorStyles.boldLabel);
        var quest = serializedObject.FindProperty("targetQuest");
        EditorGUILayout.PropertyField(quest, new GUIContent("Required Quest"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoOpenOnComplete"), new GUIContent("Open When Completed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockedPrompt"), new GUIContent("Locked Message"));

        if (quest.objectReferenceValue == null)
            EditorGUILayout.HelpBox("Assign a Quest Set. The door stays locked until that quest is completed.", MessageType.Warning);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isUnlocked"), new GUIContent("Unlocked At Start"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questSystem"), new GUIContent("Quest System Override"));

        serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(6f);
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.Toggle("Currently Unlocked", ((QuestDoor)target).IsUnlocked);
        }
    }
}
