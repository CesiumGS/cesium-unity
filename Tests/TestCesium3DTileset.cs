using CesiumForUnity;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

public class TestCesium3DTileset
{
    [UnityTest]
    public IEnumerator SampleHeightMostDetailedWorksWithAnEmptyArrayOfPositions()
    {
        GameObject go = new GameObject();
        go.name = "Cesium World Terrain";
        Cesium3DTileset tileset = go.AddComponent<Cesium3DTileset>();
        tileset.ionAccessToken = Environment.GetEnvironmentVariable("CESIUM_ION_TOKEN_FOR_TESTS") ?? "";
        tileset.ionAssetID = 1;

        // TODO: remove this
        yield return null;

        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed();

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.heightSampled);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 0);
        Assert.AreEqual(result.heightSampled.Length, 0);
        Assert.AreEqual(result.warnings.Length, 0);
    }

    [UnityTest]
    public IEnumerator SampleHeightMostDetailedWorksWithASinglePosition()
    {
        GameObject go = new GameObject();
        go.name = "Cesium World Terrain";
        Cesium3DTileset tileset = go.AddComponent<Cesium3DTileset>();
        tileset.ionAccessToken = Environment.GetEnvironmentVariable("CESIUM_ION_TOKEN_FOR_TESTS") ?? "";
        tileset.ionAssetID = 1;

        // TODO: remove this
        yield return null;

        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed(new double3(-105.1, 40.1, 1.0));

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.heightSampled);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 1);
        Assert.AreEqual(result.heightSampled.Length, 1);
        Assert.AreEqual(result.warnings.Length, 0);

        Assert.AreEqual(result.heightSampled[0], true);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].x, -105.1, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].y, 40.1, 1e-12);
        // Returned height should be different from the original height (1.0) by at least one meter.
        Assert.IsTrue(math.abs(result.longitudeLatitudeHeightPositions[0].z - 1.0) > 1.0);
    }

    [UnityTest]
    public IEnumerator SampleHeightMostDetailedWorksWithMultiplePositions()
    {
        GameObject go = new GameObject();
        go.name = "Cesium World Terrain";
        Cesium3DTileset tileset = go.AddComponent<Cesium3DTileset>();
        tileset.ionAccessToken = Environment.GetEnvironmentVariable("CESIUM_ION_TOKEN_FOR_TESTS") ?? "";
        tileset.ionAssetID = 1;

        // TODO: remove this
        yield return null;

        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed(
            new double3(-105.1, 40.1, 1.0),
            new double3(105.1, -40.1, 1.0));

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.heightSampled);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 2);
        Assert.AreEqual(result.heightSampled.Length, 2);
        Assert.AreEqual(result.warnings.Length, 0);

        Assert.AreEqual(result.heightSampled[0], true);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].x, -105.1, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].y, 40.1, 1e-12);
        // Returned height should be different from the original height (1.0) by at least one meter.
        Assert.IsTrue(math.abs(result.longitudeLatitudeHeightPositions[0].z - 1.0) > 1.0);

        Assert.AreEqual(result.heightSampled[1], true);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[1].x, 105.1, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[1].y, -40.1, 1e-12);
        // Returned height should be different from the original height (1.0) by at least one meter.
        Assert.IsTrue(math.abs(result.longitudeLatitudeHeightPositions[1].z - 1.0) > 1.0);
    }

    [UnityTest]
    public IEnumerator SampleHeightMostDetailedIndicatesNotSampledForPositionOutsideTileset()
    {
        GameObject go = new GameObject();
        go.name = "Melbourne Photogrammetry";
        Cesium3DTileset tileset = go.AddComponent<Cesium3DTileset>();
        tileset.ionAccessToken = Environment.GetEnvironmentVariable("CESIUM_ION_TOKEN_FOR_TESTS") ?? "";
        tileset.ionAssetID = 69380;

        // TODO: remove this
        yield return null;

        // Somewhere in Sydney, not Melbourne
        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed(new double3(151.20972, -33.87100, 1.0));

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.heightSampled);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 1);
        Assert.AreEqual(result.heightSampled.Length, 1);
        Assert.AreEqual(result.warnings.Length, 0);

        Assert.AreEqual(result.heightSampled[0], false);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].x, 151.20972, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].y, -33.87100, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].z, 1.0, 1e-12);
    }
}