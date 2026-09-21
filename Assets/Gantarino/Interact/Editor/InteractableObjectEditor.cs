using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(InteractableObject))]
[CanEditMultipleObjects]
public class InteractableObjectEditor : Editor
{
    private SerializedProperty interactableType;
    private SerializedProperty dialogueCanvas;
    private SerializedProperty dialogueContainer;
    private SerializedProperty targetLights;
    private SerializedProperty door;
    private SerializedProperty doorPivot;
    private SerializedProperty isOpen;
    private SerializedProperty isRotatingDoor;
    private SerializedProperty speed;
    private SerializedProperty rotationAmount;
    private SerializedProperty forwardDirection;

    private bool showInteractionSettings = true;
    private bool showRotationSettings = true;
    private bool showReferences = true;
    private bool showQuestSettings = true;

    private void OnEnable()
    {
        interactableType = serializedObject.FindProperty("interactionTypes");
        dialogueCanvas = serializedObject.FindProperty("dialogueCanvas");
        dialogueContainer = serializedObject.FindProperty("dialogueContainer");
        targetLights = serializedObject.FindProperty("targetlights");
        door = serializedObject.FindProperty("door");
        doorPivot = serializedObject.FindProperty("doorPivot");
        isOpen = serializedObject.FindProperty("IsOpen");
        isRotatingDoor = serializedObject.FindProperty("IsRotatingDoor");
        speed = serializedObject.FindProperty("Speed");
        rotationAmount = serializedObject.FindProperty("RotationAmount");
        forwardDirection = serializedObject.FindProperty("ForwardDirection");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawFlags(interactableType, (InteractionActions)interactableType.intValue);

        if (interactableType.hasMultipleDifferentValues)
        {
            EditorGUILayout.HelpBox("เลือก Interactable Type ให้ตรงกันเพื่อแก้ไขค่าของหลายวัตถุพร้อมกัน", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        InteractionActions selectedType = (InteractionActions)interactableType.intValue;
        if ((selectedType & InteractionActions.Door) != 0)
        {
            showInteractionSettings = EditorGUILayout.Foldout(showInteractionSettings, "Interaction Settings", true);
            if (showInteractionSettings)
            {
                EditorGUILayout.PropertyField(isOpen);
                EditorGUILayout.PropertyField(isRotatingDoor);
                EditorGUILayout.PropertyField(speed);
            }
            if (isRotatingDoor.boolValue || isRotatingDoor.hasMultipleDifferentValues)
            {
                showRotationSettings = EditorGUILayout.Foldout(showRotationSettings, "Rotation Settings", true);
                if (showRotationSettings)
                {
                    EditorGUILayout.PropertyField(rotationAmount);
                    EditorGUILayout.PropertyField(forwardDirection);
                }
            }
        }

        showReferences = EditorGUILayout.Foldout(showReferences, "References", true);
        if (showReferences)
        {
            if ((selectedType & InteractionActions.Dialogue) != 0)
            {
                EditorGUILayout.PropertyField(dialogueCanvas);
                EditorGUILayout.PropertyField(dialogueContainer);
            }
            if ((selectedType & InteractionActions.Light) != 0)
            {
                EditorGUILayout.PropertyField(targetLights, true);
            }
            if ((selectedType & InteractionActions.Door) != 0)
            {
                EditorGUILayout.PropertyField(door);
                EditorGUILayout.PropertyField(doorPivot);
            }
            if ((selectedType & InteractionActions.Quest) != 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("questSystem"));
                EditorGUILayout.HelpBox("หากไม่ใส่ Quest System จะค้นหาในฉากให้อัตโนมัติ", MessageType.Info);
            }
        }
        if ((selectedType & InteractionActions.Quest) != 0)
        {
            showQuestSettings = EditorGUILayout.Foldout(showQuestSettings, "Quest Settings", true);
            if (showQuestSettings)
            {
                SerializedProperty actions = serializedObject.FindProperty("questActions");
                DrawFlags(actions, (QuestInteractionActions)actions.intValue);
                QuestInteractionActions selectedActions = (QuestInteractionActions)actions.intValue;
                if (actions.hasMultipleDifferentValues || (selectedActions & QuestInteractionActions.AcceptQuest) != 0)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("questDefinition"));
                }
                if (actions.hasMultipleDifferentValues || (selectedActions & QuestInteractionActions.ReportEvent) != 0)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("questEventId"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("questTargetId"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("questEventAmount"));
                }
                EditorGUILayout.HelpBox("ทำงานทุกครั้งที่กด Interact หากเลือกทั้งสองอย่าง จะรับเควสต์ก่อนส่ง Event", MessageType.Info);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawFlags(SerializedProperty property, Enum value)
    {
        EditorGUILayout.LabelField(property.displayName, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        foreach (Enum option in Enum.GetValues(value.GetType()))
        {
            int flag = Convert.ToInt32(option);
            if (flag == 0 || (flag & (flag - 1)) != 0) continue;

            bool enabled = (property.intValue & flag) != 0;
            bool mixed = false;
            if (property.hasMultipleDifferentValues)
            {
                foreach (UnityEngine.Object selectedTarget in property.serializedObject.targetObjects)
                {
                    using (SerializedObject selectedObject = new SerializedObject(selectedTarget))
                    {
                        bool targetEnabled = (selectedObject.FindProperty(property.propertyPath).intValue & flag) != 0;
                        mixed |= targetEnabled != enabled;
                    }
                }
            }

            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(option.ToString()));
            EditorGUI.BeginProperty(rect, label, property);
            bool previousMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixed;
            EditorGUI.BeginChangeCheck();
            bool selected = EditorGUI.ToggleLeft(rect, label, enabled);
            if (EditorGUI.EndChangeCheck())
            {
                if (property.hasMultipleDifferentValues)
                {
                    // Change only this checkbox, preserving every object's other selections.
                    property.serializedObject.ApplyModifiedProperties();
                    foreach (UnityEngine.Object selectedTarget in property.serializedObject.targetObjects)
                    {
                        using (SerializedObject selectedObject = new SerializedObject(selectedTarget))
                        {
                            SerializedProperty flags = selectedObject.FindProperty(property.propertyPath);
                            flags.intValue = selected ? flags.intValue | flag : flags.intValue & ~flag;
                            selectedObject.ApplyModifiedProperties();
                        }
                    }
                    property.serializedObject.Update();
                }
                else
                {
                    property.intValue = selected ? property.intValue | flag : property.intValue & ~flag;
                }
            }
            EditorGUI.showMixedValue = previousMixedValue;
            EditorGUI.EndProperty();
        }
        EditorGUI.indentLevel--;
    }
}
