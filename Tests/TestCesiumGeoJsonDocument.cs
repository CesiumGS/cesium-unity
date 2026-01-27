using NUnit.Framework;
using CesiumForUnity;

public class TestCesiumGeoJsonDocument
{
    // Sample GeoJSON strings for testing
    private const string ValidFeatureCollectionJson = @"{
        ""type"": ""FeatureCollection"",
        ""features"": [
            {
                ""type"": ""Feature"",
                ""id"": ""feature1"",
                ""properties"": {
                    ""name"": ""Test Polygon"",
                    ""value"": 42.5
                },
                ""geometry"": {
                    ""type"": ""Polygon"",
                    ""coordinates"": [[[0, 0], [1, 0], [1, 1], [0, 1], [0, 0]]]
                }
            },
            {
                ""type"": ""Feature"",
                ""id"": 123,
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

    private const string ValidPointJson = @"{
        ""type"": ""Feature"",
        ""properties"": {},
        ""geometry"": {
            ""type"": ""Point"",
            ""coordinates"": [102.0, 0.5]
        }
    }";

    private const string ValidLineStringJson = @"{
        ""type"": ""Feature"",
        ""properties"": {},
        ""geometry"": {
            ""type"": ""LineString"",
            ""coordinates"": [[102.0, 0.0], [103.0, 1.0], [104.0, 0.0]]
        }
    }";

    private const string InvalidJson = "{ not valid json }}}";

    private const string EmptyFeatureCollectionJson = @"{
        ""type"": ""FeatureCollection"",
        ""features"": []
    }";

    [Test]
    public void ParseValidFeatureCollectionReturnsDocument()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(ValidFeatureCollectionJson);

        Assert.IsNotNull(document);
        Assert.IsTrue(document.IsValid());
    }

    [Test]
    public void ParseValidPointReturnsDocument()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(ValidPointJson);

        Assert.IsNotNull(document);
        Assert.IsTrue(document.IsValid());
    }

    [Test]
    public void ParseValidLineStringReturnsDocument()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(ValidLineStringJson);

        Assert.IsNotNull(document);
        Assert.IsTrue(document.IsValid());
    }

    [Test]
    public void ParseInvalidJsonReturnsNull()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(InvalidJson);

        Assert.IsNull(document);
    }

    [Test]
    public void ParseEmptyStringReturnsNull()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse("");

        Assert.IsNull(document);
    }

    [Test]
    public void ParseNullStringReturnsNull()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(null);

        Assert.IsNull(document);
    }

    [Test]
    public void ParseEmptyFeatureCollectionReturnsValidDocument()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(EmptyFeatureCollectionJson);

        Assert.IsNotNull(document);
        Assert.IsTrue(document.IsValid());
    }

    [Test]
    public void GetRootObjectReturnsValidObject()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(ValidFeatureCollectionJson);

        Assert.IsNotNull(document);

        CesiumGeoJsonObject root = document.GetRootObject();

        Assert.IsNotNull(root);
        Assert.IsTrue(root.IsValid());
    }

    [Test]
    public void RootObjectOfFeatureCollectionHasCorrectType()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(ValidFeatureCollectionJson);
        CesiumGeoJsonObject root = document.GetRootObject();

        Assert.AreEqual(CesiumGeoJsonObjectType.FeatureCollection, root.GetObjectType());
    }

    [Test]
    public void RootObjectOfFeatureHasCorrectType()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(ValidPointJson);
        CesiumGeoJsonObject root = document.GetRootObject();

        Assert.AreEqual(CesiumGeoJsonObjectType.Feature, root.GetObjectType());
    }

    [Test]
    public void FeatureCollectionHasCorrectChildCount()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(ValidFeatureCollectionJson);
        CesiumGeoJsonObject root = document.GetRootObject();

        Assert.AreEqual(2, root.GetChildCount());
    }

    [Test]
    public void EmptyFeatureCollectionHasZeroChildren()
    {
        CesiumGeoJsonDocument document = CesiumGeoJsonDocument.Parse(EmptyFeatureCollectionJson);
        CesiumGeoJsonObject root = document.GetRootObject();

        Assert.AreEqual(0, root.GetChildCount());
    }
}
