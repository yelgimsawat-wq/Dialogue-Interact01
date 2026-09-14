using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using DS.Elementions;
using DS.Utilities;
using System.Linq;
using Unity.EasyDialogue;


namespace DS.Elementions
{
    

    public class DialogueMultipleChoiceNode : DialogueNode
    {
        
        public List<(string choiceText, Port port)> choicesPorts { get; private set; }

        public override void Initialize(DialogueGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(dsGraphView, position);

            DialogueType = DialogueType.MultipleChoice;

            Choices.Add("New Choice");

            choicesPorts = new List<(string, Port )>();
        }

        public override void Draw()
        {
            base.Draw();
            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
            {
                Port choicePort = CreateChoicePort("New Choice");

                Choices.Add("New Choice");

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);
            
            foreach (string choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }

        private Port CreateChoicePort(string choice)
        {
            Port choicePort = this.CreatePort();

            Button deleteChoiceButton = DSElementUtility.CreateButton("X");

            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DSElementUtility.CreateTextField(choice);

            choiceTextField.AddToClassList("ds-node__textfield");
            choiceTextField.AddToClassList("ds-node__choice-textfield");
            choiceTextField.AddToClassList("ds-node__textfield__hidden");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            return choicePort;
        } 
        

    }    
    

}