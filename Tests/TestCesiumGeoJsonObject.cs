using NUnit.Framework;
using CesiumForUnity;
using Unity.Mathematics;

public class TestCesiumGeoJsonObject
{
    // Sample GeoJSON strings for testing
    private const string FeatureCollectionWithProperties = @"{
        ""type"": ""FeatureCollection"",
        ""features"": [
            {
                ""type"": ""Feature"",
                ""id"": ""feature-string-id"",
                ""properties"": {
                    ""name"": ""Test Feature"",
                    ""count"": 42,
                    ""rating"": 4.5,
                    ""active"": true
                },
                ""geometry"": {
                    ""type"": ""Polygon"",
                    ""coordinates"": [[[0, 0], [1, 0], [1, 1], [0, 1], [0, 0]]]
                }
            },
            {
                ""type"": ""Feature"",
                ""id"": 999,
                ""properties"": {
                    ""name"": ""Second Feature""
                },
                ""geometry"": {
                    ""type"": ""Polygon"",
                    ""coordinates"": [[[2, 2], [3, 2], [3, 3], [2, 3], [2, 2]]]
                }
            }
        ]
    }";

    private const string FeatureWithNoId = @"{
        ""type"": ""Feature"",
        ""properties"": {
            ""name"": ""No ID Feature""
        },
        ""geometry"": {
            ""type"": ""Point"",
            ""coordinates"": [0, 0]
        }
    }";

    private const string MultiPolygonFeature = @"{
        ""type"": ""Feature"",
        ""properties"": {},
        ""geometry"": {
            ""type"": ""MultiPolygon"",
            ""coordinates"": [
                [[[0, 0], [1, 0], [1, 1], [0, 1], [0, 0]]],
                [[[2, 2], [3, 2], [3, 3], [2, 3], [2, 2]]]
            ]
        }
    }";

    private CesiumGeoJsonDocument _featureCollectionDoc;
    private CesiumGeoJsonDocument _featureNoIdDoc;

    [SetUp]
    public void SetUp()
    {
        _featureCollectionDoc = CesiumGeoJsonDocument.Parse(FeatureCollectionWithProperties);
        _featureNoIdDoc = CesiumGeoJsonDocument.Parse(FeatureWithNoId);
    }

    #region Feature Access Tests

    [Test]
    public void GetObjectAsFeatureCollectionReturnsFeatures()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature[] features = root.GetObjectAsFeatureCollection();

        Assert.IsNotNull(features);
        Assert.AreEqual(2, features.Length);
    }

    [Test]
    public void GetObjectAsFeatureReturnsFeatureForFeatureObject()
    {
        CesiumGeoJsonObject root = _featureNoIdDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeature();

        Assert.IsNotNull(feature);
    }

    [Test]
    public void GetObjectAsFeatureReturnsNullForNonFeature()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeature();

        // Root is a FeatureCollection, not a Feature
        Assert.IsNull(feature);
    }

    [Test]
    public void GetObjectAsFeatureCollectionReturnsNullForNonCollection()
    {
        CesiumGeoJsonObject root = _featureNoIdDoc.GetRootObject();
        CesiumGeoJsonFeature[] features = root.GetObjectAsFeatureCollection();

        // Root is a Feature, not a FeatureCollection
        Assert.IsNull(features);
    }

    #endregion

    #region Feature ID Tests

    [Test]
    public void FeatureWithStringIdHasStringIdType()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.AreEqual(CesiumGeoJsonFeatureIdType.String, feature.GetIdType());
    }

    [Test]
    public void FeatureWithStringIdReturnsCorrectId()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.AreEqual("feature-string-id", feature.GetIdAsString());
    }

    [Test]
    public void FeatureWithIntIdHasIntegerIdType()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[1];

        Assert.AreEqual(CesiumGeoJsonFeatureIdType.Integer, feature.GetIdType());
    }

    [Test]
    public void FeatureWithIntIdReturnsCorrectId()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[1];

        Assert.AreEqual(999, feature.GetIdAsInteger());
    }

    [Test]
    public void FeatureWithNoIdHasNoneIdType()
    {
        CesiumGeoJsonObject root = _featureNoIdDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeature();

        Assert.AreEqual(CesiumGeoJsonFeatureIdType.None, feature.GetIdType());
    }

    #endregion

    #region Property Tests

    [Test]
    public void HasPropertyReturnsTrueForExistingProperty()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.IsTrue(feature.HasProperty("name"));
        Assert.IsTrue(feature.HasProperty("count"));
        Assert.IsTrue(feature.HasProperty("rating"));
    }

    [Test]
    public void HasPropertyReturnsFalseForNonExistentProperty()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.IsFalse(feature.HasProperty("nonexistent"));
    }

    [Test]
    public void GetStringPropertyReturnsCorrectValue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.AreEqual("Test Feature", feature.GetStringProperty("name"));
    }

    [Test]
    public void GetStringPropertyReturnsEmptyForNonExistent()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.AreEqual("", feature.GetStringProperty("nonexistent"));
    }

    [Test]
    public void GetNumericPropertyReturnsCorrectIntValue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.AreEqual(42.0, feature.GetNumericProperty("count"), 0.001);
    }

    [Test]
    public void GetNumericPropertyReturnsCorrectDoubleValue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.AreEqual(4.5, feature.GetNumericProperty("rating"), 0.001);
    }

    [Test]
    public void GetNumericPropertyReturnsZeroForNonExistent()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.AreEqual(0.0, feature.GetNumericProperty("nonexistent"), 0.001);
    }

    [Test]
    public void GetPropertiesAsJsonReturnsValidJson()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        string json = feature.GetPropertiesAsJson();

        Assert.IsNotNull(json);
        Assert.IsNotEmpty(json);
        Assert.IsTrue(json.Contains("name"));
        Assert.IsTrue(json.Contains("Test Feature"));
    }

    #endregion

    #region Style Tests

    [Test]
    public void NewFeatureGeometryHasNoStyle()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];
        CesiumGeoJsonObject geometry = feature.GetGeometry();

        Assert.IsFalse(geometry.HasStyle());
    }

    [Test]
    public void SetStyleMakesHasStyleReturnTrue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];
        CesiumGeoJsonObject geometry = feature.GetGeometry();

        CesiumVectorStyle style = CesiumVectorStyle.Default;
        geometry.SetStyle(style);

        Assert.IsTrue(geometry.HasStyle());
    }

    [Test]
    public void SetStyleWithCustomColorPreservesColor()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];
        CesiumGeoJsonObject geometry = feature.GetGeometry();

        CesiumVectorStyle style = new CesiumVectorStyle();
        style.polygonStyle.fill = true;
        style.polygonStyle.fillStyle.color = new CesiumColor32(255, 0, 0, 255);
        style.polygonStyle.fillStyle.colorMode = CesiumVectorColorMode.Normal;

        geometry.SetStyle(style);

        CesiumVectorStyle retrievedStyle = geometry.GetStyle();

        Assert.AreEqual(255, retrievedStyle.polygonStyle.fillStyle.color.r);
        Assert.AreEqual(0, retrievedStyle.polygonStyle.fillStyle.color.g);
        Assert.AreEqual(0, retrievedStyle.polygonStyle.fillStyle.color.b);
        Assert.AreEqual(255, retrievedStyle.polygonStyle.fillStyle.color.a);
    }

    [Test]
    public void ClearStyleMakesHasStyleReturnFalse()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];
        CesiumGeoJsonObject geometry = feature.GetGeometry();

        CesiumVectorStyle style = CesiumVectorStyle.Default;
        geometry.SetStyle(style);
        Assert.IsTrue(geometry.HasStyle());

        geometry.ClearStyle();
        Assert.IsFalse(geometry.HasStyle());
    }

    [Test]
    public void DifferentFeaturesCanHaveDifferentStyles()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature[] features = root.GetObjectAsFeatureCollection();
        CesiumGeoJsonObject geometry1 = features[0].GetGeometry();
        CesiumGeoJsonObject geometry2 = features[1].GetGeometry();

        // Set red style on feature 1
        CesiumVectorStyle redStyle = new CesiumVectorStyle();
        redStyle.polygonStyle.fill = true;
        redStyle.polygonStyle.fillStyle.color = new CesiumColor32(255, 0, 0, 255);
        geometry1.SetStyle(redStyle);

        // Set blue style on feature 2
        CesiumVectorStyle blueStyle = new CesiumVectorStyle();
        blueStyle.polygonStyle.fill = true;
        blueStyle.polygonStyle.fillStyle.color = new CesiumColor32(0, 0, 255, 255);
        geometry2.SetStyle(blueStyle);

        // Verify each has its own style
        CesiumVectorStyle style1 = geometry1.GetStyle();
        CesiumVectorStyle style2 = geometry2.GetStyle();

        Assert.AreEqual(255, style1.polygonStyle.fillStyle.color.r);
        Assert.AreEqual(0, style1.polygonStyle.fillStyle.color.b);

        Assert.AreEqual(0, style2.polygonStyle.fillStyle.color.r);
        Assert.AreEqual(255, style2.polygonStyle.fillStyle.color.b);
    }

    [Test]
    public void LineStylePropertiesArePreserved()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];
        CesiumGeoJsonObject geometry = feature.GetGeometry();

        CesiumVectorStyle style = new CesiumVectorStyle();
        style.lineStyle.color = new CesiumColor32(0, 255, 0, 200);
        style.lineStyle.width = 5.0;
        style.lineStyle.widthMode = CesiumVectorLineWidthMode.Meters;
        style.lineStyle.colorMode = CesiumVectorColorMode.Normal;

        geometry.SetStyle(style);

        CesiumVectorStyle retrievedStyle = geometry.GetStyle();

        Assert.AreEqual(0, retrievedStyle.lineStyle.color.r);
        Assert.AreEqual(255, retrievedStyle.lineStyle.color.g);
        Assert.AreEqual(0, retrievedStyle.lineStyle.color.b);
        Assert.AreEqual(200, retrievedStyle.lineStyle.color.a);
        Assert.AreEqual(5.0, retrievedStyle.lineStyle.width, 0.001);
        Assert.AreEqual(CesiumVectorLineWidthMode.Meters, retrievedStyle.lineStyle.widthMode);
    }

    [Test]
    public void PolygonOutlineStyleIsPreserved()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];
        CesiumGeoJsonObject geometry = feature.GetGeometry();

        CesiumVectorStyle style = new CesiumVectorStyle();
        style.polygonStyle.fill = true;
        style.polygonStyle.fillStyle.color = new CesiumColor32(100, 100, 100, 255);
        style.polygonStyle.outline = true;
        style.polygonStyle.outlineStyle.color = new CesiumColor32(255, 255, 0, 255);
        style.polygonStyle.outlineStyle.width = 3.0;

        geometry.SetStyle(style);

        CesiumVectorStyle retrievedStyle = geometry.GetStyle();

        Assert.IsTrue(retrievedStyle.polygonStyle.fill);
        Assert.IsTrue(retrievedStyle.polygonStyle.outline);
        Assert.AreEqual(255, retrievedStyle.polygonStyle.outlineStyle.color.r);
        Assert.AreEqual(255, retrievedStyle.polygonStyle.outlineStyle.color.g);
        Assert.AreEqual(0, retrievedStyle.polygonStyle.outlineStyle.color.b);
        Assert.AreEqual(3.0, retrievedStyle.polygonStyle.outlineStyle.width, 0.001);
    }

    #endregion

    #region Object Type Tests

    [Test]
    public void PolygonGeometryHasCorrectType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Polygon"",
            ""coordinates"": [[[0, 0], [1, 0], [1, 1], [0, 1], [0, 0]]]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();

        Assert.AreEqual(CesiumGeoJsonObjectType.Polygon, root.GetObjectType());
    }

    [Test]
    public void MultiPolygonGeometryHasCorrectType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(MultiPolygonFeature);
        CesiumGeoJsonObject root = doc.GetRootObject();

        // The root is a Feature, get its geometry child
        Assert.AreEqual(CesiumGeoJsonObjectType.Feature, root.GetObjectType());
    }

    [Test]
    public void LineStringGeometryHasCorrectType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""LineString"",
            ""coordinates"": [[0, 0], [1, 1], [2, 0]]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();

        Assert.AreEqual(CesiumGeoJsonObjectType.LineString, root.GetObjectType());
    }

    [Test]
    public void PointGeometryHasCorrectType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Point"",
            ""coordinates"": [0, 0]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();

        Assert.AreEqual(CesiumGeoJsonObjectType.Point, root.GetObjectType());
    }

    #endregion

    #region Geometry Subtype Tests

    [Test]
    public void GetObjectAsPointReturnsCorrectCoordinates()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Point"",
            ""coordinates"": [102.0, 0.5, 100.0]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        double3 point = root.GetObjectAsPoint();

        Assert.AreEqual(102.0, point.x, 0.001);
        Assert.AreEqual(0.5, point.y, 0.001);
        Assert.AreEqual(100.0, point.z, 0.001);
    }

    [Test]
    public void GetObjectAsPointReturnsZeroForWrongType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""LineString"",
            ""coordinates"": [[0, 0], [1, 1]]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        double3 point = root.GetObjectAsPoint();

        Assert.AreEqual(0.0, point.x, 0.001);
        Assert.AreEqual(0.0, point.y, 0.001);
        Assert.AreEqual(0.0, point.z, 0.001);
    }

    [Test]
    public void GetObjectAsMultiPointReturnsCorrectCoordinates()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""MultiPoint"",
            ""coordinates"": [[10, 20, 30], [40, 50, 60]]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        double3[] points = root.GetObjectAsMultiPoint();

        Assert.IsNotNull(points);
        Assert.AreEqual(2, points.Length);
        Assert.AreEqual(10.0, points[0].x, 0.001);
        Assert.AreEqual(20.0, points[0].y, 0.001);
        Assert.AreEqual(30.0, points[0].z, 0.001);
        Assert.AreEqual(40.0, points[1].x, 0.001);
        Assert.AreEqual(50.0, points[1].y, 0.001);
        Assert.AreEqual(60.0, points[1].z, 0.001);
    }

    [Test]
    public void GetObjectAsMultiPointReturnsNullForWrongType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Point"",
            ""coordinates"": [0, 0]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        double3[] points = root.GetObjectAsMultiPoint();

        Assert.IsNull(points);
    }

    [Test]
    public void GetObjectAsLineStringReturnsCorrectPoints()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""LineString"",
            ""coordinates"": [[0, 0], [1, 1], [2, 0]]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonLineString lineString = root.GetObjectAsLineString();

        Assert.IsNotNull(lineString);
        Assert.AreEqual(3, lineString.Points.Length);
        Assert.AreEqual(0.0, lineString.Points[0].x, 0.001);
        Assert.AreEqual(0.0, lineString.Points[0].y, 0.001);
        Assert.AreEqual(1.0, lineString.Points[1].x, 0.001);
        Assert.AreEqual(1.0, lineString.Points[1].y, 0.001);
        Assert.AreEqual(2.0, lineString.Points[2].x, 0.001);
        Assert.AreEqual(0.0, lineString.Points[2].y, 0.001);
    }

    [Test]
    public void GetObjectAsLineStringReturnsNullForWrongType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Point"",
            ""coordinates"": [0, 0]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonLineString lineString = root.GetObjectAsLineString();

        Assert.IsNull(lineString);
    }

    [Test]
    public void GetObjectAsMultiLineStringReturnsCorrectLineStrings()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""MultiLineString"",
            ""coordinates"": [
                [[0, 0], [1, 1]],
                [[2, 2], [3, 3], [4, 4]]
            ]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonLineString[] lineStrings = root.GetObjectAsMultiLineString();

        Assert.IsNotNull(lineStrings);
        Assert.AreEqual(2, lineStrings.Length);
        Assert.AreEqual(2, lineStrings[0].Points.Length);
        Assert.AreEqual(3, lineStrings[1].Points.Length);
        Assert.AreEqual(0.0, lineStrings[0].Points[0].x, 0.001);
        Assert.AreEqual(4.0, lineStrings[1].Points[2].x, 0.001);
    }

    [Test]
    public void GetObjectAsMultiLineStringReturnsNullForWrongType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""LineString"",
            ""coordinates"": [[0, 0], [1, 1]]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonLineString[] lineStrings = root.GetObjectAsMultiLineString();

        Assert.IsNull(lineStrings);
    }

    [Test]
    public void GetObjectAsPolygonReturnsCorrectRings()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Polygon"",
            ""coordinates"": [
                [[0, 0], [10, 0], [10, 10], [0, 10], [0, 0]],
                [[2, 2], [8, 2], [8, 8], [2, 8], [2, 2]]
            ]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonPolygon polygon = root.GetObjectAsPolygon();

        Assert.IsNotNull(polygon);

        CesiumGeoJsonLineString[] rings = polygon.Rings;
        Assert.IsNotNull(rings);
        Assert.AreEqual(2, rings.Length);

        // Exterior ring
        Assert.AreEqual(5, rings[0].Points.Length);
        Assert.AreEqual(0.0, rings[0].Points[0].x, 0.001);
        Assert.AreEqual(10.0, rings[0].Points[1].x, 0.001);

        // Interior ring (hole)
        Assert.AreEqual(5, rings[1].Points.Length);
        Assert.AreEqual(2.0, rings[1].Points[0].x, 0.001);
        Assert.AreEqual(8.0, rings[1].Points[1].x, 0.001);
    }

    [Test]
    public void GetObjectAsPolygonReturnsNullForWrongType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Point"",
            ""coordinates"": [0, 0]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonPolygon polygon = root.GetObjectAsPolygon();

        Assert.IsNull(polygon);
    }

    [Test]
    public void GetObjectAsMultiPolygonReturnsCorrectPolygons()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""MultiPolygon"",
            ""coordinates"": [
                [[[0, 0], [1, 0], [1, 1], [0, 1], [0, 0]]],
                [[[2, 2], [3, 2], [3, 3], [2, 3], [2, 2]]]
            ]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonPolygon[] polygons = root.GetObjectAsMultiPolygon();

        Assert.IsNotNull(polygons);
        Assert.AreEqual(2, polygons.Length);

        CesiumGeoJsonLineString[] rings0 = polygons[0].Rings;
        Assert.AreEqual(1, rings0.Length);
        Assert.AreEqual(5, rings0[0].Points.Length);
        Assert.AreEqual(0.0, rings0[0].Points[0].x, 0.001);

        CesiumGeoJsonLineString[] rings1 = polygons[1].Rings;
        Assert.AreEqual(1, rings1.Length);
        Assert.AreEqual(5, rings1[0].Points.Length);
        Assert.AreEqual(2.0, rings1[0].Points[0].x, 0.001);
    }

    [Test]
    public void GetObjectAsMultiPolygonReturnsNullForWrongType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Polygon"",
            ""coordinates"": [[[0, 0], [1, 0], [1, 1], [0, 1], [0, 0]]]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonPolygon[] polygons = root.GetObjectAsMultiPolygon();

        Assert.IsNull(polygons);
    }

    [Test]
    public void GetObjectAsGeometryCollectionReturnsCorrectObjects()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""GeometryCollection"",
            ""geometries"": [
                {
                    ""type"": ""Point"",
                    ""coordinates"": [100.0, 0.0]
                },
                {
                    ""type"": ""LineString"",
                    ""coordinates"": [[101.0, 0.0], [102.0, 1.0]]
                }
            ]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonObject[] geometries = root.GetObjectAsGeometryCollection();

        Assert.IsNotNull(geometries);
        Assert.AreEqual(2, geometries.Length);
        Assert.AreEqual(CesiumGeoJsonObjectType.Point, geometries[0].GetObjectType());
        Assert.AreEqual(CesiumGeoJsonObjectType.LineString, geometries[1].GetObjectType());
    }

    [Test]
    public void GetObjectAsGeometryCollectionReturnsNullForWrongType()
    {
        CesiumGeoJsonDocument doc = CesiumGeoJsonDocument.Parse(@"{
            ""type"": ""Point"",
            ""coordinates"": [0, 0]
        }");

        CesiumGeoJsonObject root = doc.GetRootObject();
        CesiumGeoJsonObject[] geometries = root.GetObjectAsGeometryCollection();

        Assert.IsNull(geometries);
    }

    #endregion
}
