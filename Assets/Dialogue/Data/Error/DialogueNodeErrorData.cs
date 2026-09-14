using System.Collections.Generic;
using DS.Elementions;
using UnityEngine;

namespace DS.Data.Error
{
    public class DSNodeErrorData
    {
        public DialogueErrorData ErrorData { get; set; }
        public List<DialogueNode> Nodes { get; set; }

        public DSNodeErrorData()
        {
            ErrorData = new DialogueErrorData();
            Nodes = new List<DialogueNode>();
        }
    }
}