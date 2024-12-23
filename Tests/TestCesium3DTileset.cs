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

        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed();

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.sampleSuccess);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 0);
        Assert.AreEqual(result.sampleSuccess.Length, 0);
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

        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed(new double3(-105.1, 40.1, 1.0));

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.sampleSuccess);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 1);
        Assert.AreEqual(result.sampleSuccess.Length, 1);
        Assert.AreEqual(result.warnings.Length, 0);

        Assert.AreEqual(result.sampleSuccess[0], true);
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

        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed(
            new double3(-105.1, 40.1, 1.0),
            new double3(105.1, -40.1, 1.0));

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.sampleSuccess);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 2);
        Assert.AreEqual(result.sampleSuccess.Length, 2);
        Assert.AreEqual(result.warnings.Length, 0);

        Assert.AreEqual(result.sampleSuccess[0], true);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].x, -105.1, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].y, 40.1, 1e-12);
        // Returned height should be different from the original height (1.0) by at least one meter.
        Assert.IsTrue(math.abs(result.longitudeLatitudeHeightPositions[0].z - 1.0) > 1.0);

        Assert.AreEqual(result.sampleSuccess[1], true);
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

        // Somewhere in Sydney, not Melbourne
        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed(new double3(151.20972, -33.87100, 1.0));

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.sampleSuccess);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 1);
        Assert.AreEqual(result.sampleSuccess.Length, 1);
        Assert.AreEqual(result.warnings.Length, 0);

        Assert.AreEqual(result.sampleSuccess[0], false);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].x, 151.20972, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].y, -33.87100, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].z, 1.0, 1e-12);
    }

    [UnityTest]
    public IEnumerator SampleHeightMostDetailedFailsIfTilesetFailsToLoad()
    {
        GameObject go = new GameObject();
        go.name = "Invalid";
        Cesium3DTileset tileset = go.AddComponent<Cesium3DTileset>();
        tileset.tilesetSource = CesiumDataSource.FromUrl;
        tileset.url = "http://localhost/notgonnawork";

        Task<CesiumSampleHeightResult> task = tileset.SampleHeightMostDetailed(new double3(151.20972, -33.87100, 1.0));

        yield return new WaitForTask(task);

        CesiumSampleHeightResult result = task.Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.longitudeLatitudeHeightPositions);
        Assert.IsNotNull(result.sampleSuccess);
        Assert.IsNotNull(result.warnings);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions.Length, 1);
        Assert.AreEqual(result.sampleSuccess.Length, 1);
        Assert.AreEqual(result.warnings.Length, 1);

        Assert.AreEqual(result.sampleSuccess[0], false);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].x, 151.20972, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].y, -33.87100, 1e-12);
        Assert.AreEqual(result.longitudeLatitudeHeightPositions[0].z, 1.0, 1e-12);

        Assert.IsTrue(result.warnings[0].Contains("failed to load"));
    }

    [UnityTest]
    public IEnumerator UpgradeToLargerIndexType()
    {
        // This tileset has no normals, so we need to generate flat normals for it.
        // When we do that, an index buffer will need to change from uint16 to uint32.
        GameObject goGeoreference = new GameObject();
        goGeoreference.name = "Georeference";
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();

        GameObject goTileset = new GameObject();
        goTileset.name = "Snowdon Towers No Normals";
        goTileset.transform.parent = goGeoreference.transform;

        Cesium3DTileset tileset = goTileset.AddComponent<Cesium3DTileset>();
        CesiumCameraManager cameraManager = goTileset.AddComponent<CesiumCameraManager>();
        tileset.ionAccessToken = Environment.GetEnvironmentVariable("CESIUM_ION_TOKEN_FOR_TESTS") ?? "";
        tileset.ionAssetID = 2887128;

        georeference.SetOriginLongitudeLatitudeHeight(-79.88602625, 40.02228799, 222.65);

        GameObject goCamera = new GameObject();
        goCamera.name = "Camera";
        goCamera.transform.parent = goGeoreference.transform;

        Camera camera = goCamera.AddComponent<Camera>();
        CesiumGlobeAnchor cameraAnchor = goCamera.AddComponent<CesiumGlobeAnchor>();

        cameraManager.useMainCamera = false;
        cameraManager.useSceneViewCameraInEditor = false;
        cameraManager.additionalCameras.Add(camera);

        camera.pixelRect = new Rect(0, 0, 128, 96);
        camera.fieldOfView = 60.0f;
        cameraAnchor.longitudeLatitudeHeight = new double3(-79.88593359, 40.02255615, 242.0224);
        camera.transform.LookAt(new Vector3(0.0f, 0.0f, 0.0f));

        // Make sure we can load all tiles successfully.
        while (tileset.ComputeLoadProgress() < 100.0f)
        {
            yield return null;
        }
    }
}