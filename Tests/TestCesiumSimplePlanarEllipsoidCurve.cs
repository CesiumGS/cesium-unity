using CesiumForUnity;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

public class TestCesiumSimplePlanarEllipsoidCurve
{
    private readonly double3 _philadelphiaEcef = new double3(1253264.69280105, -4732469.91065521, 4075112.40412297);
    private readonly double3 _tokyoEcef = new double3(-3960158.65587452, 3352568.87555906, 3697235.23506459);

    private double3 RelativeToAbsoluteEpsilon(double3 left, double3 right, double relativeEpsilon)
    {
        return new double3(relativeEpsilon) * math.max(math.abs(left), math.abs(right));
    }

    [UnityTest]
    public IEnumerator StartAndEndOfPathAreIdenticalToInput()
    {
        CesiumSimplePlanarEllipsoidCurve flightPath =
            CesiumSimplePlanarEllipsoidCurve.FromEarthCenteredEarthFixedCoordinates(_philadelphiaEcef, _tokyoEcef);
        Assert.IsNotNull(flightPath);

        double3 startPosition = flightPath.GetPosition(0.0);
        double3 endPosition = flightPath.GetPosition(1.0);

        IEqualityComparer<double> doubleComparer = Comparers.Double(1e-6);
        Assert.That(startPosition.x, Is.EqualTo(_philadelphiaEcef.x).Using(doubleComparer));
        Assert.That(startPosition.y, Is.EqualTo(_philadelphiaEcef.y).Using(doubleComparer));
        Assert.That(startPosition.z, Is.EqualTo(_philadelphiaEcef.z).Using(doubleComparer));

        Assert.That(endPosition.x, Is.EqualTo(_tokyoEcef.x).Using(doubleComparer));
        Assert.That(endPosition.y, Is.EqualTo(_tokyoEcef.y).Using(doubleComparer));
        Assert.That(endPosition.z, Is.EqualTo(_tokyoEcef.z).Using(doubleComparer));

        yield break;
    }

    [UnityTest]
    public IEnumerator ShouldCalculateMidpointCorrectly()
    {
        CesiumSimplePlanarEllipsoidCurve flightPath =
            CesiumSimplePlanarEllipsoidCurve.FromEarthCenteredEarthFixedCoordinates(_philadelphiaEcef, _tokyoEcef);
        Assert.IsNotNull(flightPath);

        double3 expectedResult = new double3(
            -2062499.3622640674,
            -1052346.4221710551,
            5923430.4378960524);
        double3 actualResult = flightPath.GetPosition(0.5);
        double3 epsilons = RelativeToAbsoluteEpsilon(expectedResult, actualResult, 1.0e-6);

        IEqualityComparer<double> xComparer = Comparers.Double(epsilons.x);
        IEqualityComparer<double> yComparer = Comparers.Double(epsilons.y);
        IEqualityComparer<double> zComparer = Comparers.Double(epsilons.z);
        Assert.That(actualResult.x, Is.EqualTo(expectedResult.x).Using(xComparer));
        Assert.That(actualResult.y, Is.EqualTo(expectedResult.y).Using(yComparer));
        Assert.That(actualResult.z, Is.EqualTo(expectedResult.z).Using(zComparer));

        yield break;
    }
}
