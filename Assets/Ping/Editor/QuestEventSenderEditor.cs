using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestEventSender))]
public class QuestEventSenderEditor : Editor
{
    private const string ManualCode = "GetComponent<QuestEventSender>().SendEvent();";

    private static readonly string[] TriggerHelp =
    {
        "Call SendEvent() from a UnityEvent (for example a Button's On Click) or from code.",
        "The player looks at this object and presses E. The prompt only shows while the quest is active.",
        "Sends when the player walks into this object's trigger collider (shown in yellow in the Scene view).",
        "Sends when this object is destroyed during gameplay, e.g. an enemy that dies.",
    };

    private static bool showEvents;
    private static bool showAdvanced;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var sender = (QuestEventSender)target;
        SerializedProperty quest = serializedObject.FindProperty("questDefinition");
        SerializedProperty objectiveId = serializedObject.FindProperty("objectiveId");
        SerializedProperty eventId = serializedObject.FindProperty("eventId");
        SerializedProperty targetId = serializedObject.FindProperty("targetId");
        SerializedProperty sendWhen = serializedObject.FindProperty("sendWhen");
        var issues = QuestEditorChecks.Sender(sender).ToList();

        QuestEditorUI.Header("Quest Event Sender", "Adds progress to a quest objective",
            EditorGUIUtility.IconContent("d_Animation.EventMarker").image);
        QuestEditorUI.Status(issues.Count(issue => !issue.Advisory), "Ready");

        QuestEditorUI.Section("Objective");
        bool hasObjective = QuestObjectivePicker.Draw(quest, objectiveId, eventId, targetId);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("amount"), new GUIContent("Amount", "Progress added by one send."));
        if (!hasObjective && !string.IsNullOrWhiteSpace(eventId.stringValue))
            EditorGUILayout.HelpBox("Using legacy Event ID matching. Pick an Objective above for the simpler setup.", MessageType.Info);

        QuestEditorUI.Section("When");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sendWhen, new GUIContent("Send When"));
        var trigger = (QuestSendTrigger)sendWhen.intValue;
        if (EditorGUI.EndChangeCheck())
            serializedObject.FindProperty("onlyOnce").boolValue =
                trigger == QuestSendTrigger.PlayerPressesE || trigger == QuestSendTrigger.PlayerEntersArea;
        EditorGUILayout.LabelField(TriggerHelp[Mathf.Clamp((int)trigger, 0, TriggerHelp.Length - 1)], EditorStyles.wordWrappedMiniLabel);
        if (trigger == QuestSendTrigger.Manual)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(ManualCode, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(46f))) EditorGUIUtility.systemCopyBuffer = ManualCode;
            EditorGUILayout.EndHorizontal();
        }
        if (trigger != QuestSendTrigger.ObjectDestroyed)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onlyOnce"), new GUIContent("Only Once", "Count only the first successful send."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("afterSending"),
                new GUIContent("After Sending", "Hide or destroy this object once its progress counts, e.g. a picked-up item."));
        }
        if (trigger == QuestSendTrigger.PlayerPressesE)
            QuestEditorUI.TextWithPlaceholder(serializedObject.FindProperty("promptText"), new GUIContent("Prompt"), DefaultPrompt(quest, objectiveId));

        foreach (QuestIssue issue in issues) QuestEditorUI.Issue(issue);

        showEvents = EditorGUILayout.Foldout(showEvents, "Events", true, EditorStyles.foldoutHeader);
        if (showEvents)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onSent"), new GUIContent("On Sent", "Runs after progress counts, e.g. play a sound."));

        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true, EditorStyles.foldoutHeader);
        if (showAdvanced)
        {
            QuestEditorValidation.Field(serializedObject, "questSystem", "Quest System Override");
            EditorGUILayout.PropertyField(quest, new GUIContent("Quest Set"));
            EditorGUILayout.PropertyField(objectiveId, new GUIContent("Objective ID"));
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Legacy event matching", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(eventId, new GUIContent("Event ID"));
            EditorGUILayout.PropertyField(targetId, new GUIContent("Target ID (Optional)"));
        }
        serializedObject.ApplyModifiedProperties();

        QuestEditorUI.Section("Play mode test");
        if (Application.isPlaying)
            EditorGUILayout.LabelField(sender.CanSend()
                ? "Ready: sending now adds progress."
                : "Waiting: the quest is not accepted yet, the objective is done, or it was already sent once.", EditorStyles.wordWrappedMiniLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
            if (GUILayout.Button(new GUIContent(" Send now", EditorGUIUtility.IconContent("PlayButton").image), GUILayout.Height(30f)))
                sender.SendEvent();
    }

    private static string DefaultPrompt(SerializedProperty quest, SerializedProperty objectiveId)
    {
        QuestObjective_Child objective = (quest.objectReferenceValue as QuestSet)?.FindObjective(objectiveId.stringValue);
        return objective != null && !string.IsNullOrWhiteSpace(objective.DisplayName)
            ? "Press E: " + objective.DisplayName
            : "Press E to interact.";
    }
}
