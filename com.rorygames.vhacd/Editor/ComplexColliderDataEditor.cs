using UnityEditor;

namespace VHACD.Unity
{
    [CustomEditor(typeof(ComplexColliderData)),CanEditMultipleObjects]
    public class ComplexColliderDataEditor : Editor
    {
        private SerializedProperty _quality;
        private SerializedProperty _parameters;
        private SerializedProperty _baseMeshes;
        private SerializedProperty _computedMeshes;

        private string[] _qualityNames = { "Low", "Medium", "High", "Insane", "Custom" };

        public override void OnInspectorGUI()
        {
            Properties();
            EditorGUILayout.LabelField($"Quality: {_qualityNames[_quality.intValue]}", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_parameters);
            EditorGUILayout.PropertyField(_baseMeshes);
            EditorGUILayout.PropertyField(_computedMeshes);
            EditorGUI.EndDisabledGroup();
        }

        private void Properties()
        {
            _quality = serializedObject.FindProperty("quality");
            _parameters = serializedObject.FindProperty("parameters");
            _baseMeshes = serializedObject.FindProperty("baseMeshes");
            _computedMeshes = serializedObject.FindProperty("computedMeshes");
        }

    }
}
