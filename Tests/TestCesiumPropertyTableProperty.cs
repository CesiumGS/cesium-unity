using CesiumForUnity;
using NUnit.Framework;
using System;
using Unity.Mathematics;

public class TestCesiumPropertyTableProperty
{
    #region Constructor
    [Test]
    public void ConstructsEmptyProperty()
    {
        CesiumPropertyTableProperty property = new CesiumPropertyTableProperty();
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.ErrorInvalidProperty));
        Assert.That(property.size, Is.EqualTo(0));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Invalid));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        Assert.That(property.arraySize, Is.EqualTo(0));
        Assert.That(property.isNormalized, Is.False);
        Assert.That(property.offset.isEmpty, Is.True);
        Assert.That(property.scale.isEmpty, Is.True);
        Assert.That(property.min.isEmpty, Is.True);
        Assert.That(property.max.isEmpty, Is.True);
        Assert.That(property.noData.isEmpty, Is.True);
        Assert.That(property.defaultValue.isEmpty, Is.True);
    }

    [Test]
    public void ConstructsBooleanProperty()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        model.Dispose();
    }

    [Test]
    public void ConstructsScalarProperty()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { -1, 0, 1, 2 };

        // Non-normalized property
        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));
        Assert.That(property.isNormalized, Is.False);

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        // Normalized property
        property = model.AddIntPropertyTableProperty(testValues, true);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));
        Assert.That(property.isNormalized, Is.True);

        valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        model.Dispose();
    }

    [Test]
    public void ConstructsVecNProperty()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 3),
            new float3(4, 5, 6),
            new float3(7, 8, 9)};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        model.Dispose();
    }

    [Test]
    public void ConstructsMatNProperty()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 3, 4),
            new float2x2(4, 5, 6, 7),
            new float2x2(7, 8, 9, 0)};

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        model.Dispose();
    }

    [Test]
    public void ConstructsStringProperty()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "test 1", "test 2", "test 3" };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        model.Dispose();
    }

    [Test]
    public void ConstructsArrayProperty()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 3, 4, -5, -6 };
        const Int64 count = 2;

        CesiumPropertyTableProperty property = model.AddFixedLengthArrayPropertyTableProperty(testValues, count);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length / count));
        Assert.That(property.arraySize, Is.EqualTo(count));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.True);

        model.Dispose();
    }

    #endregion

    #region GetBoolean
    [Test]
    public void GetBooleanReturnsBooleanValues()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            // Explicitly set the default value to the opposite boolean value, ensuring that
            // the values are truthfully returned.
            Assert.That(property.GetBoolean(i, !testValues[i]), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetBooleanConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { -1, 0, 1, 2 };
        bool[] expected = { true, false, true, true };

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetBoolean(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetBooleanConvertsStringValues()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "true", "false", "bad string" };
        bool[] expected = { true, false, false };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetBoolean(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetSByte
    [Test]
    public void GetSByteReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { -1, -1, 1, 2, 129 };
        SByte[] expected = { -1, -1, 1, 2, 0 };

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetSByte(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetSByteConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        SByte[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetSByte(i, -1), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetSByteConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "not a number" };
        SByte[] expected = { 10, 20, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetSByte(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetByte
    [Test]
    public void GetByteReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { 1, 2, -3, 4, -1 };
        Byte[] expected = { 1, 2, 0, 4, 0 };

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetByte(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetByteConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        Byte[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetByte(i, 10), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetByteConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1" };
        Byte[] expected = { 10, 20, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetByte(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetInt16

    [Test]
    public void GetInt16ReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { 1, 2, UInt16.MaxValue, 4, Int32.MinValue, -1 };
        Int16[] expected = { 1, 2, 0, 4, 0, -1 };

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt16(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt16ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        Int16[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt16(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt16ConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        Int16[] expected = { 10, 20, -1, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt16(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetUInt16

    [Test]
    public void GetUInt16ReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { 1, 2, Int32.MaxValue, 4, 110, -1 };
        Int16[] expected = { 1, 2, 0, 4, 110, 0 };

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt16(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt16ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        UInt16[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt16(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt16ConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        Int16[] expected = { 10, 20, 0, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt16(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetInt32

    [Test]
    public void GetInt32ReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 5.3, -2.1, 9, Int64.MaxValue };
        Int32[] expected = { 1, 2, 5, -2, 9, 0 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt32(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt32ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        Int32[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt32(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt32ConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        Int32[] expected = { 10, 20, -1, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt32(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetUInt32

    [Test]
    public void GetUInt32ReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 5.3, -2.1, 9, Int64.MaxValue };
        UInt32[] expected = { 1, 2, 5, 0, 9, 0 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt32(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt32ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        UInt32[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt32(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt32ConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        UInt32[] expected = { 10, 20, 0, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt32(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetInt64

    [Test]
    public void GetInt64ReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 5.3, -2.1, 9, UInt64.MaxValue };
        Int64[] expected = { 1, 2, 5, -2, 9, 0 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt64(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt64ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        Int64[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt64(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt64ConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        Int64[] expected = { 10, 20, -1, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt64(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetUInt64

    [Test]
    public void GetUInt64ReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 5.3, -2.1, 9, Double.MaxValue };
        UInt64[] expected = { 1, 2, 5, 0, 9, 0 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt64(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt64ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        UInt64[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt64(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt64ConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        UInt64[] expected = { 10, 20, 0, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt64(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetFloat

    [Test]
    public void GetFloatReturnsInRangeValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 5.3, -2.1, 9, Double.MaxValue };
        float[] expected = { 1, 2, (float)5.3, (float)-2.1, 9, 0 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloatConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        float[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloatConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        float[] expected = { 10, 20, -1, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetDouble

    [Test]
    public void GetDoubleReturnsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 5.3, -2.1, 9, Double.MaxValue };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble(i), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDoubleReturnsNormalizedScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { Int32.MaxValue, 0, Int32.MinValue + 1, Int32.MaxValue / 2 };
        double denominator = Int32.MaxValue;

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues, true);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));
        Assert.That(property.isNormalized, Is.True);

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble(i), Is.EqualTo(testValues[i] / denominator));
        }

        model.Dispose();
    }

    [Test]
    public void GetDoubleConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        double[] expected = { 1, 0, 0, 1 };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDoubleConvertsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "10", "20", "-1", "not a number" };
        double[] expected = { 10, 20, -1, 0 };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble(i), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetInt2

    [Test]
    public void GetInt2ReturnsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(UInt32.MaxValue)};
        int2[] expected = {
            new int2(testValues[0]),
            new int2(testValues[1]),
            new int2(testValues[2]),
            int2.zero};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2(i, int2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(UInt32.MaxValue)};
        int2[] expected = {
            new int2(1, 2),
            new int2(3, 4),
            new int2(5, 6),
            int2.zero};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2(i, int2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(UInt32.MaxValue)};
        int2[] expected = {
            new int2(1, 2),
            new int2(3, 4),
            new int2(5, 6),
            int2.zero};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2(i, int2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, -2.1, Double.MaxValue };
        int2[] expected = {
            new int2(1),
            new int2(5),
            new int2(-2),
            int2.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2(i, int2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        int2[] expected = {
            new int2(1),
            int2.zero,
            int2.zero,
            new int2(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        int2 defaultValue = new int2(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetUInt2

    [Test]
    public void GetUInt2ReturnsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(0, -1)};
        uint2[] expected = {
            new uint2(testValues[0]),
            new uint2(testValues[1]),
            new uint2(testValues[2]),
            uint2.zero};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2(i, uint2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};
        uint2[] expected = {
            new uint2(1, 2),
            new uint2(3, 4),
            new uint2(5, 6),
            uint2.zero};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2(i, uint2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};
        uint2[] expected = {
            new uint2(1, 2),
            new uint2(3, 4),
            new uint2(5, 6),
            uint2.zero};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2(i, uint2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, -2.1, 12.9 };
        uint2[] expected = {
            new uint2(1),
            new uint2(5),
            uint2.zero,
            new uint2(12)};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2(i, uint2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        uint2[] expected = {
            new uint2(1),
            uint2.zero,
            uint2.zero,
            new uint2(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        uint2 defaultValue = new uint2(11);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetFloat2

    [Test]
    public void GetFloat2ReturnsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(0, -1)};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2(i, float2.zero), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat2ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2(i, float2.zero), Is.EqualTo(testValues[i].xy));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat2ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2(i, float2.zero), Is.EqualTo(testValues[i].xy));
        }

        model.Dispose();
    }


    [Test]
    public void GetFloat2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, Double.MaxValue, 12.9 };
        float2[] expected = {
            new float2(1),
            new float2(5.3),
            float2.zero,
            new float2(12.9)};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2(i, float2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        float2[] expected = {
            new float2(1),
            float2.zero,
            float2.zero,
            new float2(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        float2 defaultValue = new float2(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetDouble2

    [Test]
    public void GetDouble2ReturnsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(0, -1)};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2(i, double2.zero), Is.EqualTo(new double2(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2(i, double2.zero), Is.EqualTo(new double2(testValues[i].xy)));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2(i, double2.zero), Is.EqualTo(new double2(testValues[i].xy)));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, Double.MaxValue, 12.9 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2(i, double2.zero), Is.EqualTo(new double2(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        double2[] expected = {
            new double2(1),
            double2.zero,
            double2.zero,
            new double2(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        double2 defaultValue = new double2(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetInt3

    [Test]
    public void GetInt3ReturnsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(UInt32.MaxValue)};
        int3[] expected = {
            new int3(testValues[0]),
            new int3(testValues[1]),
            new int3(testValues[2]),
            int3.zero};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3(i, int3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(UInt32.MaxValue)};
        int3[] expected = {
            new int3(1, 2, 0),
            new int3(3, 4, 0),
            new int3(5, 6, 0),
            int3.zero};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3(i, int3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(UInt32.MaxValue)};
        int3[] expected = {
            new int3(1, 2, 1),
            new int3(3, 4, 3),
            new int3(5, 6, 0),
            int3.zero};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3(i, int3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, -2.1, Double.MaxValue };
        int3[] expected = {
            new int3(1),
            new int3(5),
            new int3(-2),
            int3.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3(i, int3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        int3[] expected = {
            new int3(1),
            int3.zero,
            int3.zero,
            new int3(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        int3 defaultValue = new int3(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetUInt3

    [Test]
    public void GetUInt3ReturnsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};
        uint3[] expected = {
            new uint3(testValues[0]),
            new uint3(testValues[1]),
            new uint3(testValues[2]),
            uint3.zero};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3(i, uint3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(-1)};
        uint3[] expected = {
            new uint3(1, 2, 0),
            new uint3(3, 4, 0),
            new uint3(5, 6, 0),
            uint3.zero};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3(i, uint3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};
        uint3[] expected = {
            new uint3(1, 2, 1),
            new uint3(3, 4, 3),
            new uint3(5, 6, 0),
            uint3.zero};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3(i, uint3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, 2.1, -1 };
        uint3[] expected = {
            new uint3(1),
            new uint3(5),
            new uint3(2),
            uint3.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3(i, uint3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        uint3[] expected = {
            new uint3(1),
            uint3.zero,
            uint3.zero,
            new uint3(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        uint3 defaultValue = new uint3(11);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetFloat3

    [Test]
    public void GetFloat3ReturnsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3(i, float3.zero), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(0, -1)};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3(i, float3.zero), Is.EqualTo(new float3(testValues[i], 0)));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3(i, float3.zero), Is.EqualTo(testValues[i].xyz));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, Double.MaxValue, 12.9 };
        float3[] expected = {
            new float3(1),
            new float3(5.3),
            float3.zero,
            new float3(12.9)};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3(i, float3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        float3[] expected = {
            new float3(1),
            float3.zero,
            float3.zero,
            new float3(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        float3 defaultValue = new float3(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetDouble3

    [Test]
    public void GetDouble3ReturnsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3(i, double3.zero), Is.EqualTo(new double3(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(0, -1)};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3(i, double3.zero), Is.EqualTo(new double3(testValues[i], 0.0)));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3ConvertsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3(i, double3.zero), Is.EqualTo(new double3(testValues[i].xyz)));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, Double.MaxValue, 12.9 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3(i, double3.zero), Is.EqualTo(new double3(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        double3[] expected = {
            new double3(1),
            double3.zero,
            double3.zero,
            new double3(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        double3 defaultValue = new double3(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetInt4

    [Test]
    public void GetInt4ReturnsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(UInt32.MaxValue)};
        int4[] expected = {
            new int4(testValues[0]),
            new int4(testValues[1]),
            new int4(testValues[2]),
            int4.zero};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4(i, int4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(UInt32.MaxValue)};
        int4[] expected = {
            new int4(1, 2, 0, 0),
            new int4(3, 4, 0, 0),
            new int4(5, 6, 0, 0),
            int4.zero};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4(i, int4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(UInt32.MaxValue)};
        int4[] expected = {
            new int4(1, 2, 1, 0),
            new int4(3, 4, 3, 0),
            new int4(5, 6, 0, 0),
            int4.zero};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4(i, int4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, -2.1, Double.MaxValue };
        int4[] expected = {
            new int4(1),
            new int4(5),
            new int4(-2),
            int4.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4(i, int4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        int4[] expected = {
            new int4(1),
            int4.zero,
            int4.zero,
            new int4(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        int4 defaultValue = new int4(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetUInt4

    [Test]
    public void GetUInt4ReturnsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};
        uint4[] expected = {
            new uint4(testValues[0]),
            new uint4(testValues[1]),
            new uint4(testValues[2]),
            uint4.zero};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4(i, uint4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(-1)};
        uint4[] expected = {
            new uint4(1, 2, 0, 0),
            new uint4(3, 4, 0, 0),
            new uint4(5, 6, 0, 0),
            uint4.zero};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4(i, uint4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};
        uint4[] expected = {
            new uint4(1, 2, 1, 0),
            new uint4(3, 4, 3, 0),
            new uint4(5, 6, 0, 0),
            uint4.zero};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4(i, uint4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, 2.1, -1 };
        uint4[] expected = {
            new uint4(1),
            new uint4(5),
            new uint4(2),
            uint4.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4(i, uint4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        uint4[] expected = {
            new uint4(1),
            uint4.zero,
            uint4.zero,
            new uint4(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        uint4 defaultValue = new uint4(11);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetFloat4

    [Test]
    public void GetFloat4ReturnsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4(i, float4.zero), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]

    public void GetFloat4ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(0, -1)};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4(i, float4.zero), Is.EqualTo(new float4(testValues[i], 0, 0)));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat4ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4(i, float4.zero), Is.EqualTo(new float4(testValues[i], 0)));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, Double.MaxValue, 12.9 };
        float4[] expected = {
            new float4(1),
            new float4(5.3),
            float4.zero,
            new float4(12.9)};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4(i, float4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        float4[] expected = {
            new float4(1),
            float4.zero,
            float4.zero,
            new float4(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        float4 defaultValue = new float4(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetDouble4

    [Test]
    public void GetDouble4ReturnsVec4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4[] testValues = {
            new float4(1, 2, 1, 1),
            new float4(3, 4, 3, 1),
            new float4(5, 6, 0, 1),
            new float4(-1)};

        CesiumPropertyTableProperty property = model.AddVec4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4(i, double4.zero), Is.EqualTo(new double4(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble4ConvertsVec2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2[] testValues = {
            new float2(1, 2),
            new float2(3, 4),
            new float2(5, 6),
            new float2(0, -1)};

        CesiumPropertyTableProperty property = model.AddVec2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4(i, double4.zero), Is.EqualTo(new double4(testValues[i], 0.0, 0.0)));
        }

        model.Dispose();
    }


    [Test]
    public void GetDouble4ConvertsVec3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3[] testValues = {
            new float3(1, 2, 1),
            new float3(3, 4, 3),
            new float3(5, 6, 0),
            new float3(-1)};

        CesiumPropertyTableProperty property = model.AddVec3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Vec3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4(i, double4.zero), Is.EqualTo(new double4(testValues[i], 0.0)));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 5.3, Double.MaxValue, 12.9 };

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4(i, double4.zero), Is.EqualTo(new double4(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        double4[] expected = {
            new double4(1),
            double4.zero,
            double4.zero,
            new double4(1) };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        double4 defaultValue = new double4(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetInt2x2

    [Test]
    public void GetInt2x2ReturnsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(UInt32.MaxValue)};
        int2x2[] expected =
        {
            new int2x2(testValues[0]),
            new int2x2(testValues[1]),
            new int2x2(testValues[2]),
            int2x2.zero
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2x2(i, int2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2x2ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(UInt32.MaxValue)};
        int2x2[] expected = {
            new int2x2(1, 2, 2, 3),
            new int2x2(3, 4, 2, 8),
            new int2x2(5, 6, 8, 2),
            int2x2.zero};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2x2(i, int2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2x2ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(UInt32.MaxValue)};
        int2x2[] expected = {
            new int2x2(1, 2, 2, 3),
            new int2x2(3, 4, 2, 8),
            new int2x2(5, 6, 8, 2),
            int2x2.zero};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2x2(i, int2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2x2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, UInt32.MaxValue };
        int2x2[] expected = {
            new int2x2(2, 0, 0, 2),
            new int2x2(5, 0, 0, 5),
            new int2x2(-8, 0, 0, -8),
            int2x2.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2x2(i, int2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt2x2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        int2x2[] expected = {
            int2x2.identity,
            int2x2.zero,
            int2x2.zero,
            int2x2.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        int2x2 defaultValue = new int2x2(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt2x2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetUInt2x2

    [Test]
    public void GetUInt2x2ReturnsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(Int64.MaxValue)};
        uint2x2[] expected =
        {
            new uint2x2(testValues[0]),
            new uint2x2(testValues[1]),
            new uint2x2(testValues[2]),
            uint2x2.zero
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2x2(i, uint2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2x2ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(-1)};
        uint2x2[] expected = {
            new uint2x2(1, 2, 2, 3),
            new uint2x2(3, 4, 2, 8),
            new uint2x2(5, 6, 8, 2),
            uint2x2.zero};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2x2(i, uint2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2x2ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(-1)};
        uint2x2[] expected = {
            new uint2x2(1, 2, 2, 3),
            new uint2x2(3, 4, 2, 8),
            new uint2x2(5, 6, 8, 2),
            uint2x2.zero};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2x2(i, uint2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2x2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, 8.89, -1 };
        uint2x2[] expected = {
            new uint2x2(2, 0, 0, 2),
            new uint2x2(5, 0, 0, 5),
            new uint2x2(8, 0, 0, 8),
            uint2x2.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2x2(i, uint2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt2x2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        uint2x2[] expected = {
            uint2x2.identity,
            uint2x2.zero,
            uint2x2.zero,
            uint2x2.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        uint2x2 defaultValue = new uint2x2(11);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt2x2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetFloat2x2

    [Test]
    public void GetFloat2x2ReturnsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(UInt32.MaxValue)};

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2x2(i, float2x2.zero), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat2x2ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(UInt32.MaxValue)};
        float2x2[] expected = {
            new float2x2(1, 2, 2, 3),
            new float2x2(3, 4, 2, 8),
            new float2x2(5, 6, 8, 2),
            new float2x2(UInt32.MaxValue)};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2x2(i, float2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat2x2ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(UInt32.MaxValue)};
        float2x2[] expected = {
            new float2x2(1, 2, 2, 3),
            new float2x2(3, 4, 2, 8),
            new float2x2(5, 6, 8, 2),
            new float2x2(UInt32.MaxValue)};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2x2(i, float2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat2x2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, Double.MaxValue };
        float2x2[] expected = {
            2 * float2x2.identity,
            (float)5.1 * float2x2.identity,
            (float)-8.89 * float2x2.identity,
            float2x2.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2x2(i, float2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat2x2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        float2x2[] expected = {
            float2x2.identity,
            float2x2.zero,
            float2x2.zero,
            float2x2.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        float2x2 defaultValue = new float2x2(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat2x2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetDouble2x2

    [Test]
    public void GetDouble2x2ReturnsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(UInt32.MaxValue)};

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2x2(i, double2x2.zero), Is.EqualTo(new double2x2(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2x2ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(-1)};
        double2x2[] expected = {
            new double2x2(1, 2, 2, 3),
            new double2x2(3, 4, 2, 8),
            new double2x2(5, 6, 8, 2),
            new double2x2(-1)};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2x2(i, double2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2x2ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(-1)};
        double2x2[] expected = {
            new double2x2(1, 2, 2, 3),
            new double2x2(3, 4, 2, 8),
            new double2x2(5, 6, 8, 2),
            new double2x2(-1)};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2x2(i, double2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2x2ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, Double.MaxValue };
        double2x2[] expected = {
            new double2x2(2, 0, 0, 2),
            new double2x2(5.1, 0, 0, 5.1),
            new double2x2(-8.89, 0, 0, -8.89),
            new double2x2(Double.MaxValue, 0, 0, Double.MaxValue)};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2x2(i, double2x2.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble2x2ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        double2x2[] expected = {
            double2x2.identity,
            double2x2.zero,
            double2x2.zero,
            double2x2.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        double2x2 defaultValue = new double2x2(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble2x2(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetInt3x3

    [Test]
    public void GetInt3x3ReturnsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(UInt32.MaxValue)};
        int3x3[] expected = {
            new int3x3(testValues[0]),
            new int3x3(testValues[1]),
            new int3x3(testValues[2]),
            int3x3.zero};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3x3(i, int3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3x3ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(UInt32.MaxValue)};
        int3x3[] expected =
        {
            new int3x3(1, 2, 0,
                       1, 1, 0,
                       0, 0, 0),
            new int3x3(3, 4, 0,
                       3, 1, 0,
                       0, 0, 0),
            new int3x3(5, 6, 0,
                       0, 1, 0,
                       0, 0, 0),
            int3x3.zero
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3x3(i, int3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3x3ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(UInt32.MaxValue)};
        int3x3[] expected = {
            new int3x3(1, 2, 1,
                       2, 3, -1,
                       0, 4, 6),
            new int3x3(3, 4, 3,
                       2, 8, 1,
                       7, -7, -1),
            new int3x3(5, 6, 0,
                       8, 2, 5,
                       5, -2, 78),
            int3x3.zero};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3x3(i, int3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3x3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, UInt32.MaxValue };
        int3x3[] expected = {
            2 * int3x3.identity,
            5 * int3x3.identity,
            -8 * int3x3.identity,
            int3x3.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3x3(i, int3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt3x3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        int3x3[] expected = {
            int3x3.identity,
            int3x3.zero,
            int3x3.zero,
            int3x3.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        int3x3 defaultValue = new int3x3(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt3x3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetUInt3x3

    [Test]
    public void GetUInt3x3ReturnsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, 1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, 7, 1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, 2, 78),
            new float3x3(-1)};
        uint3x3[] expected = {
            new uint3x3(testValues[0]),
            new uint3x3(testValues[1]),
            new uint3x3(testValues[2]),
            uint3x3.zero};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3x3(i, uint3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3x3ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(UInt32.MaxValue)};
        uint3x3[] expected =
        {
            new uint3x3(1, 2, 0,
                        1, 1, 0,
                        0, 0, 0),
            new uint3x3(3, 4, 0,
                        3, 1, 0,
                        0, 0, 0),
            new uint3x3(5, 6, 0,
                        0, 1, 0,
                        0, 0, 0),
            uint3x3.zero
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3x3(i, uint3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3x3ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, 1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, 7, 1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, 2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(UInt32.MaxValue)};
        uint3x3[] expected = {
            new uint3x3(1, 2, 1,
                       2, 3, 1,
                       0, 4, 6),
            new uint3x3(3, 4, 3,
                       2, 8, 1,
                       7, 7, 1),
            new uint3x3(5, 6, 0,
                       8, 2, 5,
                       5, 2, 78),
            uint3x3.zero};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3x3(i, uint3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3x3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, 8.89, -1 };
        uint3x3[] expected = {
            2 * uint3x3.identity,
            5 * uint3x3.identity,
            8 * uint3x3.identity,
            uint3x3.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3x3(i, uint3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt3x3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        uint3x3[] expected = {
            uint3x3.identity,
            uint3x3.zero,
            uint3x3.zero,
            uint3x3.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        uint3x3 defaultValue = new uint3x3(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt3x3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetFloat3x3

    [Test]
    public void GetFloat3x3ReturnsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(-1)};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3x3(i, float3x3.zero), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3x3ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(-1)};
        float3x3[] expected =
        {
            new float3x3(1, 2, 0,
                         1, 1, 0,
                         0, 0, 0),
            new float3x3(3, 4, 0,
                         3, 1, 0,
                         0, 0, 0),
            new float3x3(5, 6, 0,
                         0, 1, 0,
                         0, 0, 0),
            new float3x3(-1, -1, 0,
                         -1, -1, 0,
                         0, 0, 0)
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3x3(i, float3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3x3ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(-1)};
        float3x3[] expected = {
            new float3x3(1, 2, 1,
                       2, 3, -1,
                       0, 4, 6),
            new float3x3(3, 4, 3,
                       2, 8, 1,
                       7, -7, -1),
            new float3x3(5, 6, 0,
                       8, 2, 5,
                       5, -2, 78),
            new float3x3(-1)};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3x3(i, float3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3x3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, Double.MaxValue };
        float3x3[] expected = {
            2 * float3x3.identity,
            (float)5.1 * float3x3.identity,
            (float)-8.89 * float3x3.identity,
            float3x3.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3x3(i, float3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat3x3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        float3x3[] expected = {
            float3x3.identity,
            float3x3.zero,
            float3x3.zero,
            float3x3.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        float3x3 defaultValue = new float3x3(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat3x3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetDouble3x3

    [Test]
    public void GetDouble3x3ReturnsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(-1)};

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3x3(i, double3x3.zero), Is.EqualTo(new double3x3(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3x3ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(-1)};
        double3x3[] expected =
        {
            new double3x3(1, 2, 0,
                         1, 1, 0,
                         0, 0, 0),
            new double3x3(3, 4, 0,
                         3, 1, 0,
                         0, 0, 0),
            new double3x3(5, 6, 0,
                         0, 1, 0,
                         0, 0, 0),
            new double3x3(-1, -1, 0,
                          -1, -1, 0,
                          0, 0, 0)
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3x3(i, double3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3x3ConvertsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(-1)};
        double3x3[] expected = {
            new double3x3(1, 2, 1,
                          2, 3, -1,
                          0, 4, 6),
            new double3x3(3, 4, 3,
                          2, 8, 1,
                          7, -7, -1),
            new double3x3(5, 6, 0,
                          8, 2, 5,
                          5, -2, 78),
            new double3x3(-1)};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3x3(i, double3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3x3ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, -1 };
        double3x3[] expected = {
            2 * double3x3.identity,
            5.1 * double3x3.identity,
            -8.89 * double3x3.identity,
            -1 * double3x3.identity};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3x3(i, double3x3.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble3x3ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        double3x3[] expected = {
            double3x3.identity,
            double3x3.zero,
            double3x3.zero,
            double3x3.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        double3x3 defaultValue = new double3x3(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble3x3(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetInt4x4

    [Test]
    public void GetInt4x4ReturnsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(UInt32.MaxValue)};

        int4x4[] expected = {
            new int4x4(testValues[0]),
            new int4x4(testValues[1]),
            new int4x4(testValues[2]),
            int4x4.zero };

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4x4(i, int4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4x4ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(UInt32.MaxValue)};
        int4x4[] expected =
        {
            new int4x4(1, 2, 0, 0,
                       1, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            new int4x4(3, 4, 0, 0,
                       3, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            new int4x4(5, 6, 0, 0,
                       0, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            int4x4.zero
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4x4(i, int4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4x4ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(UInt32.MaxValue)};
        int4x4[] expected =
        {
            new int4x4(1, 2, 1, 0,
                       2, 3, -1, 0,
                       0, 4, 6, 0,
                       0, 0, 0, 0),
            new int4x4(3, 4, 3, 0,
                       2, 8, 1, 0,
                       7, -7, -1, 0,
                       0, 0, 0, 0),
            new int4x4(5, 6, 0, 0,
                       8, 2, 5, 0,
                       5, -2, 78, 0,
                       0, 0, 0, 0),
            int4x4.zero
        };

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4x4(i, int4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4x4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, UInt32.MaxValue };
        int4x4[] expected = {
            2 * int4x4.identity,
            5 * int4x4.identity,
            -8 * int4x4.identity,
            int4x4.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4x4(i, int4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetInt4x4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        int4x4[] expected = {
            int4x4.identity,
            int4x4.zero,
            int4x4.zero,
            int4x4.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        int4x4 defaultValue = new int4x4(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetInt4x4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetUInt4x4

    [Test]
    public void GetUInt4x4ReturnsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, 1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, 4,
                         2, 8, 1, 2,
                         7, 7, 1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, 2, 78, 9,
                         0, 0, 0, 1),
            new float4x4(-1)};

        uint4x4[] expected = {
            new uint4x4(testValues[0]),
            new uint4x4(testValues[1]),
            new uint4x4(testValues[2]),
            uint4x4.zero };

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4x4(i, uint4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4x4ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(UInt32.MaxValue)};
        uint4x4[] expected =
        {
            new uint4x4(1, 2, 0, 0,
                       1, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            new uint4x4(3, 4, 0, 0,
                       3, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            new uint4x4(5, 6, 0, 0,
                       0, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            uint4x4.zero
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4x4(i, uint4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4x4ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, 1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, 7, 1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, 2, 78),
            new float3x3(-1)};
        uint4x4[] expected =
        {
            new uint4x4(1, 2, 1, 0,
                       2, 3, 1, 0,
                       0, 4, 6, 0,
                       0, 0, 0, 0),
            new uint4x4(3, 4, 3, 0,
                       2, 8, 1, 0,
                       7, 7, 1, 0,
                       0, 0, 0, 0),
            new uint4x4(5, 6, 0, 0,
                       8, 2, 5, 0,
                       5, 2, 78, 0,
                       0, 0, 0, 0),
            uint4x4.zero
        };

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4x4(i, uint4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4x4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, 8.89, -1 };
        uint4x4[] expected = {
            2 * uint4x4.identity,
            5 * uint4x4.identity,
            8 * uint4x4.identity,
            uint4x4.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4x4(i, uint4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetUInt4x4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        uint4x4[] expected = {
            uint4x4.identity,
            uint4x4.zero,
            uint4x4.zero,
            uint4x4.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        uint4x4 defaultValue = new uint4x4(11);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetUInt4x4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetFloat4x4

    [Test]
    public void GetFloat4x4ReturnsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(-1)};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4x4(i, float4x4.zero), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat4x4ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(-1)};
        float4x4[] expected =
        {
            new float4x4(1, 2, 0, 0,
                       1, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            new float4x4(3, 4, 0, 0,
                       3, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            new float4x4(5, 6, 0, 0,
                       0, 1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0),
            new float4x4(-1, -1, 0, 0,
                       -1, -1, 0, 0,
                       0, 0, 0, 0,
                       0, 0, 0, 0)
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4x4(i, float4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat4x4ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(-1)};
        float4x4[] expected =
        {
            new float4x4(1, 2, 1, 0,
                       2, 3, -1, 0,
                       0, 4, 6, 0,
                       0, 0, 0, 0),
            new float4x4(3, 4, 3, 0,
                       2, 8, 1, 0,
                       7, -7, -1, 0,
                       0, 0, 0, 0),
            new float4x4(5, 6, 0, 0,
                       8, 2, 5, 0,
                       5, -2, 78, 0,
                       0, 0, 0, 0),
            new float4x4(-1, -1, -1, 0,
                       -1, -1, -1, 0,
                       -1, -1, -1, 0,
                       0, 0, 0, 0)
        };

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4x4(i, float4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat4x4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, Double.MaxValue };
        float4x4[] expected = {
            2 * float4x4.identity,
            (float)5.1 * float4x4.identity,
            (float)-8.89 * float4x4.identity,
            float4x4.zero};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4x4(i, float4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetFloat4x4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        float4x4[] expected = {
            float4x4.identity,
            float4x4.zero,
            float4x4.zero,
            float4x4.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        float4x4 defaultValue = new float4x4(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetFloat4x4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetDouble4x4

    [Test]
    public void GetDouble4x4ReturnsMat4Values()
    {
        TestGltfModel model = new TestGltfModel();
        float4x4[] testValues = {
            new float4x4(1, 2, 1, 8,
                         2, 3, -1, 9,
                         0, 4, 6, 2,
                         0, 0, 0, 1),
            new float4x4(3, 4, 3, -4,
                         2, 8, 1, 2,
                         7, -7, -1, 0,
                         0, 0, 0, 1),
            new float4x4(5, 6, 0, 4,
                         8, 2, 5, 8,
                         5, -2, 78, -9,
                         0, 0, 0, 1),
            new float4x4(-1)};

        CesiumPropertyTableProperty property = model.AddMat4PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat4));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4x4(i, double4x4.zero), Is.EqualTo(new double4x4(testValues[i])));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble4x4ConvertsMat2Values()
    {
        TestGltfModel model = new TestGltfModel();
        float2x2[] testValues = {
            new float2x2(1, 2, 1, 1),
            new float2x2(3, 4, 3, 1),
            new float2x2(5, 6, 0, 1),
            new float2x2(-1)};
        double4x4[] expected =
        {
            new double4x4(1, 2, 0, 0,
                          1, 1, 0, 0,
                          0, 0, 0, 0,
                          0, 0, 0, 0),
            new double4x4(3, 4, 0, 0,
                          3, 1, 0, 0,
                          0, 0, 0, 0,
                          0, 0, 0, 0),
            new double4x4(5, 6, 0, 0,
                          0, 1, 0, 0,
                          0, 0, 0, 0,
                          0, 0, 0, 0),
            new double4x4(-1, -1, 0, 0,
                          -1, -1, 0, 0,
                          0, 0, 0, 0,
                          0, 0, 0, 0)
        };

        CesiumPropertyTableProperty property = model.AddMat2PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat2));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4x4(i, double4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble4x4ConvertsMat3Values()
    {
        TestGltfModel model = new TestGltfModel();
        float3x3[] testValues = {
            new float3x3(1, 2, 1,
                         2, 3, -1,
                         0, 4, 6),
            new float3x3(3, 4, 3,
                         2, 8, 1,
                         7, -7, -1),
            new float3x3(5, 6, 0,
                         8, 2, 5,
                         5, -2, 78),
            new float3x3(-1)};
        double4x4[] expected =
        {
            new double4x4(1, 2, 1, 0,
                          2, 3, -1, 0,
                          0, 4, 6, 0,
                          0, 0, 0, 0),
            new double4x4(3, 4, 3, 0,
                          2, 8, 1, 0,
                          7, -7, -1, 0,
                          0, 0, 0, 0),
            new double4x4(5, 6, 0, 0,
                          8, 2, 5, 0,
                          5, -2, 78, 0,
                          0, 0, 0, 0),
            new double4x4(-1, -1, -1, 0,
                          -1, -1, -1, 0,
                          -1, -1, -1, 0,
                          0, 0, 0, 0)
        };

        CesiumPropertyTableProperty property = model.AddMat3PropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Mat3));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4x4(i, double4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble4x4ConvertsScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 2, 5.1, -8.89, Double.MaxValue };
        double4x4[] expected = {
            2 * double4x4.identity,
            5.1 * double4x4.identity,
            -8.89 * double4x4.identity,
            Double.MaxValue * double4x4.identity};

        CesiumPropertyTableProperty property = model.AddDoublePropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4x4(i, double4x4.zero), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetDouble4x4ConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        double4x4[] expected = {
            double4x4.identity,
            double4x4.zero,
            double4x4.zero,
            double4x4.identity};

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        double4x4 defaultValue = new double4x4(-1);
        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetDouble4x4(i, defaultValue), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    #endregion

    #region GetString

    [Test]
    public void GetStringReturnsStringValue()
    {
        TestGltfModel model = new TestGltfModel();
        string[] testValues = { "test 1", "test 2", "test 3" };

        CesiumPropertyTableProperty property = model.AddStringPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.String));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetString(i, String.Empty), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetStringConvertsBooleanValue()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };
        string[] expected = { "true", "false", "false", "true" };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetString(i, String.Empty), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetStringConvertsScalarValue()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { 2, 5, 8, -1 };
        string[] expected = { "2", "5", "8", "-1" };

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetString(i, String.Empty), Is.EqualTo(expected[i]));
        }

        model.Dispose();
    }
    #endregion

    #region GetArray

    [Test]
    public void GetArrayReturnsFixedLengthArrayValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 3, 4, -5, -6 };
        const Int64 count = 2;
        Int64 size = testValues.Length / count;

        CesiumPropertyTableProperty property = model.AddFixedLengthArrayPropertyTableProperty(testValues, count);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(size));
        Assert.That(property.arraySize, Is.EqualTo(count));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.True);

        for (Int64 i = 0; i < size; i++)
        {
            CesiumPropertyArray array = property.GetArray(i);
            Assert.That(array.length, Is.EqualTo(count));
            Assert.That(array.elementValueType.type, Is.EqualTo(valueType.type));
            Assert.That(array.elementValueType.componentType, Is.EqualTo(valueType.componentType));

            Int64 arrayStartIndex = i * count;
            for (int j = 0; j < count; j++)
            {
                Assert.That(array[j].GetDouble(), Is.EqualTo(testValues[arrayStartIndex + j]));
            }
        }

        model.Dispose();
    }

    [Test]
    public void GetArrayReturnsVariableLengthArrayValues()
    {
        TestGltfModel model = new TestGltfModel();
        double[] testValues = { 1, 2, 3, 4, -5, -6 };
        UInt16[] offsets = { 0, 1, 4, 6 };
        Int64 size = offsets.Length - 1;

        CesiumPropertyTableProperty property = model.AddVariableLengthArrayPropertyTableProperty(testValues, offsets);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(size));
        Assert.That(property.arraySize, Is.EqualTo(0));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
        Assert.That(valueType.isArray, Is.True);

        for (Int64 i = 0; i < size; i++)
        {
            CesiumPropertyArray array = property.GetArray(i);
            Int64 arrayStartIndex = offsets[i];
            Int64 arrayEndIndex = offsets[i + 1];

            Int64 expectedLength = arrayEndIndex - arrayStartIndex;
            Assert.That(array.length, Is.EqualTo(expectedLength));
            Assert.That(array.elementValueType.type, Is.EqualTo(valueType.type));
            Assert.That(array.elementValueType.componentType, Is.EqualTo(valueType.componentType));

            for (Int64 j = 0; j < expectedLength; j++)
            {
                Assert.That(array[j].GetDouble(), Is.EqualTo(testValues[arrayStartIndex + j]));
            }
        }

        model.Dispose();
    }

    [Test]
    public void GetArrayReturnsEmptyArrayForNonArrayProperty()
    {
        TestGltfModel model = new TestGltfModel();
        bool[] testValues = { true, false, false, true };

        CesiumPropertyTableProperty property = model.AddBooleanPropertyTableProperty(testValues);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Boolean));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.None));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            Assert.That(property.GetArray(i).length, Is.EqualTo(0));
        }

        model.Dispose();
    }

    #endregion

    [Test]
    public void GetRawValueReturnsRawScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { Int32.MaxValue, 0, Int32.MinValue + 1, Int32.MaxValue / 2 };

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues, true);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));
        Assert.That(property.isNormalized, Is.True);

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            CesiumMetadataValue rawValue = property.GetRawValue(i);
            Assert.That(rawValue.valueType, Is.EqualTo(valueType));
            Assert.That(rawValue.GetInt32(), Is.EqualTo(testValues[i]));
        }

        model.Dispose();
    }

    [Test]
    public void GetValueReturnsTransformedScalarValues()
    {
        TestGltfModel model = new TestGltfModel();
        int[] testValues = { Int32.MaxValue, 0, Int32.MinValue + 1, Int32.MaxValue / 2 };
        double denominator = Int32.MaxValue;

        CesiumPropertyTableProperty property = model.AddIntPropertyTableProperty(testValues, true);
        Assert.That(property.status, Is.EqualTo(CesiumPropertyTablePropertyStatus.Valid));
        Assert.That(property.size, Is.EqualTo(testValues.Length));
        Assert.That(property.isNormalized, Is.True);

        CesiumMetadataValueType valueType = property.valueType;
        Assert.That(valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
        Assert.That(valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Int32));
        Assert.That(valueType.isArray, Is.False);

        for (Int64 i = 0; i < testValues.Length; i++)
        {
            CesiumMetadataValue rawValue = property.GetValue(i);
            Assert.That(rawValue.valueType.type, Is.EqualTo(CesiumMetadataType.Scalar));
            Assert.That(rawValue.valueType.componentType, Is.EqualTo(CesiumMetadataComponentType.Float64));
            Assert.That(rawValue.GetDouble(), Is.EqualTo(testValues[i] / denominator));
        }

        model.Dispose();
    }
}
