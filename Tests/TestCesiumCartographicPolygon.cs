using CesiumForUnity;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if SUPPORTS_SPLINES
using UnityEngine.Splines;
#endif

public class TestCesiumCartographicPolygon
{
#if SUPPORTS_SPLINES
    [Test]
    public void GetCartographicPoints()
    {
        GameObject go = new GameObject("Game Object");

        CesiumGeoreference georeference = go.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(12.0, 23.0, 456.0);

        CesiumCartographicPolygon polygonComponent = go.AddComponent<CesiumCartographicPolygon>();
        SplineContainer splineContainer = go.GetComponent<SplineContainer>();

        // Remove existing spline(s).
        IReadOnlyList<Spline> splines = splineContainer.Splines;
        for (int i = splines.Count - 1; i >= 0; i--)
        {
            splineContainer.RemoveSpline(splines[i]);
        }

        // Add a new spline.
        Spline defaultSpline = new Spline();

        BezierKnot[] knots = new BezierKnot[] {
            new BezierKnot(new float3(-100.0f, 0f, -100.0f)),
            new BezierKnot(new float3(100.0f, 0f, -100.0f)),
            new BezierKnot(new float3(100.0f, 0f, 100.0f)),
            new BezierKnot(new float3(-100.0f, 0f, 100.0f)),
        };

        defaultSpline.Knots = knots;
        defaultSpline.Closed = true;
        defaultSpline.SetTangentMode(TangentMode.Linear);

        splineContainer.AddSpline(defaultSpline);

        List<double2> cartographicPoints = polygonComponent.GetCartographicPoints(Matrix4x4.identity);
        Assert.AreEqual(cartographicPoints.Count, 4);

        // All points are near the georeference origin
        Assert.AreEqual(cartographicPoints[0].x, 12.0, 0.01);
        Assert.AreEqual(cartographicPoints[0].y, 23.0, 0.01);
        Assert.AreEqual(cartographicPoints[1].x, 12.0, 0.01);
        Assert.AreEqual(cartographicPoints[1].y, 23.0, 0.01);
        Assert.AreEqual(cartographicPoints[2].x, 12.0, 0.01);
        Assert.AreEqual(cartographicPoints[2].y, 23.0, 0.01);
        Assert.AreEqual(cartographicPoints[3].x, 12.0, 0.01);
        Assert.AreEqual(cartographicPoints[3].y, 23.0, 0.01);
    }
#endif
}