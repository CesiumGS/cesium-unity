using CesiumForUnity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

public class TestCesiumMetadataValue
{
    [Test]
    public void ConstructsEmptyValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.isEmpty, Is.True);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Invalid));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);
    }

    [Test]
    public void ConstructsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(false);
        Assert.That(value.isEmpty, Is.False);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);
    }

    [Test]
    public void ConstructsScalarValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((UInt16)10);
        Assert.That(value.isEmpty, Is.False);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Uint16));
        Assert.That(valueType.isArray, Is.False);
    }
}
