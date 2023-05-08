using CesiumForUnity;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

public class TestCesiumGeoreference
{
    [UnityTest]
    public IEnumerator ChangingOriginAtRuntimeUpdatesGlobeAnchors()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 1000.0);

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        yield return null;

        Assert.That(goAnchored.transform.localPosition.x, Is.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.y, Is.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.z, Is.EqualTo(0.0f));

        georeference.SetOriginLongitudeLatitudeHeight(-55.1, 54.9, 1001.0);

        Assert.That(goAnchored.transform.localPosition.x, Is.Not.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.y, Is.Not.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.z, Is.Not.EqualTo(0.0f));

        yield return null;

        Assert.That(goAnchored.transform.localPosition.x, Is.Not.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.y, Is.Not.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.z, Is.Not.EqualTo(0.0f));
    }

    [UnityTest]
    public IEnumerator ChangingParentTransformAndGeoreferenceMaintainsCorrectGlobePosition()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 1000.0);

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();

        yield return null;

        Assert.That(goAnchored.transform.localPosition.x, Is.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.y, Is.EqualTo(0.0f));
        Assert.That(goAnchored.transform.localPosition.z, Is.EqualTo(0.0f));

        // Change both the origin and the transform.
        georeference.transform.localPosition = new Vector3(100.0f, 200.0f, 300.0f);
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 2000.0);

        yield return null;

        // The anchor should maintain its globe position.
        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);
        Assert.That(anchor.longitudeLatitudeHeight.x, Is.EqualTo(-55.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.EqualTo(55.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.EqualTo(1000.0).Using(epsilon8));

        // Its local local position should be affected by the georeference origin change
        // but not by the parent transform change.
        IEqualityComparer<float> epsilon4 = new FloatEqualityComparer(1e-3f);
        Assert.That(goAnchored.transform.localPosition.x, Is.EqualTo(0.0f).Using(epsilon4));
        Assert.That(goAnchored.transform.localPosition.y, Is.EqualTo(-1000.0f).Using(epsilon4));
        Assert.That(goAnchored.transform.localPosition.z, Is.EqualTo(0.0f).Using(epsilon4));
    }

    [UnityTest]
    public IEnumerator GeoreferenceScaleAffectsGlobeAnchors()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 1000.0);

        GameObject goAnchored = new GameObject("Anchored");
        goAnchored.transform.parent = goGeoreference.transform;
        goAnchored.transform.localPosition = new Vector3(1.0f, 2.0f, 3.0f);
        goAnchored.transform.localScale = new Vector3(4.0f, 5.0f, 6.0f);

        CesiumGlobeAnchor anchor = goAnchored.AddComponent<CesiumGlobeAnchor>();
        double3 longitudeLatitudeHeight = anchor.longitudeLatitudeHeight;

        yield return null;

        Assert.That(goAnchored.transform.localPosition.x, Is.EqualTo(1.0f));
        Assert.That(goAnchored.transform.localPosition.y, Is.EqualTo(2.0f));
        Assert.That(goAnchored.transform.localPosition.z, Is.EqualTo(3.0f));

        Assert.That(goAnchored.transform.localScale.x, Is.EqualTo(4.0f));
        Assert.That(goAnchored.transform.localScale.y, Is.EqualTo(5.0f));
        Assert.That(goAnchored.transform.localScale.z, Is.EqualTo(6.0f));

        // The globe anchor's scale initially matches the local scale.
        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);
        Assert.That(anchor.scaleEastUpNorth.x, Is.EqualTo(4.0).Using(epsilon8));
        Assert.That(anchor.scaleEastUpNorth.y, Is.EqualTo(5.0).Using(epsilon8));
        Assert.That(anchor.scaleEastUpNorth.z, Is.EqualTo(6.0).Using(epsilon8));

        Assert.That(anchor.longitudeLatitudeHeight.x, Is.Not.EqualTo(-55.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.Not.EqualTo(55.0).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.Not.EqualTo(1000.0).Using(epsilon8));

        georeference.scale = 0.5;

        yield return null;

        // The local transforms are affected by the georeference's scale...
        IEqualityComparer<float> epsilon4 = new FloatEqualityComparer(1e-3f);
        Assert.That(goAnchored.transform.localPosition.x, Is.EqualTo(0.5f).Using(epsilon4));
        Assert.That(goAnchored.transform.localPosition.y, Is.EqualTo(1.0f).Using(epsilon4));
        Assert.That(goAnchored.transform.localPosition.z, Is.EqualTo(1.5f).Using(epsilon4));

        Assert.That(goAnchored.transform.localScale.x, Is.EqualTo(2.0f).Using(epsilon4));
        Assert.That(goAnchored.transform.localScale.y, Is.EqualTo(2.5f).Using(epsilon4));
        Assert.That(goAnchored.transform.localScale.z, Is.EqualTo(3.0f).Using(epsilon4));

        // ...while the globe-relative properties stay the same.
        Assert.That(anchor.scaleEastUpNorth.x, Is.EqualTo(4.0).Using(epsilon8));
        Assert.That(anchor.scaleEastUpNorth.y, Is.EqualTo(5.0).Using(epsilon8));
        Assert.That(anchor.scaleEastUpNorth.z, Is.EqualTo(6.0).Using(epsilon8));

        Assert.That(anchor.longitudeLatitudeHeight.x, Is.EqualTo(longitudeLatitudeHeight.x).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.y, Is.EqualTo(longitudeLatitudeHeight.y).Using(epsilon8));
        Assert.That(anchor.longitudeLatitudeHeight.z, Is.EqualTo(longitudeLatitudeHeight.z).Using(epsilon8));
    }
}
