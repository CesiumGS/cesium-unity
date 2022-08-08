using System;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Oxidize;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace CesiumForUnity;

[Oxidize]
internal partial class ConfigureOxidize
{
    public void ExposeToCPP()
    {
        Camera c = Camera.main;
        Transform t = c.transform;
        Vector3 u = t.up;
        Vector3 f = t.forward;

        t.position = new Vector3();
        Vector3 p = t.position;
        float x = p.x;
        float y = p.y;
        float z = p.z;
        c.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
        float fov = c.fieldOfView;
        int pixelHeight = c.pixelHeight;
        int pixelWidth = c.pixelWidth;
        float aspect = c.aspect;
        //IFormattable f = new Vector3();
        //IEquatable<Vector3> f2 = new Vector3();

        SceneView sv = SceneView.lastActiveSceneView;
        Camera svc = sv.camera;

        GameObject go = new GameObject();
        go = new GameObject("name");
        go.SetActive(go.activeSelf);
        Transform transform = go.transform;
        transform.parent = transform.parent;
        transform.position = transform.position;
        transform.rotation = transform.rotation;
        transform.localScale = transform.localScale;
        Matrix4x4 m = transform.localToWorldMatrix;

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, false, false);
        texture2D.LoadRawTextureData(IntPtr.Zero, 0);
        texture2D.Apply(true, true);
        Texture texture = texture2D;

        Mesh mesh = new Mesh();
        mesh.SetVertices(new NativeArray<Vector3>());
        mesh.SetNormals(new NativeArray<Vector3>());
        mesh.SetUVs(0, new NativeArray<Vector2>());
        mesh.SetIndices(new NativeArray<int>(), MeshTopology.Triangles, 0, true, 0);

        Debug.Log("Logging");

        MeshRenderer meshRenderer = new MeshRenderer();
        GameObject meshGameObject = meshRenderer.gameObject;
        meshRenderer.material = meshRenderer.material;
        meshRenderer.material.SetTexture("name", texture2D);

        MeshFilter meshFilter = new MeshFilter();
        meshFilter.mesh = mesh;

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

        Marshal.FreeCoTaskMem(Marshal.StringToCoTaskMemUTF8("hi"));

        UnityWebRequest request = UnityWebRequest.Get("url");
        bool isDone = request.isDone;
        string e = request.error;
        string method = request.method;
        string url = request.url;
        request.downloadHandler = new NativeDownloadHandler();
        request.SetRequestHeader("name", "value");
        request.GetResponseHeader("name");
        long responseCode = request.responseCode;
        UnityWebRequestAsyncOperation op = request.SendWebRequest();
        op.completed += o => {};

        Task.Run(() => { });

    }
}
