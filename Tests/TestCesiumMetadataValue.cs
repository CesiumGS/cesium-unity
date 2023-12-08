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

        value = new CesiumMetadataValue(-1.99f);
        Assert.That(value.GetSByte(), Is.EqualTo(-1));
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

        value = new CesiumMetadataValue(10000.0);
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

    #region GetByte
    [Test]
    public void GetByteReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((Byte)255);
        Assert.That(value.GetByte(), Is.EqualTo(255));

        value = new CesiumMetadataValue((Int64)128);
        Assert.That(value.GetByte(), Is.EqualTo(128));

        value = new CesiumMetadataValue(155.55);
        Assert.That(value.GetByte(), Is.EqualTo(155));
    }

    [Test]
    public void GetByteConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetByte(255), Is.EqualTo(1));

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetByte(255), Is.EqualTo(0));
    }

    [Test]
    public void GetByteConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetByte(), Is.EqualTo(123));
    }

    [Test]
    public void GetByteReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(123456);
        Assert.That(value.GetByte(), Is.EqualTo(0));

        value = new CesiumMetadataValue(-1);
        Assert.That(value.GetByte(), Is.EqualTo(0));

        value = new CesiumMetadataValue(256.0);
        Assert.That(value.GetByte(), Is.EqualTo(0));
    }

    [Test]
    public void GetByteReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123456");
        Assert.That(value.GetByte(), Is.EqualTo(0));

        value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetByte(), Is.EqualTo(0));
    }

    [Test]
    public void GetByteReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetByte(0), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetByte(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetByte(), Is.EqualTo(0));
    }
    #endregion

    #region GetInt16

    [Test]
    public void GetInt16ReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((Int16)1234);
        Assert.That(value.GetInt16(), Is.EqualTo(1234));

        value = new CesiumMetadataValue((UInt64)1111);
        Assert.That(value.GetInt16(), Is.EqualTo(1111));

        value = new CesiumMetadataValue(-155.55);
        Assert.That(value.GetInt16(), Is.EqualTo(-155));
    }

    [Test]
    public void GetInt16ConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt16(-1), Is.EqualTo(1));

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetInt16(-1), Is.EqualTo(0));
    }

    [Test]
    public void GetInt16ConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetInt16(), Is.EqualTo(123));
    }

    [Test]
    public void GetInt16ReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Int32.MaxValue);
        Assert.That(value.GetInt16(), Is.EqualTo(0));

        value = new CesiumMetadataValue(Single.MinValue);
        Assert.That(value.GetInt16(), Is.EqualTo(0));
    }

    [Test]
    public void GetInt16ReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("-12345678890");
        Assert.That(value.GetInt16(), Is.EqualTo(0));

        value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetInt16(), Is.EqualTo(0));
    }

    [Test]
    public void GetInt16ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt16(0), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetInt16(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt16(), Is.EqualTo(0));
    }
    #endregion

    #region GetUInt16

    [Test]
    public void GetUInt16ReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((UInt16)1234);
        Assert.That(value.GetUInt16(), Is.EqualTo(1234));

        value = new CesiumMetadataValue((Int64)1111);
        Assert.That(value.GetUInt16(), Is.EqualTo(1111));

        value = new CesiumMetadataValue(155.55);
        Assert.That(value.GetUInt16(), Is.EqualTo(155));
    }

    [Test]
    public void GetUInt16ConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt16(111), Is.EqualTo(1));

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetUInt16(111), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt16ConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetUInt16(), Is.EqualTo(123));
    }

    [Test]
    public void GetUInt16ReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(-1);
        Assert.That(value.GetUInt16(), Is.EqualTo(0));

        value = new CesiumMetadataValue(Single.MaxValue);
        Assert.That(value.GetUInt16(), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt16ReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("-12345678890");
        Assert.That(value.GetUInt16(), Is.EqualTo(0));

        value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetUInt16(), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt16ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt16(0), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetUInt16(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt16(), Is.EqualTo(0));
    }
    #endregion

    #region GetInt32

    [Test]
    public void GetInt32ReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((Int32)1234);
        Assert.That(value.GetInt32(), Is.EqualTo(1234));

        value = new CesiumMetadataValue((UInt64)1111);
        Assert.That(value.GetInt32(), Is.EqualTo(1111));

        value = new CesiumMetadataValue(-155.55);
        Assert.That(value.GetInt32(), Is.EqualTo(-155));
    }

    [Test]
    public void GetInt32ConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt32(-1), Is.EqualTo(1));

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetInt32(-1), Is.EqualTo(0));
    }

    [Test]
    public void GetInt32ConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetInt32(), Is.EqualTo(123));
    }

    [Test]
    public void GetInt32ReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Int64.MaxValue);
        Assert.That(value.GetInt32(), Is.EqualTo(0));

        value = new CesiumMetadataValue(Single.MinValue);
        Assert.That(value.GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public void GetInt32ReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("-12345678890");
        Assert.That(value.GetInt32(), Is.EqualTo(0));

        value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public void GetInt32ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt32(0), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetInt32(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt32(), Is.EqualTo(0));
    }
    #endregion

    #region GetUInt32

    [Test]
    public void GetUInt32ReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((UInt32)1234);
        Assert.That(value.GetUInt32(), Is.EqualTo(1234));

        value = new CesiumMetadataValue((Int64)1111);
        Assert.That(value.GetUInt32(), Is.EqualTo(1111));

        value = new CesiumMetadataValue(155.55);
        Assert.That(value.GetUInt32(), Is.EqualTo(155));
    }

    [Test]
    public void GetUInt32ConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt32(111), Is.EqualTo(1));

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetUInt32(111), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt32ConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetUInt32(), Is.EqualTo(123));
    }

    [Test]
    public void GetUInt32ReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(-1);
        Assert.That(value.GetUInt32(), Is.EqualTo(0));

        value = new CesiumMetadataValue(Single.MaxValue);
        Assert.That(value.GetUInt32(), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt32ReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("-12345678890");
        Assert.That(value.GetUInt32(), Is.EqualTo(0));

        value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetUInt32(), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt32ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt32(0), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetUInt32(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt32(), Is.EqualTo(0));
    }
    #endregion

    #region GetInt64

    [Test]
    public void GetInt64ReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((Int64)1234);
        Assert.That(value.GetInt64(), Is.EqualTo(1234));

        value = new CesiumMetadataValue((UInt64)1111);
        Assert.That(value.GetInt64(), Is.EqualTo(1111));

        value = new CesiumMetadataValue(-155.55);
        Assert.That(value.GetInt64(), Is.EqualTo(-155));
    }

    [Test]
    public void GetInt64ConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt64(-1), Is.EqualTo(1));

        value = new CesiumMetadataValue(0);
        Assert.That(value.GetInt64(-1), Is.EqualTo(0));
    }

    [Test]
    public void GetInt64ConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetInt64(), Is.EqualTo(123));
    }

    [Test]
    public void GetInt64ReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(UInt64.MaxValue);
        Assert.That(value.GetInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue(Double.MinValue);
        Assert.That(value.GetInt64(), Is.EqualTo(0));
    }

    [Test]
    public void GetInt64ReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("-12345678890");
        Assert.That(value.GetInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetInt64(), Is.EqualTo(0));
    }

    [Test]
    public void GetInt64ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt64(0), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt64(), Is.EqualTo(0));
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
