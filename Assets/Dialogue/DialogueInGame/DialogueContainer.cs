using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Container")]
public class DialogueContainer : ScriptableObject
{
    [SerializeField] private List<DialogLine> dialogLines;
    [SerializeField] private string startDialogueName;

    public DialogLine GetStartLine()
    {
        return GetLineByName(startDialogueName);
    }

    public DialogLine GetLineByName(string DialogueName)
    {
        if(string.IsNullOrEmpty(DialogueName))
        {
            return null;
        }

        foreach(DialogLine line in dialogLines)
        {
            if(line.DialogueName == DialogueName)
            {
                return line;
            }
        }
        return null;
    }
    public void Setting(List<DialogLine> lines, string startDialogueName)
    {
        dialogLines = lines;
        this.startDialogueName = startDialogueName;
    }
}
