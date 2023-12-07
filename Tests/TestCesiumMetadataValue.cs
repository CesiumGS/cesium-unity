using CesiumForUnity;
using NUnit.Framework;
using System;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

public class TestCesiumMetadataValue
{
    #region Constructor
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

    [Test]
    public void ConstructsVecNValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.isEmpty, Is.False);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);
    }

    [Test]
    public void ConstructsMatNValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint2x2(1, 2, 3, 1));
        Assert.That(value.isEmpty, Is.False);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Uint32));
        Assert.That(valueType.isArray, Is.False);
    }

    [Test]
    public void ConstructsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("string");
        Assert.That(value.isEmpty, Is.False);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);
    }

    #endregion

    #region GetBoolean
    [Test]
    public void GetBooleanReturnsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetBoolean(), Is.True);
    }

    [Test]
    public void GetsBooleanConvertsScalarValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1234);
        Assert.That(value.GetBoolean(), Is.True);

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetBoolean(), Is.False);
    }

    [Test]
    public void GetBooleanConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("true");
        Assert.That(value.GetBoolean(), Is.True);

        value = new CesiumMetadataValue("false");
        Assert.That(value.GetBoolean(), Is.False);

        value = new CesiumMetadataValue("yes");
        Assert.That(value.GetBoolean(), Is.True);

        value = new CesiumMetadataValue("no");
        Assert.That(value.GetBoolean(), Is.False);

        value = new CesiumMetadataValue("1");
        Assert.That(value.GetBoolean(), Is.True);

        value = new CesiumMetadataValue("0");
        Assert.That(value.GetBoolean(), Is.False);
    }

    [Test]
    public void GetBooleanReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("I am true");
        Assert.That(value.GetBoolean(), Is.False);

        value = new CesiumMetadataValue("11");
        Assert.That(value.GetBoolean(), Is.False);
    }

    [Test]
    public void GetBooleanReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetBoolean(true), Is.True);

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetBoolean(false), Is.False);

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetBoolean(false), Is.False);
    }
    #endregion

    #region GetSByte
    [Test]
    public void GetSByteReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((SByte)126);
        Assert.That(value.GetSByte(), Is.EqualTo(126));

        value = new CesiumMetadataValue((Int32)(-127));
        Assert.That(value.GetSByte(), Is.EqualTo(-127));
    }

    [Test]
    public void GetSByteConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetSByte(-1), Is.EqualTo(1));

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetSByte(-1), Is.EqualTo(0));
    }

    [Test]
    public void GetSByteConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetSByte(), Is.EqualTo(123));
    }

    [Test]
    public void GetSByteReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(123456);
        Assert.That(value.GetSByte(), Is.EqualTo(0));

        value = new CesiumMetadataValue(-129);
        Assert.That(value.GetSByte(), Is.EqualTo(0));
    }

    [Test]
    public void GetSByteReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123456");
        Assert.That(value.GetSByte(), Is.EqualTo(0));

        value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetSByte(), Is.EqualTo(0));
    }

    [Test]
    public void GetSByteReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetSByte(0), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetSByte(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetSByte(), Is.EqualTo(0));
    }
    #endregion

    #region GetString
    [Test]
    public void GetStringReturnsStringValue()
    {
        String str = "string";
        CesiumMetadataValue value = new CesiumMetadataValue(str);
        Assert.That(value.GetString(), Is.EqualTo(str));
    }


    [Test]
    public void GetStringConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetString(), Is.EqualTo("true"));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetString(), Is.EqualTo("false"));
    }

    [Test]
    public void GetStringConvertsScalarValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1234);
        Assert.That(value.GetString(), Is.EqualTo("1234"));
    }

    [Test]
    public void GetStringReturnsDefaultValueForUnsupportedTypes()
    {
        String defaultValue = "default";
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetString(defaultValue), Is.EqualTo(defaultValue));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetString(defaultValue), Is.EqualTo(defaultValue));
    }
    #endregion
}
