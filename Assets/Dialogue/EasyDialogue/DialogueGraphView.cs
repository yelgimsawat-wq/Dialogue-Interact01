using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using DS.Elementions;
using System;

namespace Unity.EasyDialogue
{
    using DS.Data.Error;
    using DS.Data.Save;

    public partial class DialogueGraphView : GraphView
    {
        private readonly Dictionary<DialogueType, Func<DialogueNode>> nodeFactories = new Dictionary<DialogueType, Func<DialogueNode>>();

        // Optional partial implementations register the node types they provide.
        partial void RegisterSingleChoice();
        partial void RegisterMultipleChoice();

        private Dictionary<string, DSNodeErrorData> ungroupNode;
        public DialogueGraphView()
        {
            ungroupNode = new Dictionary<string, DSNodeErrorData>();
            AddManipulators();
            AddGridBackground();
            AddStyles();

            OnElementsDeleted();

        }

        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            RegisterSingleChoice();
            RegisterMultipleChoice();
            this.AddManipulator(new ContentDragger());
            
            this.AddManipulator(CreateGroupContextualMenu());

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("DialogueGroup", actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }


        private IManipulator CreateNodeContextualMenu(string actionTitle, DialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }

        private void RegisterNodeType(DialogueType dialogueType, string menuLabel, Func<DialogueNode> factory)
        {
            nodeFactories.Add(dialogueType, factory);
            this.AddManipulator(CreateNodeContextualMenu(menuLabel, dialogueType));
        }

        private DSGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            DSGroup group = new DSGroup(title, localMousePosition);

            return group;
        }

        public DialogueNode CreateNode(DialogueType dialogueType, Vector2 position)
        {
            if (!nodeFactories.TryGetValue(dialogueType, out Func<DialogueNode> factory))
            {
                throw new ArgumentException($"Dialogue node type '{dialogueType}' is not installed.", nameof(dialogueType));
            }

            DialogueNode node = factory();
            
            node.Initialize(this, position);
            node.Draw();

            AddUngroupedNode(node);
            
            return node;
        }


        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
        {
            List<DialogueNode> nodesToDelete = new List<DialogueNode>();
            List<Edge> edgesToDelete = new List<Edge>();

            foreach (GraphElement element in selection)
            {
                if (element is DialogueNode node)
                {
                    nodesToDelete.Add(node);
                }

                if (element is Edge edge)
                {
                    edge = (Edge) element;

                    edgesToDelete.Add(edge);

                    continue;
                }
            }

            DeleteElements(edgesToDelete);

            foreach (DialogueNode node in nodesToDelete)
            {
                RemoveUngroupedNode(node);

                node.DisconnectAllPorts();
                RemoveElement(node);
            }
        };
        }


        public void AddUngroupedNode(DialogueNode node)
        {
            string nodeName = node.DialogueName;

            if (!ungroupNode.ContainsKey(nodeName))
        {
            DSNodeErrorData nodeErrorData = new DSNodeErrorData();
            nodeErrorData.Nodes.Add(node);

            ungroupNode.Add(nodeName, nodeErrorData);
            return;
        }  

            DSNodeErrorData existingData = ungroupNode[nodeName];
            existingData.Nodes.Add(node);

            Color errorColor = existingData.ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (existingData.Nodes.Count == 2)
        {
            existingData.Nodes[0].SetErrorStyle(errorColor);
        }
        }

        public void RemoveUngroupedNode(DialogueNode node)
        {
            string nodeName = node.DialogueName;

            List<DialogueNode> ungroupNodesList = ungroupNode[nodeName].Nodes;

            ungroupNodesList.Remove(node);
            node.ResetStyle();

            if (ungroupNodesList.Count == 1)
            {
                ungroupNodesList[0].ResetStyle();
            }
            else if (ungroupNodesList.Count == 0)
            {
                ungroupNode.Remove(nodeName);
            }
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            StyleSheet graphViewStyleSheet = (StyleSheet) EditorGUIUtility.Load(
                "Assets/Dialogue/EditorDefaulrResources/DialogueSystem/DialogueGraphviewStyles.uss");
            StyleSheet nodeStyleSheet = (StyleSheet) EditorGUIUtility.Load(
                "Assets/Dialogue/EditorDefaulrResources/DialogueSystem/DialogueNodeStyles.uss");
            StyleSheet variablesStyleSheet = (StyleSheet)EditorGUIUtility.Load(
                "Assets/Dialogue/EditorDefaulrResources/DialogueSystem/DialogueVariables.uss");

            styleSheets.Add(variablesStyleSheet);
            styleSheets.Add(graphViewStyleSheet);
            styleSheets.Add(nodeStyleSheet);
            

        }
        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
              if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DialogueNode nextNode = (DialogueNode) edge.input.node;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData) edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }  

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }
                        Edge edge = (Edge) element;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData) edge.output.userData;

                        choiceData.NodeID = "";
                    }
                }
                return changes;
            };
        }
    }
}
