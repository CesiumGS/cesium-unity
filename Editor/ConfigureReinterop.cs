using Reinterop;
using System.Threading.Tasks;
using UnityEditor;
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
            bool isDone = request.isDone;
            string e = request.error;
            string method = request.method;
            string url = request.url;
            if(request.result == UnityWebRequest.Result.Success){};
            request.downloadHandler = new NativeDownloadHandler();
            request.SetRequestHeader("name", "value");
            request.GetResponseHeader("name");
            request.downloadHandler.Dispose();
            long responseCode = request.responseCode;
            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            op.completed += o => { };

            Task.Run(() => { });
        }
    }
}//
