using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(QuestSet))]
public class QuestSetEditor : Editor
{
    private ReorderableList objectives;
    private bool showAdvanced;

    private void OnEnable()
    {
        objectives = new ReorderableList(serializedObject, serializedObject.FindProperty("Objectives"), true, true, true, true);
        objectives.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "OBJECTIVES", EditorStyles.miniBoldLabel);
        objectives.elementHeightCallback = index => objectives.serializedProperty.GetArrayElementAtIndex(index).isExpanded
            ? (showAdvanced ? 198f : 132f)
            : 30f;
        objectives.drawElementCallback = DrawObjective;
        objectives.onAddCallback = list =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            var item = list.serializedProperty.GetArrayElementAtIndex(index);
            string objectiveId = QuestSet.CreateGeneratedId("objective");
            item.FindPropertyRelative("ObjectiveId").stringValue = objectiveId;
            item.FindPropertyRelative("DisplayName").stringValue = "New Objective";
            item.FindPropertyRelative("description").stringValue = string.Empty;
            item.FindPropertyRelative("EventId").stringValue = objectiveId;
            item.FindPropertyRelative("TargetId").stringValue = string.Empty;
            item.FindPropertyRelative("requiredAmount").intValue = 1;
            item.isExpanded = true;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var quest = (QuestSet)target;
        var problems = QuestEditorValidation.Check(quest, QuestEditorValidation.FindQuests()).ToList();
        QuestEditorUI.Header(string.IsNullOrWhiteSpace(quest.DisplayName) ? "New Quest" : quest.DisplayName,
            "IDs are managed automatically",
            EditorGUIUtility.IconContent("ScriptableObject Icon").image);
        QuestEditorUI.Status(problems.Count, "Quest is ready");

        QuestEditorUI.Section("Quest details");
        QuestEditorValidation.Field(serializedObject, "DisplayName", "Quest Name");
        QuestEditorValidation.Field(serializedObject, "Description", "Description");

        QuestEditorUI.Section("Objectives");
        objectives.DoLayoutList();

        QuestEditorUI.Section("Unlock requirements");
        QuestEditorValidation.Field(serializedObject, "RequiredQuests", "Required Quests (Optional)");

        bool previousAdvanced = showAdvanced;
        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced IDs and event matching", true, EditorStyles.foldoutHeader);
        if (showAdvanced)
        {
            QuestEditorValidation.Field(serializedObject, "QuestId", "Quest ID");
            EditorGUILayout.HelpBox("These IDs are generated automatically. Edit them only when integrating a custom event system.", MessageType.Info);
        }
        if (previousAdvanced != showAdvanced) Repaint();
        serializedObject.ApplyModifiedProperties();

        if (problems.Count > 0)
        {
            QuestEditorUI.Section("Validation");
            foreach (var message in problems) QuestEditorUI.CompactWarning(message);
        }
    }

    private void DrawObjective(Rect rect, int index, bool active, bool focused)
    {
        var item = objectives.serializedProperty.GetArrayElementAtIndex(index);
        rect.y += 3f;
        rect.height -= 6f;
        if (index % 2 == 0)
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.025f) : new Color(0f, 0f, 0f, 0.025f));

        var displayName = item.FindPropertyRelative("DisplayName").stringValue;
        var amount = item.FindPropertyRelative("requiredAmount").intValue;
        string summary = string.IsNullOrWhiteSpace(displayName) ? "Unnamed objective" : displayName + "  x" + amount;
        item.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 4f, rect.y + 3f, rect.width - 8f, 20f), item.isExpanded,
            "Objective " + (index + 1) + "    " + summary, true, EditorStyles.foldoutHeader);
        if (!item.isExpanded) return;

        float y = rect.y + 29f;
        float line = EditorGUIUtility.singleLineHeight;
        float gap = EditorGUIUtility.standardVerticalSpacing;
        var full = new Rect(rect.x + 12f, y, rect.width - 20f, line);
        EditorGUI.PropertyField(full, item.FindPropertyRelative("DisplayName"), new GUIContent("Display Name"));
        full.y += line + gap;
        full.height = 38f;
        var descriptionRect = EditorGUI.PrefixLabel(full, new GUIContent("Description"));
        var description = item.FindPropertyRelative("description");
        description.stringValue = EditorGUI.TextArea(descriptionRect, description.stringValue);
        full.y += 42f;
        full.height = line;
        EditorGUI.PropertyField(full, item.FindPropertyRelative("requiredAmount"), new GUIContent("Required Amount"));
        if (!showAdvanced) return;
        full.y += line + gap;
        EditorGUI.PropertyField(full, item.FindPropertyRelative("ObjectiveId"), new GUIContent("Objective ID"));
        full.y += line + gap;
        EditorGUI.PropertyField(full, item.FindPropertyRelative("EventId"), new GUIContent("Legacy Event ID"));
        full.y += line + gap;
        EditorGUI.PropertyField(full, item.FindPropertyRelative("TargetId"), new GUIContent("Legacy Target ID", "Optional filter for legacy event matching."));
    }
}
