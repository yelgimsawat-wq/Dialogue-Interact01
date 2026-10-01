using UnityEngine;

/// <summary>
/// Holds the quest state for the scene. Only one is needed; if a scene has none, one is
/// created automatically (together with a <see cref="QuestHUD"/>) the first time a quest is used.
/// Code can use the static helpers, e.g. <c>QuestSystem.Progress(quest, "objective_1")</c>.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
[AddComponentMenu("Quest/Quest System")]
public class QuestSystem : MonoBehaviour
{
    private static QuestSystem current;
    private QuestManager manager;

    public QuestManager Manager => manager ?? (manager = new QuestManager());

    /// <summary>The scene's Quest System. In Play Mode one is created when the scene has none.</summary>
    public static QuestSystem Current
    {
        get
        {
            if (Existing == null && Application.isPlaying)
            {
                var gameObject = new GameObject("Quest System");
                gameObject.AddComponent<QuestSystem>(); // Awake registers it as current.
                if (FindFirstObjectByType<QuestHUD>() == null) gameObject.AddComponent<QuestHUD>();
            }
            return current;
        }
    }

    /// <summary>The scene's Quest System, or null. Never creates one.</summary>
    public static QuestSystem Existing
    {
        get
        {
            if (current == null)
                foreach (QuestSystem system in FindObjectsByType<QuestSystem>(FindObjectsSortMode.None))
                    if (system.enabled) { current = system; break; }
            return current;
        }
    }

    /// <summary>Returns <paramref name="preferred"/> when it is usable, otherwise the scene's system.</summary>
    public static QuestSystem Resolve(QuestSystem preferred, bool create = true)
    {
        if (preferred != null && preferred.enabled && preferred.gameObject.activeInHierarchy) return preferred;
        return create ? Current : Existing;
    }

    public static QuestAcceptResult Accept(QuestSet quest)
    {
        QuestSystem system = Current;
        return system != null ? system.Manager.TryAcceptQuest(quest) : QuestAcceptResult.SystemUnavailable;
    }

    /// <summary>Adds progress to one objective of an accepted quest.</summary>
    public static bool Progress(QuestSet quest, string objectiveId, int amount = 1)
        => Existing != null && current.Manager.ReportObjective(quest, objectiveId, amount);

    public static bool Progress(string questId, string objectiveId, int amount = 1)
        => Existing != null && current.Manager.ReportObjective(questId, objectiveId, amount);

    /// <summary>Progresses every accepted objective whose Event ID (and Target ID, if set) matches.</summary>
    public static bool Report(string eventId, string targetId = null, int amount = 1)
        => Existing != null && current.Manager.ProcessEvent(eventId, targetId, amount);

    public static QuestState GetState(QuestSet quest)
    {
        QuestSystem system = Current;
        return system != null ? system.Manager.GetState(quest) : QuestState.Locked;
    }

    public static bool IsCompleted(QuestSet quest) => GetState(quest) == QuestState.Completed;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Debug.LogError("Only one Quest System should exist in a scene. This one is disabled.", this);
            enabled = false;
            return;
        }
        current = this;
        if (manager == null) manager = new QuestManager();
    }

    private void OnDestroy()
    {
        if (current == this) current = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => current = null;
}
