// Compile with the real exporter, DialogLine, DialogueContainer and DSChoiceSaveData.
// These doubles test exported data; they do not test Unity UI or asset persistence.
using System;
using System.Collections.Generic;
using DS.Elementions;
using DS.Data.Save;
using Unity.EasyDialogue;
using UnityEditor.Experimental.GraphView;

namespace UnityEngine {
    public class ScriptableObject { public static T CreateInstance<T>() where T : new() => new T(); }
    public class SerializeField : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string value) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public static class Debug { public static void Log(string s) {} public static void LogWarning(string s) {} }
}
namespace UnityEngine.UIElements {
    public class VisualElement {
        private readonly List<VisualElement> children = new List<VisualElement>();
        public object userData;
        public void Add(VisualElement element) => children.Add(element);
        public void Remove(VisualElement element) => children.Remove(element);
        public IEnumerable<VisualElement> Children() => children;
    }
}
namespace UnityEditor {
    public static class EditorUtility {
        public static void SetDirty(object target) {}
        public static string SaveFilePanelInProject(string a, string b, string c, string d) => "test.asset";
    }
    public static class AssetDatabase {
        public static void SaveAssets() {}
        public static void CreateAsset(object asset, string path) {}
    }
}
namespace UnityEditor.Experimental.GraphView {
    public class Port : UnityEngine.UIElements.VisualElement {
        public DialogueNode node;
        public string portName;
        public List<Edge> connections = new List<Edge>();
    }
    public class Edge { public Port input; }
}
namespace DS.Elementions {
    public class DialogueNode {
        public string DialogueName;
        public string Text;
        public Port InputPort = new Port();
        public UnityEngine.UIElements.VisualElement outputContainer = new UnityEngine.UIElements.VisualElement();
    }
    public class DialogueSingleChoiceNode : DialogueNode {
        public List<(string choiceText, Port port)> choicesPorts = new List<(string, Port)>();
    }
    public class DialogueMultipleChoiceNode : DialogueNode { }
}
namespace Unity.EasyDialogue {
    public class DialogueGraphView { public List<DialogueNode> graphElements = new List<DialogueNode>(); }
}
public static class ExporterRegression {
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static Port Choice(DialogueNode from, string text, DialogueNode to = null) {
        var port = new Port { userData = new DSChoiceSaveData { Text = text }, portName = text };
        from.outputContainer.Add(port);
        if (from is DialogueSingleChoiceNode single) single.choicesPorts.Add((text, port));
        if (to != null) {
            to.InputPort.node = to;
            var edge = new Edge { input = to.InputPort };
            port.connections.Add(edge);
            to.InputPort.connections.Add(edge);
        }
        return port;
    }
    public static void Run() {
        var entry = new DialogueSingleChoiceNode { DialogueName = "Entry", Text = "Hello" };
        var menu = new DialogueMultipleChoiceNode { DialogueName = "Menu", Text = "Choose" };
        var finish = new DialogueSingleChoiceNode { DialogueName = "Finish", Text = "Done" };
        Choice(entry, "Next", menu);
        var renamed = Choice(menu, "Old label", finish);
        ((DSChoiceSaveData)renamed.userData).Text = "Continue";
        Choice(menu, "Leave");
        var deleted = Choice(menu, "Deleted");
        menu.outputContainer.Remove(deleted);
        Choice(finish, "End");
        var graph = new DialogueGraphView();
        graph.graphElements.AddRange(new DialogueNode[] { entry, menu, finish });
        var result = DialogueGraphExporter.Export(graph, new DialogueContainer());
        Check(result.GetStartLine().DialogueName == "Entry", "Wrong start node");
        Check(result.GetLineByName("Menu") != null, "Multiple Choice node was omitted");
        Check(result.GetLineByName("Entry").Choices[0].NextDialogueName == "Menu", "Single-to-multiple connection was lost");
        var choices = result.GetLineByName("Menu").Choices;
        Check(choices.Count == 2, "Added/deleted choices were not exported correctly");
        Check(choices[0].ChoiceText == "Continue", "Edited choice label was lost");
        Check(choices[0].NextDialogueName == "Finish", "Multiple-to-single connection was lost");
        Check(choices[1].NextDialogueName == "", "Unconnected choice must end dialogue");
        var first = new DialogueMultipleChoiceNode { DialogueName = "First" };
        var second = new DialogueMultipleChoiceNode { DialogueName = "Second" };
        Choice(first, "Go", second);
        Choice(second, "Exit");
        graph = new DialogueGraphView();
        graph.graphElements.AddRange(new DialogueNode[] { second, first });
        result = DialogueGraphExporter.Export(graph, new DialogueContainer());
        Check(result != null, "Multiple-only graph was not exported");
        Check(result.GetStartLine().DialogueName == "First", "Multiple Choice start node was not found");
        Check(result.GetStartLine().Choices[0].NextDialogueName == "Second", "Multiple-to-multiple connection was lost");
        // Follow the same name lookup used by DialogueCanvas.OnChoiceSelected.
        var duplicateStart = new DialogueSingleChoiceNode { DialogueName = "DialogueName", Text = "First line" };
        var duplicateNext = new DialogueMultipleChoiceNode { DialogueName = "DialogueName", Text = "Second line" };
        var reservedName = new DialogueSingleChoiceNode { DialogueName = "DialogueName_2", Text = "Last line" };
        Choice(duplicateStart, "Next", duplicateNext);
        Choice(duplicateNext, "Next", reservedName);
        Choice(reservedName, "End");
        graph = new DialogueGraphView();
        graph.graphElements.AddRange(new DialogueNode[] { duplicateStart, duplicateNext, reservedName });
        result = DialogueGraphExporter.Export(graph, new DialogueContainer());
        var current = result.GetStartLine();
        Check(current.Text == "First line", "Duplicate names select the wrong start dialogue");
        current = result.GetLineByName(current.Choices[0].NextDialogueName);
        Check(current.Text == "Second line", "Next repeats the same dialogue when names are duplicated");
        current = result.GetLineByName(current.Choices[0].NextDialogueName);
        Check(current.Text == "Last line", "Generated name collided with an existing dialogue name");
        Check(current.DialogueName == "DialogueName_2", "An existing unique name changed");
        Check(duplicateStart.DialogueName == "DialogueName", "Export renamed the editor node");
        graph.graphElements.Reverse();
        result = DialogueGraphExporter.Export(graph, new DialogueContainer());
        Check(result.GetStartLine().Text == "First line", "Start selection depends on node order");
        Check(result.GetLineByName(result.GetStartLine().Choices[0].NextDialogueName).Text == "Second line", "Next depends on node order");
    }
}
