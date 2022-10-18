using Reinterop;
using System;
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
            request.downloadHandler = new NativeDownloadHandler();
            request.SetRequestHeader("name", "value");
            request.GetResponseHeader("name");
            request.downloadHandler.Dispose();
            long responseCode = request.responseCode;
            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            op.completed += o => { };

            Task.Run(() => { });

            CesiumIonSession session = CesiumIonSession.Ion();

            EditorPrefs.HasKey("Key");
            EditorPrefs.GetString("Key");
            EditorPrefs.SetString("Key", "Value");
            EditorPrefs.DeleteKey("Key");

            Application.OpenURL("URL");

            TreeViewItem root = new TreeViewItem(0, -1, "root");
            int id = root.id;

            TreeViewState treeState = new TreeViewState();
            IonAssetsTreeView treeView = new IonAssetsTreeView(treeState);
            MultiColumnHeader header = treeView.multiColumnHeader;
            treeView.Reload();
            header.ResizeToFit();

            string searchString = treeView.searchString;
            int length = searchString.Length;
            searchString.Contains("string", StringComparison.CurrentCultureIgnoreCase);
            string.Compare("stringA", "stringB", true);

            IonAssetsColumn column = IonAssetsColumn.Name;
            column = IonAssetsColumn.Type;
            column = IonAssetsColumn.DateAdded;
            int sorted = header.sortedColumnIndex;
            bool ascending = header.IsSortedAscending(sorted);

            Rect r = new Rect(0, 0, 50, 50);
            GUI.Label(r, "Label");

            session.TriggerConnectionUpdate();
            session.TriggerAssetsUpdate();
            session.TriggerProfileUpdate();
            session.TriggerTokensUpdate();

            IonAssetDetails.FormatType("type");
            IonAssetDetails.FormatDate("date");

            IonTokenSelector tokenSelector = new IonTokenSelector();
        }
    }
}//
