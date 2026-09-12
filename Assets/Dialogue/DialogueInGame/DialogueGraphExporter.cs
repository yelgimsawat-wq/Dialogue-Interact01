using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using DS.Elementions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.EasyDialogue
{
   public class DialogueGraphExporter 
   {
        public static DialogueContainer Export(DialogueGraphView graphView, DialogueContainer target = null)  
        {
            if (graphView == null)
            {
                return null;
            }

            List<DialogueSingleChoiceNode> nodes = new List<DialogueSingleChoiceNode>();
            graphView.graphElements.ForEach(element =>
            {
                if (element is DialogueSingleChoiceNode node)
                {
                    nodes.Add(node);
                }
            });

            if (nodes.Count == 0)
            {
                return null;
            }

            Dictionary<DialogueSingleChoiceNode, DialogLine> nodeToLine = new Dictionary<DialogueSingleChoiceNode, DialogLine>();
     
            foreach (DialogueSingleChoiceNode node in nodes)
            {
                DialogLine line = new DialogLine
                {
                    DialogueName = node.DialogueName,
                    Text = node.Text
                };
     
                nodeToLine[node] = line;
            } 
            
            foreach (DialogueSingleChoiceNode node in nodes)
            {
                DialogLine line = nodeToLine[node];

                foreach ((string choiceText, Port port) in node.choicesPorts)
                {
                    string nextDialogueName = FindConnectedDialogueName(port);

                    line.Choices.Add(new DialogLine.DialogChoice
                    {
                        ChoiceText = choiceText, 
                        NextDialogueName = nextDialogueName
                    }); 
                }
            }

            string startDialogueName = FindStartDialogueName(nodes);
            DialogueContainer container = target != null ? target : CreateNewContainerAsset();

            if (container == null)
            {
                return null;
            }

            container.Setting(new List<DialogLine>(nodeToLine.Values), startDialogueName);

            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();

            return container;
        }   

        private static string FindConnectedDialogueName(Port outputPort)
        {
            foreach (Edge edge in outputPort.connections)
            {
                if (edge.input?.node is DialogueSingleChoiceNode targetNode)
                {
                    return targetNode.DialogueName;
                }
            }
            return "";
        }

        private static string FindStartDialogueName(List<DialogueSingleChoiceNode> nodes)
        {
            List<DialogueSingleChoiceNode> candidate = new List<DialogueSingleChoiceNode>(); 

            foreach (DialogueSingleChoiceNode node in nodes)
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
                return nodes[0].DialogueName;
            }
            if (candidate.Count > 1)
            {
                Debug.LogWarning("อย่ารุมเค้า เอาแค่เส้นเดียวเถอะขอร้อง");
                return candidate[0].DialogueName;
            }

            return candidate[0].DialogueName;
        }
        
        private static DialogueContainer CreateNewContainerAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "สร้าง Dialogue Container ใหม่",
                "New Dialogue",
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