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

    [UnityTest]
    public IEnumerator ChangingOriginAtRuntimeUpdatesSubScene()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 1000.0);

        GameObject goSubscene = new GameObject("SubScene");
        goSubscene.transform.parent = goGeoreference.transform;

        CesiumSubScene subscene = goSubscene.AddComponent<CesiumSubScene>();

        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);

        yield return null;

        // Change the origin with 3 components
        {
            georeference.SetOriginLongitudeLatitudeHeight(-10.0, 10.0, 100.0);
            yield return null;

            // The subscene should sync up
            Assert.That(subscene.longitude, Is.EqualTo(-10.0).Using(epsilon8));
            Assert.That(subscene.latitude, Is.EqualTo(10.0).Using(epsilon8));
            Assert.That(subscene.height, Is.EqualTo(100.0).Using(epsilon8));
            yield return null;
        }

        // Change with just longitude
        {
            georeference.longitude = 40.0;
            yield return null;

            Assert.That(subscene.longitude, Is.EqualTo(40.0).Using(epsilon8));
            yield return null;
        }

        // Change with just latitude
        {
            georeference.latitude = 50.0;
            yield return null;

            Assert.That(subscene.latitude, Is.EqualTo(50.0).Using(epsilon8));
            yield return null;
        }

        // Change with just height
        {
            georeference.height = 60.0;
            yield return null;

            Assert.That(subscene.height, Is.EqualTo(60.0).Using(epsilon8));
            yield return null;
        }

        // Now a 3 component change, but ECEF
        {
            georeference.SetOriginEarthCenteredEarthFixed(-3000.0, 4000.0, 5000.0);
            yield return null;

            Assert.That(subscene.ecefX, Is.EqualTo(-3000.0).Using(epsilon8));
            Assert.That(subscene.ecefY, Is.EqualTo(4000.0).Using(epsilon8));
            Assert.That(subscene.ecefZ, Is.EqualTo(5000.0).Using(epsilon8));
            yield return null;
        }

        // Change with just X
        {
            georeference.ecefX = -3050.0;
            yield return null;

            Assert.That(subscene.ecefX, Is.EqualTo(-3050.0).Using(epsilon8));
            yield return null;
        }

        // Change with just Y
        {
            georeference.ecefX = 4050.0;
            yield return null;

            Assert.That(subscene.ecefX, Is.EqualTo(4050.0).Using(epsilon8));
            yield return null;
        }

        // Change with just Z
        {
            georeference.ecefZ = 5050.0;
            yield return null;

            Assert.That(subscene.ecefZ, Is.EqualTo(5050.0).Using(epsilon8));
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator ChangingOriginAtRuntimeUpdatesActiveSubScene()
    {
        GameObject goGeoreference = new GameObject("Georeference");
        CesiumGeoreference georeference = goGeoreference.AddComponent<CesiumGeoreference>();
        georeference.SetOriginLongitudeLatitudeHeight(-55.0, 55.0, 1000.0);

        List<GameObject> goSubscenes = new List<GameObject>();
        List<CesiumSubScene> subscenes = new List<CesiumSubScene>();

        for (int i = 0; i < 3; ++i)
        {
            GameObject newGo = new GameObject("SubScene:" + i);
            newGo.transform.parent = goGeoreference.transform;

            CesiumSubScene newSubscene = newGo.AddComponent<CesiumSubScene>();
            newSubscene.longitude = -1.0;

            goSubscenes.Add(newGo);
            subscenes.Add(newSubscene);
        }

        IEqualityComparer<double> epsilon8 = Comparers.Double(1e-8);

        yield return null;

        // Set the first subscene active, and make a change
        {
            goSubscenes[0].SetActive(true);
            georeference.longitude = 42.0;
            yield return null;

            Assert.That(subscenes[0].isActiveAndEnabled, Is.EqualTo(true));
            Assert.That(subscenes[1].isActiveAndEnabled, Is.EqualTo(false));
            Assert.That(subscenes[2].isActiveAndEnabled, Is.EqualTo(false));

            Assert.That(subscenes[0].longitude, Is.EqualTo(42.0).Using(epsilon8));
            Assert.That(subscenes[1].longitude, Is.EqualTo(-1.0).Using(epsilon8));
            Assert.That(subscenes[2].longitude, Is.EqualTo(-1.0).Using(epsilon8));
            yield return null;
        }

        // Now the second
        {
            goSubscenes[1].SetActive(true);
            georeference.longitude = 52.0;
            yield return null;

            Assert.That(subscenes[0].isActiveAndEnabled, Is.EqualTo(false));
            Assert.That(subscenes[1].isActiveAndEnabled, Is.EqualTo(true));
            Assert.That(subscenes[2].isActiveAndEnabled, Is.EqualTo(false));

            Assert.That(subscenes[0].longitude, Is.EqualTo(42.0).Using(epsilon8));
            Assert.That(subscenes[1].longitude, Is.EqualTo(52.0).Using(epsilon8));
            Assert.That(subscenes[2].longitude, Is.EqualTo(-1.0).Using(epsilon8));
            yield return null;
        }

        // And finally the third
        {
            goSubscenes[2].SetActive(true);
            georeference.longitude = 62.0;
            yield return null;

            Assert.That(subscenes[0].isActiveAndEnabled, Is.EqualTo(false));
            Assert.That(subscenes[1].isActiveAndEnabled, Is.EqualTo(false));
            Assert.That(subscenes[2].isActiveAndEnabled, Is.EqualTo(true));

            Assert.That(subscenes[0].longitude, Is.EqualTo(42.0).Using(epsilon8));
            Assert.That(subscenes[1].longitude, Is.EqualTo(52.0).Using(epsilon8));
            Assert.That(subscenes[2].longitude, Is.EqualTo(62.0).Using(epsilon8));
            yield return null;
        }
    }
}
