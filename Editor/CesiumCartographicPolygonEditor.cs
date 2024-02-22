using UnityEditor;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumCartographicPolygon))]
    public class CesiumCartographicPolygonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
#if !SUPPORTS_SPLINES
#if UNITY_2022_2_OR_NEWER
            EditorGUILayout.HelpBox("CesiumCartographicPolygon requires the Splines package, which is currently " +
                "not installed in the project. Install the Splines package using the Package Manager.", MessageType.Error);
#else
            EditorGUILayout.HelpBox("CesiumCartographicPolygon requires the Splines package, which is not available " +
                "in this version of Unity.", MessageType.Error);
#endif
#endif

            base.OnInspectorGUI();
        }
    }
}
