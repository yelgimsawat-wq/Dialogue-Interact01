using System.Collections.Generic;
using DS.Elementions;
using DS.Data.Save;

namespace Unity.EasyDialogue
{
    public static partial class DialogueGraphExporter
    {
        static partial void ExportMultipleChoice(DialogueNode node, DialogLine line,
            Dictionary<DialogueNode, DialogLine> nodeToLine)
        {
            if (!(node is DialogueMultipleChoiceNode)) return;

            foreach (var choicePort in node.outputContainer.Children())
            {
                if (!(choicePort is UnityEditor.Experimental.GraphView.Port port)) continue;
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
