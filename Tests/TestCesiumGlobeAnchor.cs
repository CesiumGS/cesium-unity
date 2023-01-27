using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using CesiumForUnity;
using NUnit.Framework;
using UnityEngine.TestTools.Utils;

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

        Assert.That(anchor.longitude, Is.EqualTo(-55.0).Using(FloatEqualityComparer.Instance));
        Assert.That(anchor.latitude, Is.EqualTo(55.0).Using(FloatEqualityComparer.Instance));
        Assert.That(anchor.height, Is.EqualTo(100.0).Using(FloatEqualityComparer.Instance));
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
        anchor.SetPositionLongitudeLatitudeHeight(-56.0, 56.0, 2000.0);

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

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();
        anchor.SetPositionLongitudeLatitudeHeight(45.0, -45.0, 101.0);
        Assert.AreEqual(CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight, anchor.positionAuthority);
        Assert.AreEqual(45.0, anchor.longitude);
        Assert.AreEqual(-45.0, anchor.latitude);
        Assert.AreEqual(101.0, anchor.height);

        yield return null;

        Assert.AreEqual(CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight, anchor.positionAuthority);
        Assert.AreEqual(45.0, anchor.longitude);
        Assert.AreEqual(-45.0, anchor.latitude);
        Assert.AreEqual(101.0, anchor.height);
    }

    [UnityTest]
    public IEnumerator SettingPositionImmediatelyAfterAddingAnchorDoesNotAffectOrientation()
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
        anchor.SetPositionLongitudeLatitudeHeight(125.0, 55.0, 1000.0);

        // The orientation should be unaffected because the globe anchor hasn't been Sync'd yet.
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        //// Wait for the start of a new frame, which will cause Start to be invoked.
        yield return null;

        // The orientation should still be unaffected
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));

        // But now if we move it, the orientation will change, too.
        anchor.SetPositionLongitudeLatitudeHeight(105.0, 55.0, 1000.0);
        Assert.That(goAnchored.transform.rotation.eulerAngles.x, Is.Not.EqualTo(10.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.y, Is.Not.EqualTo(20.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.rotation.eulerAngles.z, Is.Not.EqualTo(30.0f).Using(FloatEqualityComparer.Instance));
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
        anchor.adjustOrientationForGlobeWhenMoving = false;
        anchor.SetPositionLongitudeLatitudeHeight(-56.0, 56.0, 1001.0);

        Assert.That(anchor.positionAuthority, Is.EqualTo(CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight));
        Assert.That(anchor.longitude, Is.EqualTo(-56.0));
        Assert.That(anchor.latitude, Is.EqualTo(56.0));
        Assert.That(anchor.height, Is.EqualTo(1001.0));

        Assert.That(goAnchored.transform.position.x, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));

        // Disable the game object and move the georeference
        goAnchored.SetActive(false);
        georeference.SetOriginLongitudeLatitudeHeight(-56.0, 56.0, 1001.0);

        // The disabled object should not move.
        Assert.That(goAnchored.transform.position.x, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.Not.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));

        // But then when we enable the object, its position should be updated for the new georeference.
        goAnchored.SetActive(true);
        Assert.That(goAnchored.transform.position.x, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.y, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
        Assert.That(goAnchored.transform.position.z, Is.EqualTo(0.0f).Using(FloatEqualityComparer.Instance));
    }
}
