using Reinterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Networking;

namespace CesiumForUnity
{
    [Reinterop]
    internal partial class ConfigureReinterop
    {
        // The output path for generated C++ files.
        // If this is relative, it is relative to the this file.
#if UNITY_EDITOR
        public const string CppOutputPath = "../native~/Editor/generated-Editor";
#elif UNITY_ANDROID
        public const string CppOutputPath = "../native~/Editor/generated-Android";
#elif UNITY_IOS
        public const string CppOutputPath = "../native~/Editor/generated-iOS";
#elif UNITY_64
        public const string CppOutputPath = "../native~/Editor/generated-Standalone";
#else
        public const string CppOutputPath = "../native~/Editor/generated-Unknown";
#endif

        // The namespace with which to prefix all C# namespaces. For example, if this
        // property is set to "DotNet", then anything in the "System" namespace in C#
        // will be found in the "DotNet::System" namespace in C++.
        public const string BaseNamespace = "DotNet";

        // The name of the DLL or SO containing the C++ code.
        public const string NativeLibraryName = "CesiumForUnityNative-Editor";

        // Comma-separated types to treat as non-blittable, even if their fields would
        // otherwise cause Reinterop to treat them as blittable.
        public const string NonBlittableTypes = "Unity.Collections.LowLevel.Unsafe.AtomicSafetyHandle,Unity.Collections.NativeArray,UnityEngine.MeshData,UnityEngine.MeshDataArray";

        public void ExposeToCPP()
        {
            Debug.Log("log");

            UnityWebRequest request = UnityWebRequest.Get("url");

            var uploadHandler = new UploadHandlerRaw(new byte[0]);

            var rawBytes = new NativeArray<byte>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            unsafe
            {
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(rawBytes);
            }
            uploadHandler = new UploadHandlerRaw(rawBytes, true);
            request = new UnityWebRequest("url", "method", new NativeDownloadHandler(), uploadHandler);

            bool isDone = request.isDone;
            string e = request.error;
            string method = request.method;
            string url = request.url;
            if (request.result == UnityWebRequest.Result.Success) { };
            request.downloadHandler = new NativeDownloadHandler();
            request.SetRequestHeader("name", "value");
            request.GetResponseHeader("name");
            Dictionary<string,string>.Enumerator enumerator = request.GetResponseHeaders().GetEnumerator();
            while(enumerator.MoveNext())
            {
                string key = enumerator.Current.Key;
                string value = enumerator.Current.Value;
            }
            request.downloadHandler.Dispose();
            long responseCode = request.responseCode;
            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            op.completed += o => { };

            Task.Run(() => { });

            CesiumIonSession session = CesiumIonSession.Ion();
            session.IsConnected();

            EditorPrefs.HasKey("Key");
            EditorPrefs.GetString("Key");
            EditorPrefs.SetString("Key", "Value");
            EditorPrefs.DeleteKey("Key");

            Application.OpenURL("URL");
            string applicationVersion = Application.version;
            string applicationPlatform = Helpers.ToString(Application.platform);
            string productName = Application.productName;
            string osVersion = System.Environment.OSVersion.VersionString;

            TreeViewItem root = new TreeViewItem(0, -1, "root");
            int id = root.id;

            TreeViewState treeState = new TreeViewState();
            IonAssetsTreeView treeView = new IonAssetsTreeView(treeState);
            treeView.Reload();

            MultiColumnHeader header = treeView.multiColumnHeader;
            int sorted = header.sortedColumnIndex;
            bool ascending = header.IsSortedAscending(sorted);

            string searchString = treeView.searchString;
            int length = searchString.Length;
            searchString.Contains("string", StringComparison.CurrentCultureIgnoreCase);
            string.Equals("stringA", "stringB");
            string.IsNullOrEmpty("value");

            Rect r = new Rect(0, 0, 50, 50);
            GUI.Label(r, "Label");

            session.BroadcastConnectionUpdate();
            session.BroadcastAssetsUpdate();
            session.BroadcastProfileUpdate();
            session.BroadcastTokensUpdate();

            CesiumEditorWindow editorWindow = CesiumEditorWindow.currentWindow;

            IonAssetDetails.FormatType("type");
            IonAssetDetails.FormatDate("date");

            SelectIonTokenWindow.GetDefaultNewTokenName();
            SelectIonTokenWindow.ShowWindow();
            SelectIonTokenWindow tokenWindow = SelectIonTokenWindow.currentWindow;
            tokenWindow.tokenSource = IonTokenSource.Create;
            string name = tokenWindow.createdTokenName;
            int tokenIndex = tokenWindow.selectedExistingTokenIndex;
            tokenIndex = 0;
            string token = tokenWindow.specifiedToken;
            List<string> tokens = tokenWindow.GetExistingTokenList();
            tokens.Clear();
            tokens.Add(token);
            tokenWindow.RefreshExistingTokenList();
            tokenWindow.Close();

            token = CesiumRuntimeSettings.defaultIonAccessToken;
            CesiumRuntimeSettings.defaultIonAccessToken = "token";

            string tokenID = CesiumRuntimeSettings.defaultIonAccessTokenID;
            CesiumRuntimeSettings.defaultIonAccessTokenID = "tokenID";

            Cesium3DTileset[] tilesets = UnityEngine.Object.FindObjectsOfType<Cesium3DTileset>();
            Cesium3DTileset tileset = tilesets[0];
            for (int i = 0; i < tilesets.Length; i++)
            {
                tileset = tilesets[i];
                tileset.tilesetSource = CesiumDataSource.FromCesiumIon;
                token = tileset.ionAccessToken;
                tileset.RecreateTileset();
            }

            tileset = tileset.gameObject.GetComponent<Cesium3DTileset>();

            CesiumIonRasterOverlay ionOverlay = tileset.gameObject.GetComponent<CesiumIonRasterOverlay>();
            token = ionOverlay.ionAccessToken;
            ionOverlay.Refresh();
            GameObject gameObject = new GameObject("Name");
            tileset = gameObject.AddComponent<Cesium3DTileset>();
            ionOverlay = gameObject.AddComponent<CesiumIonRasterOverlay>();
            tileset.ionAssetID = 0;
            ionOverlay.ionAssetID = 0;

            CesiumRasterOverlay[] rasterOverlays = tileset.gameObject.GetComponents<CesiumRasterOverlay>();
            CesiumRasterOverlay overlay = rasterOverlays[0];
            UnityEngine.Object.DestroyImmediate(overlay);

            CesiumIonRasterOverlay[] ionRasterOverlays =
                tileset.gameObject.GetComponents<CesiumIonRasterOverlay>();
            ionOverlay = ionRasterOverlays[0];

            string substring = "string";
            substring = string.Concat(substring, new long[] { 100 });
            string message = string.Concat(substring, "string");
            Debug.Log(message);
            Debug.LogWarning(message);
            Debug.LogError(message);

            Selection.activeGameObject = gameObject;

            CesiumEditorUtility.FindFirstTileset();
            CesiumEditorUtility.FindFirstTilesetWithAssetID(0);
            CesiumEditorUtility.CreateTileset("name", 0);
            CesiumEditorUtility.AddBaseOverlayToTileset(tileset, 0);

            IonMissingAssetWindow.ShowWindow("Asset Name", 0);

            IonTokenTroubleshootingWindow troubleshootingWindow = null!;
            IonTokenTroubleshootingWindow.UseDefaultToken(troubleshootingWindow.ionAsset);
            TokenTroubleshootingDetails tokenDetails = troubleshootingWindow.assetTokenDetails;
            tokenDetails = troubleshootingWindow.defaultTokenDetails;
            token = tokenDetails.token;
            tokenDetails.isValid = true;
            tokenDetails.allowsAccessToAsset = true;
            tokenDetails.associatedWithUserAccount = true;
            tokenDetails.loaded = true;

            CesiumIonAsset asset = troubleshootingWindow.ionAsset;
            name = asset.objectName;
            string type = asset.type;
            string componentType = asset.componentType;
            string accessToken = asset.ionAccessToken;
            long assetId = asset.ionAssetID;
            bool isNull = asset.IsNull();

            AssetTroubleshootingDetails assetDetails = troubleshootingWindow.assetDetails;
            assetDetails.assetExistsInUserAccount = true;
            assetDetails.loaded = true;

            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }
    }
}
