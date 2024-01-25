using CesiumForUnity;
using NUnit.Framework;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

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

    [UnityTest]
    public IEnumerator UsesActivationDistanceProperty()
    {
        double3 baseEcef = new double3(-2694019.41, -4297353.83809221, 3854717.9087824);

        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.ecefX = baseEcef.x;
        georeference.ecefY = baseEcef.y;
        georeference.ecefZ = baseEcef.z;

        GameObject goOriginShift = new GameObject("OriginShifter");
        goOriginShift.transform.parent = goGeoreference.transform;
        CesiumGlobeAnchor globeAnchor = goOriginShift.AddComponent<CesiumGlobeAnchor>();

        CesiumOriginShift originShift = goOriginShift.AddComponent<CesiumOriginShift>();
        originShift.useActivationDistance = true;
        originShift.activationDistance = 5000;

        yield return null;

        globeAnchor.positionGlobeFixed = baseEcef + new double3(4999.0, 0, 0);

        yield return null;

        Assert.AreEqual(baseEcef.x + 4999.0, globeAnchor.positionGlobeFixed.x);
        Assert.AreEqual(baseEcef.x + 0, georeference.ecefX);

        globeAnchor.positionGlobeFixed = baseEcef + new double3(5010.0, 0, 0);
        yield return null;

        Assert.AreEqual(baseEcef.x + 5010.0, globeAnchor.positionGlobeFixed.x);
        Assert.AreEqual(baseEcef.x + 5010.0, georeference.ecefX);
    }

    // Testing a bug where the character controller would cause a jump ahead when the activation distance is hit.
    [UnityTest]
    public IEnumerator ShiftingOriginWithCharacterController()
    {
        double3 baseEcef = new double3(-2694020, -4297355, 3854720);

        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.ecefX = baseEcef.x;
        georeference.ecefY = baseEcef.y;
        georeference.ecefZ = baseEcef.z;

        GameObject goOriginShift = new GameObject("OriginShifter");
        goOriginShift.transform.parent = goGeoreference.transform;
        CesiumGlobeAnchor globeAnchor = goOriginShift.AddComponent<CesiumGlobeAnchor>();

        CesiumOriginShift originShift = goOriginShift.AddComponent<CesiumOriginShift>();
        originShift.useActivationDistance = true;
        originShift.activationDistance = 5000;

        CharacterController controller = goOriginShift.AddComponent<CharacterController>();
        controller.radius = 1.0f;
        controller.height = 1.0f;
        controller.center = Vector3.zero;
        controller.enableOverlapRecovery = false;

        yield return new WaitForEndOfFrame();

        Assert.AreEqual((float)baseEcef.x, (float)globeAnchor.positionGlobeFixed.x);

        // speed per second
        double speed = 1000.0;
        double duration = 10.0;
        float startTime = Time.time;
        Vector3 startPos = globeAnchor.transform.position;

        while ((Time.time - startTime) < duration)
        {
            double3 previousPositionEcef = globeAnchor.positionGlobeFixed.x;

            yield return new WaitForFixedUpdate();

            double unitsEcef = speed * Time.deltaTime;
            double3 movement = georeference.TransformEarthCenteredEarthFixedDirectionToUnity(new double3(unitsEcef, 0, 0));
            controller.Move((float3)movement);

            yield return new WaitForEndOfFrame();

            globeAnchor.Sync();
            Assert.AreEqual((float)(previousPositionEcef.x + unitsEcef), (float)globeAnchor.positionGlobeFixed.x);
            Assert.Less(georeference.ecefX - globeAnchor.positionGlobeFixed.x, 5000.0);

            Debug.Log($"georeference d: ({georeference.ecefX - baseEcef.x}, {georeference.ecefY - baseEcef.y}, {georeference.ecefZ - baseEcef.z})");
            Debug.Log($"anchor d: {globeAnchor.positionGlobeFixed - baseEcef}");
        }
    }
}
