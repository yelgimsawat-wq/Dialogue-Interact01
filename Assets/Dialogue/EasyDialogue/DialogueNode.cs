using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using DS.Elementions;

namespace DS.Elements
{
    public class DialogueNode : Node
    {
        public string DialogueName { get; set; }

        public List<string> Choices { get; set; }

        public string Text { get; set; }
        
        public DialogueType DialogueType { get; set; }

        public virtual void Initialize(Vector2 position)
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue Text";

            SetPosition(new Rect(position, Vector2.zero));
        }

        public virtual void Draw()
        {
            TextField dialogueNameTextField = new TextField()
            {
                value = DialogueName,
            };
            titleContainer.Insert(0, dialogueNameTextField);

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            VisualElement customContainer = new VisualElement();

            Foldout textFoldout = new Foldout()
            {
                text = "Dialogue Text",
            };

            TextField textTextField = new TextField()
            {
                value = Text,
            };
            textFoldout.Add(textTextField);

            customContainer.Add(textFoldout);
            extensionContainer.Add(customContainer);

            RefreshExpandedState();
        }
    }
}
