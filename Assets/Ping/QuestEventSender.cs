using UnityEngine;
using UnityEngine.Events;

public enum QuestSendTrigger
{
    [InspectorName("Manual (UnityEvent or code)")] Manual = 0,
    [InspectorName("Player presses E")] PlayerPressesE = 1,
    [InspectorName("Player enters area")] PlayerEntersArea = 2,
    [InspectorName("This object is destroyed")] ObjectDestroyed = 3
}

public enum QuestAfterSend
{
    Nothing = 0,
    [InspectorName("Hide object")] DisableObject = 1,
    [InspectorName("Destroy object")] DestroyObject = 2
}

/// <summary>Adds progress to one quest objective when the chosen trigger happens.</summary>
[AddComponentMenu("Quest/Quest Event Sender")]
public class QuestEventSender : MonoBehaviour, IInteractable
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private QuestSet questDefinition;
    [SerializeField] private string objectiveId;
    [SerializeField] private string eventId;
    [SerializeField] private string targetId;
    [SerializeField, Min(1)] private int amount = 1;
    [SerializeField] private QuestSendTrigger sendWhen = QuestSendTrigger.Manual;
    [SerializeField, Tooltip("Count only the first successful send.")] private bool onlyOnce;
    [SerializeField] private QuestAfterSend afterSending = QuestAfterSend.Nothing;
    [SerializeField, Tooltip("Shown when the player looks at this object. Empty = \"Press E: <objective>\".")]
    private string promptText;
    [SerializeField] private UnityEvent onSent = new UnityEvent();

    private bool hasSent;
    private static bool applicationQuitting;

    public QuestSet QuestDefinition => questDefinition;
    public string ObjectiveId => objectiveId;
    public QuestSendTrigger SendWhen => sendWhen;
    private bool UsesObjective => questDefinition != null && !string.IsNullOrWhiteSpace(objectiveId);

    public string InteractionText
    {
        get
        {
            if (sendWhen == QuestSendTrigger.PlayerPressesE && CanSend())
                return string.IsNullOrWhiteSpace(promptText) ? DefaultPrompt : promptText;
            return QuestInteractionUtility.OtherPrompt(this);
        }
    }

    private string DefaultPrompt
    {
        get
        {
            QuestObjective_Child objective = questDefinition != null ? questDefinition.FindObjective(objectiveId) : null;
            return objective != null && !string.IsNullOrWhiteSpace(objective.DisplayName)
                ? "Press E: " + objective.DisplayName
                : "Press E to interact.";
        }
    }

    public void Interact()
    {
        if (sendWhen == QuestSendTrigger.PlayerPressesE && isActiveAndEnabled) TrySend(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sendWhen == QuestSendTrigger.PlayerEntersArea && QuestInteractionUtility.IsPlayer(other)) TrySend(false);
    }

    private void OnDestroy()
    {
        // Only gameplay destruction counts, not unloading the scene or leaving Play Mode.
        if (sendWhen == QuestSendTrigger.ObjectDestroyed && !applicationQuitting && gameObject.scene.isLoaded)
            TrySend(false);
    }

    /// <summary>Sends the progress now. Hook it to a UnityEvent (e.g. a Button's On Click) or call it from code.</summary>
    [ContextMenu("Send Quest Event")]
    public void SendEvent() => TrySend(true);

    /// <summary>True when sending now would add progress: the quest is accepted and the objective is not done.</summary>
    public bool CanSend()
    {
        if (onlyOnce && hasSent) return false;
        QuestSystem system = QuestSystem.Resolve(questSystem, false);
        if (system == null) return false;
        return UsesObjective
            ? system.Manager.CanProgress(questDefinition, objectiveId)
            : system.Manager.CanProgressEvent(eventId, targetId);
    }

    private void TrySend(bool calledManually)
    {
        if (onlyOnce && hasSent) return;
        if (amount <= 0)
        {
            Debug.LogError("Amount must be greater than zero.", this);
            return;
        }
        if (!UsesObjective && string.IsNullOrWhiteSpace(eventId))
        {
            Debug.LogError("Select a Quest Objective in the Inspector.", this);
            return;
        }

        QuestSystem system = QuestSystem.Resolve(questSystem, false);
        bool progressed = system != null && (UsesObjective
            ? system.Manager.ReportObjective(questDefinition, objectiveId, amount)
            : system.Manager.ProcessEvent(eventId, targetId, amount));
        if (!progressed)
        {
            if (calledManually)
                Debug.LogWarning("Nothing progressed: the quest is not accepted yet, or the objective is already complete.", this);
            return;
        }

        hasSent = true;
        onSent?.Invoke();
        if (sendWhen == QuestSendTrigger.ObjectDestroyed) return;
        if (afterSending == QuestAfterSend.DisableObject) gameObject.SetActive(false);
        else if (afterSending == QuestAfterSend.DestroyObject) Destroy(gameObject);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetQuitFlag()
    {
        applicationQuitting = false;
        Application.quitting -= MarkQuitting;
        Application.quitting += MarkQuitting;
    }

    private static void MarkQuitting() => applicationQuitting = true;

#if UNITY_EDITOR
    // Keeps quest areas visible in the Scene view even when they are not selected.
    private void OnDrawGizmos()
    {
        if (sendWhen != QuestSendTrigger.PlayerEntersArea) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        var fill = new Color(1f, 0.78f, 0.2f, 0.12f);
        var outline = new Color(1f, 0.78f, 0.2f, 0.85f);
        foreach (Collider area in GetComponents<Collider>())
        {
            if (!area.isTrigger) continue;
            if (area is BoxCollider box)
            {
                Gizmos.color = fill;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.color = outline;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (area is SphereCollider sphere)
            {
                Gizmos.color = fill;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
                Gizmos.color = outline;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
#endif
}
