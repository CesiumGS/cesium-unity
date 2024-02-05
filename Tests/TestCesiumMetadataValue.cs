using CesiumForUnity;
using NUnit.Framework;
using System;
using Unity.Mathematics;

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

        value = new CesiumMetadataValue(new CesiumUintVec4((Byte)1, (Byte)1, (Byte)2, (Byte)3));
        Assert.That(value.isEmpty, Is.False);

        valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Uint8));
        Assert.That(valueType.isArray, Is.False);
    }

    [Test]
    public void ConstructsMatNValue()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new float2x2(1, 2, 3, 1));
        Assert.That(value.isEmpty, Is.False);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
            new int3(1, 0, 0),
            new int3(0, 1, 0),
            new int3(0, 0, 1)));
        Assert.That(value.isEmpty, Is.False);

        valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
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

    [Test]
    public void ConstructsArrayValue()
    {
        CesiumPropertyArray array = new CesiumPropertyArray();
        array.elementValueType = new CesiumMetadataValueType(CesiumMetadataType.Scalar, CesiumMetadataComponentType.Uint8, false);

        CesiumMetadataValue value = new CesiumMetadataValue(array);
        Assert.That(value.isEmpty, Is.False);

        CesiumMetadataValueType valueType = value.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Uint8));
        Assert.That(valueType.isArray, Is.True);
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
    public void GetBooleanConvertsScalarValue()
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
        CesiumMetadataValue value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(0, 1)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));
    }

    [Test]
    public void GetInt2ConvertsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(0, 1)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));
    }

    [Test]
    public void GetInt2ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(0, 1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));
    }

    [Test]
    public void GetInt2ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetInt2(int2.zero), Is.EqualTo(new int2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
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
        CesiumMetadataValue value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new int2(10, 20));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(0, 1)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));
    }

    [Test]
    public void GetUInt2ConvertsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new int3(10, 20, 30));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(0, 1)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));
    }

    [Test]
    public void GetUInt2ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new int4(10, 20, 30, 40));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(0, 1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));
    }

    [Test]
    public void GetUInt2ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec2(10, 20));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec3(10, 20, 30));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec4(10, 20, 30, 40));
        Assert.That(value.GetUInt2(uint2.zero), Is.EqualTo(new uint2(10, 20)));
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

        value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(
            value.GetFloat2(float2.zero), Is.EqualTo(new float2((float)1.2, (float)2.3)));
    }

    [Test]
    public void GetFloat2ConvertsVec3Values()
    {
        float3 float3Value = new float3(0.5f, 1.2f, -1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float3Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float3Value.xy));

        value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2((float)1.2, (float)2.3)));
    }

    [Test]
    public void GetFloat2ConvertsVec4Values()
    {
        float4 float4Value = new float4(0.5f, 1.2f, -1.0f, 1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float4Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(float4Value.xy));

        value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));

        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        value = new CesiumMetadataValue(double4Value);
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2((float)double4Value.x, (float)double4Value.y)));
    }

    [Test]
    public void GetFloat2ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetFloat2(float2.zero), Is.EqualTo(new float2(1, 2)));
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

        value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-1, -2)));

        value = new CesiumMetadataValue(new uint2(1, 2));
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

        value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-1, -2)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(1, 2)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(0.5f, 1.2f)));
    }

    [Test]
    public void GetDouble2ConvertsVec4Values()
    {
        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        CesiumMetadataValue value = new CesiumMetadataValue(double4Value);
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(double4Value.xy));

        value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-1, -2)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(1, 2)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(0.5f, 1.2f)));
    }

    [Test]
    public void GetDouble2ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(1, 2)));

        value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(-1, -2)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetDouble2(double2.zero), Is.EqualTo(new double2(1, 2)));
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
        CesiumMetadataValue value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, -3)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(0, 1, -1)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));
    }

    [Test]
    public void GetInt3ConvertsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, 0)));

        value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(0, 1, 0)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 0)));
    }

    [Test]
    public void GetInt3ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, -3)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(0, 1, -1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));
    }

    [Test]
    public void GetInt3ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 3)));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(1, 2, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetInt3(int3.zero), Is.EqualTo(new int3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
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
        CesiumMetadataValue value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));

        value = new CesiumMetadataValue(new int3(10, 20, 30));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 30)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, 11.0f));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(0, 1, 11)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));
    }

    [Test]
    public void GetUInt3ConvertsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 0)));

        value = new CesiumMetadataValue(new int2(10, 20));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(0, 1, 0)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 0)));
    }

    [Test]
    public void GetUInt3ConvertsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));

        value = new CesiumMetadataValue(new int4(10, 20, 30, 1));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 30)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, 21.0f, 1.0f));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(0, 1, 21)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));
    }

    [Test]
    public void GetUInt3ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));

        value = new CesiumMetadataValue(new CesiumIntVec3(10, 20, 30));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 30)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec2(10, 20));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(1, 2, 3)));

        value = new CesiumMetadataValue(new CesiumIntVec4(10, 20, 30, 1));
        Assert.That(value.GetUInt3(uint3.zero), Is.EqualTo(new uint3(10, 20, 30)));
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

        value = new CesiumMetadataValue(uint3x3.identity);
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

        value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, -3)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 3)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(
            value.GetFloat3(float3.zero),
            Is.EqualTo(new float3((float)1.2, (float)2.3, (float)3.4)));
    }

    [Test]
    public void GetFloat3ConvertsVec2Values()
    {
        float2 float2Value = new float2(0.5f, 1.2f);
        CesiumMetadataValue value = new CesiumMetadataValue(float2Value);
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(float2Value, 0)));

        value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, 0)));

        value = new CesiumMetadataValue(new uint2(1, 2));
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

        value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 3)));

        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        value = new CesiumMetadataValue(double4Value);
        Assert.That(
            value.GetFloat3(float3.zero),
            Is.EqualTo(new float3((float)double4Value.x, (float)double4Value.y, (float)double4Value.z)));
    }

    [Test]
    public void GetFloat3ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 3)));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetFloat3(float3.zero), Is.EqualTo(new float3(1, 2, 3)));
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

    #region GetDouble3

    [Test]
    public void GetDouble3ReturnsVec3Values()
    {
        double3 double3Value = new double3(1.2, 2.3, 3.4);
        CesiumMetadataValue value = new CesiumMetadataValue(double3Value);
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(double3Value));

        value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(-1, -2, -3)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(1, 2, 3)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(
            value.GetDouble3(double3.zero), Is.EqualTo(new double3(0.5f, 1.2f, -1.0f)));
    }

    [Test]
    public void GetDouble3ConvertsVec2Values()
    {
        double2 double2Value = new double2(1.2, 2.3);
        CesiumMetadataValue value = new CesiumMetadataValue(double2Value);
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(double2Value, 0)));

        value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(-1, -2, 0)));

        value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(1, 2, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(0.5f, 1.2f, 0.0f)));
    }

    [Test]
    public void GetDouble3ConvertsVec4Values()
    {
        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        CesiumMetadataValue value = new CesiumMetadataValue(double4Value);
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(double4Value.xyz));

        value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(-1, -2, -3)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(1, 2, 3)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(0.5f, 1.2f, -1.0f)));
    }

    [Test]
    public void GetDouble3ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(1, 2, 3)));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(-1, -2, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(1, 2, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(-1, -2, -3)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(1, 2, 3)));
    }

    [Test]
    public void GetDouble3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345);
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(1.2345)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(-12345)));

        value = new CesiumMetadataValue(Single.MaxValue);
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(new double3(Single.MaxValue)));
    }

    [Test]
    public void GetDouble3ConvertsBooleanValue()
    {
        double3 defaultValue = new double3(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetDouble3(defaultValue), Is.EqualTo(new double3(1.0)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetDouble3(defaultValue), Is.EqualTo(new double3(0.0)));
    }


    [Test]
    public void GetDouble3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(double3.zero));

        value = new CesiumMetadataValue(double3x3.identity);
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(double3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetDouble3(double3.zero), Is.EqualTo(double3.zero));
    }
    #endregion

    #region GetInt4

    [Test]
    public void GetInt4ReturnsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(-1, -2, -3, 1)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(0, 1, -1, 1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 3, 1)));
    }

    [Test]
    public void GetInt4ConvertsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(-1, -2, 0, 0)));

        value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(0, 1, 0, 0)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 0, 0)));
    }

    [Test]
    public void GetInt4ConvertsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(-1, -2, -3, 0)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 3, 0)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(0, 1, -1, 0)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 3, 0)));
    }

    [Test]
    public void GetInt4ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(-1, -2, -3, 1)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(-1, -2, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(-1, -2, -3, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1, 2, 3, 0)));
    }

    [Test]
    public void GetInt4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(1)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(new int4(-12345)));
    }

    [Test]
    public void GetInt4ConvertsBooleanValue()
    {
        int4 defaultValue = new int4(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt4(defaultValue), Is.EqualTo(new int4(1)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetInt4(defaultValue), Is.EqualTo(new int4(0)));
    }

    [Test]
    public void GetInt4ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(UInt32.MaxValue);
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(int4.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(int4.zero));
    }

    [Test]
    public void GetInt4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(int4.zero));

        value = new CesiumMetadataValue(int4x4.identity);
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(int4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt4(int4.zero), Is.EqualTo(int4.zero));
    }
    #endregion

    #region GetUInt4

    [Test]
    public void GetUInt4ReturnsVec4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new int4(10, 20, 30, 1));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(10, 20, 30, 1)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, 21.0f, 1.0f));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(0, 1, 21, 1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 3, 1)));
    }

    [Test]
    public void GetUInt4ConvertsVec2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new int2(10, 20));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(10, 20, 0, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(0, 1, 0, 0)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 0, 0)));
    }

    [Test]
    public void GetUInt4ConvertsVec3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 3, 0)));

        value = new CesiumMetadataValue(new int3(10, 20, 30));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(10, 20, 30, 0)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, 11.0f));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(0, 1, 11, 0)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 3, 0)));
    }

    [Test]
    public void GetUInt4ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new CesiumIntVec4(10, 20, 30, 1));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(10, 20, 30, 1)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec2(10, 20));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(10, 20, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1, 2, 3, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec3(10, 20, 30));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(10, 20, 30, 0)));
    }

    [Test]
    public void GetUInt4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(1)));

        value = new CesiumMetadataValue(12345);
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(new uint4(12345)));
    }

    [Test]
    public void GetUInt4ConvertsBooleanValue()
    {
        uint4 defaultValue = new uint4(11);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt4(defaultValue), Is.EqualTo(new uint4(1)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetUInt4(defaultValue), Is.EqualTo(new uint4(0)));
    }

    [Test]
    public void GetUInt4ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Int64.MaxValue);
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(uint4.zero));

        value = new CesiumMetadataValue(new double2(1.0, -1));
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(uint4.zero));
    }

    [Test]
    public void GetUInt4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(uint4.zero));

        value = new CesiumMetadataValue(uint3x3.identity);
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(uint4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt4(uint4.zero), Is.EqualTo(uint4.zero));
    }
    #endregion

    #region GetFloat4

    [Test]
    public void GetFloat4ReturnsVec4Values()
    {
        float4 float4Value = new float4(0.5f, 1.2f, -1.0f, 1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float4Value);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(float4Value));

        value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(-1, -2, -3, 1)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new double4(1.2, 2.3, 3.4, 1.0));
        Assert.That(
            value.GetFloat4(float4.zero),
            Is.EqualTo(new float4((float)1.2, (float)2.3, (float)3.4, (float)1.0)));
    }

    [Test]
    public void GetFloat4ConvertsVec2Values()
    {
        float2 float2Value = new float2(0.5f, 1.2f);
        CesiumMetadataValue value = new CesiumMetadataValue(float2Value);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(float2Value, 0, 0)));

        value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(-1, -2, 0, 0)));

        value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new double2(1.2, 2.3));
        Assert.That(
            value.GetFloat4(float4.zero), Is.EqualTo(new float4((float)1.2, (float)2.3, 0, 0)));
    }

    [Test]
    public void GetFloat4ConvertsVec3Values()
    {
        float3 float3Value = new float3(0.5f, 1.2f, -1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float3Value);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(float3Value, 0)));

        value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(-1, -2, -3, 0)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1, 2, 3, 0)));

        value = new CesiumMetadataValue(new double3(1.2, 2.3, 3.4));
        Assert.That(
            value.GetFloat4(float4.zero), Is.EqualTo(new float4((float)1.2, (float)2.3, (float)3.4, 0)));
    }

    [Test]
    public void GetFloat4ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(-1, -2, -3, 1)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(-1, -2, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(-1, -2, -3, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1, 2, 3, 0)));
    }

    [Test]
    public void GetFloat4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(-12345)));

        value = new CesiumMetadataValue(1234.0);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(new float4(1234.0)));
    }

    [Test]
    public void GetFloat4ConvertsBooleanValue()
    {
        float4 defaultValue = new float4(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetFloat4(defaultValue), Is.EqualTo(new float4(1.0f)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetFloat4(defaultValue), Is.EqualTo(new float4(0.0f)));
    }

    [Test]
    public void GetFloat4ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MinValue);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(float4.zero));

        value = new CesiumMetadataValue(new double2(1.0, Double.MinValue));
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(float4.zero));
    }

    [Test]
    public void GetFloat4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(float4.zero));

        value = new CesiumMetadataValue(float4x4.identity);
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(float4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetFloat4(float4.zero), Is.EqualTo(float4.zero));
    }
    #endregion

    #region GetDouble4

    [Test]
    public void GetDouble4ReturnsVec4Values()
    {
        double4 double4Value = new double4(1.2, 2.3, 3.4, 1.0);
        CesiumMetadataValue value = new CesiumMetadataValue(double4Value);
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(double4Value));

        value = new CesiumMetadataValue(new int4(-1, -2, -3, 1));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(-1, -2, -3, 1)));

        value = new CesiumMetadataValue(new uint4(1, 2, 3, 1));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new float4(0.5f, 1.2f, -1.0f, 1.0f));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(0.5f, 1.2f, -1.0f, 1.0f)));
    }

    [Test]
    public void GetDouble4ConvertsVec2Values()
    {
        double2 double2Value = new double2(1.2, 2.3);
        CesiumMetadataValue value = new CesiumMetadataValue(double2Value);
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(double2Value, 0, 0)));

        value = new CesiumMetadataValue(new int2(-1, -2));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(-1, -2, 0, 0)));

        value = new CesiumMetadataValue(new uint2(1, 2));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new float2(0.5f, 1.2f));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(0.5f, 1.2f, 0.0f, 0.0f)));
    }

    [Test]
    public void GetDouble4ConvertsVec3Values()
    {
        double3 double3Value = new double3(1.2, 2.3, 3.4);
        CesiumMetadataValue value = new CesiumMetadataValue(double3Value);
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(double3Value, 0)));

        value = new CesiumMetadataValue(new int3(-1, -2, -3));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(-1, -2, -3, 0)));

        value = new CesiumMetadataValue(new uint3(1, 2, 3));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(1, 2, 3, 0)));

        value = new CesiumMetadataValue(new float3(0.5f, 1.2f, -1.0f));
        Assert.That(
            value.GetDouble4(double4.zero), Is.EqualTo(new double4(0.5f, 1.2f, -1.0f, 0)));
    }

    [Test]
    public void GetDouble4ConvertsCesiumVecNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntVec4(-1, -2, -3, 1));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(-1, -2, -3, 1)));

        value = new CesiumMetadataValue(new CesiumUintVec4(1, 2, 3, 1));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(1, 2, 3, 1)));

        value = new CesiumMetadataValue(new CesiumIntVec2(-1, -2));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(-1, -2, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec2(1, 2));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(1, 2, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntVec3(-1, -2, -3));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(-1, -2, -3, 0)));

        value = new CesiumMetadataValue(new CesiumUintVec3(1, 2, 3));
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(1, 2, 3, 0)));
    }

    [Test]
    public void GetDouble4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345);
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(1.2345)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(-12345)));

        value = new CesiumMetadataValue(Single.MaxValue);
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(new double4(Single.MaxValue)));
    }

    [Test]
    public void GetDouble4ConvertsBooleanValue()
    {
        double4 defaultValue = new double4(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetDouble4(defaultValue), Is.EqualTo(new double4(1.0)));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetDouble4(defaultValue), Is.EqualTo(new double4(0.0)));
    }

    [Test]
    public void GetDouble4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(double4.zero));

        value = new CesiumMetadataValue(double4x4.identity);
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(double4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetDouble4(double4.zero), Is.EqualTo(double4.zero));
    }
    #endregion

    #region GetInt2x2

    [Test]
    public void GetInt2x2ReturnsMat2Values()
    {
        int2x2 int2x2Value = new int2x2(-1, -2, -3, -4);
        CesiumMetadataValue value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2Value));

        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(uint2x2Value)));

        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, -1.9f, 0.0f);
        value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(float2x2Value)));

        double2x2 double2x2Value = new double2x2(1.2, 2.3, -1.9, 0.2);
        value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(double2x2Value)));
    }

    [Test]
    public void GetInt2x2ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int3x3(
            -1, -2, -3,
             0, -1, 0,
             0, 0, 1));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(-1, -2, 0, -1)));

        value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(1, 2, 4, 5)));

        value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(0, 1, -2, 4)));

        value = new CesiumMetadataValue(new double3x3(
             1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
             0.1, 20.2, -44.3));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(1, 2, -1, -2)));
    }

    [Test]
    public void GetInt2x2ConvertsMat4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(-1, -2, 0, -1)));

        value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(1, 2, 4, 5)));

        value = new CesiumMetadataValue(new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(0, 1, -2, 4)));

        value = new CesiumMetadataValue(new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(1, 2, -1, -2)));
    }

    [Test]
    public void GetInt2x2ConvertsCesiumMatNValues()
    {
        int2x2 int2x2Value = new int2x2(-1, -2, -3, -4);
        CesiumMetadataValue value = new CesiumMetadataValue(
            new CesiumIntMat2x2(int2x2Value[0], int2x2Value[1]));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2Value));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(1, 3, 2, 4)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
            new int3(-1, -2, -3),
            new int3(0, -1, 0),
            new int3(0, 0, -1)));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(-1, 0, -2, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(1, 4, 2, 5)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           new int4(-1, -2, -3, 0),
           new int4(0, -1, 0, 0),
           new int4(0, 0, 1, 0),
           new int4(1, -1, 0, 1)));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(-1, 0, -2, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(1, 4, 2, 5)));
    }

    [Test]
    public void GetInt2x2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2.identity));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(new int2x2(-12345, 0, 0, -12345)));
    }

    [Test]
    public void GetInt2x2ConvertsBooleanValue()
    {
        int2x2 defaultValue = new int2x2(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt2x2(defaultValue), Is.EqualTo(int2x2.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetInt2x2(defaultValue), Is.EqualTo(int2x2.zero));
    }

    [Test]
    public void GetInt2x2ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(UInt32.MaxValue);
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2.zero));
    }

    [Test]
    public void GetInt2x2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2.zero));

        value = new CesiumMetadataValue(new int2(1));
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt2x2(int2x2.zero), Is.EqualTo(int2x2.zero));

    }
    #endregion

    #region GetUInt2x2

    [Test]
    public void GetUInt2x2ReturnsMat2Values()
    {
        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        CesiumMetadataValue value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2Value));

        int2x2 int2x2Value = new int2x2(2, 4, 6, 8);
        value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(int2x2Value)));

        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, 1.9f, 0.0f);
        value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(float2x2Value)));

        double2x2 double2x2Value = new double2x2(1.2, 2.3, 1.9, 0.2);
        value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(double2x2Value)));
    }

    [Test]
    public void GetUInt2x2ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            0, 1, 0,
            0, 0, 1));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 2, 0, 1)));

        value = new CesiumMetadataValue(new int3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 2, 4, 5)));

        value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, -1.0f,
             2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(0, 1, 2, 4)));

        value = new CesiumMetadataValue(new double3x3(
             1.2, 2.3, 3.4,
             1.0, 2.0, -0.5,
             0.1, 20.2, -44.3));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 2, 1, 2)));
    }

    [Test]
    public void GetUInt2x2ConvertsMat4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            1, 1, 0, 1));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 2, 0, 1)));

        value = new CesiumMetadataValue(new int4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 2, 4, 5)));

        value = new CesiumMetadataValue(new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(0, 1, 2, 4)));

        value = new CesiumMetadataValue(new double4x4(
            1.2, 2.3, 3.4, 0.0,
            1.0, 2.0, -0.5, 0.0,
            0.1, 20.2, -44.3, 0.0,
            0.0, 0.0, 0.0, 1.0));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 2, 1, 2)));
    }

    [Test]
    public void GetUInt2x2ConvertsCesiumMatNValues()
    {
        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        CesiumMetadataValue value = new CesiumMetadataValue(
            new CesiumUintMat2x2(uint2x2Value[0], uint2x2Value[1]));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2Value));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(1, 2),
            new int2(3, 4)));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 3, 2, 4)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(0, 1, 0),
            new uint3(0, 0, 1)));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 0, 2, 1)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
            new int3(1, 2, 3),
            new int3(4, 5, 6),
            new int3(7, 8, 9)));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 4, 2, 5)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
           new uint4(1, 2, 3, 0),
           new uint4(0, 1, 0, 0),
           new uint4(0, 0, 1, 0),
           new uint4(1, 1, 0, 1)));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 0, 2, 1)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
            new int4(1, 2, 3, 0),
            new int4(4, 5, 6, 0),
            new int4(7, 8, 9, 0),
            new int4(0, 0, 0, 1)));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(1, 4, 2, 5)));
    }

    [Test]
    public void GetUInt2x2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2.identity));

        value = new CesiumMetadataValue(12345);
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(new uint2x2(12345, 0, 0, 12345)));
    }

    [Test]
    public void GetUInt2x2ConvertsBooleanValue()
    {
        uint2x2 defaultValue = new uint2x2(11);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt2x2(defaultValue), Is.EqualTo(uint2x2.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetUInt2x2(defaultValue), Is.EqualTo(uint2x2.zero));
    }

    [Test]
    public void GetUInt2x2ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(-1);
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2.zero));
    }

    [Test]
    public void GetUInt2x2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2.zero));

        value = new CesiumMetadataValue(new uint2(1));
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt2x2(uint2x2.zero), Is.EqualTo(uint2x2.zero));

    }
    #endregion

    #region GetFloat2x2

    [Test]
    public void GetFloat2x2ReturnsMat2Values()
    {
        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, -1.9f, 0.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(float2x2Value));

        int2x2 int2x2Value = new int2x2(-1, -2, -3, -4);
        value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(int2x2Value)));

        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(uint2x2Value)));

        double2x2 double2x2Value = new double2x2(1.2, 2.3, -1.9, 0.2);
        value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(double2x2Value)));
    }

    [Test]
    public void GetFloat2x2ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(0.5f, 1.2f, -2.2f, 4.54f)));

        value = new CesiumMetadataValue(new int3x3(
            -1, -2, -3,
             0, -1, 0,
             0, 0, 1));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(-1, -2, 0, -1)));

        value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));

        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(1, 2, 4, 5)));

        double3x3 double3x3Value = new double3x3(
             1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
             0.1, 20.2, -44.3);
        value = new CesiumMetadataValue(double3x3Value);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(
            (float)double3x3Value[0][0], (float)double3x3Value[1][0],
            (float)double3x3Value[0][1], (float)double3x3Value[1][1])));
    }

    [Test]
    public void GetFloat2x2ConvertsMat4Values()
    {
        CesiumMetadataValue
        value = new CesiumMetadataValue(new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(
            0.5f, 1.2f,
            -2.2f, 4.54f)));

        value = new CesiumMetadataValue(new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(-1, -2, 0, -1)));

        value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(1, 2, 4, 5)));

        double4x4 double4x4Value = new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0);
        value = new CesiumMetadataValue(double4x4Value);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(
            (float)double4x4Value[0][0], (float)double4x4Value[1][0],
            (float)double4x4Value[0][1], (float)double4x4Value[1][1])));
    }

    [Test]
    public void GetFloat2x2ConvertsCesiumMatNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(-1, -3, -2, -4)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(1, 3, 2, 4)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
            new int3(-1, -2, -3),
            new int3(0, -1, 0),
            new int3(0, 0, -1)));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(-1, 0, -2, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(1, 4, 2, 5)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           new int4(-1, -2, -3, 0),
           new int4(0, -1, 0, 0),
           new int4(0, 0, 1, 0),
           new int4(1, -1, 0, 1)));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(-1, 0, -2, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(1, 4, 2, 5)));
    }

    [Test]
    public void GetFloat2x2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(1.2345f, 0, 0, 1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(new float2x2(-12345, 0, 0, -12345)));
    }

    [Test]
    public void GetFloat2x2ConvertsBooleanValue()
    {
        float2x2 defaultValue = new float2x2(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetFloat2x2(defaultValue), Is.EqualTo(float2x2.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetFloat2x2(defaultValue), Is.EqualTo(float2x2.zero));
    }

    [Test]
    public void GetFloat2x2ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MaxValue);
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(float2x2.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(float2x2.zero));
    }

    [Test]
    public void GetFloat2x2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(float2x2.zero));

        value = new CesiumMetadataValue(new float2(1));
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(float2x2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetFloat2x2(float2x2.zero), Is.EqualTo(float2x2.zero));

    }
    #endregion

    #region GetDouble2x2

    [Test]
    public void GetDouble2x2ReturnsMat2Values()
    {
        double2x2 double2x2Value = new double2x2(1.2, 2.3, -1.9, 0.2);
        CesiumMetadataValue value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(double2x2Value));

        int2x2 int2x2Value = new int2x2(-1, -2, -3, -4);
        value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(int2x2Value)));

        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(uint2x2Value)));

        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, -1.9f, 0.0f);
        value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(float2x2Value)));
    }

    [Test]
    public void GetDouble2x2ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new double3x3(
             1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
             0.1, 20.2, -44.3));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(1.2, 2.3, -1.0, -2.0)));

        value = new CesiumMetadataValue(new int3x3(
            -1, -2, -3,
             0, -1, 0,
             0, 0, 1));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(-1, -2, 0, -1)));

        value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));

        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(1, 2, 4, 5)));

        value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(0.5f, 1.2f, -2.2f, 4.54f)));
    }

    [Test]
    public void GetDouble2x2ConvertsMat4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(1.2, 2.3, -1.0, -2.0)));

        value = new CesiumMetadataValue(new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(-1, -2, 0, -1)));

        value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetDouble2x2(float2x2.zero), Is.EqualTo(new double2x2(1, 2, 4, 5)));

        value = new CesiumMetadataValue(new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(0.5f, 1.2f, -2.2f, 4.54f)));
    }

    [Test]
    public void GetDouble2x2ConvertsCesiumMatNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(-1, -3, -2, -4)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(1, 3, 2, 4)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
            new int3(-1, -2, -3),
            new int3(0, -1, 0),
            new int3(0, 0, -1)));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(-1, 0, -2, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(1, 4, 2, 5)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           new int4(-1, -2, -3, 0),
           new int4(0, -1, 0, 0),
           new int4(0, 0, 1, 0),
           new int4(1, -1, 0, 1)));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(-1, 0, -2, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(1, 4, 2, 5)));
    }

    [Test]
    public void GetDouble2x2ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(1.2345f, 0, 0, 1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(new double2x2(-12345, 0, 0, -12345)));
    }

    [Test]
    public void GetDouble2x2ConvertsBooleanValue()
    {
        double2x2 defaultValue = new double2x2(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetDouble2x2(defaultValue), Is.EqualTo(double2x2.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetDouble2x2(defaultValue), Is.EqualTo(double2x2.zero));
    }

    [Test]
    public void GetDouble2x2ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(double2x2.zero));

        value = new CesiumMetadataValue(new double2(1));
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(double2x2.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetDouble2x2(double2x2.zero), Is.EqualTo(double2x2.zero));

    }
    #endregion

    #region GetInt3x3

    [Test]
    public void GetInt3x3ReturnsMat3Values()
    {
        int3x3 int3x3Value = new int3x3(
            -1, -2, -3,
             0, -1, 0,
             0, 0, 1);
        CesiumMetadataValue value = new CesiumMetadataValue(int3x3Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3Value));

        uint3x3 uint3x3Value = new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9);
        value = new CesiumMetadataValue(uint3x3Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(uint3x3Value)));

        float3x3 float3x3Value = new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f);
        value = new CesiumMetadataValue(float3x3Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(float3x3Value)));

        double3x3 double3x3Value = new double3x3(
             1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
             0.1, 20.2, -44.3);
        value = new CesiumMetadataValue(double3x3Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(double3x3Value)));
    }

    [Test]
    public void GetInt3x3ConvertsMat2Values()
    {
        int2x2 int2x2Value = new int2x2(-1, -2, -3, -4);
        CesiumMetadataValue value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            -1, -2, 0,
            -3, -4, 0,
            0, 0, 0)));

        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            1, 2, 0,
            3, 4, 0,
            0, 0, 0)));

        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, -1.9f, 0.0f);
        value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            0, 1, 0,
            -1, 0, 0,
            0, 0, 0)));

        double2x2 double2x2Value = new double2x2(1.2, 2.3, -1.9, 0.2);
        value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            1, 2, 0,
            -1, 0, 0,
            0, 0, 0)));
    }

    [Test]
    public void GetInt3x3ConvertsMat4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            -1, -2, -3,
            0, -1, 0,
            0, 0, 1)));

        value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9)));

        value = new CesiumMetadataValue(new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            0, 1, -1,
            -2, 4, 0,
            0, 0, -1)));

        value = new CesiumMetadataValue(new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            1, 2, 3,
            -1, -2, 0,
            0, 20, -44)));
    }

    [Test]
    public void GetInt3x3ConvertsCesiumMatNValues()
    {
        int3x3 int3x3Value = new int3x3(
            -1, 0, 0,
            -2, -1, 0,
            -3, 0, -1);
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat3x3(
           int3x3Value[0],
           int3x3Value[1],
           int3x3Value[2]));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3Value));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            1, 4, 7,
            2, 5, 8,
            3, 6, 9)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            -1, -3, 0,
            -2, -4, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            1, 3, 0,
            2, 4, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           new int4(-1, -2, -3, 0),
           new int4(0, -1, 0, 0),
           new int4(0, 0, 1, 0),
           new int4(1, -1, 0, 1)));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            -1, 0, 0,
            -2, -1, 0,
            -3, 0, 1)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            1, 4, 7,
            2, 5, 8,
            3, 6, 9)));
    }

    [Test]
    public void GetInt3x3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3.identity));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(new int3x3(
            -12345, 0, 0,
            0, -12345, 0,
            0, 0, -12345)));
    }

    [Test]
    public void GetInt3x3ConvertsBooleanValue()
    {
        int3x3 defaultValue = new int3x3(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt3x3(defaultValue), Is.EqualTo(int3x3.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetInt3x3(defaultValue), Is.EqualTo(int3x3.zero));
    }

    [Test]
    public void GetInt3x3ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(UInt32.MaxValue);
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3.zero));
    }

    [Test]
    public void GetInt3x3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3.zero));

        value = new CesiumMetadataValue(new int3(1));
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt3x3(int3x3.zero), Is.EqualTo(int3x3.zero));

    }
    #endregion

    #region GetUInt3x3

    [Test]
    public void GetUInt3x3ReturnsMat3Values()
    {
        uint3x3 uint3x3Value = new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9);
        CesiumMetadataValue value = new CesiumMetadataValue(uint3x3Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3Value));

        int3x3 int3x3Value = new int3x3(
            1, 2, 3,
            0, 1, 0,
            0, 0, 1);
        value = new CesiumMetadataValue(int3x3Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(int3x3Value)));


        float3x3 float3x3Value = new float3x3(
            0.5f, 1.2f, 1.0f,
            2.2f, 4.54f, 0.0f,
            0.0f, 0.0f, 1.0f);
        value = new CesiumMetadataValue(float3x3Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(float3x3Value)));

        double3x3 double3x3Value = new double3x3(
            1.2, 2.3, 3.4,
            1.0, 2.0, 0.5,
            0.1, 20.2, 44.3);
        value = new CesiumMetadataValue(double3x3Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(double3x3Value)));
    }

    [Test]
    public void GetUInt3x3ConvertsMat2Values()
    {
        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        CesiumMetadataValue value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 2, 0,
            3, 4, 0,
            0, 0, 0)));

        int2x2 int2x2Value = new int2x2(1, 2, 3, 4);
        value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 2, 0,
            3, 4, 0,
            0, 0, 0)));

        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, 1.9f, 0.0f);
        value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            0, 1, 0,
            1, 0, 0,
            0, 0, 0)));

        double2x2 double2x2Value = new double2x2(1.2, 2.3, 1.9, 0.2);
        value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 2, 0,
            1, 0, 0,
            0, 0, 0)));
    }

    [Test]
    public void GetUInt3x3ConvertsMat4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9)));

        value = new CesiumMetadataValue(new int4x4(
            1, 2, 3, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            1, 1, 0, 1));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 2, 3,
            0, 1, 0,
            0, 0, 1)));

        value = new CesiumMetadataValue(new float4x4(
            0.5f, 1.2f, 1.0f, 1.0f,
            2.2f, 4.54f, 0.0f, 2.0f,
            0.0f, 0.0f, 1.0f, 3.0f,
            0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            0, 1, 1,
            2, 4, 0,
            0, 0, 1)));

        value = new CesiumMetadataValue(new double4x4(
            1.2, 2.3, 3.4, 0.0,
            1.0, 2.0, 0.5, 0.0,
            0.1, 20.2, 44.3, 0.0,
            0.0, 0.0, 0.0, 1.0));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 2, 3,
            1, 2, 0,
            0, 20, 44)));
    }

    [Test]
    public void GetUInt3x3ConvertsCesiumMatNValues()
    {
        uint3x3 uint3x3Value = new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9);
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUintMat3x3(
            uint3x3Value[0],
            uint3x3Value[1],
            uint3x3Value[2]));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3Value));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
           new int3(1, 0, 0),
           new int3(2, 1, 0),
           new int3(3, 0, 1)));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 2, 3,
            0, 1, 0,
            0, 0, 1)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(1, 2),
            new int2(3, 4)));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 3, 0,
            2, 4, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(2, 4),
            new uint2(6, 8)));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            2, 6, 0,
            4, 8, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           new int4(1, 2, 3, 0),
           new int4(0, 1, 0, 0),
           new int4(0, 0, 1, 0),
           new int4(1, -1, 0, 1)));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 0, 0,
            2, 1, 0,
            3, 0, 1)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            1, 4, 7,
            2, 5, 8,
            3, 6, 9)));
    }

    [Test]
    public void GetUInt3x3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3.identity));

        value = new CesiumMetadataValue(12345);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(new uint3x3(
            12345, 0, 0,
            0, 12345, 0,
            0, 0, 12345)));
    }

    [Test]
    public void GetUInt3x3ConvertsBooleanValue()
    {
        uint3x3 defaultValue = new uint3x3(11);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt3x3(defaultValue), Is.EqualTo(uint3x3.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetUInt3x3(defaultValue), Is.EqualTo(uint3x3.zero));
    }

    [Test]
    public void GetUInt3x3ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(-1);
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3.zero));
    }

    [Test]
    public void GetUInt3x3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3.zero));

        value = new CesiumMetadataValue(new uint3(1));
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt3x3(uint3x3.zero), Is.EqualTo(uint3x3.zero));

    }
    #endregion

    #region GetFloat3x3

    [Test]
    public void GetFloat3x3ReturnsMat3Values()
    {
        float3x3 float3x3Value = new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float3x3Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(float3x3Value));

        int3x3 int3x3Value = new int3x3(
            -1, -2, -3,
             0, -1, 0,
             0, 0, 1);
        value = new CesiumMetadataValue(int3x3Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(int3x3Value)));

        uint3x3 uint3x3Value = new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9);
        value = new CesiumMetadataValue(uint3x3Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(uint3x3Value)));

        double3x3 double3x3Value = new double3x3(
             1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
             0.1, 20.2, -44.3);
        value = new CesiumMetadataValue(double3x3Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(double3x3Value)));
    }

    [Test]
    public void GetFloat3x3ConvertsMat2Values()
    {
        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, -1.9f, 0.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            0.5f, 1.2f, 0,
            -1.9f, 0.0f, 0,
            0, 0, 0)));

        int2x2 int2x2Value = new int2x2(-1, -2, -3, -4);
        value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            -1, -2, 0,
            -3, -4, 0,
            0, 0, 0)));

        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            1, 2, 0,
            3, 4, 0,
            0, 0, 0)));

        double2x2 double2x2Value = new double2x2(1.2, 2.3, -1.9, 0.2);
        value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            (float)1.2, (float)2.3, 0,
            (float)-1.9, (float)0.2, 0,
            0, 0, 0)));
    }

    [Test]
    public void GetFloat3x3ConvertsMat4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f)));

        value = new CesiumMetadataValue(new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            -1, -2, -3,
            0, -1, 0,
            0, 0, 1)));

        value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9)));

        value = new CesiumMetadataValue(new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            (float)1.2, (float)2.3, (float)3.4,
            (float)-1.0, (float)-2.0, (float)-0.5,
            (float)0.1, (float)20.2, (float)-44.3)));
    }

    [Test]
    public void GetFloat3x3ConvertsCesiumMatNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat3x3(
            new int3(-1, 0, 0),
            new int3(-2, -1, 0),
            new int3(-3, 0, -1)));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            -1, -2, -3,
            0, -1, 0,
            0, 0, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            1, 4, 7,
            2, 5, 8,
            3, 6, 9)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            -1, -3, 0,
            -2, -4, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            1, 3, 0,
            2, 4, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           new int4(-1, -2, -3, 0),
           new int4(0, -1, 0, 0),
           new int4(0, 0, 1, 0),
           new int4(1, -1, 0, 1)));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            -1, 0, 0,
            -2, -1, 0,
            -3, 0, 1)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            1, 4, 7,
            2, 5, 8,
            3, 6, 9)));
    }

    [Test]
    public void GetFloat3x3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            1.2345f, 0, 0,
            0, 1.2345f, 0,
            0, 0, 1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(new float3x3(
            -12345, 0, 0,
            0, -12345, 0,
            0, 0, -12345)));
    }

    [Test]
    public void GetFloat3x3ConvertsBooleanValue()
    {
        float3x3 defaultValue = new float3x3(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetFloat3x3(defaultValue), Is.EqualTo(float3x3.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetFloat3x3(defaultValue), Is.EqualTo(float3x3.zero));
    }

    [Test]
    public void GetFloat3x3ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MaxValue);
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(float3x3.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(float3x3.zero));
    }

    [Test]
    public void GetFloat3x3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(float3x3.zero));

        value = new CesiumMetadataValue(new float3(1));
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(float3x3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetFloat3x3(float3x3.zero), Is.EqualTo(float3x3.zero));
    }
    #endregion

    #region GetDouble3x3

    [Test]
    public void GetDouble3x3ReturnsMat3Values()
    {
        double3x3 double3x3Value = new double3x3(
             1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
             0.1, 20.2, -44.3);
        CesiumMetadataValue value = new CesiumMetadataValue(double3x3Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(double3x3Value));

        int3x3 int3x3Value = new int3x3(
            -1, -2, -3,
             0, -1, 0,
             0, 0, 1);
        value = new CesiumMetadataValue(int3x3Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(int3x3Value)));

        uint3x3 uint3x3Value = new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9);
        value = new CesiumMetadataValue(uint3x3Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(uint3x3Value)));

        float3x3 float3x3Value = new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f);
        value = new CesiumMetadataValue(float3x3Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(float3x3Value)));
    }

    [Test]
    public void GetDouble3x3ConvertsMat2Values()
    {
        double2x2 double2x2Value = new double2x2(1.2, 2.3, -1.9, 0.2);
        CesiumMetadataValue value = new CesiumMetadataValue(double2x2Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1.2, 2.3, 0,
           -1.9, 0.2, 0,
            0, 0, 0)));

        float2x2 float2x2Value = new float2x2(0.5f, 1.2f, -1.9f, 0.0f);
        value = new CesiumMetadataValue(float2x2Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            0.5f, 1.2f, 0,
            -1.9f, 0.0f, 0,
            0, 0, 0)));

        int2x2 int2x2Value = new int2x2(-1, -2, -3, -4);
        value = new CesiumMetadataValue(int2x2Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            -1, -2, 0,
            -3, -4, 0,
            0, 0, 0)));

        uint2x2 uint2x2Value = new uint2x2(1, 2, 3, 4);
        value = new CesiumMetadataValue(uint2x2Value);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1, 2, 0,
            3, 4, 0,
            0, 0, 0)));
    }

    [Test]
    public void GetDouble3x3ConvertsMat4Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
            0.1, 20.2, -44.3)));

        value = new CesiumMetadataValue(new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            -1, -2, -3,
            0, -1, 0,
            0, 0, 1)));

        value = new CesiumMetadataValue(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9)));

        value = new CesiumMetadataValue(new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f)));
    }

    [Test]
    public void GetDouble3x3ConvertsCesiumMatNValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat3x3(
            new int3(-1, 0, 0),
            new int3(-2, -1, 0),
            new int3(-3, 0, -1)));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            -1, -2, -3,
            0, -1, 0,
            0, 0, -1)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1, 4, 7,
            2, 5, 8,
            3, 6, 9)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            -1, -3, 0,
            -2, -4, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1, 3, 0,
            2, 4, 0,
            0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           new int4(-1, -2, -3, 0),
           new int4(0, -1, 0, 0),
           new int4(0, 0, 1, 0),
           new int4(1, -1, 0, 1)));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            -1, 0, 0,
            -2, -1, 0,
            -3, 0, 1)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1, 4, 7,
            2, 5, 8,
            3, 6, 9)));
    }

    [Test]
    public void GetDouble3x3ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            1.2345f, 0, 0,
            0, 1.2345f, 0,
            0, 0, 1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(new double3x3(
            -12345, 0, 0,
            0, -12345, 0,
            0, 0, -12345)));
    }

    [Test]
    public void GetDouble3x3ConvertsBooleanValue()
    {
        double3x3 defaultValue = new double3x3(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetDouble3x3(defaultValue), Is.EqualTo(double3x3.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetDouble3x3(defaultValue), Is.EqualTo(double3x3.zero));
    }

    [Test]
    public void GetDouble3x3ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(double3x3.zero));

        value = new CesiumMetadataValue(new double3(1));
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(double3x3.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetDouble3x3(double3x3.zero), Is.EqualTo(double3x3.zero));
    }
    #endregion

    #region GetInt4x4
    [Test]
    public void GetInt4x4ReturnsMat4Values()
    {
        int4x4 int4x4Value = new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1);
        CesiumMetadataValue value = new CesiumMetadataValue(int4x4Value);
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4Value));

        uint4x4 uint4x4Value = new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1);
        value = new CesiumMetadataValue(uint4x4Value);
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(uint4x4Value)));

        float4x4 float4x4Value = new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f);
        value = new CesiumMetadataValue(float4x4Value);
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(float4x4Value)));

        double4x4 double4x4Value = new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0);
        value = new CesiumMetadataValue(double4x4Value);
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(double4x4Value)));
    }

    [Test]
    public void GetInt4x4ConvertsMat2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int2x2(-1, -2, -3, -4));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            -1, -2, 0, 0,
            -3, -4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new uint2x2(1, 2, 3, 4));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            1, 2, 0, 0,
            3, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new float2x2(0.5f, 1.2f, -1.9f, 0.0f));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            0, 1, 0, 0,
            -1, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new double2x2(1.2, 2.3, -1.9, 0.2));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            1, 2, 0, 0,
            -1, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));
    }


    [Test]
    public void GetInt4x4ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new int3x3(
            -1, -2, -3,
             0, -1, 0,
             0, 0, 1));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             0, 0, 0, 0)));

        value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            0, 1, -1, 0,
            -2, 4, 0, 0,
            0, 0, -1, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new double3x3(
             1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
             0.1, 20.2, -44.3));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            1, 2, 3, 0,
            -1, -2, 0, 0,
            0, 20, -44, 0,
            0, 0, 0, 0)));
    }


    [Test]
    public void GetInt4x4ConvertsCesiumMatNValues()
    {
        int4x4 int4x4Value = new int4x4(
            -1, 0, 0, 0,
            -2, -1, 0, 0,
            -3, 0, -1, 0,
            0, 0, 0, 1);
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat4x4(
           int4x4Value[0],
           int4x4Value[1],
           int4x4Value[2],
           int4x4Value[3]));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4Value));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            1, 4, 7, 0,
            2, 5, 8, 0,
            3, 6, 9, 0,
            0, 0, 0, 1)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            -1, -3, 0, 0,
            -2, -4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            1, 3, 0, 0,
            2, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
           new int3(-1, -2, -3),
           new int3(0, -1, 0),
           new int3(0, 0, 1)));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            -1, 0, 0, 0,
            -2, -1, 0, 0,
            -3, 0, 1, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            1, 4, 7, 0,
            2, 5, 8, 0,
            3, 6, 9, 0,
            0, 0, 0, 0)));
    }

    [Test]
    public void GetInt4x4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4.identity));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(new int4x4(
            -12345, 0, 0, 0,
            0, -12345, 0, 0,
            0, 0, -12345, 0,
            0, 0, 0, -12345)));
    }

    [Test]
    public void GetInt4x4ConvertsBooleanValue()
    {
        int4x4 defaultValue = new int4x4(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetInt4x4(defaultValue), Is.EqualTo(int4x4.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetInt4x4(defaultValue), Is.EqualTo(int4x4.zero));
    }

    [Test]
    public void GetInt4x4ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(UInt32.MaxValue);
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4.zero));
    }

    [Test]
    public void GetInt4x4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4.zero));

        value = new CesiumMetadataValue(new int3(1));
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetInt4x4(int4x4.zero), Is.EqualTo(int4x4.zero));

    }
    #endregion

    #region GetUInt4x4
    [Test]
    public void GetUInt4x4ReturnsMat4Values()
    {
        uint4x4 uint4x4Value = new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1);
        CesiumMetadataValue value = new CesiumMetadataValue(uint4x4Value);
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4Value));

        int4x4 int4x4Value = new int4x4(
            1, 2, 3, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            1, 1, 0, 1);
        value = new CesiumMetadataValue(int4x4Value);
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(int4x4Value)));

        float4x4 float4x4Value = new float4x4(
             0.5f, 1.2f, 1.0f, 1.0f,
             2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, 1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f);
        value = new CesiumMetadataValue(float4x4Value);
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(float4x4Value)));

        double4x4 double4x4Value = new double4x4(
             1.2, 2.3, 3.4, 0.0,
             1.0, 2.0, 0.5, 0.0,
             0.1, 20.2, 44.3, 0.0,
             0.0, 0.0, 0.0, 1.0);
        value = new CesiumMetadataValue(double4x4Value);
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(double4x4Value)));
    }

    [Test]
    public void GetUInt4x4ConvertsMat2Values()
    {
        CesiumMetadataValue
        value = new CesiumMetadataValue(new uint2x2(1, 2, 3, 4));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 2, 0, 0,
            3, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new int2x2(1, 2, 3, 4));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 2, 0, 0,
            3, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new float2x2(0.5f, 1.2f, 2.9f, 0.0f));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            0, 1, 0, 0,
            2, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new double2x2(1.2, 2.3, 3.9, 0.2));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 2, 0, 0,
            3, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));
    }


    [Test]
    public void GetUInt4x4ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new int3x3(
             1, 2, 3,
             0, 1, 0,
             0, 0, 1));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
             1, 2, 3, 0,
             0, 1, 0, 0,
             0, 0, 1, 0,
             0, 0, 0, 0)));

        value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, 1.0f,
             2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, 1.0f));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            0, 1, 1, 0,
            2, 4, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new double3x3(
             1.2, 2.3, 3.4,
             1.0, 2.0, 0.5,
             0.1, 20.2, 44.3));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 2, 3, 0,
            1, 2, 0, 0,
            0, 20, 44, 0,
            0, 0, 0, 0)));
    }

    [Test]
    public void GetUInt4x4ConvertsCesiumMatNValues()
    {
        uint4x4 uint4x4Value = new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1);
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumUintMat4x4(
            uint4x4Value[0],
            uint4x4Value[1],
            uint4x4Value[2],
            uint4x4Value[3]));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4Value));

        int4x4 int4x4Value = new int4x4(
            1, 0, 0, 0,
            2, 1, 0, 0,
            3, 0, 1, 0,
            0, 0, 0, 1);
        value = new CesiumMetadataValue(new CesiumIntMat4x4(
           int4x4Value[0],
           int4x4Value[1],
           int4x4Value[2],
           int4x4Value[3]));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(int4x4Value)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(1, 2),
            new int2(3, 4)));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 3, 0, 0,
            2, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 3, 0, 0,
            2, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
           new int3(1, 2, 3),
           new int3(0, 1, 0),
           new int3(0, 0, 1)));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 0, 0, 0,
            2, 1, 0, 0,
            3, 0, 1, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            1, 4, 7, 0,
            2, 5, 8, 0,
            3, 6, 9, 0,
            0, 0, 0, 0)));
    }

    [Test]
    public void GetUInt4x4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4.identity));

        value = new CesiumMetadataValue(12345);
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(new uint4x4(
            12345, 0, 0, 0,
            0, 12345, 0, 0,
            0, 0, 12345, 0,
            0, 0, 0, 12345)));
    }

    [Test]
    public void GetUInt4x4ConvertsBooleanValue()
    {
        uint4x4 defaultValue = new uint4x4(10);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetUInt4x4(defaultValue), Is.EqualTo(uint4x4.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetUInt4x4(defaultValue), Is.EqualTo(uint4x4.zero));
    }

    [Test]
    public void GetUInt4x4ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Int64.MaxValue);
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4.zero));
    }

    [Test]
    public void GetUInt4x4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4.zero));

        value = new CesiumMetadataValue(new int3(1));
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetUInt4x4(uint4x4.zero), Is.EqualTo(uint4x4.zero));

    }
    #endregion

    #region GetFloat4x4
    [Test]
    public void GetFloat4x4ReturnsMat4Values()
    {
        float4x4 float4x4Value = new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f);
        CesiumMetadataValue value = new CesiumMetadataValue(float4x4Value);
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(float4x4Value));

        int4x4 int4x4Value = new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1);
        value = new CesiumMetadataValue(int4x4Value);
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(int4x4Value)));

        uint4x4 uint4x4Value = new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1);
        value = new CesiumMetadataValue(uint4x4Value);
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(uint4x4Value)));


        double4x4 double4x4Value = new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0);
        value = new CesiumMetadataValue(double4x4Value);
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(double4x4Value)));
    }

    [Test]
    public void GetFloat4x4ConvertsMat2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new float2x2(0.5f, 1.2f, -1.9f, 0.0f));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            0.5f, 1.2f, 0, 0,
            -1.9f, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new int2x2(-1, -2, -3, -4));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            -1, -2, 0, 0,
            -3, -4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new uint2x2(1, 2, 3, 4));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            1, 2, 0, 0,
            3, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new double2x2(1.2, 2.3, -1.9, 0.2));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            (float)1.2, (float)2.3, 0, 0,
            (float)-1.9, (float)0.2, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));
    }


    [Test]
    public void GetFloat4x4ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
             0.5f, 1.2f, -1.0f, 0,
            -2.2f, 4.54f, 0.0f, 0,
             0.0f, 0.0f, -1.0f, 0,
             0, 0, 0, 0)));

        value = new CesiumMetadataValue(new int3x3(
           -1, -2, -3,
            0, -1, 0,
            0, 0, 1));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             0, 0, 0, 0)));

        value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new double3x3(
            1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
            0.1, 20.2, -44.3));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            (float)1.2, (float)2.3, (float)3.4, 0,
            (float)-1.0, (float)-2.0, (float)-0.5, 0,
            (float)0.1, (float)20.2, (float)-44.3, 0,
            0, 0, 0, 0)));
    }


    [Test]
    public void GetFloat4x4ConvertsCesiumMatNValues()
    {
        int4x4 int4x4Value = new int4x4(
            -1, 0, 0, 0,
            -2, -1, 0, 0,
            -3, 0, -1, 0,
            0, 0, 0, 1);
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat4x4(
           int4x4Value[0],
           int4x4Value[1],
           int4x4Value[2],
           int4x4Value[3]));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(int4x4Value)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            1, 4, 7, 0,
            2, 5, 8, 0,
            3, 6, 9, 0,
            0, 0, 0, 1)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            -1, -3, 0, 0,
            -2, -4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            1, 3, 0, 0,
            2, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
           new int3(-1, -2, -3),
           new int3(0, -1, 0),
           new int3(0, 0, 1)));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            -1, 0, 0, 0,
            -2, -1, 0, 0,
            -3, 0, 1, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            1, 4, 7, 0,
            2, 5, 8, 0,
            3, 6, 9, 0,
            0, 0, 0, 0)));
    }

    [Test]
    public void GetFloat4x4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            1.2345f, 0, 0, 0,
            0, 1.2345f, 0, 0,
            0, 0, 1.2345f, 0,
            0, 0, 0, 1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(new float4x4(
            -12345, 0, 0, 0,
            0, -12345, 0, 0,
            0, 0, -12345, 0,
            0, 0, 0, -12345)));
    }

    [Test]
    public void GetFloat4x4ConvertsBooleanValue()
    {
        float4x4 defaultValue = new float4x4(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetFloat4x4(defaultValue), Is.EqualTo(float4x4.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetFloat4x4(defaultValue), Is.EqualTo(float4x4.zero));
    }

    [Test]
    public void GetFloat4x4ReturnsDefaultValueForOutOfRangeValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(Double.MaxValue);
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(float4x4.zero));

        value = new CesiumMetadataValue(new double2x2(Double.MinValue));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(float4x4.zero));
    }

    [Test]
    public void GetFloat4x4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(float4x4.zero));

        value = new CesiumMetadataValue(new int3(1));
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(float4x4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetFloat4x4(float4x4.zero), Is.EqualTo(float4x4.zero));

    }
    #endregion

    #region GetDouble4x4
    [Test]
    public void GetDouble4x4ReturnsMat4Values()
    {
        double4x4 double4x4Value = new double4x4(
             1.2, 2.3, 3.4, 0.0,
            -1.0, -2.0, -0.5, 0.0,
             0.1, 20.2, -44.3, 0.0,
             0.0, 0.0, 0.0, 1.0);
        CesiumMetadataValue value = new CesiumMetadataValue(double4x4Value);
        Assert.That(value.GetDouble4x4(float4x4.zero), Is.EqualTo(double4x4Value));

        int4x4 int4x4Value = new int4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             1, -1, 0, 1);
        value = new CesiumMetadataValue(int4x4Value);
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(int4x4Value)));

        uint4x4 uint4x4Value = new uint4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1);
        value = new CesiumMetadataValue(uint4x4Value);
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(uint4x4Value)));

        float4x4 float4x4Value = new float4x4(
             0.5f, 1.2f, -1.0f, 1.0f,
            -2.2f, 4.54f, 0.0f, 2.0f,
             0.0f, 0.0f, -1.0f, 3.0f,
             0.0f, 0.0f, 0.0f, 1.0f);
        value = new CesiumMetadataValue(float4x4Value);
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(float4x4Value)));
    }

    [Test]
    public void GetDouble4x4ConvertsMat2Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new double2x2(1.2, 2.3, -1.9, 0.2));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1.2, 2.3, 0, 0,
            -1.9, 0.2, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new int2x2(-1, -2, -3, -4));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            -1, -2, 0, 0,
            -3, -4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new uint2x2(1, 2, 3, 4));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1, 2, 0, 0,
            3, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new float2x2(0.5f, 1.2f, -1.9f, 0.0f));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            0.5f, 1.2f, 0, 0,
            -1.9f, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));
    }

    [Test]
    public void GetDouble4x4ConvertsMat3Values()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(new double3x3(
            1.2, 2.3, 3.4,
            -1.0, -2.0, -0.5,
            0.1, 20.2, -44.3));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1.2, 2.3, 3.4, 0,
            -1.0, -2.0, -0.5, 0,
            0.1, 20.2, -44.3, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new int3x3(
           -1, -2, -3,
            0, -1, 0,
            0, 0, 1));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            -1, -2, -3, 0,
             0, -1, 0, 0,
             0, 0, 1, 0,
             0, 0, 0, 0)));

        value = new CesiumMetadataValue(new uint3x3(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new float3x3(
             0.5f, 1.2f, -1.0f,
            -2.2f, 4.54f, 0.0f,
             0.0f, 0.0f, -1.0f));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
             0.5f, 1.2f, -1.0f, 0,
            -2.2f, 4.54f, 0.0f, 0,
             0.0f, 0.0f, -1.0f, 0,
             0, 0, 0, 0)));
    }

    [Test]
    public void GetDouble4x4ConvertsCesiumMatNValues()
    {
        int4x4 int4x4Value = new int4x4(
            -1, 0, 0, 0,
            -2, -1, 0, 0,
            -3, 0, -1, 0,
            0, 0, 0, 1);
        CesiumMetadataValue value = new CesiumMetadataValue(new CesiumIntMat4x4(
           int4x4Value[0],
           int4x4Value[1],
           int4x4Value[2],
           int4x4Value[3]));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(int4x4Value)));

        value = new CesiumMetadataValue(new CesiumUintMat4x4(
            new uint4(1, 2, 3, 0),
            new uint4(4, 5, 6, 0),
            new uint4(7, 8, 9, 0),
            new uint4(0, 0, 0, 1)));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1, 4, 7, 0,
            2, 5, 8, 0,
            3, 6, 9, 0,
            0, 0, 0, 1)));

        value = new CesiumMetadataValue(new CesiumIntMat2x2(
            new int2(-1, -2),
            new int2(-3, -4)));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            -1, -3, 0, 0,
            -2, -4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat2x2(
            new uint2(1, 2),
            new uint2(3, 4)));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1, 3, 0, 0,
            2, 4, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumIntMat3x3(
           new int3(-1, -2, -3),
           new int3(0, -1, 0),
           new int3(0, 0, 1)));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            -1, 0, 0, 0,
            -2, -1, 0, 0,
            -3, 0, 1, 0,
            0, 0, 0, 0)));

        value = new CesiumMetadataValue(new CesiumUintMat3x3(
            new uint3(1, 2, 3),
            new uint3(4, 5, 6),
            new uint3(7, 8, 9)));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1, 4, 7, 0,
            2, 5, 8, 0,
            3, 6, 9, 0,
            0, 0, 0, 0)));
    }

    [Test]
    public void GetDouble4x4ConvertsScalarValues()
    {
        CesiumMetadataValue value = new CesiumMetadataValue(1.2345f);
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            1.2345f, 0, 0, 0,
            0, 1.2345f, 0, 0,
            0, 0, 1.2345f, 0,
            0, 0, 0, 1.2345f)));

        value = new CesiumMetadataValue(-12345);
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(new double4x4(
            -12345, 0, 0, 0,
            0, -12345, 0, 0,
            0, 0, -12345, 0,
            0, 0, 0, -12345)));
    }

    [Test]
    public void GetDouble4x4ConvertsBooleanValue()
    {
        double4x4 defaultValue = new double4x4(-1);
        CesiumMetadataValue value = new CesiumMetadataValue(true);
        Assert.That(value.GetDouble4x4(defaultValue), Is.EqualTo(double4x4.identity));

        value = new CesiumMetadataValue(false);
        Assert.That(value.GetDouble4x4(defaultValue), Is.EqualTo(double4x4.zero));
    }

    [Test]
    public void GetDouble4x4ReturnsDefaultValueForUnsupportedTypes()
    {
        CesiumMetadataValue value = new CesiumMetadataValue();
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(double4x4.zero));

        value = new CesiumMetadataValue(new int3(1));
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(double4x4.zero));

        value = new CesiumMetadataValue(new CesiumPropertyArray());
        Assert.That(value.GetDouble4x4(double4x4.zero), Is.EqualTo(double4x4.zero));
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
