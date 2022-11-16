using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace CesiumForUnity
{
    public class TextMeshProPromptWindow : EditorWindow
    {
        private static AddRequest _addTextMeshProRequest;

        public static void ShowWindow()
        {
            EditorWindow currentWindow = EditorWindow.GetWindow<TextMeshProPromptWindow>();

            Rect position = currentWindow.position;
            position.width = 400;
            position.height = 225;
            position.x = (Screen.width / 2) + (position.width / 2);
            position.y = (Screen.height / 2);
            currentWindow.position = position;

            currentWindow.Show();
            currentWindow.Focus();
        }

        private void OnEnable()
        {
            // Load the icon separately from the other resources.
            Texture2D icon = (Texture2D)Resources.Load("Cesium-64x64");
            icon.wrapMode = TextureWrapMode.Clamp;
            this.titleContent
                = new GUIContent("Missing TextMeshPro Essential Resources", icon);
        }

        private void OnGUI()
        {
            GUILayout.Space(5);

            EditorGUILayout.LabelField(
                "Cesium depends on the TextMeshPro package to be present in the project, " +
                "but no TMP settings object (\"TMP Settings.asset\") could be found in " +
                "any of this project's Resource folders." +
                "\n\n" +
                "If the package is already included in the project, then TextMeshPro will " +
                "need to import additional resources for it to work.",
                EditorStyles.wordWrappedLabel);

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Import TextMeshPro Essential Resources",
                GUILayout.Width(350), GUILayout.Height(35)))
            {
                ImportEssentialResources();
                this.Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }

        private void ImportEssentialResources()
        {
            // Prompt the user to import the TextMeshPro essential resources.
            // If the package isn't in the project, this will return false.
            if (EditorApplication.ExecuteMenuItem(
                "Window/TextMeshPro/Import TMP Essential Resources"))
            {
                return;
            }

            // Make a request to add TextMeshPro to the project.
            _addTextMeshProRequest = Client.Add("com.unity.textmeshpro");
            EditorApplication.update += ProcessTextMeshProRequest;
        }

        static void ProcessTextMeshProRequest()
        {
            if (_addTextMeshProRequest.IsCompleted)
            {
                if (_addTextMeshProRequest.Status == StatusCode.Success)
                {
                    // Prompt the user to import TextMeshPro resources.
                    EditorApplication.ExecuteMenuItem(
                        "Window/TextMeshPro/Import TMP Essential Resources");
                }
                else if (_addTextMeshProRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log("Could not add the TextMeshPro package to the project: "
                        + _addTextMeshProRequest.Error.message);
                }

                EditorApplication.update -= ProcessTextMeshProRequest;
            }
        }
    }
}
