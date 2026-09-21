using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS.Elementions
{
    public class DSGroup : Group
    {
        public string ID { get; set; }
        public string OldTitle { get; set; }

        public DSGroup(string groupTitle, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            title = groupTitle;
            OldTitle = groupTitle;

            SetPosition(new Rect(position, Vector2.zero));
        }
    }
}
