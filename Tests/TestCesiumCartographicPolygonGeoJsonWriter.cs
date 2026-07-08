using CesiumForUnity;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR && SUPPORTS_SPLINES
using UnityEngine.Splines;

public class TestCesiumCartographicPolygonGeoJsonWriter
{
    [Test]
    public void SaveAsGeoJsonWritesFeatureCollection()
    {
        GameObject go = new GameObject("GeoRef");
        go.AddComponent<CesiumGeoreference>();
        
        GameObject poly = new GameObject("Polygon");
        poly.transform.SetParent(go.transform);
        CesiumCartographicPolygon polygon = poly.AddComponent<CesiumCartographicPolygon>();

        SplineContainer splineContainer = poly.GetComponent<SplineContainer>(); // Must add automatically through RequireComponent
        IReadOnlyList<Spline> splines = splineContainer.Splines;
        for (int i = splines.Count - 1; i >= 0; i--)
            splineContainer.RemoveSpline(splines[i]);

        Spline spline = new Spline();
        spline.Knots = new BezierKnot[] {
            new BezierKnot(new float3(-100.0f, 0f, -100.0f)),
            new BezierKnot(new float3(100.0f, 0f, -100.0f)),
            new BezierKnot(new float3(100.0f, 0f, 100.0f)),
            new BezierKnot(new float3(-100.0f, 0f, 100.0f)),
        };
        spline.Closed = true;
        spline.SetTangentMode(TangentMode.Linear);
        splineContainer.AddSpline(spline);

        string path = Path.Combine(Path.GetTempPath(), "CesiumTestPolygon.geojson");
        try
        {
            Assert.IsTrue(CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJson(polygon, path));
            Assert.IsTrue(File.Exists(path));

            string contents = File.ReadAllText(path);
            // Round-trip through the parser to confirm it's valid GeoJSON and the
            // outer ring was written with the first point duplicated as the closing point.
            CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(contents);
            Assert.IsNotNull(document);
            Assert.IsTrue(document.IsValid());

            CesiumGeoJsonObject root = document.GetRootObject();
            Assert.AreEqual(CesiumGeoJsonObjectType.FeatureCollection, root.GetObjectType());

            CesiumGeoJsonFeature[] features = root.GetObjectAsFeatureCollection();
            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Length);

            CesiumGeoJsonObject geometry = features[0].GetGeometry();
            Assert.AreEqual(CesiumGeoJsonObjectType.Polygon, geometry.GetObjectType());

            CesiumGeoJsonPolygon testPoly = geometry.GetObjectAsPolygon();
            Assert.IsNotNull(testPoly);
            Assert.IsNotNull(testPoly.rings);
            Assert.AreEqual(1, testPoly.rings.Length);
            Assert.AreEqual(5, testPoly.rings[0].points.Length);
            Assert.AreEqual(testPoly.rings[0].points[0], testPoly.rings[0].points[^1]); // First and Last element must match!
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Test]
    public void SaveAsGeoJsonNullPolygonReturnsFalse()
    {
        string path = Path.Combine(Path.GetTempPath(), "CesiumTestPolygon.geojson");
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error,"CesiumCartographicPolygonGeoJsonWriter: polygon is null.");
        Assert.IsFalse(CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJson(null, path));
    }

    [Test]
    public void SaveAsGeoJsonEmptyPathReturnsFalse()
    {
        GameObject go = new GameObject("MyPolygon");
        go.AddComponent<CesiumGeoreference>();
        CesiumCartographicPolygon polygon = go.AddComponent<CesiumCartographicPolygon>();

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "CesiumCartographicPolygonGeoJsonWriter: filePath is null or empty.");
        Assert.IsFalse(CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJson(polygon, ""));

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "CesiumCartographicPolygonGeoJsonWriter: filePath is null or empty.");
        Assert.IsFalse(CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJson(polygon, null));
    }

    [Test]
    public void SaveAsGeoJsonCoordinatesUseInvariantCulture()
    {
        // A polygon whose points carry decimal coordinates: the writer must emit
        // '.' as the decimal separator regardless of the machine's culture.
        CultureInfo previous = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE"); // de-DE locale uses ',' as decimal seperator.

            GameObject go = new GameObject("TestPolygon");
            go.AddComponent<CesiumGeoreference>();
            CesiumCartographicPolygon polygon = go.AddComponent<CesiumCartographicPolygon>();

            SplineContainer splineContainer = go.GetComponent<SplineContainer>();
            IReadOnlyList<Spline> splines = splineContainer.Splines;
            for (int i = splines.Count - 1; i >= 0; i--)
                splineContainer.RemoveSpline(splines[i]);

            Spline spline = new Spline();
            spline.Knots = new BezierKnot[] {
                new BezierKnot(new float3(0.5f, 0f, 0.25f)),
                new BezierKnot(new float3(1.5f, 0f, 0.25f)),
                new BezierKnot(new float3(1.5f, 0f, 1.25f)),
                new BezierKnot(new float3(0.5f, 0f, 1.25f)),
            };
            spline.Closed = true;
            spline.SetTangentMode(TangentMode.Linear);
            splineContainer.AddSpline(spline);

            string path = Path.Combine(Path.GetTempPath(), "CesiumTestPolygonCulture.geojson");
            try
            {
                Assert.IsTrue(CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJson(polygon, path));
                string contents = File.ReadAllText(path);
                Assert.IsTrue(contents.Contains("TestPolygon"));

                // Matches GeoJSON coordinate pairs and verifies decimals use '.' as the separator.
                const string validationPattern = @"\[-?\d+\.\d+,-?\d+\.\d+\]";
                Assert.IsTrue(Regex.IsMatch(contents, validationPattern));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = previous;
        }
    }
}
#endif