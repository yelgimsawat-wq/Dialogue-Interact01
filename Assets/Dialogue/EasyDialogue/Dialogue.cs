using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using UnityEditor.Experimental.GraphView;
using Unity.EasyDialogue;

public class Dialogue : EditorWindow
{
    [SerializeField] private string fileName = "New Dialogue";
    private DialogueGraphView graphView;
    private TextField fileNameField;

    [MenuItem("Window/Dialogue/EasyDialogue")]
    public static void ShowExample()
    {
        GetWindow<Dialogue>("Dialogue");
    }

    private void OnEnable()
    {
        rootVisualElement.Clear();
        AddToolbar();
        AddGraphView();
    }

    private void AddToolbar()
    {
        Toolbar toolbar = new Toolbar();
        toolbar.style.flexShrink = 0;

        fileNameField = new TextField("File Name") { value = fileName };
        fileNameField.tooltip = "ชื่อไฟล์ Dialogue Container ที่จะ Export";
        fileNameField.style.flexGrow = 1;
        fileNameField.style.minWidth = 160;
        fileNameField.labelElement.style.minWidth = 65;
        fileNameField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
        toolbar.Add(fileNameField);
        toolbar.Add(new ToolbarButton(ExportGraph) { text = "Export", tooltip = "เลือกตำแหน่งและบันทึก Dialogue Container" });
        rootVisualElement.Add(toolbar);
    }

    public void ExportGraph()
    {
        string exportName = fileName.Trim();
        if (exportName.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
        {
            exportName = exportName.Substring(0, exportName.Length - 6);
        }

        if (string.IsNullOrWhiteSpace(exportName) || exportName == "." || exportName == ".." ||
            exportName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || exportName.EndsWith("."))
        {
            ShowNotification(new GUIContent("กรุณาใส่ชื่อไฟล์ที่ถูกต้อง โดยไม่ใส่เส้นทางโฟลเดอร์"));
            return;
        }

        if (graphView == null || !graphView.nodes.ToList().Exists(node => node is DS.Elementions.DialogueNode))
        {
            ShowNotification(new GUIContent("เพิ่มโหนดบทสนทนาก่อน Export"));
            return;
        }

        DialogueContainer container = DialogueGraphExporter.Export(graphView, defaultFileName: exportName);
        if (container == null) return;

        fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(container));
        fileNameField.SetValueWithoutNotify(fileName);
        EditorGUIUtility.PingObject(container);
        ShowNotification(new GUIContent($"Export สำเร็จ: {fileName}.asset"));
    }

    private void AddGraphView()
    {
        graphView = new DialogueGraphView();
        graphView.style.flexGrow = 1;
        graphView.style.minHeight = 0;
        rootVisualElement.Add(graphView);
    }   
}
