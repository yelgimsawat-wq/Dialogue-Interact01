using UnityEditor;

[CustomEditor(typeof(PlayerInteraction))]
public class PlayerInteractEditor : Editor
{
    #region Serialized Properties
    SerializedProperty interactionType;
    SerializedProperty layerMask;
    SerializedProperty range;
    SerializedProperty sphereCastRadius;
    SerializedProperty interactionText;
    SerializedProperty player;
    SerializedProperty mainCamera;

    bool showInteractionSettings, showInteractionText, showInteractReference = false;
    
    #endregion

    private void OnEnable()
    {
        interactionType = serializedObject.FindProperty("InteractionType");

        layerMask = serializedObject.FindProperty("LayerMask");

        range = serializedObject.FindProperty("Range");

        sphereCastRadius = serializedObject.FindProperty("SphereCastRadius");

        interactionText = serializedObject.FindProperty("InteractionText");

        player = serializedObject.FindProperty("player");

        mainCamera = serializedObject.FindProperty("mainCamera");

    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(interactionType);

        showInteractionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showInteractionSettings, "Interaction Settings");
        if (showInteractionSettings)
        {
            EditorGUILayout.PropertyField(range);
            if (interactionType.enumValueIndex == (int)InteractionType.ThirdPerson)
            {
                EditorGUILayout.PropertyField(sphereCastRadius);
            }
            EditorGUILayout.PropertyField(layerMask);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        showInteractionText = EditorGUILayout.BeginFoldoutHeaderGroup(showInteractionText, "UI");
        if (showInteractionText)
        {
            EditorGUILayout.PropertyField(interactionText);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        showInteractReference = EditorGUILayout.BeginFoldoutHeaderGroup(showInteractReference, "References");
        if (showInteractReference)
        {
            EditorGUILayout.PropertyField(player);
            EditorGUILayout.PropertyField(mainCamera);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
