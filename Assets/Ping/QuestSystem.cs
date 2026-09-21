using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class QuestSystem : MonoBehaviour
{
    public QuestManager Manager { get; private set; }
    
    private void Awake() {
        QuestSystem existing = FindFirstObjectByType<QuestSystem>();
        if (existing != null && existing != this)
        {
            Debug.LogError("Only one Quest System should exist in a scene.", this);
            enabled = false;
            return;
        }
        Manager = new QuestManager();
    }
}
