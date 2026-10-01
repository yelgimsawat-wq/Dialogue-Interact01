using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(
    fileName = "NewQuest",
    menuName = "Quest/Quest Set"
)]

public class QuestSet : ScriptableObject
{
    [Tooltip("QuestId, QuestName and description")]
    public string QuestId;

    public string DisplayName;

    [TextArea(3, 6)]
    public string Description;

    public List<QuestObjective_Child> Objectives = new List<QuestObjective_Child>();
    [Tooltip("Optional: all these quests must be completed before accepting this quest.")]
    public List<QuestSet> RequiredQuests = new List<QuestSet>();

    /// <summary>Display Name, or the asset name when none is set.</summary>
    public string Title => string.IsNullOrWhiteSpace(DisplayName) ? name : DisplayName;

    public QuestObjective_Child FindObjective(string objectiveId)
        => string.IsNullOrWhiteSpace(objectiveId) || Objectives == null
            ? null
            : Objectives.Find(objective => objective != null && objective.ObjectiveId == objectiveId);

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(QuestId))
            QuestId = CreateGeneratedId("quest");

        if (Objectives == null) Objectives = new List<QuestObjective_Child>();
        var usedIds = new HashSet<string>();
        foreach (QuestObjective_Child objective in Objectives)
        {
            if (objective == null) continue;
            if (string.IsNullOrWhiteSpace(objective.ObjectiveId) || !usedIds.Add(objective.ObjectiveId))
            {
                objective.ObjectiveId = CreateGeneratedId("objective");
                usedIds.Add(objective.ObjectiveId);
            }

            // Legacy event reporting remains available without requiring setup.
            if (string.IsNullOrWhiteSpace(objective.EventId))
                objective.EventId = objective.ObjectiveId;
            objective.requiredAmount = Mathf.Max(1, objective.requiredAmount);
        }
    }

    public static string CreateGeneratedId(string prefix)
        => prefix + "_" + Guid.NewGuid().ToString("N");
}
