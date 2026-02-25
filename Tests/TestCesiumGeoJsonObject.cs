using NUnit.Framework;
using CesiumForUnity;

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
    public void NewFeatureHasNoStyle()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        Assert.IsFalse(feature.HasStyle());
    }

    [Test]
    public void SetStyleMakesHasStyleReturnTrue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        CesiumVectorStyle style = CesiumVectorStyle.Default;
        feature.SetStyle(style);

        Assert.IsTrue(feature.HasStyle());
    }

    [Test]
    public void SetStyleWithCustomColorPreservesColor()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature feature = root.GetObjectAsFeatureCollection()[0];

        CesiumVectorStyle style = new CesiumVectorStyle();
        style.polygonStyle.fill = true;
        style.polygonStyle.fillStyle.color = new CesiumColor32(255, 0, 0, 255);
        style.polygonStyle.fillStyle.colorMode = CesiumVectorColorMode.Normal;

        feature.SetStyle(style);

        CesiumVectorStyle retrievedStyle = feature.GetStyle();

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

        CesiumVectorStyle style = CesiumVectorStyle.Default;
        feature.SetStyle(style);
        Assert.IsTrue(feature.HasStyle());

        feature.ClearStyle();
        Assert.IsFalse(feature.HasStyle());
    }

    [Test]
    public void DifferentFeaturesCanHaveDifferentStyles()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonFeature[] features = root.GetObjectAsFeatureCollection();
        CesiumGeoJsonFeature feature1 = features[0];
        CesiumGeoJsonFeature feature2 = features[1];

        // Set red style on feature 1
        CesiumVectorStyle redStyle = new CesiumVectorStyle();
        redStyle.polygonStyle.fill = true;
        redStyle.polygonStyle.fillStyle.color = new CesiumColor32(255, 0, 0, 255);
        feature1.SetStyle(redStyle);

        // Set blue style on feature 2
        CesiumVectorStyle blueStyle = new CesiumVectorStyle();
        blueStyle.polygonStyle.fill = true;
        blueStyle.polygonStyle.fillStyle.color = new CesiumColor32(0, 0, 255, 255);
        feature2.SetStyle(blueStyle);

        // Verify each has its own style
        CesiumVectorStyle style1 = feature1.GetStyle();
        CesiumVectorStyle style2 = feature2.GetStyle();

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

        CesiumVectorStyle style = new CesiumVectorStyle();
        style.lineStyle.color = new CesiumColor32(0, 255, 0, 200);
        style.lineStyle.width = 5.0;
        style.lineStyle.widthMode = CesiumVectorLineWidthMode.Meters;
        style.lineStyle.colorMode = CesiumVectorColorMode.Normal;

        feature.SetStyle(style);

        CesiumVectorStyle retrievedStyle = feature.GetStyle();

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

        CesiumVectorStyle style = new CesiumVectorStyle();
        style.polygonStyle.fill = true;
        style.polygonStyle.fillStyle.color = new CesiumColor32(100, 100, 100, 255);
        style.polygonStyle.outline = true;
        style.polygonStyle.outlineStyle.color = new CesiumColor32(255, 255, 0, 255);
        style.polygonStyle.outlineStyle.width = 3.0;

        feature.SetStyle(style);

        CesiumVectorStyle retrievedStyle = feature.GetStyle();

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
}
