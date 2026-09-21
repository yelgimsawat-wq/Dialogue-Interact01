using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class QuestEditorValidation
{
    internal static List<QuestSet> FindQuests() => AssetDatabase.FindAssets("t:QuestSet")
        .Select(g => AssetDatabase.LoadAssetAtPath<QuestSet>(AssetDatabase.GUIDToAssetPath(g)))
        .Where(q => q != null).ToList();

    internal static IEnumerable<string> Check(QuestSet quest, List<QuestSet> all)
    {
        if (string.IsNullOrWhiteSpace(quest.QuestId)) yield return "Quest ID is empty.";
        else if (all.Count(q => q.QuestId == quest.QuestId) > 1) yield return "Duplicate Quest ID: " + quest.QuestId;
        if (string.IsNullOrWhiteSpace(quest.DisplayName)) yield return "Quest Name is empty.";
        if (quest.Objectives == null || quest.Objectives.Count == 0) yield return "Add at least one Objective.";
        else
        {
            var ids = new HashSet<string>();
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var o = quest.Objectives[i];
                string prefix = "Objective " + (i + 1) + ": ";
                if (o == null) { yield return prefix + "Missing definition."; continue; }
                if (string.IsNullOrWhiteSpace(o.ObjectiveId)) yield return prefix + "Objective ID is empty (legacy data remains usable).";
                else if (!ids.Add(o.ObjectiveId)) yield return prefix + "Duplicate Objective ID.";
                if (o.requiredAmount <= 0) yield return prefix + "Required Amount must be greater than zero.";
            }
        }
        if (quest.RequiredQuests != null)
            foreach (var prerequisite in quest.RequiredQuests)
                if (prerequisite == null) yield return "Missing prerequisite Quest Set.";
                else if (string.IsNullOrWhiteSpace(prerequisite.QuestId)) yield return "Prerequisite has no Quest ID.";
        if (HasCycle(quest, new HashSet<QuestSet>(), new HashSet<QuestSet>()))
            yield return "Prerequisites contain a cycle; this quest cannot be unlocked normally.";
    }

    private static bool HasCycle(QuestSet quest, HashSet<QuestSet> visiting, HashSet<QuestSet> visited)
    {
        if (quest == null || visited.Contains(quest)) return false;
        if (!visiting.Add(quest)) return true;
        if (quest.RequiredQuests != null)
            foreach (var prerequisite in quest.RequiredQuests)
                if (HasCycle(prerequisite, visiting, visited)) return true;
        visiting.Remove(quest);
        visited.Add(quest);
        return false;
    }

    internal static void Field(SerializedObject obj, string name, string label)
        => EditorGUILayout.PropertyField(obj.FindProperty(name), new GUIContent(label), true);
}
