using UnityEngine;

/// <summary>Shared helpers for quest components the player interacts with.</summary>
internal static class QuestInteractionUtility
{
    private static bool resolvingPrompt;

    /// <summary>
    /// Prompt of another interactable on the same object (e.g. a dialogue), so a quest component
    /// with nothing to say does not blank out the prompt of its neighbours.
    /// </summary>
    internal static string OtherPrompt(Component self)
    {
        if (resolvingPrompt) return string.Empty;
        resolvingPrompt = true;
        try
        {
            foreach (MonoBehaviour behaviour in self.GetComponents<MonoBehaviour>())
            {
                if (behaviour == self || !behaviour.isActiveAndEnabled || !(behaviour is IInteractable other)) continue;
                string text = other.InteractionText;
                if (!string.IsNullOrEmpty(text)) return text;
            }
            return string.Empty;
        }
        finally { resolvingPrompt = false; }
    }

    /// <summary>True for the player's collider: tagged "Player", carrying PlayerInteraction, or holding the main camera.</summary>
    internal static bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        Transform body = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
        if (other.CompareTag("Player") || body.CompareTag("Player")) return true;
        if (body.GetComponentInChildren<PlayerInteraction>(true) != null) return true;
        Camera camera = Camera.main;
        return camera != null && camera.transform.IsChildOf(body);
    }
}
