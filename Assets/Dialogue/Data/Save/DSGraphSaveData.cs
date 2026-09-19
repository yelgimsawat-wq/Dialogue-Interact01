using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DS.Data.Save
{
    public class DSGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField]public List<DSGroupSaveData> Group { get; set; }
        [field: SerializeField] public List<DSNodeSaveData> Nodes { get; set;}

        [field: SerializeField] public List<string> OldGroupNames { get; set;}
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set;}
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }
    
        public void Initialize(string fileName)
        {
            FileName = fileName;

            Group = new List<DSGroupSaveData>();
            Nodes = new List<DSNodeSaveData>();

            OldGroupNames = new List<string>();
            OldUngroupedNodeNames = new List<string>();
            OldGroupedNodeNames = new SerializableDictionary<string, List<string>>();
        }
    }
}

