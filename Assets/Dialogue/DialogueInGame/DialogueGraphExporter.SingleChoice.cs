using System.Collections.Generic;
using DS.Elementions;
using DS.Data.Save;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Unity.EasyDialogue
{
    public static partial class DialogueGraphExporter
    {
        static partial void ExportSingleChoice(DialogueNode node, DialogLine line,
            Dictionary<DialogueNode, DialogLine> nodeToLine)
        {
            if (node is DialogueSingleChoiceNode)
            {
                foreach (VisualElement element in node.outputContainer.Children())
                {
                    if (!(element is Port port)) continue;
                    string choiceText = port.userData is DSChoiceSaveData choice ? choice.Text : port.portName;
                    line.Choices.Add(new DialogLine.DialogChoice
                    {
                        ChoiceText = choiceText,
                        NextDialogueName = FindConnectedDialogueName(port, nodeToLine)
                    });
                }
            }
        }
    }
}
