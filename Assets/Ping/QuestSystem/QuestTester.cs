using UnityEngine;

[AddComponentMenu("Quest/Testing/Quest Tester")]
public class QuestTester : MonoBehaviour{

    public QuestSet TestQuest;
    public QuestSet SecondTestQuest;
    [SerializeField] private QuestSystem questSystem;

    [ContextMenu("Run Quest Test (Play Mode)")]
    public void RunTest(){
        if (!Application.isPlaying || questManager != null) return;
        Debug.Log("Start test");
        if (questSystem == null)
        {
            Debug.LogError("กรุณากำหนด QuestSystem ใน Inspector");
            return;
        }
        questManager = questSystem.Manager;
        questManager.onQuestAccepted += HandleQuestAccepted;
        QuestAcceptResult accepted = questManager.AcceptQuest(TestQuest);
        QuestAcceptResult secondAccepted = questManager.AcceptQuest(SecondTestQuest);

        if (accepted != QuestAcceptResult.Success)
        {
            Debug.Log("รับเควสต์ไม่สำเร็จ: " + accepted);
            return;
        }
        QuestAcceptResult acceptedAgain = questManager.AcceptQuest(TestQuest);
        Debug.Log(acceptedAgain);
        Debug.Log(secondAccepted);

        questManager.onQuestCompleted += HandleQuestCompleted;
        questManager.onQuestProgressChanged += HandleProgressionChanged;

        //questManager.ProcessEvent("item_collected", "apple", 1);
        //questManager.ProcessEvent("item_collected", "apple", 1);
        //questManager.ProcessEvent("item_collected", "water", 1);
        //questManager.ProcessEvent("item_collected", "apple", 1);
        //questManager.ProcessEvent("item_collected", "apple", 1);

        foreach(QuestInstance quest in questManager.GetAllQuests()){
            PrintProgress(quest);
        }
    }

    private void PrintProgress(QuestInstance questToPrint){
        foreach(var progression in questToPrint.progressions){
            Debug.Log(progression.GetDisplayName() + " " + progression.GetCurrentAmount() + "/" + progression.GetRequiredAmount());
        }        
    }

    private void HandleQuestCompleted(QuestInstance changeQuest){
        Debug.Log("Quest " + changeQuest.quest.QuestId + " Completed");
    }

    private void OnDestroy() {

        if(questManager != null){
            questManager.onQuestCompleted -= HandleQuestCompleted;
            questManager.onQuestProgressChanged -= HandleProgressionChanged;
            questManager.onQuestAccepted -= HandleQuestAccepted;
        }
    }

    private void HandleProgressionChanged(QuestInstance changeQuest, QuestProgression progression){
        Debug.Log(changeQuest.quest.QuestId + " " + progression.GetDisplayName() + " Progress Changed " + progression.GetCurrentAmount() + "/" + progression.GetRequiredAmount());

    }

    private QuestManager questManager;

    public void HandleQuestAccepted(QuestInstance quest){
        Debug.Log("Quest " + quest.quest.QuestId + " Accepted");
    }

    

}