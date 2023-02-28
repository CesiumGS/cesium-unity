using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using CesiumForUnity;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.TestTools.Utils;
using NUnit.Framework;

public class TestCesiumFlyToController
{
    [UnityTest]
    public IEnumerator FlyToLocationLongitudeLatitudeHeight()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goFlyer = new GameObject("Flyer");
        goFlyer.transform.parent = goGeoreference.transform;

        CesiumFlyToController flyToController = goFlyer.AddComponent<CesiumFlyToController>();
        CesiumGlobeAnchor anchor = goFlyer.GetComponent<CesiumGlobeAnchor>();

        // Make the flight fast so the test doesn't take too long.
        flyToController.flyToDuration = 0.25;

        anchor.adjustOrientationForGlobeWhenMoving = false;
        anchor.longitudeLatitudeHeight = new double3(20.0, -25.0, 1000.0);
        anchor.adjustOrientationForGlobeWhenMoving = true;

        yield return null;

        // The origin should have been shifted so the flyer is at the origin.
        Assert.That(goFlyer.transform.position.x, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goFlyer.transform.position.y, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goFlyer.transform.position.z, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));

        // Start a flight to elsewhere
        bool flightComplete = false;
        flyToController.OnFlightComplete += () =>
        {
            flightComplete = true;
        };

        flyToController.OnFlightInterrupted += () =>
        {
            Assert.Fail("Flight should not be interrupted.");
        };
        
        flyToController.FlyToLocationLongitudeLatitudeHeight(new double3(100.0, 25.0, 800.0), 0.0f, 0.0f, true);

        // Wait for the flight to complete
        while (!flightComplete)
        {
            yield return null;
        }


        // But the ECEF position should be the destination of the flight.
        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);
        Assert.That(anchor.longitudeLatitudeHeight.x, Is.EqualTo(100.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.EqualTo(25.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.EqualTo(800.0).Using(epsilon8));

        // This code will execute before the current frame's LateUpdate, so the origin won't be shifted
        // to the new location yet. Wait one more frame and it will be.
        yield return null;

        Assert.That(goFlyer.transform.position.x, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goFlyer.transform.position.y, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goFlyer.transform.position.z, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
    }
}
