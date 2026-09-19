using UnityEngine;
using System;
using System.Collections.Generic;


namespace DS.Data.Save
{
        [Serializable]
        public class DSChoiceSaveData
    {
        [field: SerializeField]public string Text { get; set; }
        [field: SerializeField]public string NodeID { get; set; }
    }
}

