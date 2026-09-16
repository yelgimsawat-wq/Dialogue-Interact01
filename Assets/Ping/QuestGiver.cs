using UnityEngine;

public class QuestGiver : MonoBehaviour
{

    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestSet questDefinition;

    [ContextMenu("Give Quest")]
    public void GiveQuest()
    {
        if (questSystem == null)
        {
            Debug.LogError("กรุณากำหนด QuestSystem ใน Inspector");
            return;
        }

        if (questSystem.Manager == null)
        {
            Debug.LogError("QuestManager ยังไม่พร้อม");
            return;
        }

        QuestAcceptResult result =
            questSystem.Manager.AcceptQuest(questDefinition);

        Debug.Log("ผลการรับเควสต์: " + result);
    }
    
}
