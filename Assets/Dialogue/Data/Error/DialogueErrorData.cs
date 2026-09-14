using UnityEngine;
using System.Collections.Generic;
using DS.Elementions;


namespace DS.Data.Error
{
    public class DialogueErrorData
    {
        public Color Color { get; set; }

        public DialogueErrorData()
        {
            GenerateRandomColor();
        }
        private void GenerateRandomColor()
        {
            Color = new Color32(
                (byte) Random.Range(65, 256),
                (byte) Random.Range(50, 176),
                (byte) Random.Range(50, 176),
                255
            );
        }
    }
}