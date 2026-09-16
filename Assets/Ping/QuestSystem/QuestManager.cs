using UnityEngine;
using System.Collections.Generic;
using System;

public class QuestManager{
    public event Action<QuestInstance> onQuestAccepted;
    public event Action<QuestInstance> onQuestCompleted;
    public event Action<QuestInstance, QuestProgression> onQuestProgressChanged;
    private List<QuestInstance> quests = new List<QuestInstance>();
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
                string.IsNullOrWhiteSpace(objective.TargetId) ||
                objective.requiredAmount <= 0)
            {
                return QuestAcceptResult.InvalidObjectives;
            }
        }

        foreach(QuestInstance existingQuest in quests){
            if(existingQuest.quest.QuestId == questDefinition.QuestId){
                return QuestAcceptResult.AlreadyAccepted;
            }
        }
        
        QuestInstance newQuest = new QuestInstance(questDefinition);
        quests.Add(newQuest);
        newQuest.onCompleted += HandleQuestCompleted;
        newQuest.onProgressChanged += HandleQuestProgressChanged;
        onQuestAccepted?.Invoke(newQuest);
        return QuestAcceptResult.Success;
    }

    public void ProcessEvent(string eventId, string targetId, int amount){

        foreach(QuestInstance quest in quests){
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