#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CesiumForUnity
{
    [InitializeOnLoad]
    public static class CesiumCreditSystemManager
    {
        static CesiumCreditSystemManager()
        {
            EditorApplication.playModeStateChanged += HandleEnteringPlayMode;
            EditorSceneManager.sceneClosing += HandleClosingScene;
        }

        /// <summary>
        /// This handles the destruction of the default credit system between scene switches in the 
        /// Unity Editor.
        /// Without this, the credit system will live between instances and fail to capture the current 
        /// scene's credits.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="removingScene">Whether the scene is being removed.</param>
        private static void HandleClosingScene(Scene scene, bool removingScene)
        {
            Destroy(CesiumCreditSystem.GetDefaultCreditSystem());
        }

        /// <summary>   
        /// This handles the destruction of the default credit system while entering Play Mode.
        /// Without this, the persisting credit system's UI will not register with the Play Mode view, leading
        /// to missing credits.
        /// </summary>
        /// <param name="state">The state change between the Edit and Play modes.</param>
        private static void HandleEnteringPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Destroy(CesiumCreditSystem.GetDefaultCreditSystem());
            }
        }

        /// <summary>
        /// Destroys the input credit system depending on the current Editor context. This is the same as
        /// the internal UnityLifetime class (which is currently inaccessible from Editor classes).
        /// </summary>
        /// <param name="creditSystem">The credit system.</param>
        private static void Destroy(CesiumCreditSystem creditSystem)
        {
            if (creditSystem == null) { return; }

            // In the Editor, we must use DestroyImmediate because Destroy won't
            // actually destroy the object.
            if (!EditorApplication.isPlaying)
            {
                Object.DestroyImmediate(creditSystem.gameObject);
                return;
            }

            Object.Destroy(creditSystem.gameObject);
        }
    }
}
#endif
