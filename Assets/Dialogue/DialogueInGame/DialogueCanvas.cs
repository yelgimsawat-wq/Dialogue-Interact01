using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DialogueCanvas : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonContainer;

    private DialogueContainer _currentDialogue;
    private DialogLine _currentLine;
    private Coroutine _autoAdvanceCoroutine;

    public void ShowDialogue(DialogueContainer container)
    {
        if (container == null) return;
        
        _currentDialogue = container;
        DialogLine startLine = container.GetStartLine();
        if (startLine == null) return;

        dialoguePanel.SetActive(true);
        DisplayLine(startLine);
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        StopAutoAdvanceIfRunning();
        ClearChoiceButtons();
        
        _currentDialogue = null;
        _currentLine = null;
    }

    private void DisplayLine(DialogLine line)
    {
        _currentLine = line;
        dialogueText.text = line.Text;

        ClearChoiceButtons();
        StopAutoAdvanceIfRunning();

        if (line.IsAutoAdvance)
        {
            _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine(line));
        }
        else
        {
            CreateChoiceButtons(line);
        }  
    }

    private IEnumerator AutoAdvanceRoutine(DialogLine line)
    {
        yield return new WaitForSeconds(line.AutoNextTime);
        
        if (string.IsNullOrEmpty(line.AutoAdvanceNextDialogueName))
        {
            HideDialogue();
            yield break;
        }

        DialogLine nextLine = _currentDialogue.GetLineByName(line.AutoAdvanceNextDialogueName);
        if (nextLine == null)
        {
            HideDialogue();
            yield break;
        }
        DisplayLine(nextLine);
    }

    private void StopAutoAdvanceIfRunning()
    {
        if (_autoAdvanceCoroutine != null)
        {
            StopCoroutine(_autoAdvanceCoroutine);
            _autoAdvanceCoroutine = null;
        }        
    }

    private void CreateChoiceButtons(DialogLine line)
    {
        if (line.Choices == null) return;

        for (int i = 0; i < line.Choices.Count; i++)
        {  
            int choiceIndex = i;
            Button newButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            newButton.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = line.Choices[choiceIndex].ChoiceText; // แก้จาก .Text เป็น .ChoiceText ตาม DialogLine.cs
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (_currentLine == null || _currentDialogue == null) return;

        DialogLine.DialogChoice selectedChoice = _currentLine.Choices[choiceIndex];

        if (string.IsNullOrEmpty(selectedChoice.NextDialogueName))
        {
            HideDialogue();
            return;
        }

        DialogLine nextLine = _currentDialogue.GetLineByName(selectedChoice.NextDialogueName);
        if (nextLine == null)
        {
            HideDialogue();
            return;
        }
        DisplayLine(nextLine);
    }

    private void ClearChoiceButtons()
    {
        for (int i = choiceButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(choiceButtonContainer.GetChild(i).gameObject);
        }
    }
}