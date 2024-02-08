using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumCartographicPolygon))]
    public class CesiumCartographicPolygonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
#if !UNITY_2022_2_OR_NEWER
            EditorGUILayout.HelpBox("CesiumCartographicPolygon requires the Splines package, which is not available " +
                "in this version of Unity.", MessageType.Error);
#endif
        }
    }
}
