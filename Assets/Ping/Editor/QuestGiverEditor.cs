using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestGiver))]
public class QuestGiverEditor : Editor
{
    private static bool showEvents;
    private static bool showAdvanced;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var giver = (QuestGiver)target;
        var issues = QuestEditorChecks.Giver(giver).ToList();

        QuestEditorUI.Header("Quest Giver", "Gives a quest when the player presses E",
            EditorGUIUtility.IconContent("GameObject Icon").image);
        QuestEditorUI.Status(issues.Count(issue => !issue.Advisory), "Ready to give a quest");

        QuestEditorUI.Section("Quest");
        QuestEditorValidation.Field(serializedObject, "questDefinition", "Quest Set");
        QuestEditorValidation.Field(serializedObject, "canGiveQuest", "Can Give Quest");
        var quest = serializedObject.FindProperty("questDefinition").objectReferenceValue as QuestSet;
        if (quest != null)
        {
            var required = (quest.RequiredQuests ?? new System.Collections.Generic.List<QuestSet>())
                .Where(other => other != null && other != quest).Select(other => other.Title).ToList();
            if (required.Count > 0)
                EditorGUILayout.LabelField("Unlocks after", string.Join(", ", required), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Prompt", "Press E: Accept quest \"" + quest.Title + "\"", EditorStyles.miniLabel);
            if (GUILayout.Button("Open in Quest System window")) QuestSystemWindow.Open(quest);
        }

        foreach (QuestIssue issue in issues) QuestEditorUI.Issue(issue);

        showEvents = EditorGUILayout.Foldout(showEvents, "Events", true, EditorStyles.foldoutHeader);
        if (showEvents)
        {
            QuestEditorValidation.Field(serializedObject, "onAccepted", "On Accepted");
            QuestEditorValidation.Field(serializedObject, "onQuestCompleted", "On Quest Completed");
            QuestEditorValidation.Field(serializedObject, "onQuestResult", "On Any Result");
        }

        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true, EditorStyles.foldoutHeader);
        if (showAdvanced) QuestEditorValidation.Field(serializedObject, "questSystem", "Quest System Override");
        serializedObject.ApplyModifiedProperties();

        QuestEditorUI.Section("Play mode test");
        if (Application.isPlaying && quest != null)
            EditorGUILayout.LabelField("State", QuestSystem.GetState(quest).ToString());
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
            if (GUILayout.Button(new GUIContent(" Give Quest", EditorGUIUtility.IconContent("PlayButton").image), GUILayout.Height(30f)))
                giver.GiveQuest();
    }
}
