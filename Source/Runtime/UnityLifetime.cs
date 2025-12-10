using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    internal static class UnityLifetime
    {
        public static void Destroy(Object o)
        {
#if UNITY_EDITOR
            // In the Editor, we must use DestroyImmediate because Destroy won't
            // actually destroy the object.
            if (!EditorApplication.isPlaying)
            {
                Object.DestroyImmediate(o);
                return;
            }
#endif

            Object.Destroy(o);
        }
    }

}