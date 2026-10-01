/// <summary>Where a quest stands for the player.</summary>
public enum QuestState
{
    /// <summary>A required quest is not completed yet.</summary>
    Locked,
    /// <summary>Can be accepted now.</summary>
    Available,
    /// <summary>Accepted; objectives are in progress.</summary>
    Active,
    Completed
}
