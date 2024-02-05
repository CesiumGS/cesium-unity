using CesiumForUnity;
using NUnit.Framework;
using System;
using UnityEngine;
using Unity.Mathematics;

public class TestCesiumFeatureIdTexture
{
    [Test]
    public void ConstructsEmptyTexture()
    {
        CesiumFeatureIdTexture texture = new CesiumFeatureIdTexture();
        Assert.That(texture.type, Is.EqualTo(CesiumFeatureIdSetType.Texture));
        Assert.That(texture.status, Is.EqualTo(CesiumFeatureIdTextureStatus.ErrorInvalidTexture));
        Assert.That(texture.featureCount, Is.EqualTo(0));
    }

    [Test]
    public void ConstructsValidTexture()
    {
        TestGltfModel model = new TestGltfModel();
        UInt16[] featureIds = { 0, 1, 2, 3 };
        const Int64 featureCount = 4;
        float2[] uvs = {
            new float2(0, 0),
            new float2(0.5f, 0),
            new float2(0, 0.5f),
            new float2(0.5f, 0.5f) };

        CesiumFeatureIdTexture texture = model.AddFeatureIdTexture(featureIds, featureCount, uvs);
        Assert.That(texture.type, Is.EqualTo(CesiumFeatureIdSetType.Texture));
        Assert.That(texture.status, Is.EqualTo(CesiumFeatureIdTextureStatus.Valid));
        Assert.That(texture.featureCount, Is.EqualTo(featureCount));

        model.Dispose();
    }

    [Test]
    public void GetFeatureIdForVertexHandlesInvalidTexture()
    {
        CesiumFeatureIdTexture texture = new CesiumFeatureIdTexture();
        Assert.That(texture.type, Is.EqualTo(CesiumFeatureIdSetType.Texture));
        Assert.That(texture.status, Is.EqualTo(CesiumFeatureIdTextureStatus.ErrorInvalidTexture));
        Assert.That(texture.featureCount, Is.EqualTo(0));

        Assert.That(texture.GetFeatureIdForVertex(0), Is.EqualTo(-1));
    }

    [Test]
    public void GetFeatureIdForVertexHandlesInvalidVertex()
    {
        TestGltfModel model = new TestGltfModel();
        UInt16[] featureIds = { 0, 1, 2, 3 };
        const Int64 featureCount = 4;
        float2[] uvs = {
            new float2(0, 0),
            new float2(0.5f, 0),
            new float2(0, 0.5f),
            new float2(0.5f, 0.5f) };

        CesiumFeatureIdTexture texture = model.AddFeatureIdTexture(featureIds, featureCount, uvs);
        Assert.That(texture.type, Is.EqualTo(CesiumFeatureIdSetType.Texture));
        Assert.That(texture.status, Is.EqualTo(CesiumFeatureIdTextureStatus.Valid));
        Assert.That(texture.featureCount, Is.EqualTo(featureCount));

        Assert.That(texture.GetFeatureIdForVertex(-1), Is.EqualTo(-1));
        Assert.That(texture.GetFeatureIdForVertex(featureIds.Length), Is.EqualTo(-1));

        model.Dispose();
    }

    [Test]
    public void GetFeatureIdForUVHandlesInvalidTexture()
    {
        CesiumFeatureIdTexture texture = new CesiumFeatureIdTexture();
        Assert.That(texture.type, Is.EqualTo(CesiumFeatureIdSetType.Texture));
        Assert.That(texture.status, Is.EqualTo(CesiumFeatureIdTextureStatus.ErrorInvalidTexture));
        Assert.That(texture.featureCount, Is.EqualTo(0));

        Assert.That(texture.GetFeatureIdForUV(Vector2.zero), Is.EqualTo(-1));
    }

    [Test]
    public void GetFeatureIdForUVReturnsFeatureIds()
    {
        TestGltfModel model = new TestGltfModel();
        UInt16[] featureIds = { 0, 1, 2, 3 };
        const Int64 featureCount = 4;
        float2[] uvs = {
            new float2(0, 0),
            new float2(0.5f, 0),
            new float2(0, 0.5f),
            new float2(0.5f, 0.5f) };

        float2[] testUvs =
        {
            new float2(0.25f, 0.1f),
            new float2(0.75f, 0.66f),
            new float2(0.001f, 0.55f)
        };
        UInt16[] expected = { 0, 3, 2 };

        CesiumFeatureIdTexture texture = model.AddFeatureIdTexture(featureIds, featureCount, uvs);
        Assert.That(texture.type, Is.EqualTo(CesiumFeatureIdSetType.Texture));
        Assert.That(texture.status, Is.EqualTo(CesiumFeatureIdTextureStatus.Valid));
        Assert.That(texture.featureCount, Is.EqualTo(featureCount));

        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(texture.GetFeatureIdForUV(testUvs[i]), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
}
