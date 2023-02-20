using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomPropertyDrawer(typeof(CesiumPointCloudShading))]
    public class CesiumPointCloudShadingDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 5 + 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect rect = new Rect(position);
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, "Point Cloud Shading", EditorStyles.boldLabel);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            rect.y += EditorGUIUtility.singleLineHeight + 2.5f;
            GUIContent attenuationContent = new GUIContent(
                "Attenuation",
                "Whether or not to perform point attenuation. Attenuation controls the size of " +
                "the points rendered based on the geometric error of their tile.");
            EditorGUI.PropertyField(
                rect,
                property.FindPropertyRelative("_attenuation"),
                attenuationContent);

            rect.y += EditorGUIUtility.singleLineHeight + 2.5f;
            GUIContent geometricErrorScaleContent = new GUIContent(
                "Geometric Error Scale",
                "The scale to be applied to the tile's geometric error before it is used " +
                "to compute attenuation. Larger values will result in larger points.");
            EditorGUI.PropertyField(
                rect,
                property.FindPropertyRelative("_geometricErrorScale"),
                geometricErrorScaleContent);

            rect.y += EditorGUIUtility.singleLineHeight + 2.5f;
            GUIContent maximumAttenuationContent = new GUIContent(
                "Maximum Attenuation",
                "The maximum point attenuation in pixels. If this is zero, the " +
                "Cesium3DTileset's maximumScreenSpaceError will be used as the " +
                "maximum point attenuation.");
            EditorGUI.PropertyField(
                rect,
                property.FindPropertyRelative("_maximumAttenuation"),
                maximumAttenuationContent);

            rect.y += EditorGUIUtility.singleLineHeight + 2.5f;
            GUIContent baseResolutionContent = new GUIContent(
                "Base Resolution",
                "The average base resolution for the dataset in meters. " +
                "For example, a base resolution of 0.05 assumes an original " +
                "capture resolution of 5 centimeters between neighboring points." +
                "\n\n" +
                "This is used in place of geometric error when the tile's " +
                "geometric error is 0. If this value is zero, each tile with " +
                "a geometric error of 0 will have its geometric error " +
                "approximated instead.");
            EditorGUI.PropertyField(
                rect,
                property.FindPropertyRelative("_baseResolution"),
                baseResolutionContent);

            // Reset the editor indent level
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}