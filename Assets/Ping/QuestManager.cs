using UnityEngine;
using System.Collections.Generic;

public class QuestManager{
    private List<QuestInstance> quests = new List<QuestInstance>();
    public bool AcceptQuest(QuestSet questDefinition){
        if(questDefinition == null){
            return false;
        }
        if(string.IsNullOrWhiteSpace(questDefinition.QuestId)){
            return false;
        }
        foreach(QuestInstance existingQuest in quests){
            if(existingQuest.quest.QuestId == questDefinition.QuestId){
                return false;
            }
        }
        
        QuestInstance newQuest = new QuestInstance(questDefinition);
        quests.Add(newQuest);

        return true;
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
    
}