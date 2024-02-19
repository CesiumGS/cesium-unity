using CesiumForUnity;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class TestCesiumSubScene
{
    [UnityTest]
    public IEnumerator AddingSubSceneCopiesGeoreferenceCoordinates()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 1000.0);

        GameObject goSubScene = new GameObject("SubScene");
        goSubScene.transform.parent = goGeoreference.transform;
        CesiumSubScene subScene = goSubScene.AddComponent<CesiumSubScene>();

        yield return null;

        Assert.AreEqual(-55.0, georeference.longitude);
        Assert.AreEqual(55.0, georeference.latitude);
        Assert.AreEqual(1000.0, georeference.height);
        Assert.AreEqual(georeference.longitude, subScene.longitude);
        Assert.AreEqual(georeference.latitude, subScene.latitude);
        Assert.AreEqual(georeference.height, subScene.height);
    }

    [UnityTest]
    public IEnumerator ModifyingSubsceneModifiesParentGeoreference()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 1000.0);

        GameObject goSubScene = new GameObject("SubScene");
        goSubScene.transform.parent = goGeoreference.transform;
        CesiumSubScene subScene = goSubScene.AddComponent<CesiumSubScene>();

        yield return null;

        subScene.SetOriginLongitudeLatitudeHeight(-10.0, 10.0, 100.0);

        yield return null;

        Assert.AreEqual(subScene.longitude, -10.0);
        Assert.AreEqual(subScene.latitude, 10.0);
        Assert.AreEqual(subScene.height, 100.0);

        Assert.AreEqual(georeference.longitude, -10.0);
        Assert.AreEqual(georeference.latitude, 10.0);
        Assert.AreEqual(georeference.height, 100.0);
    }
}