using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Gives a duplicated (Ctrl+D / copy-pasted) Quest Set its own Quest ID. Without this the copy keeps the
/// original's ID and the two quests block each other at runtime. Only Quest Sets new to this editor
/// session are touched, so existing IDs never change behind your back.
/// </summary>
internal sealed class QuestSetIdPostprocessor : AssetPostprocessor
{
    private static HashSet<string> knownQuests;

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var questPaths = importedAssets
            .Where(path => path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) &&
                           AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(QuestSet))
            .ToList();
        if (knownQuests == null)
        {
            // Everything that existed before this batch counts as known.
            var batch = new HashSet<string>(questPaths.Select(AssetDatabase.AssetPathToGUID));
            knownQuests = new HashSet<string>(AssetDatabase.FindAssets("t:QuestSet").Where(guid => !batch.Contains(guid)));
        }

        List<string> fresh = questPaths.Where(path => knownQuests.Add(AssetDatabase.AssetPathToGUID(path))).ToList();
        if (fresh.Count > 0) EditorApplication.delayCall += () => GiveUniqueIds(fresh);
    }

    private static void GiveUniqueIds(List<string> paths)
    {
        List<QuestSet> all = QuestEditorValidation.FindQuests();
        foreach (string path in paths)
        {
            var quest = AssetDatabase.LoadAssetAtPath<QuestSet>(path);
            if (quest == null || string.IsNullOrWhiteSpace(quest.QuestId)) continue;
            if (!all.Any(other => other != quest && other.QuestId == quest.QuestId)) continue;

            string oldId = quest.QuestId;
            quest.QuestId = QuestEditorAutomation.UniqueQuestId(quest, all);
            EditorUtility.SetDirty(quest);
            AssetDatabase.SaveAssetIfDirty(quest);
            Debug.Log("Quest Set \"" + quest.name + "\" was a copy, so its Quest ID changed from \"" + oldId +
                      "\" to \"" + quest.QuestId + "\".", quest);
        }
    }
}
