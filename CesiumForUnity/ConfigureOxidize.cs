using System;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Oxidize;
using System.Text;

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

        Debug.Log("Logging");

        MeshRenderer meshRenderer = new MeshRenderer();
        meshRenderer.material = meshRenderer.material;
        meshRenderer.material.SetTexture("name", texture2D);

        MeshFilter meshFilter = new MeshFilter();

        Resources.Load<Material>("name");

        byte b;
        unsafe
        {
            string s = Encoding.UTF8.GetString(&b, 0);
        }

        NativeArray<Vector3> nav = new NativeArray<Vector3>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    }
}
