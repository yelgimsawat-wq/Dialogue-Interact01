using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using DS.Elementions;

namespace DS.Elementions 
{
    public class DialogueSingleChoiceNode : DialogueNode
    {
        public List<(string choiceText, Port port)> choicesPorts { get; private set; }

        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            
            DialogueType = DialogueType.SingleChoice;

            Choices.Add("Next Dialogue");

            choicesPorts = new List<(string, Port)>();
        }

        public override void Draw()
        {
            base.Draw();
            
            foreach (string choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                choicePort.portName = choice;
                outputContainer.Add(choicePort);

                choicesPorts.Add((choice, choicePort));
            }
            
            RefreshExpandedState();
        }
    }
}