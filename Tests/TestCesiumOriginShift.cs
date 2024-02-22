using CesiumForUnity;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
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
    public IEnumerator UsesDistanceProperty()
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
        originShift.distance = 5000;

        yield return null;

        globeAnchor.positionGlobeFixed = baseEcef + new double3(4999.0, 0, 0);

        yield return null;

        IEqualityComparer<double> epsilon6 = Comparers.Double(1e-6);

        // The anchor is still within the distance threshold, so the georeference should remain unchanged.
        Assert.That(baseEcef.x + 4999.0, Is.EqualTo(globeAnchor.positionGlobeFixed.x).Using(epsilon6));
        Assert.That(baseEcef.x, Is.EqualTo(georeference.ecefX).Using(epsilon6));

        globeAnchor.positionGlobeFixed = baseEcef + new double3(5010.0, 0, 0);
        yield return null;

        // The anchor has surpassed the distance threshold, so the georeference should shift to the anchor's ECEF coordinates.
        Assert.That(baseEcef.x + 5010.0, Is.EqualTo(globeAnchor.positionGlobeFixed.x).Using(epsilon6));
        Assert.That(globeAnchor.positionGlobeFixed.x, Is.EqualTo(georeference.ecefX).Using(epsilon6));
    }

    // Testing a bug where the character controller would cause a jump ahead when the activation distance is hit.
    [UnityTest]
    public IEnumerator ShiftsOriginWithCharacterController()
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
        originShift.distance = 5000;

        CharacterController controller = goOriginShift.AddComponent<CharacterController>();
        controller.radius = 1.0f;
        controller.height = 1.0f;
        controller.center = Vector3.zero;
        controller.enableOverlapRecovery = false;

        yield return null;

        IEqualityComparer<double> epsilon6 = Comparers.Double(1e-6, 1e-4);

        Assert.That(baseEcef.x, Is.EqualTo(globeAnchor.positionGlobeFixed.x).Using(epsilon6));

        yield return null;

        // Move, but not far enough to trigger an origin shift
        double3 previousPositionEcef = globeAnchor.positionGlobeFixed.x;
        double3 movement = georeference.TransformEarthCenteredEarthFixedDirectionToUnity(new double3(2500.0, 0, 0));
        controller.Move((float3)movement);

        // Explicitly sync the globe anchor, so that the origin shift's LateUpdate sees the new position this frame.
        // Otherwise, it will base the shift on the position in the previous frame because CesiumGlobeAnchor
        // coroutine that updates from the Transform already ran for this frame before the Move above was called.
        globeAnchor.Sync();

        yield return null;

        Assert.That(previousPositionEcef.x + 2500.0, Is.EqualTo(globeAnchor.positionGlobeFixed.x).Using(epsilon6));
        Assert.Less(georeference.ecefX - globeAnchor.positionGlobeFixed.x, 5000.0);

        // Move again, this time triggering an origin shift
        previousPositionEcef = globeAnchor.positionGlobeFixed.x;
        movement = georeference.TransformEarthCenteredEarthFixedDirectionToUnity(new double3(3000.0, 0, 0));
        controller.Move((float3)movement);

        globeAnchor.Sync();

        yield return null;

        Assert.That(previousPositionEcef.x + 3000.0, Is.EqualTo(globeAnchor.positionGlobeFixed.x).Using(epsilon6));
        Assert.Less(System.Math.Abs(georeference.ecefX - globeAnchor.positionGlobeFixed.x), 5000.0);
    }
}
