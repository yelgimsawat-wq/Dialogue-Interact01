using UnityEngine;

public class QuestEventSender : MonoBehaviour
{

    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private string eventId;
    [SerializeField] private string targetId;
    [SerializeField] private int amount = 1;

    [ContextMenu("Send Quest Event")]
    public void SendEvent(){
        if(questSystem == null){
            Debug.LogError("QuestSystem ไม่ได้กำหนด");
            return;
        }
        if(questSystem.Manager == null){
            Debug.LogError("QuestManager ไม่ได้กำหนด");
            return;
        }
        questSystem.Manager.ProcessEvent(eventId, targetId, amount);

    }

}
