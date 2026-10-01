using UnityEngine;
using System.Collections.Generic;
using System;

public class QuestInstance
{
    public event Action<QuestInstance> onCompleted;
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

    /// <returns>True when at least one objective progressed.</returns>
    public bool ProcessEvent(string eventId, string targetId, int amount){

        if(completionHandled || processingEvent || amount <= 0 || string.IsNullOrWhiteSpace(eventId)){
            return false;
        }

        bool progressed = false;
        processingEvent = true;
        try {
        foreach(var progression in progressions){
            if(!progression.IsCompleted() && progression.MatchesEvent(eventId, targetId)){
                progression.Increment(amount);
                progressed = true;
            }
        }

        if(IsCompleted() == true){
            completionHandled = true;
            onCompleted?.Invoke(this);
        }
        }
        finally { processingEvent = false; }
        return progressed;
    }

    public QuestProgression FindProgression(string objectiveId)
        => progressions.Find(progression => progression.MatchesObjective(objectiveId));

    /// <summary>True when the objective exists and still needs progress.</summary>
    public bool CanProgress(string objectiveId)
    {
        QuestProgression progression = FindProgression(objectiveId);
        return !completionHandled && progression != null && !progression.IsCompleted();
    }

    public bool CanProgressEvent(string eventId, string targetId)
        => !completionHandled && !string.IsNullOrWhiteSpace(eventId) &&
           progressions.Exists(progression => !progression.IsCompleted() && progression.MatchesEvent(eventId, targetId));

    public bool ProcessObjective(string objectiveId, int amount)
    {
        if (completionHandled || processingEvent || amount <= 0 || string.IsNullOrWhiteSpace(objectiveId))
            return false;

        QuestProgression matched = progressions.Find(progression => progression.MatchesObjective(objectiveId));
        if (matched == null || matched.IsCompleted()) return false;

        processingEvent = true;
        try
        {
            matched.Increment(amount);
            if (IsCompleted())
            {
                completionHandled = true;
                onCompleted?.Invoke(this);
            }
        }
        finally { processingEvent = false; }
        return true;
    }

    private bool completionHandled = false;
    private bool processingEvent;
    public bool IsActive => !IsCompleted();

    public event Action<QuestInstance, QuestProgression> onProgressChanged;

    private void HandleObjectiveChanged(QuestProgression progression){
        onProgressChanged?.Invoke(this, progression);
    }


}
