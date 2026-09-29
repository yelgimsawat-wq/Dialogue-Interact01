using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class DialogueSetupWizard : EditorWindow
{
    private Canvas canvas;
    private PlayerInteraction playerInteraction;
    private Button choiceButtonPrefab;

    [MenuItem("Tools/Dialogue/Setup Wizard")]
    private static void Open()
    {
        GetWindow<DialogueSetupWizard>("Dialogue Setup");
    }

    private void OnEnable()
    {
        canvas = FindFirstObjectByType<Canvas>();
        playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        choiceButtonPrefab = AssetDatabase.LoadAssetAtPath<Button>("Assets/Gantarino/prefab/Button.prefab");
    }

    private void OnGUI()
    {
        canvas = (Canvas)EditorGUILayout.ObjectField("Canvas (optional)", canvas, typeof(Canvas), true);
        playerInteraction = (PlayerInteraction)EditorGUILayout.ObjectField("Player Interaction", playerInteraction, typeof(PlayerInteraction), true);
        choiceButtonPrefab = (Button)EditorGUILayout.ObjectField("Choice Button Prefab", choiceButtonPrefab, typeof(Button), false);

        EditorGUILayout.HelpBox("Uses the project's 1920 x 1080 reference resolution and dialogue dimensions from Test interact.unity.", MessageType.Info);
        using (new EditorGUI.DisabledScope(playerInteraction == null || choiceButtonPrefab == null))
        {
            if (GUILayout.Button("Create Dialogue UI")) CreateDialogueUI();
        }

        if (GUILayout.Button("Validate Selected Dialogue UI")) ValidateSelected();
    }

    private void CreateDialogueUI()
    {
        if (Object.FindFirstObjectByType<DialogueCanvas>() != null)
        {
            EditorUtility.DisplayDialog("Dialogue UI exists", "A DialogueCanvas is already in the active scene.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Create Dialogue UI");

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Dialogue Canvas");
            canvas = canvasObject.GetComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>() ?? Undo.AddComponent<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0;
        if (canvas.GetComponent<GraphicRaycaster>() == null) Undo.AddComponent<GraphicRaycaster>(canvas.gameObject);

        GameObject interactTextObject = CreateUIObject("InteractText", canvas.transform, new Vector2(606, 100), new Vector2(0, 80));
        TextMeshProUGUI interactText = interactTextObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(interactText, 36, TextAlignmentOptions.Center);
        AssignInteractionText(playerInteraction, interactText);

        GameObject dialogueObject = CreateUIObject("Dialogue", canvas.transform, new Vector2(1736.3052f, 351.4088f), new Vector2(0, -277.86566f));
        dialogueObject.SetActive(false);
        GameObject panel = CreateUIObject("DialoguePanel", dialogueObject.transform, new Vector2(1736.3052f, 351.4088f), Vector2.zero);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        GameObject dialogueTextObject = CreateUIObject("DialogueText", dialogueObject.transform, new Vector2(1625.9f, 236.3f), new Vector2(0, -42.63434f));
        TextMeshProUGUI dialogueText = dialogueTextObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(dialogueText, 32, TextAlignmentOptions.TopLeft);

        GameObject npcNameObject = CreateUIObject("NpcName", dialogueObject.transform, new Vector2(996, 50), new Vector2(-335, 112.86566f));
        TextMeshProUGUI npcName = npcNameObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(npcName, 28, TextAlignmentOptions.MidlineLeft);

        GameObject choicesObject = CreateUIObject("ChoicesContainer", dialogueObject.transform, new Vector2(253.72f, 30), new Vector2(-22, 316));
        VerticalLayoutGroup layout = choicesObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 8;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        DialogueCanvas dialogueCanvas = Undo.AddComponent<DialogueCanvas>(dialogueObject);
        SerializedObject serialized = new SerializedObject(dialogueCanvas);
        serialized.FindProperty("dialoguePanel").objectReferenceValue = dialogueObject;
        serialized.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        serialized.FindProperty("choiceButtonPrefab").objectReferenceValue = choiceButtonPrefab;
        serialized.FindProperty("choiceButtonContainer").objectReferenceValue = choicesObject.transform;
        serialized.FindProperty("playerInteraction").objectReferenceValue = playerInteraction;
        serialized.FindProperty("objectNames").objectReferenceValue = npcNameObject;
        serialized.ApplyModifiedProperties();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = dialogueObject;
        EditorUtility.DisplayDialog("Dialogue UI created", "Dialogue UI and scene references are ready.", "OK");
    }

    private static GameObject CreateUIObject(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        obj.layer = 5;
        obj.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)obj.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return obj;
    }

    private static void ConfigureText(TMP_Text text, float size, TextAlignmentOptions alignment)
    {
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
    }

    private static void AssignInteractionText(PlayerInteraction target, TMP_Text text)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty("InteractionText");
        if (property != null)
        {
            property.objectReferenceValue = text;
            serialized.ApplyModifiedProperties();
        }
    }

    private static void ValidateSelected()
    {
        DialogueCanvas dialogue = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<DialogueCanvas>()
            : null;
        if (dialogue == null)
        {
            Debug.LogError("Select the Dialogue object created by this wizard.");
            return;
        }

        SerializedObject serialized = new SerializedObject(dialogue);
        string[] required = { "dialoguePanel", "dialogueText", "choiceButtonPrefab", "choiceButtonContainer", "playerInteraction", "objectNames" };
        foreach (string name in required)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.objectReferenceValue == null)
            {
                Debug.LogError("Dialogue UI reference is missing: " + name, dialogue);
                return;
            }
        }
        Debug.Log("Dialogue UI references are assigned.", dialogue);
    }
}
