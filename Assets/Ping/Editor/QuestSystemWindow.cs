using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class QuestSystemWindow : EditorWindow
{
    private sealed class Issue
    {
        internal Object Context;
        internal string Message;
        internal bool Advisory;
    }

    private List<QuestSet> quests = new List<QuestSet>();
    private readonly List<Issue> issues = new List<Issue>();
    private QuestSet selectedQuest;
    private Vector2 questScroll;
    private Vector2 detailScroll;
    private string search = string.Empty;
    private int viewMode;

    [MenuItem("Window/Quest System")]
    public static void Open()
    {
        var window = GetWindow<QuestSystemWindow>("Quest System");
        window.minSize = new Vector2(720f, 430f);
    }

    private void OnEnable()
    {
        minSize = new Vector2(720f, 430f);
        Scan();
    }

    private void Add(Object context, string message, bool advisory = false)
        => issues.Add(new Issue { Context = context, Message = message, Advisory = advisory });

    private void Scan()
    {
        quests = QuestEditorValidation.FindQuests()
            .OrderBy(q => string.IsNullOrWhiteSpace(q.DisplayName) ? q.name : q.DisplayName)
            .ToList();
        issues.Clear();
        foreach (var quest in quests)
            foreach (var message in QuestEditorValidation.Check(quest, quests)) Add(quest, message);

        var sceneGivers = Object.FindObjectsByType<QuestGiver>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        var sceneSenders = Object.FindObjectsByType<QuestEventSender>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        var givers = sceneGivers.ToList();
        var senders = sceneSenders.ToList();
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab == null) continue;
            givers.AddRange(prefab.GetComponentsInChildren<QuestGiver>(true));
            senders.AddRange(prefab.GetComponentsInChildren<QuestEventSender>(true));
        }

        foreach (var giver in givers.Distinct())
        {
            var obj = new SerializedObject(giver);
            if (obj.FindProperty("questDefinition").objectReferenceValue == null) Add(giver, "Quest Giver has no Quest Set.");
        }

        var objectives = quests.Where(q => q.Objectives != null).SelectMany(q => q.Objectives).Where(o => o != null).ToList();
        foreach (var sender in senders.Distinct())
        {
            var obj = new SerializedObject(sender);
            var selectedQuest = obj.FindProperty("questDefinition").objectReferenceValue as QuestSet;
            string objectiveId = obj.FindProperty("objectiveId").stringValue;
            string eventId = obj.FindProperty("eventId").stringValue;
            string targetId = obj.FindProperty("targetId").stringValue;
            if (obj.FindProperty("amount").intValue <= 0) Add(sender, "Event Sender amount must be greater than zero.");
            if (QuestObjectivePicker.IsValid(selectedQuest, objectiveId)) continue;
            if (string.IsNullOrWhiteSpace(eventId)) Add(sender, "Event Sender has no Objective selected.");
            else if (!objectives.Any(o => o.MatchesEvent(eventId, targetId)))
                Add(sender, "No objective matches " + eventId + FormatTarget(targetId) + ".");
        }

        if (QuestEditorAutomation.FindSceneSystem() == null && (sceneGivers.Count > 0 || sceneSenders.Count > 0))
            Add(sceneGivers.Cast<Object>().Concat(sceneSenders).FirstOrDefault(), "This scene has Quest components but no Quest System. Click Setup Scene.");

        foreach (var quest in quests.Where(q => q.Objectives != null))
            foreach (var objective in quest.Objectives.Where(o => o != null && !string.IsNullOrWhiteSpace(o.EventId)))
                if (!senders.Any(sender => SenderMatches(sender, quest, objective)))
                    Add(quest, "No sender found for " + objective.DisplayName + ". It may be progressed by another scene.", true);

        if (selectedQuest == null || !quests.Contains(selectedQuest)) selectedQuest = quests.FirstOrDefault();
        Repaint();
    }

    private static string FormatTarget(string targetId)
        => string.IsNullOrWhiteSpace(targetId) ? string.Empty : " / " + targetId;

    private static bool SenderMatches(QuestEventSender sender, QuestSet quest, QuestObjective_Child objective)
    {
        var obj = new SerializedObject(sender);
        if (obj.FindProperty("questDefinition").objectReferenceValue == quest &&
            obj.FindProperty("objectiveId").stringValue == objective.ObjectiveId) return true;
        return objective.MatchesEvent(obj.FindProperty("eventId").stringValue, obj.FindProperty("targetId").stringValue);
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
        search = GUILayout.TextField(search, GUI.skin.FindStyle("ToolbarSearchTextField"), GUILayout.Width(210f));
        if (GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSearchCancelButton")))
        {
            search = string.Empty;
            GUI.FocusControl(null);
        }
        if (GUILayout.Button(new GUIContent(" Refresh", EditorGUIUtility.IconContent("Refresh").image), EditorStyles.toolbarButton, GUILayout.Width(76f))) Scan();
        if (GUILayout.Button(new GUIContent(" Setup Scene", EditorGUIUtility.IconContent("SceneAsset Icon").image), EditorStyles.toolbarButton, GUILayout.Width(96f)))
        {
            var system = QuestEditorAutomation.SetupScene(out int assigned);
            ShowNotification(new GUIContent("Scene ready: " + system.name + ", assigned " + assigned + " component(s)"));
            Scan();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawQuestList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(292f));
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("QUESTS", EditorStyles.miniBoldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label(quests.Count.ToString(), QuestEditorUI.Pill(QuestEditorUI.Accent));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button(new GUIContent(" Create Quest Set", EditorGUIUtility.IconContent("Toolbar Plus").image), GUILayout.Height(28f)))
            CreateQuest();

        questScroll = EditorGUILayout.BeginScrollView(questScroll);
        // Asset deletion can leave a destroyed Unity object in the cached list
        // until the next refresh. Skip it before calling any Unity properties.
        foreach (var quest in quests.Where(q => q != null && MatchesSearch(q))) DrawQuestRow(quest);
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
        var rect = GUILayoutUtility.GetRect(270f, 52f, GUILayout.ExpandWidth(true));
        bool selected = quest == selectedQuest;
        int count = issues.Count(i => i.Context == quest && !i.Advisory);
        EditorGUI.DrawRect(rect, selected
            ? new Color(QuestEditorUI.Accent.r, QuestEditorUI.Accent.g, QuestEditorUI.Accent.b, EditorGUIUtility.isProSkin ? 0.22f : 0.15f)
            : (EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.025f) : new Color(0f, 0f, 0f, 0.025f)));
        if (selected) EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height), QuestEditorUI.Accent);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) selectedQuest = quest;

        string title = string.IsNullOrWhiteSpace(quest.DisplayName) ? quest.name : quest.DisplayName;
        GUI.Label(new Rect(rect.x + 11f, rect.y + 7f, rect.width - 60f, 19f), title, EditorStyles.boldLabel);
        GUI.Label(new Rect(rect.x + 11f, rect.y + 27f, rect.width - 60f, 17f),
            string.IsNullOrWhiteSpace(quest.QuestId) ? "ID not set" : quest.QuestId, EditorStyles.miniLabel);
        string badge = count == 0 ? "READY" : count.ToString();
        GUI.Label(new Rect(rect.xMax - 48f, rect.y + 16f, 40f, 20f), badge,
            QuestEditorUI.Pill(count == 0 ? QuestEditorUI.Success : QuestEditorUI.Warning));
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
        viewMode = GUILayout.Toolbar(viewMode, new[] { "Quest Details", "All Issues (" + issues.Count + ")" });
        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
        if (viewMode == 0) DrawSelectedQuest(); else DrawAllIssues();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawSelectedQuest()
    {
        if (selectedQuest == null)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("No Quest Set found", new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 14 });
            GUILayout.FlexibleSpace();
            return;
        }

        string title = string.IsNullOrWhiteSpace(selectedQuest.DisplayName) ? selectedQuest.name : selectedQuest.DisplayName;
        QuestEditorUI.Header(title, string.IsNullOrWhiteSpace(selectedQuest.QuestId) ? "Quest ID not set" : selectedQuest.QuestId,
            EditorGUIUtility.IconContent("ScriptableObject Icon").image);
        var questIssues = issues.Where(i => i.Context == selectedQuest).ToList();
        QuestEditorUI.Status(questIssues.Count(i => !i.Advisory), "Quest data is ready");

        QuestEditorUI.Section("Overview");
        EditorGUILayout.LabelField("Objectives", (selectedQuest.Objectives?.Count ?? 0).ToString());
        EditorGUILayout.LabelField("Required Quests", (selectedQuest.RequiredQuests?.Count ?? 0).ToString());
        EditorGUILayout.LabelField("Description", string.IsNullOrWhiteSpace(selectedQuest.Description) ? "Not set" : selectedQuest.Description, EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Open in Inspector", GUILayout.Height(27f)))
        {
            Selection.activeObject = selectedQuest;
            EditorGUIUtility.PingObject(selectedQuest);
        }

        QuestEditorUI.Section("Validation");
        if (questIssues.Count == 0)
            EditorGUILayout.HelpBox("No issues found for this Quest Set.", MessageType.Info);
        else
            foreach (var issue in questIssues) DrawIssue(issue);
    }

    private void DrawAllIssues()
    {
        QuestEditorUI.Section("Project validation");
        int errors = issues.Count(i => !i.Advisory);
        EditorGUILayout.LabelField(errors == 0 ? "No required fixes" : errors + " required fix" + (errors == 1 ? string.Empty : "es"), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Advisories explain possible cross-scene or code-driven events.", EditorStyles.wordWrappedMiniLabel);
        GUILayout.Space(6f);
        if (issues.Count == 0) EditorGUILayout.HelpBox("Everything checked is ready.", MessageType.Info);
        foreach (var issue in issues.OrderBy(i => i.Advisory)) DrawIssue(issue);
    }

    private static void DrawIssue(Issue issue)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(EditorGUIUtility.IconContent(issue.Advisory ? "console.infoicon.sml" : "console.warnicon.sml"), GUILayout.Width(22f), GUILayout.Height(22f));
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button(issue.Context == null ? "Missing object" : issue.Context.name, EditorStyles.linkLabel))
        {
            Selection.activeObject = issue.Context;
            if (issue.Context != null) EditorGUIUtility.PingObject(issue.Context);
        }
        GUILayout.Label(issue.Message, EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5f);
    }

    private void CreateQuest()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Quest Set", "NewQuest", "asset", "Choose a location");
        if (string.IsNullOrEmpty(path)) return;
        var quest = CreateInstance<QuestSet>();
        string assetName = Path.GetFileNameWithoutExtension(path);
        quest.DisplayName = ObjectNames.NicifyVariableName(assetName);
        quest.QuestId = QuestEditorAutomation.MakeId(assetName, "new_quest");
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
