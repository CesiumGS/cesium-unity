using UnityEngine;
using UnityEditor;
using Oxidize;

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

        Vector3 p = t.position;
        float x = p.x;
        float y = p.y;
        float z = p.z;
        c.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
        float fov = c.fieldOfView;
        int pixelHeight = c.pixelHeight;
        int pixelWidth = c.pixelWidth;
        float aspect = c.aspect;
        t.position = new Vector3();
        //IFormattable f = new Vector3();
        //IEquatable<Vector3> f2 = new Vector3();

        SceneView sv = SceneView.lastActiveSceneView;
        Camera svc = sv.camera;

        GameObject go = new GameObject();
    }
}
