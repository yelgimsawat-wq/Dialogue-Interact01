using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using DS.Elementions;

namespace DS.Elementions
{
    

    public class DialogueMultipleChoiceNode : DialogueNode
    {
        
        public List<(string choiceText, Port port)> choicesPorts { get; private set; }

        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);

            DialogueType = DialogueType.MultipleChoice;

            Choices.Add("New Choice");

            choicesPorts = new List<(string, Port )>();
        }

        public override void Draw()
        {
            base.Draw();
            Button addChoiceButton = new Button()
            {
                text = "Add Choice"
            };

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);
            
            foreach (string choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                choicePort.portName = "";

                Button deleteChoiceButton = new Button()
                {
                    text = "x"
                };

                deleteChoiceButton.AddToClassList("ds-node__button");

                TextField choiceTextField = new TextField()
                {
                    value = choice
                };

                outputContainer.Add(choicePort);

                
                choiceTextField.AddToClassList("ds-node__textfield");
                choiceTextField.AddToClassList("ds-node__choice-textfield");
                choiceTextField.AddToClassList("ds-node__textfield__hidden");

                choicesPorts.Add((choice, choicePort));
                choicePort.Add(choiceTextField);
                choicePort.Add(deleteChoiceButton);
            }
            
            RefreshExpandedState();        }

    }    
    

}