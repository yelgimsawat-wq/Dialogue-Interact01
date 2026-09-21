using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DialogueCanvas : MonoBehaviour
{
    [SerializeField] 
    private GameObject dialoguePanel;
    [SerializeField] 
    private TMP_Text dialogueText;
    [SerializeField] 
    private Button choiceButtonPrefab;
    [SerializeField] 
    private Transform choiceButtonContainer;
    [SerializeField]
    private PlayerInteraction playerInteraction;

    private DialogueContainer _currentDialogue;
    private DialogLine _currentLine;
    private Coroutine _autoAdvanceCoroutine;

    public void Start()
    {


        if (playerInteraction == null)
        {
            Debug.LogError("ไม่พบ PlayerInteraction ในฉาก", this);
            return;
        }
        Debug.Log("ใช้งาน" + playerInteraction.CurrentInteractionType);
    }

    public void ShowDialogue(DialogueContainer container)
    {
        if (dialoguePanel == null) Debug.LogError("ลืมใส่ dialoguePanel ");
        if (dialogueText == null) Debug.LogError("ลืมใส่ dialogueText ");


        if (container == null)
        {
            Debug.LogError("ข้อมูล container ป็น Null!");
            return;
        }
        Debug.Log($"ได้รับไฟล์ชื่อ: {container.name}");

        
        _currentDialogue = container;
        DialogLine startLine = container.GetStartLine();
        if (startLine == null) 
        {
            Debug.LogError("หาจุดเริ่มต้นไม่เจอ");
            return;
        }
        Debug.Log($"ชื่อโหนดเริ่มต้น: {startLine.DialogueName}");
        dialoguePanel.SetActive(true);

        if (playerInteraction.CurrentInteractionType == InteractionType.FirstPerson)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (playerInteraction.CurrentInteractionType == InteractionType.ThirdPerson)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        DisplayLine(startLine);
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        StopAutoAdvanceIfRunning();
        ClearChoiceButtons();
        
        _currentDialogue = null;
        _currentLine = null;

        if (playerInteraction.CurrentInteractionType == InteractionType.FirstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (playerInteraction.CurrentInteractionType == InteractionType.ThirdPerson)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        
    }

    private void DisplayLine(DialogLine line)
    {
        Debug.Log($"แสดงข้อความ: {line.Text}");
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

        // Containers without a layout stack every prefab at the same position.
        // Keep any layout configured in the scene; otherwise provide a vertical list.
        if (choiceButtonContainer.GetComponent<LayoutGroup>() == null)
        {
            VerticalLayoutGroup layout = choiceButtonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

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

        if (choiceButtonContainer is RectTransform rectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        // if (_currentLine == null || _currentDialogue == null) return;
        if (_currentLine == null)
        {
            Debug.LogError("ไม่มีบรรทัดปัจจุบัน");
            return;
        }
        if (_currentDialogue == null)
        {
            Debug.LogError("ไม่มีข้อมูลบทสนทนาปัจจุบัน");
            return;
        }

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
            GameObject oldButton = choiceButtonContainer.GetChild(i).gameObject;
            // Destroy is deferred until the end of the frame; exclude old buttons
            // from layout and input before rebuilding the next set of choices.
            oldButton.SetActive(false);
            Destroy(oldButton);
        }
    }
}
