using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "NewQuest",
    menuName = "QuestSystem/QuestSet"
)]

public class QuestSet : ScriptableObject
{
    [Tooltip("QuestId, QuestName and description")]
    public string QuestId;

    public string DisplayName;

    [TextArea(3, 6)]
    public string Description;

    public List<QuestObjective_Child> Objectives = new List<QuestObjective_Child>();
    
}
