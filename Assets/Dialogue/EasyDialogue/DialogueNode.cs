using System.Collections.Generic; 
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DS.Elementions
{
    public class DialogueNode : Node
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public DialogueType DialogueType { get; set; }
        public Port InputPort { get; private set; }

        public virtual void Initialize(Vector2 position)
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue Text";

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            VisualElement nameContainer = new VisualElement();
            nameContainer.AddToClassList("ds-node__name-container");
            // nameContainer.style.flexDirection = FlexDirection.Row;
            // nameContainer.style.alignItems = Align.Center;
            // nameContainer.style.paddingLeft = 5;
            // nameContainer.style.paddingRight = 5;
            // nameContainer.style.paddingTop = 5;
            // nameContainer.style.paddingBottom = 5;

            Label nameLabel = new Label("DialogueName:");
            
            TextField dialogueNameTextField = new TextField()
            {
                value = DialogueName,
            };

            dialogueNameTextField.AddToClassList("ds-node__textfield");
            dialogueNameTextField.AddToClassList("ds-node__textname-textfield");
            dialogueNameTextField.AddToClassList("ds-node__textfield_hidden");

            // dialogueNameTextField.style.flexGrow = 1;
            dialogueNameTextField.AddToClassList("ds-node__name-field");
            dialogueNameTextField.RegisterValueChangedCallback(evt => DialogueName = evt.newValue);

            nameContainer.Add(dialogueNameTextField);
            titleContainer.Add(nameContainer);

            Port InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "Dialogue Connection";
            inputContainer.Add(InputPort);

            VisualElement customContainer = new VisualElement();

            customContainer.AddToClassList("ds-node__custom-data-container");

            // customContainer.style.paddingLeft = 10;
            // customContainer.style.paddingRight = 10;
            // customContainer.style.paddingTop = 5;
            // customContainer.style.paddingBottom = 5;

            Foldout textFoldout = new Foldout()
            {
                text = "Dialogue Text",
                value = true
            };

            TextField textTextField = new TextField()
            {
                value = Text,
                multiline = true
            };

            // textTextField.style.height = 60;
            textTextField.RegisterValueChangedCallback(evt => { Text = evt.newValue; });
            
            textTextField.AddToClassList("ds-node__textfield");
            textTextField.AddToClassList("ds-node__quote-textfield");

            textFoldout.Add(textTextField);
            customContainer.Add(textFoldout);
            extensionContainer.Add(customContainer);

            RefreshExpandedState();
        }
    }
}