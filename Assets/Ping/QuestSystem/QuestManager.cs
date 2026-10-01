using UnityEngine;
using System.Collections.Generic;
using System;

public class QuestManager{
    public event Action<QuestInstance> onQuestAccepted;
    public event Action<QuestInstance> onQuestCompleted;
    public event Action<QuestInstance, QuestProgression> onQuestProgressChanged;
    private List<QuestInstance> quests = new List<QuestInstance>();
    public QuestAcceptResult TryAcceptQuest(QuestSet definition) => AcceptQuest(definition);
    public QuestInstance GetQuestInstance(string questId) => GetQuest(questId);
    public bool IsQuestCompleted(string questId) => GetQuest(questId)?.IsCompleted() == true;
    public bool ReportEvent(string eventId, int amount = 1) => ProcessEvent(eventId, null, amount);
    public bool ReportEvent(string eventId, string targetId, int amount = 1) => ProcessEvent(eventId, targetId, amount);
    public bool ReportObjective(QuestSet definition, string objectiveId, int amount = 1)
        => definition != null && ReportObjective(definition.QuestId, objectiveId, amount);

    public bool ReportObjective(string questId, string objectiveId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(objectiveId) || amount <= 0) return false;
        QuestInstance quest = GetQuest(questId);
        return quest != null && quest.ProcessObjective(objectiveId, amount);
    }

    /// <summary>True when the quest is accepted and the objective still needs progress.</summary>
    public bool CanProgress(QuestSet definition, string objectiveId)
        => definition != null && GetQuest(definition.QuestId)?.CanProgress(objectiveId) == true;

    public bool CanProgressEvent(string eventId, string targetId)
    {
        foreach (QuestInstance quest in quests)
            if (quest.CanProgressEvent(eventId, targetId)) return true;
        return false;
    }

    public QuestState GetState(QuestSet definition)
    {
        if (definition == null) return QuestState.Locked;
        QuestInstance quest = GetQuest(definition.QuestId);
        if (quest != null) return quest.IsCompleted() ? QuestState.Completed : QuestState.Active;
        return AreRequirementsMet(definition) ? QuestState.Available : QuestState.Locked;
    }

    public bool AreRequirementsMet(QuestSet definition)
    {
        if (definition == null) return false;
        if (definition.RequiredQuests == null) return true;
        foreach (var prerequisite in definition.RequiredQuests)
            if (prerequisite == null || prerequisite == definition ||
                !IsQuestCompleted(prerequisite.QuestId)) return false;
        return true;
    }
    public QuestAcceptResult AcceptQuest(QuestSet questDefinition){
        if(questDefinition == null){
            return QuestAcceptResult.NullDefinition;
        }
        if(string.IsNullOrWhiteSpace(questDefinition.QuestId)){
            return QuestAcceptResult.MissingQuestId;
        }
        if (questDefinition.Objectives == null || questDefinition.Objectives.Count == 0) {
            return QuestAcceptResult.NoObjectives;
        }

        foreach (var objective in questDefinition.Objectives)
        {
            if (objective == null)
            {
                return QuestAcceptResult.InvalidObjectives;
            }

            if ((string.IsNullOrWhiteSpace(objective.ObjectiveId) && string.IsNullOrWhiteSpace(objective.EventId)) ||
                objective.requiredAmount <= 0)
            {
                return QuestAcceptResult.InvalidObjectives;
            }
        }

        foreach(QuestInstance existingQuest in quests){
            if(existingQuest.quest.QuestId == questDefinition.QuestId){
                return existingQuest.IsCompleted() ? QuestAcceptResult.AlreadyCompleted : QuestAcceptResult.AlreadyActive;
            }
        }
        
        if (!AreRequirementsMet(questDefinition)) return QuestAcceptResult.RequirementNotMet;
        QuestInstance newQuest = new QuestInstance(questDefinition);
        quests.Add(newQuest);
        newQuest.onCompleted += HandleQuestCompleted;
        newQuest.onProgressChanged += HandleQuestProgressChanged;
        onQuestAccepted?.Invoke(newQuest);
        return QuestAcceptResult.Success;
    }

    /// <returns>True when any accepted quest progressed.</returns>
    public bool ProcessEvent(string eventId, string targetId, int amount){
        if (string.IsNullOrWhiteSpace(eventId) || amount <= 0) return false;
        bool progressed = false;
        foreach(QuestInstance quest in quests.ToArray()){
            progressed |= quest.ProcessEvent(eventId, targetId, amount);
        }
        return progressed;
    }

    public QuestInstance GetQuest(string questId){
        foreach(QuestInstance quest in quests){
            if(quest.quest.QuestId == questId){
                return quest;
            } 
        }
        return null;
    }

    private void HandleQuestCompleted(QuestInstance completedQuest){
        onQuestCompleted?.Invoke(completedQuest);
    }
    
    private void HandleQuestProgressChanged(QuestInstance changeQuest, QuestProgression progression){
        onQuestProgressChanged?.Invoke(changeQuest, progression);
    }

    public IReadOnlyList<QuestInstance> GetAllQuests(){
        return quests.AsReadOnly();
    }
}
