using UnityEngine;
using System;

public class QuestProgression{

    [Min(0)]
    private int currentAmount = 0;
    
    private QuestObjective_Child definition;

    public QuestProgression(QuestObjective_Child objective){
        definition = objective;
    }

    public void Increment(int Amount){

        if(Amount <= 0){
            return;
        }

        if(IsCompleted()){
            return;
        }
        
        currentAmount += Math.Min(Amount, definition.requiredAmount - currentAmount);

        if(currentAmount > definition.requiredAmount){
            currentAmount = definition.requiredAmount;
        }

        onProgressChanged?.Invoke(this);

    }

    public bool IsCompleted(){
        return currentAmount >= definition.requiredAmount;
    }

    public bool MatchesEvent(string eventId , string targetId){
        return definition.MatchesEvent(eventId, targetId);
    }

    public int GetCurrentAmount(){
        return currentAmount;
    }

    public int GetRequiredAmount(){
        return definition.requiredAmount;
    }

    public string GetDisplayName(){
        return definition.DisplayName;
    }

    public event Action<QuestProgression> onProgressChanged;
    

}