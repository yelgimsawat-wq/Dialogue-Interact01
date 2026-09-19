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
    public void ReportEvent(string eventId, int amount = 1) => ProcessEvent(eventId, null, amount);
    public void ReportEvent(string eventId, string targetId, int amount = 1) => ProcessEvent(eventId, targetId, amount);

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

            if (string.IsNullOrWhiteSpace(objective.EventId) ||
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

    public void ProcessEvent(string eventId, string targetId, int amount){
        if (string.IsNullOrWhiteSpace(eventId) || amount <= 0) return;
        foreach(QuestInstance quest in quests.ToArray()){
            quest.ProcessEvent(eventId, targetId, amount);
        }
        
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