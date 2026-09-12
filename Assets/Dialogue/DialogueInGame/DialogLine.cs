using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class DialogLine 
{
    public string DialogueName;
    public string Text;
    
    public List<DialogChoice> Choices;

    public string NextDialogueName;
    
    [Header("Auto Advance Settings")]
    public bool IsAutoAdvance;
    public float AutoNextTime = 5f;
    public string AutoAdvanceNextDialogueName;
    
    public DialogLine()
    {
        Choices = new List<DialogChoice>();
    }

    [Serializable]
    public class DialogChoice
    {
        public string ChoiceText;
        public string NextDialogueName;
    }
}