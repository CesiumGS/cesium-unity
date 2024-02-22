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
using UnityEngine.Experimental.Rendering;
using Unity.Mathematics;
using UnityEngine.Pool;
using UnityEngine.UIElements;

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
#elif UNITY_WSA
        public const string CppOutputPath = "../native~/Runtime/generated-WSA";
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
            go.SetActive(go.activeInHierarchy);
            int layer = go.layer;
            go.layer = layer;
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
            texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, 1, false);
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
            int vertexCount = mesh.vertexCount;
            int instanceID = mesh.GetInstanceID();

            Vector3[] vertices = mesh.vertices;
            Vector3 vertex = vertices[0];

            Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 2, 1));

            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            Debug.Log("Logging");
            Debug.LogWarning("Warning");

            MeshRenderer meshRenderer = new MeshRenderer();
            GameObject meshGameObject = meshRenderer.gameObject;
            meshRenderer.material = UnityEngine.Object.Instantiate(meshRenderer.material);

            int id = Shader.PropertyToID("name");
            int crc = meshRenderer.material.ComputeCRC();
            meshRenderer.material.SetTexture(id, texture2D);
            meshRenderer.material.SetFloat(id, 1.0f);
            meshRenderer.material.SetVector(id, new Vector4());
            meshRenderer.material.DisableKeyword("keywordName");
            meshRenderer.material.EnableKeyword("keywordName");
            meshRenderer.material.GetTexture(id);
            meshRenderer.material.SetTextureOffset(id, new Vector2());
            meshRenderer.material.SetTextureScale(id, new Vector2());
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
            UnityEngine.Object.DestroyImmediate(meshGameObject, true);
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
            string unityVersion = Application.unityVersion;
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
            if (request.result == UnityWebRequest.Result.Success) { };
            request.downloadHandler = new NativeDownloadHandler();
            request.SetRequestHeader("name", "value");
            request.GetResponseHeader("name");
            Dictionary<string, string>.Enumerator enumerator = request.GetResponseHeaders().GetEnumerator();
            while (enumerator.MoveNext())
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
            tileset.generateSmoothNormals = tileset.generateSmoothNormals;
            tileset.createPhysicsMeshes = tileset.createPhysicsMeshes;
            tileset.suspendUpdate = tileset.suspendUpdate;
            tileset.previousSuspendUpdate = tileset.previousSuspendUpdate;
            tileset.showTilesInHierarchy = tileset.showTilesInHierarchy;
            tileset.updateInEditor = tileset.updateInEditor;
            tileset.showCreditsOnScreen = tileset.showCreditsOnScreen;
            tileset.ionServer = tileset.ionServer;
            tileset.RecreateTileset();
            tileset.UpdateTilesetOptions();

            GraphicsFormat gfxFmt = GraphicsFormat.RGB_ETC_UNorm;
            FormatUsage fmtUsage = FormatUsage.Sample;
            SystemInfo.IsFormatSupported(gfxFmt, fmtUsage);

            Cesium3DTileset tilesetFromGameObject = go.GetComponent<Cesium3DTileset>();
            MeshRenderer meshRendererFromGameObject = go.GetComponent<MeshRenderer>();
            MeshFilter meshFilterFromGameObject = go.GetComponent<MeshFilter>();
            CesiumIonRasterOverlay ionOverlay = go.GetComponent<CesiumIonRasterOverlay>();
            ionOverlay.ionAssetID = ionOverlay.ionAssetID;
            ionOverlay.ionAccessToken = ionOverlay.ionAccessToken;
            ionOverlay.ionServer = ionOverlay.ionServer;
            ionOverlay.AddToTilesetLater(null);

            CesiumRasterOverlay overlay = go.GetComponent<CesiumRasterOverlay>();
            overlay.materialKey = overlay.materialKey;
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


#pragma warning disable 0618
            CesiumMetadata metadata = go.AddComponent<CesiumMetadata>();
            metadata = go.GetComponent<CesiumMetadata>();
            CesiumMetadata metadataParent = go.GetComponentInParent<CesiumMetadata>();
            MetadataType type = MetadataType.String;
            if (type == MetadataType.None)
            {
                type = MetadataType.Int16;
            }
            metadata.GetFeatures(transform, 3);
            CesiumFeature[] features = new CesiumFeature[2];
            var feature = features[0] = new CesiumFeature();
            feature.className = "";
            feature.featureTableName = "";
            feature.properties = new string[4];
            feature.properties[2] = "";
#pragma warning restore 0618

            CesiumGeoreference georeference = go.AddComponent<CesiumGeoreference>();
            georeference = go.GetComponent<CesiumGeoreference>();
            georeference.longitude = georeference.longitude;
            georeference.latitude = georeference.latitude;
            georeference.height = georeference.height;
            georeference.ecefX = georeference.ecefX;
            georeference.ecefY = georeference.ecefY;
            georeference.ecefZ = georeference.ecefZ;
            georeference.originAuthority = georeference.originAuthority;
            georeference.scale = georeference.scale;
            double4x4 ecefToLocal = georeference.ecefToLocalMatrix;

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

            CesiumCreditComponent creditComponent = new CesiumCreditComponent("text", "link", -1);
            List<CesiumCreditComponent> creditComponents = new List<CesiumCreditComponent>();
            creditComponents.Add(creditComponent);
            int creditCount = creditComponents.Count;

            CesiumCredit credit = new CesiumCredit();
            credit = new CesiumCredit(creditComponents);
            creditComponents = credit.components;

            CesiumCreditSystem creditSystem = go.AddComponent<CesiumCreditSystem>();
            creditSystem = CesiumCreditSystem.GetDefaultCreditSystem();
            creditSystem.StartCoroutine(creditSystem.LoadImage("string"));

            List<CesiumCredit> credits = creditSystem.onScreenCredits;
            credits = creditSystem.popupCredits;
            credits.Add(credit);
            credits.Clear();

            if (!creditSystem.HasLoadingImages())
            {
                creditSystem.BroadcastCreditsUpdate();
            }

            List<Texture2D> images = creditSystem.images;
            int count = images.Count;

            List<string> stringList = new List<string>();
            stringList.Add("item");
            stringList.Clear();

            string test = string.Concat("string", "string2");
            string[] stringArray = stringList.ToArray();
            test = stringArray[0];
            test = string.Join(" ", stringArray);
            string.IsNullOrEmpty("value");
            string.IsNullOrWhiteSpace("value");

            int length = test.Length;

            int requestsPerCachePrune = CesiumRuntimeSettings.requestsPerCachePrune;
            ulong maxItems = CesiumRuntimeSettings.maxItems;

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

            tileset.BroadcastNewGameObjectCreated(new GameObject());

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
            globeAnchor._lastLocalsAreValid = true;
            globeAnchor._lastLocalPosition = new Vector3();
            globeAnchor._lastLocalRotation = new Quaternion();
            globeAnchor._lastLocalScale = new Vector3();
            globeAnchor.UpdateGeoreferenceIfNecessary();

            CesiumTileExcluder[] excluders = go.GetComponentsInParent<CesiumTileExcluder>();
            CesiumTileExcluder excluder = excluders[0];
            excluder.AddToTileset(null);
            excluder.RemoveFromTileset(null);
            excluder.ShouldExclude(new Cesium3DTile());
            Cesium3DTile tile = new Cesium3DTile();
            tile._transform = new double4x4();
            tile._pTile = IntPtr.Zero;

            Cesium3DTileInfo info;
            info.usesAdditiveRefinement = true;
            info.geometricError = 1.0f;
            info.dimensions = Vector3.zero;
            info.isTranslucent = true;

            CesiumPointCloudRenderer renderer = go.AddComponent<CesiumPointCloudRenderer>();
            renderer.tileInfo = info;

            CesiumObjectPool<Mesh> meshPool = CesiumObjectPools.MeshPool;
            Mesh pooledMesh = meshPool.Get();
            meshPool.Release(pooledMesh);

            System.Object myObject = null;

            CesiumIntVec2 myIntVec2 = new CesiumIntVec2((SByte)1, (SByte)2);
            myIntVec2 = new CesiumIntVec2((Int16)1, (Int16)2);
            myIntVec2 = new CesiumIntVec2((Int32)1, (Int32)2);
            myIntVec2 = new CesiumIntVec2((Int64)1, (Int64)2);
            CesiumIntVec3 myIntVec3 = new CesiumIntVec3((SByte)1, (SByte)2, (SByte)3);
            myIntVec3 = new CesiumIntVec3((Int16)1, (Int16)2, (Int16)3);
            myIntVec3 = new CesiumIntVec3((Int32)1, (Int32)2, (Int32)3);
            myIntVec3 = new CesiumIntVec3((Int64)1, (Int64)2, (Int64)3);
            CesiumIntVec4 myIntVec4 = new CesiumIntVec4((SByte)1, (SByte)2, (SByte)3, (SByte)4);
            myIntVec4 = new CesiumIntVec4((Int16)1, (Int16)2, (Int16)3, (Int16)4);
            myIntVec4 = new CesiumIntVec4((Int32)1, (Int32)2, (Int32)3, (Int32)4);
            myIntVec4 = new CesiumIntVec4((Int64)1, (Int64)2, (Int64)3, (Int64)4);
            myObject = myIntVec2[0];
            myObject = myIntVec3[0];
            myObject = myIntVec4[0];
            CesiumUintVec2 myUintVec2 = new CesiumUintVec2((Byte)1, (Byte)2);
            myUintVec2 = new CesiumUintVec2((UInt16)1, (UInt16)2);
            myUintVec2 = new CesiumUintVec2((UInt32)1, (UInt32)2);
            myUintVec2 = new CesiumUintVec2((UInt64)1, (UInt64)2);
            CesiumUintVec3 myUintVec3 = new CesiumUintVec3((Byte)1, (Byte)2, (Byte)3);
            myUintVec3 = new CesiumUintVec3((UInt16)1, (UInt16)2, (UInt16)3);
            myUintVec3 = new CesiumUintVec3((UInt32)1, (UInt32)2, (UInt32)3);
            myUintVec3 = new CesiumUintVec3((UInt64)1, (UInt64)2, (UInt64)3);
            CesiumUintVec4 myUintVec4 = new CesiumUintVec4((Byte)1, (Byte)2, (Byte)3, (Byte)4);
            myUintVec4 = new CesiumUintVec4((UInt16)1, (UInt16)2, (UInt16)3, (UInt16)4);
            myUintVec4 = new CesiumUintVec4((UInt32)1, (UInt32)2, (UInt32)3, (UInt32)4);
            myUintVec4 = new CesiumUintVec4((UInt64)1, (UInt64)2, (UInt64)3, (UInt64)4);
            myObject = myUintVec2[0];
            myObject = myUintVec3[0];
            myObject = myUintVec4[0];
            CesiumIntMat2x2 myIntMat2 = new CesiumIntMat2x2(myIntVec2, myIntVec2);
            CesiumIntMat3x3 myIntMat3 = new CesiumIntMat3x3(myIntVec3, myIntVec3, myIntVec3);
            CesiumIntMat4x4 myIntMat4 = new CesiumIntMat4x4(myIntVec4, myIntVec4, myIntVec4, myIntVec4);
            myObject = myIntMat2[0];
            myObject = myIntMat3[0];
            myObject = myIntMat4[0];
            CesiumUintMat2x2 myUintMat2 = new CesiumUintMat2x2(myUintVec2, myUintVec2);
            CesiumUintMat3x3 myUintMat3 = new CesiumUintMat3x3(myUintVec3, myUintVec3, myUintVec3);
            CesiumUintMat4x4 myUintMat4 = new CesiumUintMat4x4(myUintVec4, myUintVec4, myUintVec4, myUintVec4);
            myObject = myUintMat2[0];
            myObject = myUintMat3[0];
            myObject = myUintMat4[0];

            int2 myInt2 = new int2(1, 2);
            int3 myInt3 = new int3(1, 2, 3);
            int4 myInt4 = new int4(1, 2, 3, 4);
            uint2 myUint2 = new uint2(1, 2);
            uint3 myUint3 = new uint3(1, 2, 3);
            uint4 myUint4 = new uint4(1, 2, 3, 4);
            float2 myFloat2 = new float2(1, 2);
            float3 myFloat3 = new float3(1, 2, 3);
            float4 myFloat4 = new float4(1, 2, 3, 4);
            myObject = myFloat2[0];
            myObject = myFloat3[0];
            myObject = myFloat4[0];
            double2 myDouble2 = new double2(1, 2);
            double3 myDouble3 = new double3(1, 2, 3);
            double4 myDouble4 = new double4(1, 2, 3, 4);
            myObject = myDouble2[0];
            myObject = myDouble3[0];
            myObject = myDouble4[0];
            int2x2 myInt2x2 = new int2x2(myInt2, myInt2);
            int3x3 myInt3x3 = new int3x3(myInt3, myInt3, myInt3);
            int4x4 myInt4x4 = new int4x4(myInt4, myInt4, myInt4, myInt4);
            uint2x2 myUint2x2 = new uint2x2(myUint2, myUint2);
            uint3x3 myUint3x3 = new uint3x3(myUint3, myUint3, myUint3);
            uint4x4 myUint4x4 = new uint4x4(myUint4, myUint4, myUint4, myUint4);
            float2x2 myFloat2x2 = new float2x2(myFloat2, myFloat2);
            float3x3 myFloat3x3 = new float3x3(myFloat3, myFloat3, myFloat3);
            float4x4 myFloat4x4 = new float4x4(myFloat4, myFloat4, myFloat4, myFloat4);
            myObject = myFloat2x2[0];
            myObject = myFloat3x3[0];
            myObject = myFloat4x4[0];
            double2x2 myDouble2x2 = new double2x2(myDouble2, myDouble2);
            double3x3 myDouble3x3 = new double3x3(myDouble3, myDouble3, myDouble3);
            double4x4 myDouble4x4 = new double4x4(myDouble4, myDouble4, myDouble4, myDouble4);
            myObject = myDouble2x2[0];
            myObject = myDouble3x3[0];
            myObject = myDouble4x4[0];

            CesiumMetadataValueType valueType = new CesiumMetadataValueType(
                CesiumMetadataType.Invalid,
                CesiumMetadataComponentType.None,
                false);
            valueType.type = CesiumMetadataType.Invalid;
            valueType.componentType = CesiumMetadataComponentType.None;
            valueType.isArray = false;

            CesiumPropertyArray array = new CesiumPropertyArray();
            array.elementValueType = new CesiumMetadataValueType();
            array.values = new CesiumMetadataValue[10];
            array.values[0] = new CesiumMetadataValue();
            length = array.values.Length;

            CesiumMetadataValue myValue = new CesiumMetadataValue();
            myValue.SetObjectValue(false);
            myValue.SetObjectValue((SByte)0);
            myValue.SetObjectValue((Byte)0);
            myValue.SetObjectValue((Int16)0);
            myValue.SetObjectValue((UInt16)0);
            myValue.SetObjectValue((Int32)0);
            myValue.SetObjectValue((UInt32)0);
            myValue.SetObjectValue((Int64)0);
            myValue.SetObjectValue((UInt64)0);
            myValue.SetObjectValue(0.0f);
            myValue.SetObjectValue(0.0);
            myValue.SetObjectValue(myIntVec2);
            myValue.SetObjectValue(myIntVec3);
            myValue.SetObjectValue(myIntVec4);
            myValue.SetObjectValue(myUintVec2);
            myValue.SetObjectValue(myUintVec3);
            myValue.SetObjectValue(myUintVec4);
            myValue.SetObjectValue(myFloat2);
            myValue.SetObjectValue(myFloat3);
            myValue.SetObjectValue(myFloat4);
            myValue.SetObjectValue(myDouble2);
            myValue.SetObjectValue(myDouble3);
            myValue.SetObjectValue(myDouble4);
            myValue.SetObjectValue(myIntMat2);
            myValue.SetObjectValue(myIntMat3);
            myValue.SetObjectValue(myIntMat4);
            myValue.SetObjectValue(myUintMat2);
            myValue.SetObjectValue(myUintMat3);
            myValue.SetObjectValue(myUintMat4);
            myValue.SetObjectValue(myFloat2x2);
            myValue.SetObjectValue(myFloat3x3);
            myValue.SetObjectValue(myFloat4x4);
            myValue.SetObjectValue(myDouble2x2);
            myValue.SetObjectValue(myDouble3x3);
            myValue.SetObjectValue(myDouble4x4);
            myValue.SetObjectValue("test");
            myValue.SetObjectValue(array);
            valueType = myValue.valueType;

            myObject = myValue.objectValue;
            CesiumMetadataValue.GetObjectAsBoolean(myObject);
            CesiumMetadataValue.GetObjectAsSByte(myObject);
            CesiumMetadataValue.GetObjectAsByte(myObject);
            CesiumMetadataValue.GetObjectAsInt16(myObject);
            CesiumMetadataValue.GetObjectAsUInt16(myObject);
            CesiumMetadataValue.GetObjectAsInt32(myObject);
            CesiumMetadataValue.GetObjectAsUInt32(myObject);
            CesiumMetadataValue.GetObjectAsInt64(myObject);
            CesiumMetadataValue.GetObjectAsUInt64(myObject);
            CesiumMetadataValue.GetObjectAsFloat(myObject);
            CesiumMetadataValue.GetObjectAsDouble(myObject);
            CesiumMetadataValue.GetObjectAsCesiumIntVec2(myObject);
            CesiumMetadataValue.GetObjectAsCesiumIntVec3(myObject);
            CesiumMetadataValue.GetObjectAsCesiumIntVec4(myObject);
            CesiumMetadataValue.GetObjectAsCesiumUintVec2(myObject);
            CesiumMetadataValue.GetObjectAsCesiumUintVec3(myObject);
            CesiumMetadataValue.GetObjectAsCesiumUintVec4(myObject);
            CesiumMetadataValue.GetObjectAsFloat2(myObject);
            CesiumMetadataValue.GetObjectAsFloat3(myObject);
            CesiumMetadataValue.GetObjectAsFloat4(myObject);
            CesiumMetadataValue.GetObjectAsDouble2(myObject);
            CesiumMetadataValue.GetObjectAsDouble3(myObject);
            CesiumMetadataValue.GetObjectAsDouble4(myObject);
            CesiumMetadataValue.GetObjectAsCesiumIntMat2x2(myObject);
            CesiumMetadataValue.GetObjectAsCesiumIntMat3x3(myObject);
            CesiumMetadataValue.GetObjectAsCesiumIntMat4x4(myObject);
            CesiumMetadataValue.GetObjectAsCesiumUintMat2x2(myObject);
            CesiumMetadataValue.GetObjectAsCesiumUintMat3x3(myObject);
            CesiumMetadataValue.GetObjectAsCesiumUintMat4x4(myObject);
            CesiumMetadataValue.GetObjectAsFloat2x2(myObject);
            CesiumMetadataValue.GetObjectAsFloat3x3(myObject);
            CesiumMetadataValue.GetObjectAsFloat4x4(myObject);
            CesiumMetadataValue.GetObjectAsDouble2x2(myObject);
            CesiumMetadataValue.GetObjectAsDouble3x3(myObject);
            CesiumMetadataValue.GetObjectAsDouble4x4(myObject);
            CesiumMetadataValue.GetObjectAsString(myObject);

            myValue.GetBoolean();
            myValue.GetSByte();
            myValue.GetByte();
            myValue.GetInt16();
            myValue.GetUInt16();
            myValue.GetInt32();
            myValue.GetUInt32();
            myValue.GetInt64();
            myValue.GetUInt64();
            myValue.GetFloat();
            myValue.GetDouble();
            myValue.GetString();
            myValue.GetArray();

            CesiumPrimitiveFeatures primitiveFeatures = go.AddComponent<CesiumPrimitiveFeatures>();
            primitiveFeatures = go.GetComponent<CesiumPrimitiveFeatures>();
            CesiumFeatureIdSet[] sets = primitiveFeatures.featureIdSets;
            sets = new CesiumFeatureIdSet[10];
            sets[0] = new CesiumFeatureIdSet();
            sets[0] = new CesiumFeatureIdSet(1);
            sets[0].featureCount = 1;
            sets[0].label = "label";
            sets[0].nullFeatureId = 0;
            sets[0].propertyTableIndex = 0;
            sets[0].Dispose();

            CesiumFeatureIdAttribute featureIdAttribute = new CesiumFeatureIdAttribute();
            featureIdAttribute.status = featureIdAttribute.status;
            featureIdAttribute.featureCount = 1;
            featureIdAttribute.label = "label";
            featureIdAttribute.nullFeatureId = 0;
            featureIdAttribute.propertyTableIndex = 0;

            CesiumFeatureIdTexture featureIdTexture = new CesiumFeatureIdTexture();
            featureIdTexture.status = featureIdTexture.status;
            featureIdTexture.featureCount = 1;
            featureIdTexture.label = "label";
            featureIdTexture.nullFeatureId = 0;
            featureIdTexture.propertyTableIndex = 0;

            primitiveFeatures.featureIdSets[0] = featureIdAttribute;
            primitiveFeatures.featureIdSets[1] = featureIdTexture;

            CesiumModelMetadata modelMetadata = go.AddComponent<CesiumModelMetadata>();
            modelMetadata = go.GetComponent<CesiumModelMetadata>();
            modelMetadata.propertyTables = modelMetadata.propertyTables;
            modelMetadata.propertyTables[0] = modelMetadata.propertyTables[0];
            length = modelMetadata.propertyTables.Length;

            CesiumPropertyTable propertyTable = new CesiumPropertyTable();
            propertyTable.status = CesiumPropertyTableStatus.Valid;
            propertyTable.name = "";
            propertyTable.count = 0;
            propertyTable.properties = propertyTable.properties;
            propertyTable.properties = new Dictionary<String, CesiumPropertyTableProperty>(10);
            propertyTable.properties.Add("Test", new CesiumPropertyTableProperty());
            propertyTable.DisposeProperties();

            CesiumPropertyTableProperty property = new CesiumPropertyTableProperty();
            property.status = property.status;
            property.size = property.size;
            property.arraySize = property.arraySize;
            property.isNormalized = property.isNormalized;
            property.offset = myValue;
            property.scale = myValue;
            property.min = myValue;
            property.max = myValue;
            property.noData = myValue;
            property.defaultValue = myValue;
            property.valueType = property.valueType;

            RaycastHit hitInfo = new RaycastHit();
            int triangleIndex = hitInfo.triangleIndex;
            Vector3 coordinate = hitInfo.barycentricCoordinate;
            Vector2 textureCoordinate = new Vector2();
            textureCoordinate.x = textureCoordinate.y;

            CesiumIonServer server = CesiumIonServer.defaultServer;
            server.serverUrl = "";
            server.apiUrl = "";
            server.oauth2ApplicationID = 1;
            server.defaultIonAccessToken = "";
            server.defaultIonAccessTokenId = "";
            server.serverUrlThatIsLoadingApiUrl = "";

            CesiumCartographicPolygon polygon = go.GetComponent<CesiumCartographicPolygon>();
            polygon.enabled = polygon.enabled;

            List<double2> points = polygon.GetCartographicPoints(m);
            len = points.Count;
            myDouble2 = points[0];

            CesiumPolygonRasterOverlay polygonRasterOverlay = go.GetComponent<CesiumPolygonRasterOverlay>();
            List<CesiumCartographicPolygon> polygons = polygonRasterOverlay.polygons;
            polygonRasterOverlay.excludeSelectedTiles = polygonRasterOverlay.excludeSelectedTiles;
            polygonRasterOverlay.invertSelection = polygonRasterOverlay.invertSelection;
            polygon = polygons[0];
            len = polygons.Count;

            TestGltfModel testModel = new TestGltfModel();

            bool[] boolArray = { };
            UInt16[] uint16Array = { };
            int[] intArray = { };
            double[] doubleArray = { };
            float2[] float2Array = { };
            float3[] float3Array = { };
            float4[] float4Array = { };
            float2x2[] float2x2Array = { };
            float3x3[] float3x3Array = { };
            float4x4[] float4x4Array = { };

            bool boolValue = boolArray[0];
            UInt16 uint16Value = uint16Array[0];
            int intValue = intArray[0];
            double doubleValue = doubleArray[0];
            myFloat2 = float2Array[0];
            myFloat3 = float3Array[0];
            myFloat4 = float4Array[0];
            myFloat2x2 = float2x2Array[0];
            myFloat3x3 = float3x3Array[0];
            myFloat4x4 = float4x4Array[0];

            length = boolArray.Length;
            length = uint16Array.Length;
            length = intArray.Length;
            length = doubleArray.Length;
            length = stringArray.Length;
            length = float2Array.Length;
            length = float3Array.Length;
            length = float4Array.Length;
            length = float2x2Array.Length;
            length = float3x3Array.Length;
            length = float4x4Array.Length;

#if UNITY_EDITOR
            SceneView sv = SceneView.lastActiveSceneView;
            sv.pivot = sv.pivot;
            sv.rotation = sv.rotation;
            Camera svc = sv.camera;
            svc.transform.SetPositionAndRotation(p, q);

            bool isPlaying = EditorApplication.isPlaying;
            EditorApplication.update += () => { };

            EditorUtility.SetDirty(null);
#endif
        }
    }
}
