using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using CesiumForUnity;
using NUnit.Framework;
using UnityEngine.TestTools.Utils;
using Unity.Mathematics;
using System.Collections.Generic;

public class TestCesiumGlobeAnchor
{
    [Test]
    public void AddingGlobeAnchorImmediatelySyncsGlobePositionFromTransform()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.SetPositionAndRotation(new Vector3(0.0f, 100.0f, 0.0f), Quaternion.Euler(10.0f, 20.0f, 30.0f));

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);
        Assert.That(anchor.longitudeLatitudeHeight.x, Is.EqualTo(-55.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.EqualTo(55.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.EqualTo(1100.0).Using(epsilon8));
    }

    [Test]
    public void SettingGlobeAnchorPositionUpdatesTransform()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.SetPositionAndRotation(new Vector3(0.0f, 100.0f, 0.0f), Quaternion.Euler(10.0f, 20.0f, 30.0f));

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        // Transform position should be unmodified by adding the globe anchor.
        Assert.That(goAnchored.transform.position.x, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.EqualTo(100.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));

        // Setting the globe anchor position should change the Transform position.
        anchor.longitudeLatitudeHeight = new double3(-56.0, 56.0, 2000.0);

        Assert.That(goAnchored.transform.position.x, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.Not.EqualTo(100.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
    }

    [UnityTest]
    public IEnumerator StartDoesNotClobberPreviouslySetPosition()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;

        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();
        anchor.longitudeLatitudeHeight = new double3(45.0, -45.0, 101.0);
        Assert.That(anchor.longitudeLatitudeHeight.x, Is.EqualTo(45.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.EqualTo(-45.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.EqualTo(101.0).Using(epsilon8));

        yield return null;

        Assert.That(anchor.longitudeLatitudeHeight.x, Is.EqualTo(45.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.EqualTo(-45.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.EqualTo(101.0).Using(epsilon8));
    }

    [Test]
    public void SettingPositionImmediatelyAfterAddingAnchorDoesAffectOrientation()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.SetPositionAndRotation(new Vector3(100.0f, 200.0f, 300.0f), Quaternion.Euler(10.0f, 20.0f, 30.0f));

        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        // Move to the opposite side of the globe in terms of longitude.
        anchor.longitudeLatitudeHeight = new double3(125.0, 55.0, 1000.0);

        // The orientation should be affected because the globe anchor syncs immediately when it's created.
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.Not.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.Not.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.Not.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));
    }

    [UnityTest]
    public IEnumerator DoesNotChangeOrientationIfAdjustOrientationForGlobeWhenMovingIsFalse()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.SetPositionAndRotation(new Vector3(100.0f, 200.0f, 300.0f), Quaternion.Euler(10.0f, 20.0f, 30.0f));

        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        // Move to the opposite side of the globe in terms of longitude.
        anchor.adjustOrientationForGlobeWhenMoving = false;
        anchor.longitudeLatitudeHeight = new double3(125.0, 55.0, 1000.0);

        // The orientation should not be affected because adjustOrientationForGlobeWhenMoving is disabled.
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        // The same should be true if we move by changing the Transform
        goAnchored.transform.position = new Vector3(-80000.0f, 300.0f, 30000.0f);

        yield return null;

        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));
    }

    [Test]
    public void DisabledGlobeAnchorAdjustsForNewGeoreferenceWhenEnabled()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.SetPositionAndRotation(new Vector3(100.0f, 200.0f, 300.0f), Quaternion.Euler(10.0f, 20.0f, 30.0f));

        // Set the globe anchor to a known position on the globe.
        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        Assert.That(goAnchored.transform.position.x, Is.EqualTo(100.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.EqualTo(200.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.EqualTo(300.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        anchor.adjustOrientationForGlobeWhenMoving = false;
        anchor.longitudeLatitudeHeight = new double3(-56.0, 56.0, 1001.0);
        anchor.rotationEastUpNorth = quaternion.EulerYZX(0.0f, 0.0f, 0.0f);

        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);
        Assert.That(anchor.longitudeLatitudeHeight.x, Is.EqualTo(-56.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.EqualTo(56.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.EqualTo(1001.0).Using(epsilon8));

        // The object should initially be away from the origin and have a non-zero rotation.
        Assert.That(goAnchored.transform.position.x, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));

        // Disable the game object and move the georeference
        goAnchored.SetActive(false);
        georeference.SetOriginLongitudeLatitudeHeight(-56.0, 56.0, 1001.0);

        // The disabled object should not move or rotate
        Assert.That(goAnchored.transform.position.x, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));

        // But then when we enable the object, its position and orientation should be updated for the new georeference.
        goAnchored.SetActive(true);
        Assert.That(goAnchored.transform.position.x, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
    }

    [UnityTest]
    public IEnumerator SettingTransformPositionAlsoUpdatesOrientation()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.longitude = -55.0;
        georeference.latitude = 55.0;
        georeference.height = 1000.0;

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.SetPositionAndRotation(new Vector3(100.0f, 200.0f, 300.0f), Quaternion.Euler(10.0f, 20.0f, 30.0f));

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        Assert.That(goAnchored.transform.position.x, Is.EqualTo(100.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.EqualTo(200.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.EqualTo(300.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        // Modify the object's position via the Transform.
        goAnchored.transform.position = new Vector3(20000.0f, 3000.0f, -80000.0f);
        
        // The orientation should not immediately change, because the coroutine has to run first.
        Assert.That(goAnchored.transform.position.x, Is.EqualTo(20000.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.EqualTo(3000.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.EqualTo(-80000.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        yield return null;

        // After a tick, the orientation should be updated as well.
        Assert.That(goAnchored.transform.position.x, Is.EqualTo(20000.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.EqualTo(3000.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.EqualTo(-80000.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.Not.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.Not.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.Not.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));
    }
}
