using Reinterop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using Unity.Mathematics;
using UnityEngine.Pool;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    [Reinterop]
    internal partial class ConfigureReinterop
    {
        // The output path for generated C++ files.
        // If this is relative, it is relative to the this file.
#if UNITY_EDITOR
        public const string CppOutputPath = "../native~/Runtime/generated-Editor";
#elif UNITY_ANDROID
        public const string CppOutputPath = "../native~/Runtime/generated-Android";
#elif UNITY_IOS
        public const string CppOutputPath = "../native~/Runtime/generated-iOS";
#elif UNITY_64
        public const string CppOutputPath = "../native~/Runtime/generated-Standalone";
#else
        public const string CppOutputPath = "../native~/Runtime/generated-Unknown";
#endif

        // The namespace with which to prefix all C# namespaces. For example, if this
        // property is set to "DotNet", then anything in the "System" namespace in C#
        // will be found in the "DotNet::System" namespace in C++.
        public const string BaseNamespace = "DotNet";

        // The name of the DLL or SO containing the C++ code.
#if UNITY_IOS && !UNITY_EDITOR
        public const string NativeLibraryName = "__Internal";
#else
        public const string NativeLibraryName = "CesiumForUnityNative-Runtime";
#endif
        // Comma-separated types to treat as non-blittable, even if their fields would
        // otherwise cause Reinterop to treat them as blittable.
        public const string NonBlittableTypes = "Unity.Collections.LowLevel.Unsafe.AtomicSafetyHandle,Unity.Collections.NativeArray,UnityEngine.MeshData,UnityEngine.MeshDataArray";

        public void ExposeToCPP()
        {
            Camera c = Camera.main;
            Transform t = c.transform;
            Vector3 u = t.up;
            Vector3 f = t.forward;

            Vector4 v = new Vector4(1.0f, 0.0f, 1.0f, 0.0f);

            t.position = new Vector3();
            Vector3 p = t.position;
            float x = p.x;
            float y = p.y;
            float z = p.z;
            Quaternion q = new Quaternion();
            q = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            c.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            float fov = c.fieldOfView;
            int pixelHeight = c.pixelHeight;
            int pixelWidth = c.pixelWidth;
            float aspect = c.aspect;
            //IFormattable f = new Vector3();
            //IEquatable<Vector3> f2 = new Vector3();

            GameObject go = new GameObject();
            go.name = go.name;
            go = new GameObject("name");
            go.SetActive(go.activeSelf);
            Transform transform = go.transform;
            transform.parent = transform.parent;
            transform.SetParent(transform.parent, false);
            transform.position = transform.position;
            transform.rotation = transform.rotation;
            transform.localPosition = transform.localPosition;
            transform.localRotation = transform.localRotation;
            transform.localScale = transform.localScale;
            transform.SetPositionAndRotation(transform.position, transform.rotation);
            Transform root = transform.root;
            int siblingIndex = transform.GetSiblingIndex();
            Matrix4x4 m = transform.localToWorldMatrix;
            Matrix4x4 m2 = transform.worldToLocalMatrix;

            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();

            go.transform.Find("Child Name");
            go.transform.GetChild(go.transform.childCount - 1);
            go.transform.DetachChildren();
            go.hideFlags = HideFlags.DontSave;

            Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, false, false);
            texture2D.LoadRawTextureData(IntPtr.Zero, 0);
            NativeArray<byte> textureBytes = texture2D.GetRawTextureData<byte>();

            unsafe
            {
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(textureBytes);
            }

            int textureBytesLength = textureBytes.Length;
            texture2D.Apply(true, true);
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.anisoLevel = 16;
            texture2D.filterMode = FilterMode.Trilinear;
            Texture texture = texture2D;
            texture.wrapModeU = texture.wrapModeU;
            texture.wrapModeV = texture.wrapModeV;
            texture.wrapModeW = texture.wrapModeW;
            

            Mesh mesh = new Mesh();
            Mesh[] meshes = new[] { mesh };
            mesh = meshes[0];
            int meshesLength = meshes.Length;
            mesh.SetVertices(new NativeArray<Vector3>());
            mesh.SetNormals(new NativeArray<Vector3>());
            mesh.SetUVs(0, new NativeArray<Vector2>());
            mesh.SetIndices(new NativeArray<int>(), MeshTopology.Triangles, 0, true, 0);
            mesh.RecalculateBounds();
            int instanceID = mesh.GetInstanceID();

            Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 2, 1));

            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            Debug.Log("Logging");

            MeshRenderer meshRenderer = new MeshRenderer();
            GameObject meshGameObject = meshRenderer.gameObject;
            meshRenderer.material = UnityEngine.Object.Instantiate(meshRenderer.material);
            int id = Shader.PropertyToID("name");
            meshRenderer.material.SetTexture(id, texture2D);
            meshRenderer.material.SetFloat(id, 1.0f); 
            meshRenderer.material.SetVector(id, new Vector4());
            meshRenderer.material.DisableKeyword("keywordName");
            meshRenderer.material.EnableKeyword("keywordName");
            meshRenderer.material.GetTexture(id);
            var ids = new List<int>();
            meshRenderer.material.GetTexturePropertyNameIDs(ids);
            for (int i = 0; i < ids.Count; ++i)
            {
                meshRenderer.material.GetTexture(ids[i]);
            }
            meshRenderer.material.shaderKeywords = meshRenderer.material.shaderKeywords;
            meshRenderer.sharedMaterial = meshRenderer.sharedMaterial;
            meshRenderer.material.shader = meshRenderer.material.shader;
            UnityEngine.Object.Destroy(meshGameObject);
            UnityEngine.Object.DestroyImmediate(meshGameObject);

            MeshFilter meshFilter = new MeshFilter();
            meshFilter.mesh = mesh;
            meshFilter.sharedMesh = mesh;

            Resources.Load<Material>("name");

            byte b;
            unsafe
            {
                string s = Encoding.UTF8.GetString(&b, 0);
            }

            NativeArray<Vector3> nav = new NativeArray<Vector3>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NativeArray<Vector2> nav2 = new NativeArray<Vector2>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> nai = new NativeArray<int>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            unsafe
            {
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nav);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nav2);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nai);
            }

            nav.Dispose();
            nav2.Dispose();
            nai.Dispose();

            string temporaryCachePath = Application.temporaryCachePath;
            bool isEditor = Application.isEditor;
            string applicationVersion = Application.version;
            string applicationPlatform = Helpers.ToString(Application.platform);
            string productName = Application.productName;
            string osVersion = System.Environment.OSVersion.VersionString;

            int frames = Time.frameCount;

            Marshal.FreeCoTaskMem(Marshal.StringToCoTaskMemUTF8("hi"));

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
            if(request.result == UnityWebRequest.Result.Success){};
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
            //Action<AsyncOperation> foo = (ao) => { };
            //var asdfx = foo + foo;
            op.completed += o => { };

            UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture("url");

            Task.Run(() => { });

            Cesium3DTileset tileset = new Cesium3DTileset();
            tileset.tilesetSource = tileset.tilesetSource;
            tileset.url = tileset.url;
            tileset.ionAssetID = tileset.ionAssetID;
            tileset.ionAccessToken = tileset.ionAccessToken;
            tileset.logSelectionStats = tileset.logSelectionStats;
            tileset.opaqueMaterial = tileset.opaqueMaterial;
            tileset.enabled = tileset.enabled;
            tileset.maximumScreenSpaceError = tileset.maximumScreenSpaceError;
            tileset.preloadAncestors = tileset.preloadAncestors;
            tileset.preloadSiblings = tileset.preloadSiblings;
            tileset.forbidHoles = tileset.forbidHoles;
            tileset.maximumSimultaneousTileLoads = tileset.maximumSimultaneousTileLoads;
            tileset.maximumCachedBytes = tileset.maximumCachedBytes;
            tileset.loadingDescendantLimit = tileset.loadingDescendantLimit;
            tileset.enableFrustumCulling = tileset.enableFrustumCulling;
            tileset.enableFogCulling = tileset.enableFogCulling;
            tileset.enforceCulledScreenSpaceError = tileset.enforceCulledScreenSpaceError;
            tileset.culledScreenSpaceError = tileset.culledScreenSpaceError;
            //tileset.useLodTransitions = tileset.useLodTransitions;
            //tileset.lodTransitionLength = tileset.lodTransitionLength;
            // tileset.generateSmoothNormals = tileset.generateSmoothNormals;
            tileset.createPhysicsMeshes = tileset.createPhysicsMeshes;
            tileset.suspendUpdate = tileset.suspendUpdate;
            tileset.previousSuspendUpdate = tileset.previousSuspendUpdate;
            tileset.showTilesInHierarchy = tileset.showTilesInHierarchy;
            tileset.updateInEditor = tileset.updateInEditor;
            tileset.showCreditsOnScreen = tileset.showCreditsOnScreen;

            Cesium3DTileset tilesetFromGameObject = go.GetComponent<Cesium3DTileset>();
            MeshRenderer meshRendererFromGameObject = go.GetComponent<MeshRenderer>();
            MeshFilter meshFilterFromGameObject = go.GetComponent<MeshFilter>();
            CesiumIonRasterOverlay ionOverlay = go.GetComponent<CesiumIonRasterOverlay>();
            ionOverlay.ionAssetID = ionOverlay.ionAssetID;
            ionOverlay.ionAccessToken = ionOverlay.ionAccessToken;

            CesiumRasterOverlay overlay = go.GetComponent<CesiumRasterOverlay>();
            overlay.showCreditsOnScreen = overlay.showCreditsOnScreen;
            overlay.maximumScreenSpaceError = overlay.maximumScreenSpaceError;
            overlay.maximumTextureSize = overlay.maximumTextureSize;
            overlay.maximumSimultaneousTileLoads = overlay.maximumSimultaneousTileLoads;
            overlay.subTileCacheBytes = overlay.subTileCacheBytes;

            CesiumRasterOverlay baseOverlay = ionOverlay;
            baseOverlay.AddToTileset();
            baseOverlay.RemoveFromTileset();

            CesiumBingMapsRasterOverlay bingMapsRasterOverlay =
                go.GetComponent<CesiumBingMapsRasterOverlay>();
            bingMapsRasterOverlay.bingMapsKey = bingMapsRasterOverlay.bingMapsKey;
            bingMapsRasterOverlay.mapStyle = bingMapsRasterOverlay.mapStyle;
            baseOverlay = bingMapsRasterOverlay;
            
            CesiumTileMapServiceRasterOverlay tileMapServiceRasterOverlay =
                go.GetComponent<CesiumTileMapServiceRasterOverlay>();
            tileMapServiceRasterOverlay.url = tileMapServiceRasterOverlay.url;
            tileMapServiceRasterOverlay.specifyZoomLevels =
                tileMapServiceRasterOverlay.specifyZoomLevels;
            tileMapServiceRasterOverlay.minimumLevel = tileMapServiceRasterOverlay.minimumLevel;
            tileMapServiceRasterOverlay.maximumLevel = tileMapServiceRasterOverlay.maximumLevel;
            baseOverlay = tileMapServiceRasterOverlay;

            CesiumWebMapServiceRasterOverlay webMapServiceRasterOverlay =
                go.GetComponent<CesiumWebMapServiceRasterOverlay>();
            webMapServiceRasterOverlay.baseUrl = webMapServiceRasterOverlay.baseUrl;
            webMapServiceRasterOverlay.layers = webMapServiceRasterOverlay.layers;
            webMapServiceRasterOverlay.tileWidth = webMapServiceRasterOverlay.tileWidth;
            webMapServiceRasterOverlay.tileHeight = webMapServiceRasterOverlay.tileHeight;
            webMapServiceRasterOverlay.minimumLevel = webMapServiceRasterOverlay.minimumLevel;
            webMapServiceRasterOverlay.maximumLevel = webMapServiceRasterOverlay.maximumLevel;
            baseOverlay = webMapServiceRasterOverlay;

            CesiumRasterOverlay[] overlaysArray = go.GetComponents<CesiumRasterOverlay>();
            int len = overlaysArray.Length;
            overlay = overlaysArray[0];

            MonoBehaviour mb = tileset;
            mb.StartCoroutine(new NativeCoroutine(endIteration => endIteration).GetEnumerator());

            CesiumMetadata metadata = go.AddComponent<CesiumMetadata>();
            metadata = go.GetComponent<CesiumMetadata>();
            CesiumMetadata metadataParent = go.GetComponentInParent<CesiumMetadata>();
            MetadataType type = MetadataType.String;
            if(type == MetadataType.None){
                type = MetadataType.Int16;
            }
            metadata.GetFeatures(transform, 3);
            CesiumFeature[] features = new CesiumFeature[2];
            var feature = features[0] = new CesiumFeature();
            feature.className = "";
            feature.featureTableName = "";
            feature.properties = new string[4];
            feature.properties[2] = "";

            CesiumGeoreference georeference = go.AddComponent<CesiumGeoreference>();
            georeference = go.GetComponent<CesiumGeoreference>();
            georeference.longitude = georeference.longitude;
            georeference.latitude = georeference.latitude;
            georeference.height = georeference.height;
            georeference.ecefX = georeference.ecefX;
            georeference.ecefY = georeference.ecefY;
            georeference.ecefZ = georeference.ecefZ;
            georeference.originAuthority = georeference.originAuthority;

            CesiumGeoreference inParent = go.GetComponentInParent<CesiumGeoreference>();
            inParent.MoveOrigin();
            inParent.changed += () => { };

            float time = Time.deltaTime;

            GameObject[] gos = GameObject.FindGameObjectsWithTag("test");
            for (int i = 0; i < gos.Length; ++i)
            {
                GameObject goFromArray = gos[i];
                gos[i] = goFromArray;
            }

            go = Resources.Load<GameObject>("name");
            go = UnityEngine.Object.Instantiate(go);

            CesiumCreditSystem creditSystem = go.AddComponent<CesiumCreditSystem>();
            creditSystem = go.GetComponent<CesiumCreditSystem>();

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[meshDataArray.Length - 1];

            VertexAttributeDescriptor[] descriptorsArray = new VertexAttributeDescriptor[1];
            VertexAttributeDescriptor descriptor0 = descriptorsArray[0];

            meshData.SetVertexBufferParams(1, descriptorsArray);
            meshData.SetIndexBufferParams(1, IndexFormat.UInt16);
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, 1, MeshTopology.Triangles));

            NativeArray<Vector3> positionNormal = meshData.GetVertexData<Vector3>(0);
            NativeArray<Vector2> texCoord = meshData.GetVertexData<Vector2>(0);
            NativeArray<byte> vertexData = meshData.GetVertexData<byte>(0);
            NativeArray<ushort> indices = meshData.GetIndexData<ushort>();
            NativeArray<uint> indices32 = meshData.GetIndexData<uint>();

            int positionNormalLength = positionNormal.Length;
            int texCoordLength = texCoord.Length;
            int indicesLength = indices.Length;
            int indices32Length = indices32.Length;

            unsafe
            {
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(positionNormal);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(texCoord);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(indices);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(indices32);
            }

            meshDataArray.Dispose();

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes, MeshUpdateFlags.Default);

            Physics.BakeMesh(mesh.GetInstanceID(), false);

            CesiumCreditSystem[] creditSystems = UnityEngine.Object.FindObjectsOfType<CesiumCreditSystem>();
            for (int i = 0; i < creditSystems.Length; ++i)
            {
                creditSystem = creditSystems[i];
                creditSystem.gameObject.name.StartsWith("name");
            }

            int numImages = creditSystem.numberOfImages;
            creditSystem.SetCreditsText("Popup", "OnScreen");
            creditSystem.StartCoroutine(creditSystem.LoadImage("string"));
            string delimiter = creditSystem.defaultDelimiter;

            List<string> stringList = new List<string>();
            stringList.Add("item");
            stringList.Clear();

            string test = string.Concat("string", "string2");
            string[] stringArray = stringList.ToArray();
            test = string.Join(" ", stringArray);
            string.IsNullOrEmpty("value");

            string token = CesiumRuntimeSettings.defaultIonAccessToken;

            Cesium3DTilesetLoadFailureDetails tilesetDetails
                = new Cesium3DTilesetLoadFailureDetails(tileset, Cesium3DTilesetLoadType.Unknown, 0, "");
            Cesium3DTileset.BroadcastCesium3DTilesetLoadFailure(tilesetDetails);

            CesiumRasterOverlayLoadFailureDetails
                overlayDetails = new CesiumRasterOverlayLoadFailureDetails(
                                                overlay,
                                                CesiumRasterOverlayLoadType.Unknown,
                                                0,
                                                "");
            CesiumRasterOverlay.BroadcastCesiumRasterOverlayLoadFailure(overlayDetails);

            double3 cv3 = new double3();
            cv3.x = cv3.y = cv3.z;
            double3 cv4 = new double3(1.0, 2.0, 3.0);
            double3x3 matrix3x3 = double3x3.identity;

            go.GetComponent<CesiumGlobeAnchor>();
            CesiumGlobeAnchor[] globeAnchors = go.GetComponentsInChildren<CesiumGlobeAnchor>();
            globeAnchors = go.GetComponentsInChildren<CesiumGlobeAnchor>(true);
            CesiumGlobeAnchor globeAnchor = globeAnchors[globeAnchors.Length - 1];
            globeAnchor.positionGlobeFixed = globeAnchor.positionGlobeFixed;

            globeAnchor = go.AddComponent<CesiumGlobeAnchor>();
            globeAnchor.detectTransformChanges = globeAnchor.detectTransformChanges;
            globeAnchor.adjustOrientationForGlobeWhenMoving = globeAnchor.adjustOrientationForGlobeWhenMoving;
            globeAnchor.longitudeLatitudeHeight = globeAnchor.longitudeLatitudeHeight;
            globeAnchor.localToGlobeFixedMatrix = globeAnchor.localToGlobeFixedMatrix;

            // Private properties for use by the C++ class.
            globeAnchor._georeference = null;
            globeAnchor._localToGlobeFixedMatrix = new double4x4();
            globeAnchor._localToGlobeFixedMatrixIsValid = true;
            globeAnchor._lastLocalToWorld = new Matrix4x4();
            globeAnchor.UpdateGeoreferenceIfNecessary();

            ObjectPool<Mesh> meshPool = CesiumObjectPool.MeshPool;
            Mesh pooledMesh = meshPool.Get();
            meshPool.Release(pooledMesh);
            
#if UNITY_EDITOR
            SceneView sv = SceneView.lastActiveSceneView;
            sv.pivot = sv.pivot;
            sv.rotation = sv.rotation;
            Camera svc = sv.camera;
            svc.transform.SetPositionAndRotation(p, q);

            bool isPlaying = EditorApplication.isPlaying;
            EditorApplication.update += () => {};
#endif
        }
    }
}
