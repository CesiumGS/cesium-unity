using UnityEngine;
using UnityEditor;

namespace TestOxidize;

public partial class Oxidize
{
    partial void Initialize();

    public void BindCamera()
    {
        var c = Camera.main;
        var t = c.transform;
        var p = t.position;
        c.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
        t.position = new Vector3();
        IFormattable f = new Vector3();
        IEquatable<Vector3> f2 = new Vector3();
    }

    public void BindSceneView()
    {
        var sv = SceneView.lastActiveSceneView;
    }
}
