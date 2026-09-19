using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestGiver))]
public class QuestGiverEditor : Editor
{
    private bool showAdvanced;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var definition = serializedObject.FindProperty("questDefinition");
        var system = serializedObject.FindProperty("questSystem");
        var resolvedSystem = system.objectReferenceValue as QuestSystem ?? QuestEditorAutomation.FindSceneSystem();
        int issues = (definition.objectReferenceValue == null ? 1 : 0) + (resolvedSystem == null ? 1 : 0);

        QuestEditorUI.Header("Quest Giver", "Connect this NPC to a quest",
            EditorGUIUtility.IconContent("GameObject Icon").image);
        QuestEditorUI.Status(issues, "NPC is ready to give a quest");

        QuestEditorUI.Section("Quest setup");
        QuestEditorValidation.Field(serializedObject, "questDefinition", "Quest Set");
        QuestEditorValidation.Field(serializedObject, "canGiveQuest", "Can Give Quest");

        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true, EditorStyles.foldoutHeader);
        if (showAdvanced)
        {
            QuestEditorValidation.Field(serializedObject, "questSystem", "Quest System Override");
            QuestEditorValidation.Field(serializedObject, "onQuestResult", "On Quest Result");
            QuestEditorValidation.Field(serializedObject, "onAccepted", "On Accepted");
        }
        serializedObject.ApplyModifiedProperties();

        if (definition.objectReferenceValue == null) QuestEditorUI.CompactWarning("Assign the Quest Set this NPC should offer.");
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

        QuestEditorUI.Section("Play mode test");
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
            if (GUILayout.Button(new GUIContent(" Give Quest", EditorGUIUtility.IconContent("PlayButton").image), GUILayout.Height(30f)))
                ((QuestGiver)target).GiveQuest();
    }
}
