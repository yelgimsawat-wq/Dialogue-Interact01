using UnityEngine;
using System.Collections.Generic;
using System;

public class QuestInstance
{
    public event Action onCompleted;
    public QuestSet quest;
    public List<QuestProgression> progressions;

    public QuestInstance(QuestSet questDefinition){
        quest = questDefinition;
        progressions = new List<QuestProgression>();

        foreach(var objective in quest.Objectives){
            QuestProgression progression = new QuestProgression(objective);
            progression.onProgressChanged += HandleObjectiveChanged;
            progressions.Add(progression);
        }

    }
    

    public bool IsCompleted(){

        if(progressions.Count == 0){
            return false;
        }
        
        foreach(var progression in progressions){
            if(!progression.IsCompleted()){
                return false;
            }
        }
        return true;
    }

    public void ProcessEvent(string eventId, string targetId, int amount){
        
        if(completionHandled == true){
            return;
        }

        foreach(var progression in progressions){
            if(!progression.IsCompleted() && progression.MatchesEvent(eventId, targetId)){
                progression.Increment(amount);
            }
        }

        if(IsCompleted() == true){
            completionHandled = true;
            onCompleted?.Invoke();
        }
    }

    private bool completionHandled = false;

    public event Action<QuestProgression> onProgressChanged;

    private void HandleObjectiveChanged(QuestProgression progression){
        onProgressChanged?.Invoke(progression);
    }


}
