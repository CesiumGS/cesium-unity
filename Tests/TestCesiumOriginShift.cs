using CesiumForUnity;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

public class TestCesiumOriginShift
{
    [UnityTest]
    public IEnumerator ActivatesSubSceneWithinRange()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goOriginShift = new GameObject("OriginShifter");
        goOriginShift.transform.parent = goGeoreference.transform;
        CesiumOriginShift originShift = goOriginShift.AddComponent<CesiumOriginShift>();

        // Start the sub-scene disabled; otherwise it will immediately set the georeference origin.
        GameObject goSubScene = new GameObject("SubScene");
        goSubScene.SetActive(false);
        goSubScene.transform.parent = goGeoreference.transform;
        CesiumSubScene subScene = goSubScene.AddComponent<CesiumSubScene>();
        subScene.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 3000.0);

        yield return null;

        Assert.AreEqual(subScene.isActiveAndEnabled, false);

        // Move the origin shifter up so that it's in range.
        // Do an explicit Sync to avoid a one frame delay if the Transfer change check coroutine
        // runs before this test coroutine.
        goOriginShift.transform.position = new Vector3(0.0f, 1500.0f, 0.0f);
        goOriginShift.GetComponent<CesiumGlobeAnchor>().Sync();

        yield return null;

        Assert.AreEqual(subScene.isActiveAndEnabled, true);
    }

    [UnityTest]
    public IEnumerator IgnoresSubSceneTransformForActivationCheck()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goOriginShift = new GameObject("OriginShifter");
        goOriginShift.transform.parent = goGeoreference.transform;
        CesiumOriginShift originShift = goOriginShift.AddComponent<CesiumOriginShift>();

        // Start the sub-scene disabled; otherwise it will immediately set the georeference origin.
        GameObject goSubScene = new GameObject("SubScene");
        goSubScene.SetActive(false);
        goSubScene.transform.parent = goGeoreference.transform;

        // This transform would put the sub-scene in range of the goOriginShift if it were considered,
        // but it should _not_ be.
        goSubScene.transform.localPosition = new Vector3(0.0f, -1500.0f, 0.0f);

        CesiumSubScene subScene = goSubScene.AddComponent<CesiumSubScene>();
        subScene.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 3000.0);

        yield return null;

        Assert.AreEqual(subScene.isActiveAndEnabled, false);
    }

    [UnityTest]
    public IEnumerator UsesGeoreferenceTransformForActivationCheck()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;
        georeference.transform.localPosition = new Vector3(0.0f, -1500.0f, 0.0f);

        GameObject goOriginShift = new GameObject("OriginShifter");
        goOriginShift.transform.parent = goGeoreference.transform;
        CesiumOriginShift originShift = goOriginShift.AddComponent<CesiumOriginShift>();

        // Start the sub-scene disabled; otherwise it will immediately set the georeference origin.
        GameObject goSubScene = new GameObject("SubScene");
        goSubScene.SetActive(false);
        goSubScene.transform.parent = goGeoreference.transform;
        CesiumSubScene subScene = goSubScene.AddComponent<CesiumSubScene>();
        subScene.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 3000.0);

        yield return null;

        Assert.AreEqual(subScene.isActiveAndEnabled, true);
    }
}
