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

            currentWindow.ShowModalUtility();
        }

        private void OnEnable() {
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

    [InitializeOnLoad]
    public static class CesiumEditorUtility
    {
        static CesiumEditorUtility()
        {
            EditorApplication.update += CheckProjectFilesForTextMeshPro;

            Cesium3DTileset.OnCesium3DTilesetLoadFailure +=
                HandleCesium3DTilesetLoadFailure;
            CesiumRasterOverlay.OnCesiumRasterOverlayLoadFailure +=
                HandleCesiumRasterOverlayLoadFailure;
        }

        static void CheckProjectFilesForTextMeshPro()
        {
            Object tmpSettings = Resources.Load("TMP Settings");
            if (tmpSettings != null)
            {
                return;
            }

            TextMeshProPromptWindow.ShowWindow();

            EditorApplication.update -= CheckProjectFilesForTextMeshPro;
        }

        static void
        HandleCesium3DTilesetLoadFailure(Cesium3DTilesetLoadFailureDetails details)
        {
            if (details.tileset == null)
            {
                return;
            }

            // Don't open a troubleshooting panel during play mode.
            if (EditorApplication.isPlaying)
            {
                return;
            }

            // Check for a 401 connecting to Cesium ion, which means the token is invalid
            // (or perhaps the asset ID is). Also check for a 404, because ion returns 404
            // when the token is valid but not authorized for the asset.
            if (details.type == Cesium3DTilesetLoadType.CesiumIon
                && (details.httpStatusCode == 401 || details.httpStatusCode == 404))
            {
                IonTokenTroubleshootingWindow.ShowWindow(details.tileset, true);
            }

            Debug.Log(details.message);
        }

        static void
        HandleCesiumRasterOverlayLoadFailure(CesiumRasterOverlayLoadFailureDetails details)
        {
            if (details.overlay == null)
            {
                return;
            }

            // Don't open a troubleshooting panel during play mode.
            if (EditorApplication.isPlaying)
            {
                return;
            }

            // Check for a 401 connecting to Cesium ion, which means the token is invalid
            // (or perhaps the asset ID is). Also check for a 404, because ion returns 404
            // when the token is valid but not authorized for the asset.
            if (details.type == CesiumRasterOverlayLoadType.CesiumIon
                && (details.httpStatusCode == 401 || details.httpStatusCode == 404))
            {
                IonTokenTroubleshootingWindow.ShowWindow(details.overlay, true);
            }

            Debug.Log(details.message);
        }

        public static Cesium3DTileset? FindFirstTileset()
        {
            Cesium3DTileset[] tilesets =
                Object.FindObjectsOfType<Cesium3DTileset>(true);
            for (int i = 0; i < tilesets.Length; i++)
            {
                Cesium3DTileset tileset = tilesets[i];
                if (tileset != null)
                {
                    return tileset;
                }
            }

            return null;
        }

        public static Cesium3DTileset? FindFirstTilesetWithAssetID(long assetID)
        {
            Cesium3DTileset[] tilesets =
                Object.FindObjectsOfType<Cesium3DTileset>(true);
            for (int i = 0; i < tilesets.Length; i++)
            {
                Cesium3DTileset tileset = tilesets[i];
                if (tileset != null && tileset.ionAssetID == assetID)
                {
                    return tileset;
                }
            }

            return null;
        }

        public static CesiumGeoreference? FindFirstGeoreference()
        {
            CesiumGeoreference[] georeferences =
               Object.FindObjectsOfType<CesiumGeoreference>(true);
            for (int i = 0; i < georeferences.Length; i++)
            {
                CesiumGeoreference georeference = georeferences[i];
                if (georeference != null)
                {
                    return georeference;
                }
            }

            return null;
        }

        public static Cesium3DTileset CreateTileset(string name, long assetID)
        {
            // Find a georeference in the scene, or create one if none exists.
            CesiumGeoreference? georeference = CesiumEditorUtility.FindFirstGeoreference();
            if (georeference == null)
            {
                GameObject georeferenceGameObject =
                    new GameObject("CesiumGeoreference");
                georeference =
                    georeferenceGameObject.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georeferenceGameObject, "Create Georeference");
            }

            GameObject tilesetGameObject = new GameObject(name);
            tilesetGameObject.transform.SetParent(georeference.gameObject.transform);

            Cesium3DTileset tileset = tilesetGameObject.AddComponent<Cesium3DTileset>();
            tileset.name = name;
            tileset.ionAssetID = assetID;

            Undo.RegisterCreatedObjectUndo(tilesetGameObject, "Create Tileset");

            return tileset;
        }

        public static CesiumIonRasterOverlay
            AddBaseOverlayToTileset(Cesium3DTileset tileset, long assetID)
        {
            GameObject gameObject = tileset.gameObject;
            CesiumIonRasterOverlay overlay = gameObject.GetComponent<CesiumIonRasterOverlay>();
            if (overlay != null)
            {
                Undo.RecordObject(overlay, "Update Base Overlay of Tileset");
            }
            else
            {
                overlay = Undo.AddComponent<CesiumIonRasterOverlay>(gameObject);
            }

            overlay.ionAssetID = assetID;

            return overlay;
        }
    }
}
