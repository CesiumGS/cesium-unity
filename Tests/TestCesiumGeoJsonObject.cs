using NUnit.Framework;
using CesiumForUnity;
using System.Collections.Generic;

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

    #region Child Access Tests

    [Test]
    public void GetChildReturnsValidFeature()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject child = root.GetChild(0);

        Assert.IsNotNull(child);
        Assert.IsTrue(child.IsValid());
        Assert.AreEqual(CesiumGeoJsonObjectType.Feature, child.GetObjectType());
    }

    [Test]
    public void GetChildWithInvalidIndexReturnsNull()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject child = root.GetChild(100);

        Assert.IsNull(child);
    }

    [Test]
    public void GetChildWithNegativeIndexReturnsNull()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject child = root.GetChild(-1);

        Assert.IsNull(child);
    }

    [Test]
    public void GetChildrenReturnsAllChildren()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        List<CesiumGeoJsonObject> children = root.GetChildren();

        Assert.AreEqual(2, children.Count);
        foreach (var child in children)
        {
            Assert.IsNotNull(child);
            Assert.IsTrue(child.IsValid());
        }
    }

    #endregion

    #region Feature ID Tests

    [Test]
    public void FeatureWithStringIdHasFeatureId()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.IsTrue(feature.HasFeatureId());
        Assert.IsTrue(feature.HasFeatureIdString());
    }

    [Test]
    public void FeatureWithStringIdReturnsCorrectId()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.AreEqual("feature-string-id", feature.GetFeatureIdString());
    }

    [Test]
    public void FeatureWithIntIdHasFeatureId()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(1);

        Assert.IsTrue(feature.HasFeatureId());
        Assert.IsFalse(feature.HasFeatureIdString());
    }

    [Test]
    public void FeatureWithIntIdReturnsCorrectId()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(1);

        Assert.AreEqual(999, feature.GetFeatureIdInt());
    }

    [Test]
    public void FeatureWithNoIdReturnsFalse()
    {
        CesiumGeoJsonObject root = _featureNoIdDoc.GetRootObject();

        Assert.IsFalse(root.HasFeatureId());
    }

    #endregion

    #region Property Tests

    [Test]
    public void HasPropertyReturnsTrueForExistingProperty()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.IsTrue(feature.HasProperty("name"));
        Assert.IsTrue(feature.HasProperty("count"));
        Assert.IsTrue(feature.HasProperty("rating"));
    }

    [Test]
    public void HasPropertyReturnsFalseForNonExistentProperty()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.IsFalse(feature.HasProperty("nonexistent"));
    }

    [Test]
    public void GetStringPropertyReturnsCorrectValue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.AreEqual("Test Feature", feature.GetStringProperty("name"));
    }

    [Test]
    public void GetStringPropertyReturnsEmptyForNonExistent()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.AreEqual("", feature.GetStringProperty("nonexistent"));
    }

    [Test]
    public void GetNumericPropertyReturnsCorrectIntValue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.AreEqual(42.0, feature.GetNumericProperty("count"), 0.001);
    }

    [Test]
    public void GetNumericPropertyReturnsCorrectDoubleValue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.AreEqual(4.5, feature.GetNumericProperty("rating"), 0.001);
    }

    [Test]
    public void GetNumericPropertyReturnsZeroForNonExistent()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.AreEqual(0.0, feature.GetNumericProperty("nonexistent"), 0.001);
    }

    [Test]
    public void GetPropertiesAsJsonReturnsValidJson()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

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
        CesiumGeoJsonObject feature = root.GetChild(0);

        Assert.IsFalse(feature.HasStyle());
    }

    [Test]
    public void SetStyleMakesHasStyleReturnTrue()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

        CesiumVectorStyle style = CesiumVectorStyle.Default;
        feature.SetStyle(style);

        Assert.IsTrue(feature.HasStyle());
    }

    [Test]
    public void SetStyleWithCustomColorPreservesColor()
    {
        CesiumGeoJsonObject root = _featureCollectionDoc.GetRootObject();
        CesiumGeoJsonObject feature = root.GetChild(0);

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
        CesiumGeoJsonObject feature = root.GetChild(0);

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
        CesiumGeoJsonObject feature1 = root.GetChild(0);
        CesiumGeoJsonObject feature2 = root.GetChild(1);

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
        CesiumGeoJsonObject feature = root.GetChild(0);

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
        CesiumGeoJsonObject feature = root.GetChild(0);

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
