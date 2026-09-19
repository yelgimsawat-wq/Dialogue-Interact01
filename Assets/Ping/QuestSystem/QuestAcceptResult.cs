using UnityEngine;

public enum QuestAcceptResult
{
    Success,
    NullDefinition,
    MissingQuestId,
    NoObjectives,
    InvalidObjectives,
    AlreadyAccepted,
    AlreadyCompleted,
    RequirementNotMet,
    InvalidQuest,
    GiverDisabled,
    SystemUnavailable,
    Accepted = Success,
    AlreadyActive = AlreadyAccepted
    
}