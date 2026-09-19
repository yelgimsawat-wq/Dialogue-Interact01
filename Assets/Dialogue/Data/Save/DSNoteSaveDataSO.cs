using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DS.Elementions;
using System;
namespace DS.Data.Save
{

    [Serializable]
public class DSNodeSaveData
{
    [field: SerializeField]public string Name { get; set; }
    [field: SerializeField]public string Text { get; set; }
    [field: SerializeField]public List<DSChoiceSaveData> Choices { get; set; }
    [field: SerializeField]public string  GroupID { get; set; }
    [field: SerializeField]public DialogueType DialogueType { get; set; }
    [field: SerializeField]public Vector2 Position { get; set; }     
}
}