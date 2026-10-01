using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// On-screen quest tracker and notifications ("New quest", "Quest complete").
/// Builds its own UI at runtime, so it works with no setup; assign your own texts under
/// Advanced to use a custom layout instead.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Quest/Quest HUD")]
public class QuestHUD : MonoBehaviour
{
    public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

    [SerializeField] private bool showTracker = true;
    [SerializeField] private Corner trackerCorner = Corner.TopLeft;
    [SerializeField] private bool showNotifications = true;
    [SerializeField, Min(0.5f)] private float notificationSeconds = 3f;
    [SerializeField, Tooltip("Key that shows/hides the tracker. None = always shown.")]
    private KeyCode toggleKey = KeyCode.None;
    [SerializeField, Tooltip("Empty = use the font of the scene's other UI text. Pick a Thai font asset here for Thai quest names.")]
    private TMP_FontAsset font;
    [SerializeField, Min(8f)] private float fontSize = 26f;
    [Header("Texts ({0} = name)")]
    [SerializeField] private string newQuestText = "New quest: {0}";
    [SerializeField] private string objectiveCompleteText = "Objective complete: {0}";
    [SerializeField] private string questCompleteText = "Quest complete: {0}";
    [Header("Advanced")]
    [SerializeField] private QuestSystem questSystem;
    [SerializeField, Tooltip("Optional: your own text for the tracker instead of the built-in panel.")]
    private TMP_Text trackerText;
    [SerializeField, Tooltip("Optional: your own text for notifications instead of the built-in banner.")]
    private TMP_Text notificationText;

    private QuestManager manager;
    private GameObject trackerRoot;
    private GameObject notificationRoot;
    private CanvasGroup notificationGroup;
    private readonly Queue<string> notifications = new Queue<string>();
    private Coroutine notificationRoutine;
    private bool trackerHidden;

    private void Awake()
    {
        // Subscribing in Awake catches quests accepted on the same frame this HUD was created.
        QuestSystem system = QuestSystem.Resolve(questSystem);
        if (system == null) return;
        manager = system.Manager;
        manager.onQuestAccepted += HandleAccepted;
        manager.onQuestCompleted += HandleCompleted;
        manager.onQuestProgressChanged += HandleProgressChanged;

        if (font == null) font = FindSceneFont();
        BuildMissingUI();
        RefreshTracker();
    }

    private void OnDestroy()
    {
        if (manager == null) return;
        manager.onQuestAccepted -= HandleAccepted;
        manager.onQuestCompleted -= HandleCompleted;
        manager.onQuestProgressChanged -= HandleProgressChanged;
    }

    private void OnDisable()
    {
        notificationRoutine = null;
        notifications.Clear();
        if (notificationRoot != null) notificationRoot.SetActive(false);
    }

    private void Update()
    {
        if (toggleKey == KeyCode.None || !Input.GetKeyDown(toggleKey)) return;
        trackerHidden = !trackerHidden;
        RefreshTracker();
    }

    /// <summary>Shows a short message in the notification banner.</summary>
    public void Notify(string message)
    {
        if (!showNotifications || notificationText == null || !isActiveAndEnabled || string.IsNullOrWhiteSpace(message)) return;
        notifications.Enqueue(message);
        if (notificationRoutine == null) notificationRoutine = StartCoroutine(ShowNotifications());
    }

    private void HandleAccepted(QuestInstance quest)
    {
        Notify(Format(newQuestText, quest.quest.Title));
        RefreshTracker();
    }

    private void HandleCompleted(QuestInstance quest)
    {
        Notify(Format(questCompleteText, quest.quest.Title));
        RefreshTracker();
    }

    private void HandleProgressChanged(QuestInstance quest, QuestProgression progression)
    {
        // The last objective is announced by "Quest complete" instead.
        if (progression.IsCompleted() && !quest.IsCompleted())
            Notify(Format(objectiveCompleteText, progression.GetDisplayName()));
        RefreshTracker();
    }

    // Replace rather than string.Format, so a stray brace in a designer's text cannot throw.
    private static string Format(string template, string name)
        => string.IsNullOrEmpty(template) ? name : template.Replace("{0}", name);

    private void RefreshTracker()
    {
        if (trackerText == null || manager == null) return;
        var builder = new StringBuilder();
        foreach (QuestInstance quest in manager.GetAllQuests())
        {
            if (quest.quest == null || quest.IsCompleted()) continue;
            if (builder.Length > 0) builder.Append("\n\n");
            builder.Append("<b><noparse>").Append(quest.quest.Title).Append("</noparse></b>");
            foreach (QuestProgression progression in quest.progressions)
            {
                string count = progression.GetCurrentAmount() + "/" + progression.GetRequiredAmount();
                builder.Append("\n<indent=0.8em>");
                if (progression.IsCompleted())
                    builder.Append("<color=#FFFFFF80><s><noparse>").Append(progression.GetDisplayName())
                        .Append("</noparse></s>  ").Append(count).Append("</color>");
                else
                    builder.Append("<noparse>").Append(progression.GetDisplayName())
                        .Append("</noparse>  <color=#FFD54F>").Append(count).Append("</color>");
                builder.Append("</indent>");
            }
        }
        trackerText.text = builder.ToString();
        if (trackerRoot != null) trackerRoot.SetActive(showTracker && !trackerHidden && builder.Length > 0);
    }

    private IEnumerator ShowNotifications()
    {
        notificationRoot.SetActive(true);
        while (notifications.Count > 0)
        {
            notificationText.text = "<noparse>" + notifications.Dequeue() + "</noparse>";
            yield return Fade(0f, 1f, 0.2f);
            // Hurry through a backlog so messages don't lag behind the game.
            yield return new WaitForSecondsRealtime(notifications.Count > 0 ? Mathf.Min(1.2f, notificationSeconds) : notificationSeconds);
            yield return Fade(1f, 0f, 0.3f);
        }
        notificationRoot.SetActive(false);
        notificationRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
        {
            notificationGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        notificationGroup.alpha = to;
    }

    private static TMP_FontAsset FindSceneFont()
    {
        foreach (TMP_Text text in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (text.font != null) return text.font;
        return TMP_Settings.defaultFontAsset;
    }

    private void BuildMissingUI()
    {
        if (trackerText == null || notificationText == null)
        {
            var canvasObject = new GameObject("Quest HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.layer = 5; // UI
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (trackerText == null)
            {
                bool right = trackerCorner == Corner.TopRight || trackerCorner == Corner.BottomRight;
                bool bottom = trackerCorner == Corner.BottomLeft || trackerCorner == Corner.BottomRight;
                var anchor = new Vector2(right ? 1f : 0f, bottom ? 0f : 1f);
                var offset = new Vector2(right ? -32f : 32f, bottom ? 32f : -32f);
                trackerText = CreatePanel(canvasObject.transform, "Quest Tracker", anchor, offset, 480f,
                    right ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft, out trackerRoot);
            }
            if (notificationText == null)
                notificationText = CreatePanel(canvasObject.transform, "Quest Notification", new Vector2(0.5f, 1f),
                    new Vector2(0f, -40f), 720f, TextAlignmentOptions.Center, out notificationRoot);
        }

        if (trackerRoot == null) trackerRoot = trackerText.gameObject;
        if (notificationRoot == null) notificationRoot = notificationText.gameObject;
        notificationGroup = notificationRoot.GetComponent<CanvasGroup>();
        if (notificationGroup == null) notificationGroup = notificationRoot.AddComponent<CanvasGroup>();
        notificationGroup.alpha = 0f;
        notificationRoot.SetActive(false);
    }

    private TMP_Text CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position, float width,
        TextAlignmentOptions alignment, out GameObject panel)
    {
        panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panel.layer = 5;
        var rect = (RectTransform)panel.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, 0f);

        var image = panel.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);
        image.raycastTarget = false;
        var layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 14, 14);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.layer = 5;
        textObject.transform.SetParent(panel.transform, false);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.richText = true;
        text.raycastTarget = false;
        return text;
    }
}
