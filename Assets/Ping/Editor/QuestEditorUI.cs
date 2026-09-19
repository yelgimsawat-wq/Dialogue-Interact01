using UnityEditor;
using UnityEngine;

internal static class QuestEditorUI
{
    internal static readonly Color Accent = new Color(0.20f, 0.62f, 0.86f);
    internal static readonly Color Success = new Color(0.28f, 0.70f, 0.45f);
    internal static readonly Color Warning = new Color(0.95f, 0.62f, 0.18f);

    internal static void Header(string title, string subtitle, Texture icon = null)
    {
        var rect = EditorGUILayout.GetControlRect(false, 58f);
        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
            ? new Color(0.13f, 0.15f, 0.18f)
            : new Color(0.88f, 0.92f, 0.95f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), Accent);

        var textRect = new Rect(rect.x + 14f, rect.y + 9f, rect.width - 28f, 22f);
        if (icon != null)
        {
            GUI.DrawTexture(new Rect(rect.x + 12f, rect.y + 12f, 32f, 32f), icon, ScaleMode.ScaleToFit);
            textRect.x += 42f;
            textRect.width -= 42f;
        }

        var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };
        var subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.68f, 0.72f, 0.77f) : new Color(0.30f, 0.34f, 0.38f) }
        };
        GUI.Label(textRect, title, titleStyle);
        GUI.Label(new Rect(textRect.x, textRect.y + 23f, textRect.width, 18f), subtitle, subtitleStyle);
        GUILayout.Space(8f);
    }

    internal static void Section(string label)
    {
        GUILayout.Space(7f);
        var rect = EditorGUILayout.GetControlRect(false, 20f);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f),
            EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.10f) : new Color(0f, 0f, 0f, 0.12f));
        GUI.Label(new Rect(rect.x, rect.y, rect.width, 18f), label.ToUpperInvariant(), EditorStyles.miniBoldLabel);
    }

    internal static void Status(int count, string validText = "Ready")
    {
        var rect = EditorGUILayout.GetControlRect(false, 28f);
        var color = count == 0 ? Success : Warning;
        EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, EditorGUIUtility.isProSkin ? 0.16f : 0.11f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height), color);
        var icon = EditorGUIUtility.IconContent(count == 0 ? "TestPassed" : "console.warnicon.sml");
        var label = count == 0 ? validText : count + (count == 1 ? " issue needs attention" : " issues need attention");
        GUI.Label(new Rect(rect.x + 9f, rect.y + 5f, rect.width - 14f, 18f), new GUIContent(label, icon.image), EditorStyles.label);
        GUILayout.Space(4f);
    }

    internal static void CompactWarning(string message)
    {
        var content = new GUIContent(message, EditorGUIUtility.IconContent("console.warnicon.sml").image);
        var height = Mathf.Max(24f, EditorStyles.wordWrappedMiniLabel.CalcHeight(content, EditorGUIUtility.currentViewWidth - 44f) + 8f);
        var rect = EditorGUILayout.GetControlRect(false, height);
        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.28f, 0.20f, 0.08f, 0.42f) : new Color(1f, 0.78f, 0.25f, 0.18f));
        GUI.Label(new Rect(rect.x + 7f, rect.y + 4f, rect.width - 14f, rect.height - 8f), content, EditorStyles.wordWrappedMiniLabel);
    }

    internal static GUIStyle Pill(Color color)
    {
        return new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = color },
            padding = new RectOffset(6, 6, 2, 2)
        };
    }
}