using UnityEngine;
using UnityEditor;
using Unity.EasyDialogue;
using UnityEngine.UIElements;

public static class DialogueExportMenu
{
    [MenuItem("Assets/Create/Dialogue/Export Current Graph")]
    public static void ExportCurrentGraph()
    {
        Dialogue window = EditorWindow.GetWindow<Dialogue>("dialogue");

        DialogueGraphView graphView = window.rootVisualElement.Q<DialogueGraphView>();
        if (graphView == null)
        {
            Debug.LogError("No DialogueGraphView found in the current window.");
            return;
        }
        window.ExportGraph();
    }
}



