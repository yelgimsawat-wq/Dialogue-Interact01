using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Unity.EasyDialogue;

public class Dialogue : EditorWindow
{
    [MenuItem("Window/Dialogue/EasyDialogue")]
    public static void ShowExample()
    {
        GetWindow<Dialogue>("Dialogue");
    }

    private void OnEnable()
    {
        AddGraphView();   
    }

    private void AddGraphView()
    {
        DialogueGraphView graphView = new DialogueGraphView();
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);     
        
        graphView.StretchToParentSize();
    }   
}
