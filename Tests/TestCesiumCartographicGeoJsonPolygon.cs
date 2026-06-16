using CesiumForUnity;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TestCesiumCartographicGeoJsonPolygon
{
    private const string GeoJson = @"{
    ""type"": ""FeatureCollection"",
    ""name"": ""ACutdownUnder"",
    ""crs"": {
        ""type"": ""name"",
        ""properties"": {
            ""name"": ""urn:ogc:def:crs:OGC:1.3:CRS84""
        }
    },
    ""features"": [
        {
            ""type"": ""Feature"",
            ""properties"": {
                ""id"": null
            },
            ""geometry"": {
                ""type"": ""MultiPolygon"",
                ""coordinates"": [
                    [
                        [
                            [
                                144.955067619588505,
                                -37.820997183519978
                            ],
                            [
                                144.945455376251061,
                                -37.802543659411839
                            ],
                            [
                                144.97583342808133,
                                -37.792888868990701
                            ],
                            [
                                144.972386472015785,
                                -37.816038321301427
                            ],
                            [
                                144.955067619588505,
                                -37.820997183519978
                            ]
                        ]
                    ]
                ]
            }
        }
    ]
}";

    [Test]
    public void GetCartographicPointsReturnsExpectedPoints()
    {
        GameObject go = new GameObject("GeoJson Polygon Test");

        CesiumGeoJsonPolygonOverlay overlay = go.AddComponent<CesiumGeoJsonPolygonOverlay>();
        overlay.document = CesiumGeoJsonDocument.Parse(GeoJson);

        List<double2> points = overlay.GetCartographicPoints(Matrix4x4.identity);

        Assert.GreaterOrEqual(points.Count, 4);

        Assert.AreEqual(144.955067619588505, points[0].x, 1e-6);
        Assert.AreEqual(-37.820997183519978, points[0].y, 1e-6);

        Assert.AreEqual(144.945455376251061, points[1].x, 1e-6);
        Assert.AreEqual(-37.802543659411839, points[1].y, 1e-6);

        Assert.AreEqual(144.97583342808133, points[2].x, 1e-6);
        Assert.AreEqual(-37.792888868990701, points[2].y, 1e-6);

        Assert.AreEqual(144.972386472015785, points[3].x, 1e-6);
        Assert.AreEqual(-37.816038321301427, points[3].y, 1e-6);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void GetCartographicPointsReturnsEmptyForNullDocument()
    {
        GameObject go = new GameObject("GeoJson Polygon Test null");

        CesiumGeoJsonPolygonOverlay overlay = go.AddComponent<CesiumGeoJsonPolygonOverlay>();

        List<double2> points = overlay.GetCartographicPoints(Matrix4x4.identity);

        Assert.AreEqual(0, points.Count);

        Object.DestroyImmediate(go);
    }
}
