using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using AIGD;
using Unity.VisualScripting;
using System;

namespace DS.Utilities
{
    using Data.Save;
    using System.Text.RegularExpressions;
    using ScriptableObjects;
    using Elementions;
    using Unity.EasyDialogue;

    public static class DSIOUtility
    {
        private static DialogueGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;
        
        private static List<DSGroup> groups;
        private static List<DialogueNode> nodes;

        private static Dictionary<string, DSDialogueGroupSO> createdDialogueGroups;
        public static void Initialize(DialogueGraphView dSGraphView, string graphName)
        {
            graphView = dSGraphView;
            graphFileName = graphName;
            containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphFileName}";

            groups = new List<DSGroup>();
            nodes = new List<DialogueNode>();

            createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
        }
        
        #region  Save Methods
        public static void Save()
        {
            CreateStaticFolders();

            GetElementsFromGraphViews();

           DSGraphSaveDataSO graphData = CreateAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");

           graphData.Initialize(graphFileName);

           DSDialogueContainerSO dialogueContainer = CreateAsset<DSDialogueContainerSO>(containerFolderPath, graphFileName);

           dialogueContainer.Initialize(graphFileName);

           SaveGroups(graphData, dialogueContainer);
           SaveNodes(graphData, dialogueContainer);

           SaveAsset(graphData);
           SaveAsset(dialogueContainer);      
        }


        #region Groups
        private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            foreach (DSGroup group in groups)
            {
                SaveGroupsToGraph(group, graphData);
                SaveGroupsToScriptableObject(group, dialogueContainer);
            }
        }

        private static void SaveGroupsToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
              ID = group.ID,
              Name = group.title,
              Position = group.GetPosition().position
            };

            graphData.Group.Add(groupData);
        }
        
        private static void SaveGroupsToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DSDialogueGroupSO dialogueGroup = CreateAsset<DSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

            SaveAsset(dialogueGroup);
        }
        #endregion
        #region  Nodes
        private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            foreach (DialogueNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);
            }
        }
        private static void SaveNodeToGraph(DialogueNode node, DSGraphSaveDataSO graphData)

        {
            List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();

            foreach (DSChoiceSaveData choice in node.Choices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID =choice.NodeID
                };

                choices.Add(choiceData);
            }
            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
              ID = node.ID,
              Name = node.DialogueName,
              Choices = node.Choices,
              Text = node.Text,
              GroupID = node.Group != null ? node.Group.ID : "",
              DialogueType = node.DialogueType,
              Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }
        
        
        private static void SaveNodeToScriptableObject(DialogueNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);

                dialogueContainer.DialogueGroups[createdDialogueGroups[node.Group.ID]].Add(dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);

                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }
        }
        #endregion
        #endregion
        #region  Creation Methods
        private static void CreateStaticFolders()

        {
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");

            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");
            CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }
        #endregion
        
        #region Fetc Methods
        private static void GetElementsFromGraphViews()
        {
            Type groupType = typeof(DSGroup);
            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DialogueNode node)
                {
                    nodes.Add(node);
                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    DSGroup group = (DSGroup) graphElement;

                    groups.Add(group);

                    return;
                }
            });
        }
        #endregion
        #region Utility Methods

        private static void CreateFolder(string path , string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullpath = $"{path}/{assetName}.asset";

            T asset = AssetDatabase.LoadAssetAtPath<T>(fullpath);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullpath);
            }

            return asset;
        }
        
        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion
    }
}