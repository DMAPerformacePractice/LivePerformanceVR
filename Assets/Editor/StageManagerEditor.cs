using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StageManager))]
[CanEditMultipleObjects]
public class StageManagerEditor : Editor
{
    private SerializedProperty _inTheatreProperty;
    private SerializedProperty _stageProperty;

    private bool showLightsProperties = true;

    private SerializedProperty _centerStageLightProperty;
    private SerializedProperty _roomLightProperty;

    private SerializedProperty _dimTimeProperty;
    private SerializedProperty _brightenTimeProperty;

    private SerializedProperty _audienceMemberPrefabsProperty;

    private void OnEnable()
    {
        _inTheatreProperty = serializedObject.FindProperty("inTheatre");
        _stageProperty = serializedObject.FindProperty("stage");

        _centerStageLightProperty = serializedObject.FindProperty("centerStageLight");
        _roomLightProperty = serializedObject.FindProperty("roomLight");

        _dimTimeProperty = serializedObject.FindProperty("dimTime");
        _brightenTimeProperty = serializedObject.FindProperty("brightenTime");

        _audienceMemberPrefabsProperty = serializedObject.FindProperty("audienceMemberPrefabs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_inTheatreProperty);

        EditorGUILayout.PropertyField(_stageProperty);

        if (_inTheatreProperty.boolValue)
        {
            EditorGUILayout.Space(10);

            showLightsProperties = EditorGUILayout.Foldout(showLightsProperties, "Lights Related Properties", true);

            if (showLightsProperties)
            {
                EditorGUILayout.PropertyField(_centerStageLightProperty);
                EditorGUILayout.PropertyField(_roomLightProperty);

                EditorGUILayout.Space(5);

                EditorGUILayout.PropertyField(_dimTimeProperty);
                EditorGUILayout.PropertyField(_brightenTimeProperty);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_audienceMemberPrefabsProperty);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
