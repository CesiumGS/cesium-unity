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

        value = new CesiumMetadataValue(false);
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
        Assert.That(value.GetSByte(), Is.EqualTo(0));

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

        value = new CesiumMetadataValue(false);
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
        Assert.That(value.GetByte(), Is.EqualTo(0));

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

        value = new CesiumMetadataValue(false);
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
        Assert.That(value.GetInt16(), Is.EqualTo(0));

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

        value = new CesiumMetadataValue(false);
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
        Assert.That(value.GetUInt16(), Is.EqualTo(0));

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

        value = new CesiumMetadataValue(false);
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
        Assert.That(value.GetInt32(), Is.EqualTo(0));

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

        value = new CesiumMetadataValue(false);
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
        Assert.That(value.GetUInt32(), Is.EqualTo(0));

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

        value = new CesiumMetadataValue(false);
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
        CesiumMetadataValue value = new CesiumMetadataValue("NaN");
        Assert.That(value.GetInt64(), Is.EqualTo(0));
    }

    [Test]
    public void GetInt64ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt64(), Is.EqualTo(0));
    }
    #endregion

    #region GetUInt64

    [Test]
    public void GetUInt64ReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue((UInt64)1234);
        Assert.That(value.GetUInt64(), Is.EqualTo(1234));

        value = new CesiumMetadataValue((Int64)1111);
        Assert.That(value.GetUInt64(), Is.EqualTo(1111));

        value = new CesiumMetadataValue(155.55);
        Assert.That(value.GetUInt64(), Is.EqualTo(155));
    }

    [Test]
    public void GetUInt64ConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt64(111), Is.EqualTo(1));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetUInt64(111), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt64ConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetUInt64(), Is.EqualTo(123));
    }

    [Test]
    public void GetUInt64ReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MaxValue);
        Assert.That(value.GetUInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue(-1);
        Assert.That(value.GetUInt64(), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt64ReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("-1");
        Assert.That(value.GetUInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue("abc");
        Assert.That(value.GetUInt64(), Is.EqualTo(0));
    }

    [Test]
    public void GetUInt64ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new int2(1, 1));
        Assert.That(value.GetUInt64(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt64(), Is.EqualTo(0));
    }
    #endregion

    #region GetFloat

    [Test]
    public void GetFloatReturnsInRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetFloat(), Is.EqualTo(1.2345f));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetFloat(), Is.EqualTo(-12345));

        value = new CesiumMetadataValue((double)Single.MaxValue);
        Assert.That(value.GetFloat(), Is.EqualTo(Single.MaxValue));
    }

    [Test]
    public void GetFloatConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetFloat(-1.0f), Is.EqualTo(1.0f));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetFloat(-1.0f), Is.EqualTo(0.0f));
    }

    [Test]
    public void GetFloatConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetFloat(), Is.EqualTo(123.0f));
    }

    [Test]
    public void GetFloatReturnsDefaultValueForOutOfRangeNumbers()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MinValue);
        Assert.That(value.GetFloat(), Is.EqualTo(0));
    }

    [Test]
    public void GetFloatReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("abc");
        Assert.That(value.GetFloat(), Is.EqualTo(0));
    }

    [Test]
    public void GetFloatReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetFloat(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new float2(1, 1));
        Assert.That(value.GetFloat(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetFloat(), Is.EqualTo(0));
    }
    #endregion

    #region GetDouble

    [Test]
    public void GetDoubleReturnsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345);
        Assert.That(value.GetDouble(), Is.EqualTo(1.2345));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetDouble(), Is.EqualTo(-12345));

        value = new CesiumMetadataValue(Single.MaxValue);
        Assert.That(value.GetDouble(), Is.EqualTo(Single.MaxValue));
    }

    [Test]
    public void GetDoubleConvertsBooleanValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetDouble(-1.0), Is.EqualTo(1.0));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetDouble(-1.0), Is.EqualTo(0.0));
    }

    [Test]
    public void GetDoubleConvertsStringValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("123");
        Assert.That(value.GetDouble(), Is.EqualTo(123.0));
    }

    [Test]
    public void GetDoubleReturnsDefaultValueForInvalidStrings()
    {
        CesiumMetadataValue value = new CesiumMetadataValue("abc");
        Assert.That(value.GetDouble(), Is.EqualTo(0));
    }

    [Test]
    public void GetDoubleReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetDouble(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new float2(1, 1));
        Assert.That(value.GetDouble(), Is.EqualTo(0));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetDouble(), Is.EqualTo(0));
    }
    #endregion

    #region GetInt2

    [Test]
    public void GetInt2ReturnsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUIntVec2(1, 2));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(0, 1)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));
    }

    [Test]
    public void GetInt2ConvertsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUIntVec3(1, 2, 3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(0, 1)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));
    }

    [Test]
    public void GetInt2ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUIntVec4(1, 2, 3, 1));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(0, 1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));
    }

    [Test]
    public void GetInt2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-12345)));
    }

    [Test]
    public void GetInt2ConvertsBooleanValue()
    {
        int2 defaultValue = new int2(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt2(defaultValue), Is.EqualTo(new int2(1)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetInt2(defaultValue), Is.EqualTo(new int2(0)));
    }

    [Test]
    public void GetInt2ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(UInt32.MaxValue);
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(int2.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(int2.zero));
    }

    [Test]
    public void GetInt2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(int2.zero));

        value = new CesiumMetadataValue(int2x2.identity);
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(int2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(int2.zero));

    }
    #endregion

    #region GetUInt2

    [Test]
    public void GetUInt2ReturnsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUIntVec2(1, 2));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec2(10, 20));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(0, 1)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));
    }

    [Test]
    public void GetUInt2ConvertsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUIntVec3(1, 2, 3));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec3(10, 20, 30));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(0, 1)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));
    }

    [Test]
    public void GetUInt2ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUIntVec4(1, 2, 3, 1));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec4(10, 20, 30, 40));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(0, 1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));
    }

    [Test]
    public void GetUInt2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1)));

        value = new CesiumMetadataValue(12345);
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(12345)));
    }

    [Test]
    public void GetUInt2ConvertsBooleanValue()
    {
        uint2 defaultValue = new uint2(111);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt2(defaultValue), Is.EqualTo(new uint2(1)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetUInt2(defaultValue), Is.EqualTo(new uint2(0)));
    }

    [Test]
    public void GetUInt2ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Int64.MaxValue);
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(uint2.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(uint2.zero));
    }

    [Test]
    public void GetUInt2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(uint2.zero));

        value = new CesiumMetadataValue(uint2x2.identity);
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(uint2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(uint2.zero));

    }
    #endregion

    #region GetFloat2

    [Test]
    public void GetFloat2ReturnsVec2Values()
    {
        float2 float2Value = new float2(0.5f, 1.2f);
        CesiumMetadataValue value = new CesiumMetadataValue(float2Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float2Value));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUIntVec2(1, 2));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));

        double2 double2Value = new double2(1.2, 2.3);
        value = new CesiumMetadataValue(double2Value);
        Assert.That(
            value.GetFloat2(float2.zero), Is.EqualTo(new float2((float)double2Value.x, (float)double2Value.y)));
    }

    [Test]
    public void GetFloat2ConvertsVec3Values()
    {
        float3 float3Value = new float3(0.5f, 1.2f, -1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float3Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float3Value.xy));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUIntVec3(1, 2, 3));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));

        double3 double3Value = new double3(1.2, 2.3, 3.4);
        value = new CesiumMetadataValue(double3Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2((float)double3Value.x, (float)double3Value.y)));
    }

    [Test]
    public void GetFloat2ConvertsVec4Values()
    {
        float4 float4Value = new float4(0.5f, 1.2f, -1.0f, 1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float4Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float4Value.xy));

        CesiumIntVec4 intVec4Value = new CesiumIntVec4(-1, -2, -3, 1);
        value = new CesiumMetadataValue(intVec4Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(intVec4Value.x, intVec4Value.y)));

        CesiumUIntVec4 uintVec4Value = new CesiumUIntVec4(1, 2, 3, 1);
        value = new CesiumMetadataValue(uintVec4Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(uintVec4Value.x, uintVec4Value.y)));

        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        value = new CesiumMetadataValue(double4Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2((float)double4Value.x, (float)double4Value.y)));
    }

    [Test]
    public void GetFloat2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-12345)));

        value = new CesiumMetadataValue(1234.0);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2((float)1234.0)));
    }

    [Test]
    public void GetFloat2ConvertsBooleanValue()
    {
        float2 defaultValue = new float2(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetFloat2(defaultValue), Is.EqualTo(new float2(1.0f)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetFloat2(defaultValue), Is.EqualTo(new float2(0.0f)));
    }

    [Test]
    public void GetFloat2ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MinValue);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float2.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float2.zero));
    }

    [Test]
    public void GetFloat2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float2.zero));

        value = new CesiumMetadataValue(float2x2.identity);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float2.zero));
    }
    #endregion

    #region GetDouble2

    [Test]
    public void GetDouble2ReturnsVec2Values()
    {
        double2 double2Value = new double2(1.2, 2.3);
        CesiumMetadataValue value = new CesiumMetadataValue(double2Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(double2Value));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUIntVec2(1, 2));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(1, 2)));

        float2 float2Value = new float2(0.5f, 1.2f);
        value = new CesiumMetadataValue(float2Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(float2Value.x, float2Value.y)));
    }

    [Test]
    public void GetDouble2ConvertsVec3Values()
    {
        double3 double3Value = new double3(1.2, 2.3, 3.4);
        CesiumMetadataValue value = new CesiumMetadataValue(double3Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(double3Value.xy));

        CesiumIntVec3 intVec3Value = new CesiumIntVec3(-1, -2, -3);
        value = new CesiumMetadataValue(intVec3Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(intVec3Value.x, intVec3Value.y)));

        CesiumUIntVec3 uintVec3Value = new CesiumUIntVec3(1, 2, 3);
        value = new CesiumMetadataValue(uintVec3Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(uintVec3Value.x, uintVec3Value.y)));

        float3 float3Value = new float3(0.5f, 1.2f, -1.0f);
        value = new CesiumMetadataValue(float3Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(float3Value.x, float3Value.y)));
    }

    [Test]
    public void GetDouble2ConvertsVec4Values()
    {
        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        CesiumMetadataValue value = new CesiumMetadataValue(double4Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(double4Value.xy));

        CesiumIntVec4 intVec4Value = new CesiumIntVec4(-1, -2, -3, 1);
        value = new CesiumMetadataValue(intVec4Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(intVec4Value.x, intVec4Value.y)));

        CesiumUIntVec4 uintVec4Value = new CesiumUIntVec4(1, 2, 3, 1);
        value = new CesiumMetadataValue(uintVec4Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(uintVec4Value.x, uintVec4Value.y)));

        float4 float4Value = new float4(0.5f, 1.2f, -1.0f, 1.0f);
        value = new CesiumMetadataValue(float4Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(float4Value.x, float4Value.y)));
    }

    [Test]
    public void GetDouble2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(1.2345)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-12345)));

        value = new CesiumMetadataValue(Single.MaxValue);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(Single.MaxValue)));
    }

    [Test]
    public void GetDouble2ConvertsBooleanValue()
    {
        double2 defaultValue = new double2(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetDouble2(defaultValue), Is.EqualTo(new double2(1.0)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetDouble2(defaultValue), Is.EqualTo(new double2(0.0)));
    }


    [Test]
    public void GetDouble2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(double2.zero));

        value = new CesiumMetadataValue(float2x2.identity);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(double2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(double2.zero));
    }
    #endregion

    #region GetInt3

    [Test]
    public void GetInt3ReturnsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUIntVec3(1, 2, 3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(0, 1, -1)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));
    }

    [Test]
    public void GetInt3ConvertsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, 0)));

        value = new CesiumMetadataValue(new CesiumUIntVec2(1, 2));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(0, 1, 0)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 0)));
    }

    [Test]
    public void GetInt3ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUIntVec4(1, 2, 3, 1));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(0, 1, -1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));
    }

    [Test]
    public void GetInt3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-12345)));
    }

    [Test]
    public void GetInt3ConvertsBooleanValue()
    {
        int3 defaultValue = new int3(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt3(defaultValue), Is.EqualTo(new int3(1)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetInt3(defaultValue), Is.EqualTo(new int3(0)));
    }

    [Test]
    public void GetInt3ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(UInt32.MaxValue);
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(int3.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(int3.zero));
    }

    [Test]
    public void GetInt3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(int3.zero));

        value = new CesiumMetadataValue(int3x3.identity);
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(int3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(int3.zero));

    }
    #endregion

    #region GetUInt3

    [Test]
    public void GetUInt3ReturnsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUIntVec3(1, 2, 3));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));

        value = new CesiumMetadataValue(new CesiumIntVec3(10, 20, 30));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 30)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, 11.0f));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(0, 1, 11)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));
    }

    [Test]
    public void GetUInt3ConvertsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUIntVec2(1, 2));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec2(10, 20));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(0, 1, 0)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 0)));
    }

    [Test]
    public void GetUInt3ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUIntVec4(1, 2, 3, 1));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));

        value = new CesiumMetadataValue(new CesiumIntVec4(10, 20, 30, 1));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 30)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, 21.0f, 1.0f));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(0, 1, 21)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));
    }

    [Test]
    public void GetUInt3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1)));

        value = new CesiumMetadataValue(12345);
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(12345)));
    }

    [Test]
    public void GetUInt3ConvertsBooleanValue()
    {
        uint3 defaultValue = new uint3(11);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt3(defaultValue), Is.EqualTo(new uint3(1)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetUInt3(defaultValue), Is.EqualTo(new uint3(0)));
    }

    [Test]
    public void GetUInt3ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Int64.MaxValue);
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(uint3.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(uint3.zero));
    }

    [Test]
    public void GetUInt3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(uint3.zero));

        value = new CesiumMetadataValue(int3x3.identity);
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(uint3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(uint3.zero));

    }
    #endregion

    #region GetFloat3

    [Test]
    public void GetFloat3ReturnsVec3Values()
    {
        float3 float3Value = new float3(0.5f, 1.2f, -1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float3Value);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(float3Value));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, -3)));

        CesiumUIntVec3 uintVec3Value = new CesiumUIntVec3(1, 2, 3);
        value = new CesiumMetadataValue(uintVec3Value);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 3)));

        double3 double3Value = new double3(1.2, 2.3, 3.4);
        value = new CesiumMetadataValue(double3Value);
        Assert.That(
            value.GetFloat3(float3.zero),
            Is.EqualTo(new float3((float)double3Value.x, (float)double3Value.y, (float)double3Value.z)));
    }

    [Test]
    public void GetFloat3ConvertsVec2Values()
    {
        float2 float2Value = new float2(0.5f, 1.2f);
        CesiumMetadataValue value = new CesiumMetadataValue(float2Value);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(float2Value, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, 0)));

        value = new CesiumMetadataValue(new CesiumUIntVec2(1, 2));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 0)));

        double2 double2Value = new double2(1.2, 2.3);
        value = new CesiumMetadataValue(double2Value);
        Assert.That(
            value.GetFloat3(float3.zero),
            Is.EqualTo(new float3((float)double2Value.x, (float)double2Value.y, 0)));
    }

    [Test]
    public void GetFloat3ConvertsVec4Values()
    {
        float4 float4Value = new float4(0.5f, 1.2f, -1.0f, 1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float4Value);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(float4Value.xyz));

        CesiumIntVec4 intVec4Value = new CesiumIntVec4(-1, -2, -3, 1);
        value = new CesiumMetadataValue(intVec4Value);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, -3)));

        CesiumUIntVec4 uintVec4Value = new CesiumUIntVec4(1, 2, 3, 1);
        value = new CesiumMetadataValue(uintVec4Value);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 3)));

        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        value = new CesiumMetadataValue(double4Value);
        Assert.That(
            value.GetFloat3(float3.zero),
            Is.EqualTo(new float3((float)double4Value.x, (float)double4Value.y, (float)double4Value.z)));
    }

    [Test]
    public void GetFloat3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-12345)));

        value = new CesiumMetadataValue(1234.0);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3((float)1234.0)));
    }

    [Test]
    public void GetFloat3ConvertsBooleanValue()
    {
        float3 defaultValue = new float3(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetFloat3(defaultValue), Is.EqualTo(new float3(1.0f)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetFloat3(defaultValue), Is.EqualTo(new float3(0.0f)));
    }

    [Test]
    public void GetFloat3ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MinValue);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(float3.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(float3.zero));
    }

    [Test]
    public void GetFloat3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(float3.zero));

        value = new CesiumMetadataValue(float3x3.identity);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(float3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(float3.zero));
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
