using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using DS.Elementions;
using Unity.EasyDialogue;

namespace DS.Elementions 
{
    using Data.Save;
    using DS.Utilities;

    public class DialogueSingleChoiceNode : DialogueNode
    {
        public List<(string choiceText, Port port)> choicesPorts { get; private set; }

        public override void Initialize(DialogueGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(dsGraphView, position);
            
            DialogueType = DialogueType.SingleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "Next Dialogue" 
            };
            Choices.Add(choiceData);

            choicesPorts = new List<(string, Port)>();
        }

        public override void Draw()
        {
            base.Draw();
            
            foreach (DSChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);
                choicePort.userData = choice;
                outputContainer.Add(choicePort);

                choicesPorts.Add((choice.Text, choicePort));
            }
            
            
            RefreshExpandedState();
        }
    }
}
