#if UNITY_EDITOR

using Reinterop;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity;

[Reinterop]
internal partial class ConfigureReinteropEditor
{
    public void ExposeToCPP()
    {
        SceneView sv = SceneView.lastActiveSceneView;
        Camera svc = sv.camera;

        bool isPlaying = EditorApplication.isPlaying;
        EditorApplication.update += () => {};
    }
}

#endif // #if UNITY_EDITOR
