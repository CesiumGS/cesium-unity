using CesiumForUnity;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

public class TestCesiumCreditSystem
{
    [UnityTest]
    public IEnumerator DisabledCreditSystemHasNonNullArrays()
    {
        GameObject goCreditSystem = new GameObject();
        goCreditSystem.SetActive(false);
        CesiumCreditSystem creditSystem = goCreditSystem.AddComponent<CesiumCreditSystem>();

        yield return null;

        Assert.IsFalse(creditSystem.isActiveAndEnabled);
        Assert.IsNotNull(creditSystem.onScreenCredits);
        Assert.IsNotNull(creditSystem.popupCredits);
        Assert.IsNotNull(creditSystem.images);
    }

    [UnityTest]
    public IEnumerator DisabledCreditSystemDestroysWithoutNullReferenceException()
    {
        GameObject goCreditSystem = new GameObject();
        goCreditSystem.SetActive(false);
        CesiumCreditSystem creditSystem = goCreditSystem.AddComponent<CesiumCreditSystem>();

        yield return null;

        Assert.IsFalse(creditSystem.isActiveAndEnabled);
        UnityEngine.Object.Destroy(goCreditSystem);
    }
}