using UnityEngine;
using UnityEditor;
using Oxidize;

namespace TestOxidize;

[Oxidize]
public partial class ConfigureOxidize
{
    public void ExposeToCPP()
    {
        var c = Camera.main;
        var t = c.transform;
        var u = t.up;

        var p = t.position;
        var x = p.x;
        var y = p.y;
        var z = p.z;
        c.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
        float fov = c.fieldOfView;
        int pixelHeight = c.pixelHeight;
        int pixelWidth = c.pixelWidth;
        float aspect = c.aspect;
        t.position = new Vector3();
        //IFormattable f = new Vector3();
        //IEquatable<Vector3> f2 = new Vector3();

        var sv = SceneView.lastActiveSceneView;
        var svc = sv.camera;

        var go = new GameObject();
    }
}
