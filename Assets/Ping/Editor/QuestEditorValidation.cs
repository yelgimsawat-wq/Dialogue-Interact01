using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class QuestEditorValidation
{
    internal static List<QuestSet> FindQuests() => AssetDatabase.FindAssets("t:QuestSet")
        .Select(g => AssetDatabase.LoadAssetAtPath<QuestSet>(AssetDatabase.GUIDToAssetPath(g)))
        .Where(q => q != null).ToList();

    internal static IEnumerable<QuestIssue> Issues(QuestSet quest, List<QuestSet> all)
    {
        if (string.IsNullOrWhiteSpace(quest.QuestId))
            yield return new QuestIssue(quest, "Quest ID is empty.", "Generate ID",
                () => Edit(quest, "Generate Quest ID", () => quest.QuestId = QuestEditorAutomation.UniqueQuestId(quest)));
        else if (all.Count(q => q != null && q.QuestId == quest.QuestId) > 1)
            yield return new QuestIssue(quest,
                "Another Quest Set uses the same Quest ID (\"" + quest.QuestId + "\"), so only one of them can be accepted. This usually happens after duplicating an asset.",
                "Make ID unique",
                () =>
                {
                    // With "Fix all" the other copy may already have been renamed.
                    if (FindQuests().Count(q => q.QuestId == quest.QuestId) > 1)
                        Edit(quest, "Make Quest ID Unique", () => quest.QuestId = QuestEditorAutomation.UniqueQuestId(quest));
                });

        if (string.IsNullOrWhiteSpace(quest.DisplayName))
            yield return new QuestIssue(quest, "Quest Name is empty.", "Use file name",
                () => Edit(quest, "Name Quest", () => quest.DisplayName = ObjectNames.NicifyVariableName(quest.name)));

        if (quest.Objectives == null || quest.Objectives.Count == 0)
        {
            yield return new QuestIssue(quest, "Add at least one Objective.", "Add objective",
                () => QuestEditorAutomation.AddObjective(quest));
        }
        else
        {
            var ids = new HashSet<string>();
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective_Child objective = quest.Objectives[i];
                string prefix = "Objective " + (i + 1) + ": ";
                if (objective == null)
                {
                    yield return new QuestIssue(quest, prefix + "Missing definition.");
                    continue;
                }
                bool emptyId = string.IsNullOrWhiteSpace(objective.ObjectiveId);
                if (emptyId || !ids.Add(objective.ObjectiveId))
                    yield return new QuestIssue(quest, prefix + (emptyId ? "Objective ID is empty." : "Duplicate Objective ID."),
                        "Generate ID", () => Edit(quest, "Generate Objective ID",
                            () => objective.ObjectiveId = QuestSet.CreateGeneratedId("objective")));
                if (string.IsNullOrWhiteSpace(objective.DisplayName))
                {
                    int number = i + 1;
                    yield return new QuestIssue(quest, prefix + "Name is empty; players see it in the quest tracker.",
                        "Name it", () => Edit(quest, "Name Objective", () => objective.DisplayName = "Objective " + number));
                }
                if (objective.requiredAmount <= 0)
                    yield return new QuestIssue(quest, prefix + "Required Amount must be greater than zero.", "Set to 1",
                        () => Edit(quest, "Set Required Amount", () => objective.requiredAmount = 1));
            }
        }

        if (quest.RequiredQuests != null)
        {
            if (quest.RequiredQuests.Any(required => required == null || required == quest))
                yield return new QuestIssue(quest, "Required Quests has an empty slot or lists this quest itself.", "Clean up",
                    () => Edit(quest, "Clean Up Required Quests",
                        () => quest.RequiredQuests.RemoveAll(required => required == null || required == quest)));
            foreach (QuestSet required in quest.RequiredQuests.Where(required => required != null && required != quest))
                if (string.IsNullOrWhiteSpace(required.QuestId))
                    yield return new QuestIssue(quest, "Required quest \"" + required.Title + "\" has no Quest ID.");
        }
        if (HasCycle(quest, new HashSet<QuestSet>(), new HashSet<QuestSet>()))
            yield return new QuestIssue(quest, "Required Quests loop back to this quest, so it can never be unlocked.");
    }

    /// <summary>Records an undoable change to a Quest Set asset.</summary>
    internal static void Edit(QuestSet quest, string undoName, Action change)
    {
        Undo.RecordObject(quest, undoName);
        change();
        EditorUtility.SetDirty(quest);
    }

    private static bool HasCycle(QuestSet quest, HashSet<QuestSet> visiting, HashSet<QuestSet> visited)
    {
        if (quest == null || visited.Contains(quest)) return false;
        if (!visiting.Add(quest)) return true;
        if (quest.RequiredQuests != null)
            foreach (var prerequisite in quest.RequiredQuests)
                if (prerequisite != quest && HasCycle(prerequisite, visiting, visited)) return true;
        visiting.Remove(quest);
        visited.Add(quest);
        return false;
    }

    internal static void Field(SerializedObject obj, string name, string label)
        => EditorGUILayout.PropertyField(obj.FindProperty(name), new GUIContent(label), true);
}
