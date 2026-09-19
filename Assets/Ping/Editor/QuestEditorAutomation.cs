using UnityEditor;
using UnityEngine;

internal static class QuestEditorAutomation
{
    internal static QuestSystem FindSceneSystem()
        => Object.FindFirstObjectByType<QuestSystem>(FindObjectsInactive.Include);

    internal static QuestSystem SetupScene(out int assignedComponents)
    {
        var system = FindSceneSystem();
        if (system == null)
        {
            var gameObject = new GameObject("Quest System");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Quest System");
            system = Undo.AddComponent<QuestSystem>(gameObject);
        }

        assignedComponents = 0;
        foreach (var giver in Object.FindObjectsByType<QuestGiver>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            assignedComponents += AssignIfEmpty(giver, "questSystem", system);
        foreach (var sender in Object.FindObjectsByType<QuestEventSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            assignedComponents += AssignIfEmpty(sender, "questSystem", system);

        EditorGUIUtility.PingObject(system.gameObject);
        return system;
    }

    private static int AssignIfEmpty(Object component, string propertyName, QuestSystem system)
    {
        if (EditorUtility.IsPersistent(component)) return 0;
        var serialized = new SerializedObject(component);
        var property = serialized.FindProperty(propertyName);
        if (property == null || property.objectReferenceValue != null) return 0;
        Undo.RecordObject(component, "Assign Quest System");
        property.objectReferenceValue = system;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(component);
        return 1;
    }

    internal static string MakeId(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) value = fallback;
        var result = new System.Text.StringBuilder();
        bool separator = false;
        foreach (char character in value.Trim())
        {
            if (char.IsLetterOrDigit(character))
            {
                if (separator && result.Length > 0) result.Append('_');
                result.Append(char.ToLowerInvariant(character));
                separator = false;
            }
            else separator = true;
        }
        return result.Length == 0 ? fallback : result.ToString();
    }
}