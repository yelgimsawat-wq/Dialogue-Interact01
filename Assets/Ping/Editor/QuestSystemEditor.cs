using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestSystem))]
public class QuestSystemEditor : Editor
{
    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI()
    {
        QuestEditorUI.Header("Quest System", "Keeps track of accepted quests in this scene",
            EditorGUIUtility.IconContent("ScriptableObject Icon").image);

        int systems = FindObjectsByType<QuestSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        if (systems > 1)
            EditorGUILayout.HelpBox("This scene has " + systems + " Quest Systems. Keep only one.", MessageType.Warning);
        if (FindFirstObjectByType<QuestHUD>(FindObjectsInactive.Include) == null)
        {
            EditorGUILayout.HelpBox("No Quest HUD: players won't see their quests or progress.", MessageType.Info);
            if (GUILayout.Button("Add Quest HUD")) Undo.AddComponent<QuestHUD>(((QuestSystem)target).gameObject);
        }
        if (GUILayout.Button("Open Quest System window", GUILayout.Height(26f))) QuestSystemWindow.Open();

        if (!Application.isPlaying) return;
        QuestEditorUI.Section("Accepted quests");
        var manager = ((QuestSystem)target).Manager;
        if (manager.GetAllQuests().Count == 0) EditorGUILayout.LabelField("None yet.", EditorStyles.miniLabel);
        foreach (QuestInstance quest in manager.GetAllQuests())
        {
            EditorGUILayout.LabelField(quest.quest.Title, quest.IsCompleted() ? "Completed" : "Active", EditorStyles.boldLabel);
            foreach (QuestProgression progression in quest.progressions)
                EditorGUILayout.LabelField("    " + progression.GetDisplayName(),
                    progression.GetCurrentAmount() + "/" + progression.GetRequiredAmount());
        }
    }
}
