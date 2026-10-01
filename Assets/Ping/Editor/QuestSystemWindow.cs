using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One place to create quests, see who gives them and what progresses each objective,
/// fix setup problems, and drive quests by hand in Play Mode.
/// </summary>
public class QuestSystemWindow : EditorWindow
{
    private static readonly QuestManager NothingAccepted = new QuestManager();

    private List<QuestSet> quests = new List<QuestSet>();
    private readonly List<QuestIssue> issues = new List<QuestIssue>();
    private readonly Dictionary<QuestSet, List<QuestIssue>> issuesByQuest = new Dictionary<QuestSet, List<QuestIssue>>();
    private QuestSceneIndex index = new QuestSceneIndex();
    private List<QuestLink> prefabLinks;
    private QuestSet selectedQuest;
    private Vector2 questScroll;
    private Vector2 detailScroll;
    private string search = string.Empty;
    private int viewMode;
    private bool scanQueued;

    [MenuItem("Window/Quest System")]
    public static void Open() => OpenWindow(null);

    internal static void Open(QuestSet quest) => OpenWindow(quest);

    private static void OpenWindow(QuestSet quest)
    {
        var window = GetWindow<QuestSystemWindow>("Quest System");
        window.minSize = new Vector2(760f, 460f);
        if (quest != null)
        {
            window.selectedQuest = quest;
            window.viewMode = 0;
        }
        window.Scan();
    }

    private void OnEnable()
    {
        minSize = new Vector2(760f, 460f);
        EditorApplication.hierarchyChanged += QueueScan;
        EditorApplication.projectChanged += OnProjectChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        ObjectChangeEvents.changesPublished += OnChangesPublished;
        Undo.undoRedoPerformed += QueueScan;
        Selection.selectionChanged += OnSelectionChanged;
        Scan();
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= QueueScan;
        EditorApplication.projectChanged -= OnProjectChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        ObjectChangeEvents.changesPublished -= OnChangesPublished;
        Undo.undoRedoPerformed -= QueueScan;
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnInspectorUpdate()
    {
        if (EditorApplication.isPlaying) Repaint(); // live progress
    }

    private void OnChangesPublished(ref ObjectChangeEventStream stream) => QueueScan();

    private void OnProjectChanged()
    {
        prefabLinks = null;
        QueueScan();
    }

    private void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredEditMode) QueueScan();
        Repaint();
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeObject is QuestSet quest)
        {
            selectedQuest = quest;
            viewMode = 0;
        }
        Repaint();
    }

    /// <summary>Rescans once on the next editor tick, however many changes arrive meanwhile.</summary>
    private void QueueScan()
    {
        if (scanQueued || EditorApplication.isPlaying) return;
        scanQueued = true;
        EditorApplication.delayCall += () =>
        {
            scanQueued = false;
            if (this != null) Scan();
        };
    }

    private void Scan()
    {
        quests = QuestEditorValidation.FindQuests().OrderBy(quest => quest.Title).ToList();
        if (prefabLinks == null) prefabLinks = QuestSceneIndex.ScanPrefabs();
        index = QuestSceneIndex.Build(prefabLinks);

        issues.Clear();
        foreach (QuestSet quest in quests) issues.AddRange(QuestEditorValidation.Issues(quest, quests));
        foreach (QuestLink link in index.Links) issues.AddRange(QuestEditorChecks.For(link.Component));

        issuesByQuest.Clear();
        foreach (QuestSet quest in quests)
            issuesByQuest[quest] = issues.Where(issue => issue.Context == quest ||
                index.Links.Any(link => link.Component == issue.Context && link.RelatesTo(quest))).ToList();

        if (selectedQuest == null || !quests.Contains(selectedQuest)) selectedQuest = quests.FirstOrDefault();
        Repaint();
    }

    private void OnGUI()
    {
        DrawToolbar();
        EditorGUILayout.BeginHorizontal();
        DrawQuestList();
        DrawDivider();
        DrawDetails();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(28f));
        GUILayout.Label(new GUIContent(" Quest System", EditorGUIUtility.IconContent("ScriptableObject Icon").image), EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(200f));
        if (!string.IsNullOrEmpty(search) && GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20f)))
        {
            search = string.Empty;
            GUI.FocusControl(null);
        }
        if (GUILayout.Button(new GUIContent(" Refresh", EditorGUIUtility.IconContent("Refresh").image), EditorStyles.toolbarButton, GUILayout.Width(76f)))
        {
            prefabLinks = null;
            Scan();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawQuestList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(280f));
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("QUESTS", EditorStyles.miniBoldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label(quests.Count.ToString(), QuestEditorUI.Pill(QuestEditorUI.Accent));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button(new GUIContent(" Create Quest Set", EditorGUIUtility.IconContent("Toolbar Plus").image), GUILayout.Height(28f)))
        {
            CreateQuest();
            GUIUtility.ExitGUI(); // the save dialog invalidates the current layout pass
        }

        questScroll = EditorGUILayout.BeginScrollView(questScroll);
        // A deleted asset stays in the cached list as a destroyed object until the next scan.
        foreach (QuestSet quest in quests.Where(quest => quest != null && MatchesSearch(quest))) DrawQuestRow(quest);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private bool MatchesSearch(QuestSet quest)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        return (quest.name + " " + quest.QuestId + " " + quest.DisplayName)
            .IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void DrawQuestRow(QuestSet quest)
    {
        var rect = GUILayoutUtility.GetRect(260f, 50f, GUILayout.ExpandWidth(true));
        bool selected = quest == selectedQuest;
        EditorGUI.DrawRect(rect, selected
            ? new Color(QuestEditorUI.Accent.r, QuestEditorUI.Accent.g, QuestEditorUI.Accent.b, EditorGUIUtility.isProSkin ? 0.22f : 0.15f)
            : (EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.025f) : new Color(0f, 0f, 0f, 0.025f)));
        if (selected) EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height), QuestEditorUI.Accent);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) SelectQuest(quest);

        GUI.Label(new Rect(rect.x + 11f, rect.y + 7f, rect.width - 96f, 19f), quest.Title, EditorStyles.boldLabel);
        GUI.Label(new Rect(rect.x + 11f, rect.y + 27f, rect.width - 96f, 17f),
            string.IsNullOrWhiteSpace(quest.QuestId) ? "ID not set" : quest.QuestId, EditorStyles.miniLabel);

        string badge;
        Color color;
        if (EditorApplication.isPlaying)
        {
            QuestState state = LiveState(quest);
            badge = state.ToString().ToUpperInvariant();
            color = QuestEditorUI.StateColor(state);
        }
        else
        {
            int count = issuesByQuest.TryGetValue(quest, out var related) ? related.Count(issue => !issue.Advisory) : 0;
            badge = count == 0 ? "READY" : count + (count == 1 ? " FIX" : " FIXES");
            color = count == 0 ? QuestEditorUI.Success : QuestEditorUI.Warning;
        }
        GUI.Label(new Rect(rect.xMax - 84f, rect.y + 15f, 78f, 20f), badge, QuestEditorUI.Pill(color));
        GUILayout.Space(3f);
    }

    private static void DrawDivider()
    {
        var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.12f) : new Color(0f, 0f, 0f, 0.15f));
    }

    private void DrawDetails()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        int required = issues.Count(issue => !issue.Advisory);
        int mode = GUILayout.Toolbar(viewMode, new[] { "Quest", required == 0 ? "Issues" : "Issues (" + required + ")" }, GUILayout.Height(24f));
        if (mode != viewMode)
        {
            viewMode = mode;
            GUIUtility.ExitGUI(); // the other tab has a different layout; redraw from scratch
        }
        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
        if (viewMode == 0) DrawSelectedQuest(); else DrawAllIssues();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawSelectedQuest()
    {
        if (selectedQuest == null)
        {
            GUILayout.Space(40f);
            GUILayout.Label("No quests yet", new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 14 });
            GUILayout.Label("Click \"Create Quest Set\" on the left to make your first quest.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        QuestSet quest = selectedQuest;
        List<QuestIssue> related = issuesByQuest.TryGetValue(quest, out var list) ? list : new List<QuestIssue>();
        QuestEditorUI.Header(quest.Title, string.IsNullOrWhiteSpace(quest.QuestId) ? "Quest ID not set" : "ID: " + quest.QuestId,
            EditorGUIUtility.IconContent("ScriptableObject Icon").image);
        QuestEditorUI.Status(related.Count(issue => !issue.Advisory), "Quest is ready");

        if (EditorApplication.isPlaying) DrawLive(quest);

        QuestEditorUI.Section("Overview");
        EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(quest.Description) ? "No description." : quest.Description, EditorStyles.wordWrappedLabel);
        List<QuestSet> requiredQuests = quest.RequiredQuests?.Where(other => other != null && other != quest).ToList() ?? new List<QuestSet>();
        if (requiredQuests.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Unlocks after", GUILayout.Width(90f));
            foreach (QuestSet other in requiredQuests)
                if (GUILayout.Button(other.Title, EditorStyles.linkLabel, GUILayout.ExpandWidth(false))) SelectQuest(other);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Edit in Inspector", GUILayout.Height(24f))) QuestEditorAutomation.Ping(quest);

        GameObject[] targets = SceneSelection();

        QuestEditorUI.Section("Given by");
        List<QuestLink> givers = index.Links.Where(link => link.Gives(quest)).ToList();
        if (givers.Count == 0)
            EditorGUILayout.LabelField("Nothing in this scene gives this quest yet. Select an NPC or object, then click the button below.",
                EditorStyles.wordWrappedMiniLabel);
        foreach (QuestLink link in givers) DrawLink(link);
        using (new EditorGUI.DisabledScope(targets.Length == 0))
        {
            string label = targets.Length == 0
                ? "Select an object in the scene to make it give this quest"
                : "Make " + Describe(targets) + " give this quest";
            if (GUILayout.Button(label, GUILayout.Height(24f)))
                EditorApplication.delayCall += () =>
                {
                    QuestEditorAutomation.AddGivers(targets, quest);
                    QueueScan();
                };
        }

        QuestEditorUI.Section("Objectives");
        if (quest.Objectives != null)
            for (int i = 0; i < quest.Objectives.Count; i++)
                if (quest.Objectives[i] != null) DrawObjective(quest, quest.Objectives[i], i, targets);
        if (GUILayout.Button("+ Add objective"))
            EditorApplication.delayCall += () =>
            {
                QuestEditorAutomation.AddObjective(quest);
                QueueScan();
            };

        List<QuestLink> doors = index.Links.Where(link => link.Kind == QuestLinkKind.Door && link.Quest == quest).ToList();
        if (doors.Count > 0)
        {
            QuestEditorUI.Section("Unlocks doors");
            foreach (QuestLink link in doors) DrawLink(link);
        }

        if (related.Count > 0)
        {
            QuestEditorUI.Section("Issues");
            foreach (QuestIssue issue in related)
                if (QuestEditorUI.Issue(issue, true)) QueueScan();
        }
    }

    private void DrawObjective(QuestSet quest, QuestObjective_Child objective, int number, GameObject[] targets)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        string name = string.IsNullOrWhiteSpace(objective.DisplayName) ? "Objective " + (number + 1) : objective.DisplayName;
        GUILayout.Label((number + 1) + ". " + name, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("x" + objective.requiredAmount, EditorStyles.miniLabel);
        var content = new GUIContent("Add progress", "Choose what progresses this objective.");
        Rect buttonRect = GUILayoutUtility.GetRect(content, EditorStyles.miniPullDown, GUILayout.Width(104f));
        if (EditorGUI.DropdownButton(buttonRect, content, FocusType.Passive, EditorStyles.miniPullDown))
            AddProgressMenu(quest, objective, targets).DropDown(buttonRect);
        EditorGUILayout.EndHorizontal();

        List<QuestLink> senders = index.Links.Where(link => link.Progresses(quest, objective)).ToList();
        if (senders.Count == 0)
            EditorGUILayout.LabelField("Nothing in this scene progresses this yet. Use \"Add progress\".", EditorStyles.wordWrappedMiniLabel);
        foreach (QuestLink link in senders) DrawLink(link);
        EditorGUILayout.EndVertical();
    }

    private GenericMenu AddProgressMenu(QuestSet quest, QuestObjective_Child objective, GameObject[] targets)
    {
        var menu = new GenericMenu();
        string selection = targets.Length == 0
            ? "Selected object (select one in the scene first)"
            : Describe(targets).Replace("/", "-");
        AddSenderItem(menu, selection + "/When the player presses E", targets, quest, objective, QuestSendTrigger.PlayerPressesE);
        AddSenderItem(menu, selection + "/When the player enters its area", targets, quest, objective, QuestSendTrigger.PlayerEntersArea);
        AddSenderItem(menu, selection + "/When it is destroyed", targets, quest, objective, QuestSendTrigger.ObjectDestroyed);
        AddSenderItem(menu, selection + "/Manually (UnityEvent or code)", targets, quest, objective, QuestSendTrigger.Manual);
        menu.AddItem(new GUIContent("New area trigger at the Scene view centre"), false, () =>
        {
            QuestEditorAutomation.CreateAreaTrigger(quest, objective);
            QueueScan();
        });
        menu.AddSeparator(string.Empty);
        string code = "QuestSystem.Progress(\"" + quest.QuestId + "\", \"" + objective.ObjectiveId + "\");";
        menu.AddItem(new GUIContent("Copy code for scripts"), false, () =>
        {
            EditorGUIUtility.systemCopyBuffer = code;
            ShowNotification(new GUIContent("Copied: " + code));
        });
        return menu;
    }

    private void AddSenderItem(GenericMenu menu, string path, GameObject[] targets, QuestSet quest,
        QuestObjective_Child objective, QuestSendTrigger trigger)
    {
        if (targets.Length == 0)
        {
            menu.AddDisabledItem(new GUIContent(path));
            return;
        }
        menu.AddItem(new GUIContent(path), false, () =>
        {
            QuestEditorAutomation.AddSenders(targets, quest, objective, trigger);
            QueueScan();
        });
    }

    private void DrawLink(QuestLink link)
    {
        if (link.Component == null) return;
        bool hasIssue = issues.Any(issue => !issue.Advisory && issue.Context == link.Component);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(EditorGUIUtility.IconContent(hasIssue ? "console.warnicon.sml" : "TestPassed"), GUILayout.Width(18f), GUILayout.Height(18f));
        if (GUILayout.Button(link.Describe(), EditorStyles.linkLabel)) QuestEditorAutomation.Ping(link.Component);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLive(QuestSet quest)
    {
        QuestEditorUI.Section("Live (Play Mode)");
        QuestSystem system = QuestSystem.Existing;
        QuestState state = LiveState(quest);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("State", GUILayout.Width(60f));
        GUILayout.Label(state.ToString().ToUpperInvariant(), QuestEditorUI.Pill(QuestEditorUI.StateColor(state)));
        GUILayout.FlexibleSpace();
        if (state == QuestState.Available && GUILayout.Button("Accept now", GUILayout.Width(120f)))
        {
            QuestSystem.Accept(quest);
            GUIUtility.ExitGUI();
        }
        if (state == QuestState.Active && GUILayout.Button("Complete quest", GUILayout.Width(120f)))
        {
            CompleteAll(system, quest);
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();
        if (state == QuestState.Locked)
            EditorGUILayout.LabelField("Complete the required quests first.", EditorStyles.miniLabel);

        QuestInstance instance = system != null ? system.Manager.GetQuest(quest.QuestId) : null;
        if (instance == null) return;
        foreach (QuestProgression progression in instance.progressions)
        {
            int current = progression.GetCurrentAmount();
            int required = progression.GetRequiredAmount();
            EditorGUILayout.BeginHorizontal();
            Rect bar = EditorGUILayout.GetControlRect(false, 20f);
            EditorGUI.ProgressBar(bar, required > 0 ? (float)current / required : 1f,
                progression.GetDisplayName() + "  " + current + "/" + required);
            using (new EditorGUI.DisabledScope(progression.IsCompleted() || instance.IsCompleted()))
            {
                if (GUILayout.Button("+1", GUILayout.Width(34f)))
                    system.Manager.ReportObjective(quest, progression.Objective.ObjectiveId, 1);
                if (GUILayout.Button("Done", GUILayout.Width(48f)))
                    system.Manager.ReportObjective(quest, progression.Objective.ObjectiveId, required);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private static void CompleteAll(QuestSystem system, QuestSet quest)
    {
        QuestInstance instance = system != null ? system.Manager.GetQuest(quest.QuestId) : null;
        if (instance == null) return;
        foreach (QuestProgression progression in instance.progressions.ToList())
            system.Manager.ReportObjective(quest, progression.Objective.ObjectiveId, progression.GetRequiredAmount());
    }

    private static QuestState LiveState(QuestSet quest)
    {
        QuestSystem system = QuestSystem.Existing;
        return (system != null ? system.Manager : NothingAccepted).GetState(quest);
    }

    private void DrawAllIssues()
    {
        QuestEditorUI.Section("Project check");
        int required = issues.Count(issue => !issue.Advisory);
        EditorGUILayout.LabelField(required == 0 ? "No required fixes" : required + (required == 1 ? " thing needs fixing" : " things need fixing"),
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Checks every Quest Set, the open scene and prefabs. Blue notes are optional.", EditorStyles.wordWrappedMiniLabel);

        List<QuestIssue> fixable = issues.Where(issue => !issue.Advisory && issue.Fix != null).ToList();
        if (fixable.Count > 1 && GUILayout.Button("Fix all (" + fixable.Count + ")", GUILayout.Height(26f)))
        {
            foreach (QuestIssue issue in fixable) QuestEditorUI.RunLater(issue);
            QueueScan();
        }
        GUILayout.Space(4f);
        if (issues.Count == 0) EditorGUILayout.HelpBox("Everything checked is ready.", MessageType.Info);
        foreach (QuestIssue issue in issues.OrderBy(issue => issue.Advisory).ToList())
            if (QuestEditorUI.Issue(issue, true)) QueueScan();
    }

    private void SelectQuest(QuestSet quest)
    {
        selectedQuest = quest;
        viewMode = 0;
        GUI.FocusControl(null);
        GUIUtility.ExitGUI(); // the details pane changes shape; redraw from scratch
    }

    private static GameObject[] SceneSelection()
        => Selection.gameObjects.Where(gameObject => gameObject != null && !EditorUtility.IsPersistent(gameObject)).ToArray();

    private static string Describe(GameObject[] targets)
        => targets.Length == 1 ? "\"" + targets[0].name + "\"" : targets.Length + " selected objects";

    private void CreateQuest()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Quest Set", "NewQuest", "asset", "Choose where to save the new quest");
        if (string.IsNullOrEmpty(path)) return;
        var quest = CreateInstance<QuestSet>();
        string assetName = Path.GetFileNameWithoutExtension(path);
        quest.DisplayName = ObjectNames.NicifyVariableName(assetName);
        quest.QuestId = QuestEditorAutomation.UniqueQuestId(assetName, null);
        quest.Objectives.Add(new QuestObjective_Child
        {
            ObjectiveId = "objective_1",
            DisplayName = "Objective 1",
            EventId = "objective_1",
            requiredAmount = 1
        });
        AssetDatabase.CreateAsset(quest, path);
        AssetDatabase.SaveAssets();
        selectedQuest = quest;
        Selection.activeObject = quest;
        Scan();
    }
}
