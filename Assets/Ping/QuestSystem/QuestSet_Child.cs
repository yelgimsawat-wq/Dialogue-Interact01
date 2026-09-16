using UnityEngine;
using System;

[Serializable]
public class QuestObjective_Child
{
    [Tooltip("What do quest want")]
    public string DisplayName;

    [Tooltip("Quest Details")]
    public string description;

    [Tooltip("ระบุว่ารอeventไหนเกิดขึ้นอยู่ เช่น enemy_killed")]
    public string EventId;
    
    [Tooltip("Target, Ex.Goblin or smth")]
    public string TargetId;

    [Min(1)]
    public int requiredAmount = 1;
}