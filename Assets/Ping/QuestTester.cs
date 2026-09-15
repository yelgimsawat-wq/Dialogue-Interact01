using UnityEngine;

public class QuestTester : MonoBehaviour{

    public QuestSet TestQuest;
    public QuestSet SecondTestQuest;

    private QuestInstance CurrentQuest;

    private void Start(){
        Debug.Log("Start test");
        questManager = new QuestManager();
        bool accepted = questManager.AcceptQuest(TestQuest);
        bool secondAccepted = questManager.AcceptQuest(SecondTestQuest);

        if (!accepted)
        {
            Debug.Log("รับเควสต์ไม่สำเร็จ");
            return;
        }
        bool acceptedAgain = questManager.AcceptQuest(TestQuest);
        Debug.Log(acceptedAgain);
        Debug.Log(secondAccepted);

        CurrentQuest = questManager.GetQuest(TestQuest.QuestId);
        CurrentQuest.onCompleted += HandleQuestCompleted;
        CurrentQuest.onProgressChanged += HandleProgressionChanged;

        questManager.ProcessEvent("item_collected", "apple", 1);
        questManager.ProcessEvent("item_collected", "apple", 1);
        questManager.ProcessEvent("item_collected", "water", 1);

        PrintProgress();
    }

    private void PrintProgress(){
        foreach(var progression in CurrentQuest.progressions){
            Debug.Log(progression.GetDisplayName() + " " + progression.GetCurrentAmount() + "/" + progression.GetRequiredAmount());
        }        
    }

    private void HandleQuestCompleted(){
        Debug.Log("Quest " + CurrentQuest.quest.QuestId + " Completed");
    }

    private void OnDestroy() {

        if(CurrentQuest != null) {
            CurrentQuest.onCompleted -= HandleQuestCompleted;
            CurrentQuest.onProgressChanged -= HandleProgressionChanged;

        }
    }

    private void HandleProgressionChanged(QuestProgression progression){
        Debug.Log(progression.GetDisplayName() + " Progress Changed " + progression.GetCurrentAmount() + "/" + progression.GetRequiredAmount());

    }

    private QuestManager questManager;

    

}
