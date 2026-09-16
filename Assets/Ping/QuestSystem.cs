using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public QuestManager Manager { get; private set; }
    
    private void Awake() {
        Manager = new QuestManager();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
