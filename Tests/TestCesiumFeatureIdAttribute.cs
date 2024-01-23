using CesiumForUnity;
using NUnit.Framework;
using System;

public class TestCesiumFeatureIdAttribute
{
    [Test]
    public void ConstructsEmptyAttribute()
    {
        CesiumFeatureIdAttribute attribute = new CesiumFeatureIdAttribute();
        Assert.That(attribute.type, Is.EqualTo(CesiumFeatureIdSetType.Attribute));
        Assert.That(attribute.status, Is.EqualTo(CesiumFeatureIdAttributeStatus.ErrorInvalidAttribute));
        Assert.That(attribute.featureCount, Is.EqualTo(0));
    }

    [Test]
    public void ConstructsValidAttribute()
    {
        TestGltfModel model = new TestGltfModel();
        UInt16[] featureIds = { 0, 0, 0, 1, 1, 1 };
        const Int64 featureCount = 2;

        CesiumFeatureIdAttribute attribute = model.AddFeatureIdAttribute(featureIds, featureCount);
        Assert.That(attribute.type, Is.EqualTo(CesiumFeatureIdSetType.Attribute));
        Assert.That(attribute.status, Is.EqualTo(CesiumFeatureIdAttributeStatus.Valid));
        Assert.That(attribute.featureCount, Is.EqualTo(featureCount));

        model.Dispose();
    }

    [Test]
    public void GetFeatureIdForVertexHandlesInvalidAttribute()
    {
        CesiumFeatureIdAttribute attribute = new CesiumFeatureIdAttribute();
        Assert.That(attribute.type, Is.EqualTo(CesiumFeatureIdSetType.Attribute));
        Assert.That(attribute.status, Is.EqualTo(CesiumFeatureIdAttributeStatus.ErrorInvalidAttribute));
        Assert.That(attribute.featureCount, Is.EqualTo(0));

        Assert.That(attribute.GetFeatureIdForVertex(0), Is.EqualTo(-1));
    }

    [Test]
    public void GetFeatureIdForVertexHandlesInvalidVertex()
    {
        TestGltfModel model = new TestGltfModel();
        UInt16[] featureIds = { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
        const Int64 featureCount = 3;

        CesiumFeatureIdAttribute attribute = model.AddFeatureIdAttribute(featureIds, featureCount);
        Assert.That(attribute.type, Is.EqualTo(CesiumFeatureIdSetType.Attribute));
        Assert.That(attribute.status, Is.EqualTo(CesiumFeatureIdAttributeStatus.Valid));
        Assert.That(attribute.featureCount, Is.EqualTo(featureCount));

        Assert.That(attribute.GetFeatureIdForVertex(-1), Is.EqualTo(-1));
        Assert.That(attribute.GetFeatureIdForVertex(featureIds.Length), Is.EqualTo(-1));

        model.Dispose();
    }

    [Test]
    public void GetFeatureIdForVertexReturnsFeatureIds()
    {
        TestGltfModel model = new TestGltfModel();
        UInt16[] featureIds = { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
        const Int64 featureCount = 3;

        CesiumFeatureIdAttribute attribute = model.AddFeatureIdAttribute(featureIds, featureCount);
        Assert.That(attribute.type, Is.EqualTo(CesiumFeatureIdSetType.Attribute));
        Assert.That(attribute.status, Is.EqualTo(CesiumFeatureIdAttributeStatus.Valid));
        Assert.That(attribute.featureCount, Is.EqualTo(featureCount));

        for (int i = 0; i < featureIds.Length; i++)
        {
            Assert.That(attribute.GetFeatureIdForVertex(i), Is.EqualTo(featureIds[i]));
        }

        model.Dispose();
    }
}
