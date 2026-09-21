using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using DS.Elementions;
using DS.Data.Save;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.EasyDialogue
{
   public class DialogueGraphExporter 
   {
        public static DialogueContainer Export(DialogueGraphView graphView, DialogueContainer target = null, string defaultFileName = "New Dialogue")
        {
            if (graphView == null)
            {
                return null;
            }

            List<DialogueNode> nodes = new List<DialogueNode>();
            graphView.graphElements.ForEach(element =>
            {
                if (element is DialogueNode node)
                {
                    nodes.Add(node);
                }
            });

            if (nodes.Count == 0)
            {
                return null;
            }

            Dictionary<DialogueNode, DialogLine> nodeToLine = new Dictionary<DialogueNode, DialogLine>();
            // Runtime links use names, so each exported node needs a unique key.
            // Reserve original names before assigning suffixes to avoid collisions.
            HashSet<string> reservedNames = new HashSet<string>();
            HashSet<string> exportedNames = new HashSet<string>();
            foreach (DialogueNode node in nodes)
            {
                reservedNames.Add(node.DialogueName);
            }
     
            foreach (DialogueNode node in nodes)
            {
                string dialogueName = node.DialogueName;
                if (string.IsNullOrWhiteSpace(dialogueName) || !exportedNames.Add(dialogueName))
                {
                    string baseName = string.IsNullOrWhiteSpace(dialogueName) ? "Dialogue" : dialogueName;
                    int suffix = 2;
                    do
                    {
                        dialogueName = $"{baseName}_{suffix++}";
                    }
                    while (reservedNames.Contains(dialogueName) || exportedNames.Contains(dialogueName));

                    exportedNames.Add(dialogueName);
                }

                DialogLine line = new DialogLine
                {
                    DialogueName = dialogueName,
                    Text = node.Text
                };
     
                nodeToLine[node] = line;
            } 
            
            foreach (DialogueNode node in nodes)
            {
                DialogLine line = nodeToLine[node];

                foreach (VisualElement element in node.outputContainer.Children())
                {
                    if (!(element is Port port))
                    {
                        continue;
                    }

                    string choiceText = port.userData is DSChoiceSaveData choice
                        ? choice.Text
                        : port.portName;
                    string nextDialogueName = FindConnectedDialogueName(port, nodeToLine);

                    line.Choices.Add(new DialogLine.DialogChoice
                    {
                        ChoiceText = choiceText, 
                        NextDialogueName = nextDialogueName
                    }); 
                }
            }

            string startDialogueName = nodeToLine[FindStartNode(nodes)].DialogueName;
            DialogueContainer container = target != null ? target : CreateNewContainerAsset(defaultFileName);

            if (container == null)
            {
                return null;
            }

            container.Setting(new List<DialogLine>(nodeToLine.Values), startDialogueName);

            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();

            return container;
        }   

        private static string FindConnectedDialogueName(Port outputPort, Dictionary<DialogueNode, DialogLine> nodeToLine)
        {
            foreach (Edge edge in outputPort.connections)
            {
                if (edge.input?.node is DialogueNode targetNode && nodeToLine.TryGetValue(targetNode, out DialogLine targetLine))
                {
                    return targetLine.DialogueName;
                }
            }
            return "";
        }

        private static DialogueNode FindStartNode(List<DialogueNode> nodes)
        {
            List<DialogueNode> candidate = new List<DialogueNode>();

            foreach (DialogueNode node in nodes)
            {
                bool hasIncomingConnections = false;
                foreach (Edge edge in node.InputPort.connections)
                {
                    hasIncomingConnections = true;
                    break;
                }

                if (!hasIncomingConnections)
                {
                    candidate.Add(node);
                }
            }

            if (candidate.Count == 0) 
            {
                Debug.Log("อย่าทำเป็นวงกลมโว้ยยยยยยยยย");
                return nodes[0];
            }
            if (candidate.Count > 1)
            {
                Debug.LogWarning("อย่ารุมเค้า เอาแค่เส้นเดียวเถอะขอร้อง");
                return candidate[0];
            }

            return candidate[0];
        }
        
        private static DialogueContainer CreateNewContainerAsset(string defaultFileName)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "สร้าง Dialogue Container ใหม่",
                defaultFileName,
                "asset",
                "เลือกตำแหน่งที่จะเซฟไฟล์ dialogue"
            );
 
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
 
            DialogueContainer container = ScriptableObject.CreateInstance<DialogueContainer>();
            AssetDatabase.CreateAsset(container, path);
 
            return container;
        }
   }   
}
