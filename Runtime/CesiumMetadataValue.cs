using System;
using Unity.Mathematics;
using Reinterop;
using System.Collections.Generic;
namespace CesiumForUnity
{
    /// <summary>
    /// Represents the value type of a metadata value or property, akin to the 
    /// property types in EXT_structural_metadata.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataValueImpl", "CesiumMetadataValueImpl.h", staticOnly: true)]
    public partial class CesiumMetadataValue
    {
        /// <summary>
        /// The value as an object. This functions similarly to a std::any in C++.
        /// </summary>
        /// <remarks>
        /// If this is intended to hold an integer vecN or matN value, use the appropriate
        /// CesiumIntN, CesiumUIntN, CesiumIntMatN, or CesiumUIntMatN structs. Only
        /// use Unity.Mathematics for vecNs or matNs with floating point components.
        /// </remarks>
        internal System.Object valueImpl { get; set; }

        #region Getters

        /// <summary>
        /// The type of the metadata value as defined in the 
        /// EXT_structural_metadata extension. Some of these types are not 
        /// accessible from Unity, but can be converted to a close-fitting type.
        /// </summary>
        public CesiumMetadataValueType valueType
        {
            get
            {
                return CesiumMetadataValueType.GetValueType(this.valueImpl);
            }
        }

        /// <summary>
        /// Whether the value is empty, i.e., whether it does not actually represent 
        /// any data. 
        /// </summary>
        /// <remarks>
        /// A CesiumMetadataValue can be empty, similar to how a C# nullable may be null.
        /// For example, when the raw value of a property matches the property's specified 
        /// "no data" value, it will return an empty CesiumMetadataValue.
        /// </remarks>
        /// <returns>Whether the value is empty.</returns>
        public bool isEmpty
        {
            get { return this.valueImpl == null; }
        }
        #endregion

        #region Constructors
        public CesiumMetadataValue() : this(null)
        { }

        public CesiumMetadataValue(System.Object value)
        {
            // For consistent implementation, convert intN to CesiumIntVecN and uintN to CesiumUintVecN
            switch (value)
            {
                case int2:
                    int2 asInt2 = (value as int2?).Value;
                    this.valueImpl = new CesiumIntVec2(asInt2.x, asInt2.y);
                    break;
                case int3:
                    int3 asInt3 = (value as int3?).Value;
                    this.valueImpl = new CesiumIntVec3(asInt3.x, asInt3.y, asInt3.z);
                    break;
                case int4:
                    int4 asInt4 = (value as int4?).Value;
                    this.valueImpl = new CesiumIntVec4(asInt4.x, asInt4.y, asInt4.z, asInt4.w);
                    break;
                case uint2:
                    uint2 asUint2 = (value as uint2?).Value;
                    this.valueImpl = new CesiumUIntVec2(asUint2.x, asUint2.y);
                    break;
                case uint3:
                    uint3 asUint3 = (value as uint3?).Value;
                    this.valueImpl = new CesiumUIntVec3(asUint3.x, asUint3.y, asUint3.z);
                    break;
                case uint4:
                    uint4 asUint4 = (value as uint4?).Value;
                    this.valueImpl = new CesiumUIntVec4(asUint4.x, asUint4.y, asUint4.z, asUint4.w);
                    break;
                default:
                    this.valueImpl = value;
                    break;
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Attempts to retrieve the value as a boolean.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is a boolean, it is returned as-is.<br/>
        /// 
        /// If the value is a scalar, zero is converted to false, while any 
        /// other value is converted to true.<br/>
        /// 
        /// If the value is a string, "0", "false", and "no" (case-insensitive)
        /// are converted to false, while "1", "true", and "yes" are converted to
        /// true. All other strings, including strings that can be converted to 
        /// numbers, will return the default value.<br/><br/>
        /// </para>
        /// <para>
        /// All other types return the default value.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The value as a Boolean.</returns>
        public Boolean GetBoolean(Boolean defaultValue = false)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToBoolean(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a signed 8-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between -128 and 127, it is returned 
        /// as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between -128 and 127, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.<br/>
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a SByte.</param>
        /// <returns>The property value as a SByte.</returns>
        public SByte GetSByte(SByte defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToSByte(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as an unsigned 8-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between 0 and 255, it is returned 
        /// as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between 0 and 255, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.<br/>
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a Byte.</param>
        /// <returns>The property value as a Byte.</returns>
        public Byte GetByte(Byte defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToByte(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a signed 16-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between -32768 and 32767, it is returned 
        /// as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between -32768 and 32767, the parsed value is returned. 
        /// The string is parsed in a locale-independent way and does not support
        /// the use of commas or other delimiters to group digits together.<br/>
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a Int16.</param>
        /// <returns>The property value as a Int16.</returns>
        public Int16 GetInt16(Int16 defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToInt16(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a unsigned 16-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between 0 and 65535, it is returned 
        /// as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between 0 and 65535, the parsed value is returned. The
        /// string is parsed in a locale-independent way and does not support
        /// the use of commas or other delimiters to group digits together.<br/>
        /// 
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a UInt16.</param>
        /// <returns>The property value as a UInt16.</returns>
        public UInt16 GetUInt16(UInt16 defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToUInt16(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a signed 32-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between -2,147,483,648 and 2,147,483,647,
        /// it is returned as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between -2,147,483,648 and 2,147,483,647, the parsed value 
        /// is returned. The string is parsed in a locale-independent way and does 
        /// not support the use of commas or other delimiters to group digits 
        /// together.<br/>
        /// 
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a Int32.</param>
        /// <returns>The property value as a Int32.</returns>
        public Int32 GetInt32(Int32 defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToInt32(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a unsigned 32-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between 0 and 4,294,967,295, it is
        /// returned as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between 0 and 4,294,967,295, the parsed value is returned. 
        /// The string is parsed in a locale-independent way and does not support
        /// the use of commas or other delimiters to group digits together.<br/>
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a UInt32.</param>
        /// <returns>The property value as a UInt32.</returns>
        public UInt32 GetUInt32(UInt32 defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToUInt32(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a signed 64-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between -2^63 and (2^63 - 1),
        /// it is returned as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between -2^63 and (2^63 - 1), the parsed value 
        /// is returned. The string is parsed in a locale-independent way and does 
        /// not support the use of commas or other delimiters to group digits 
        /// together.<br/>
        /// 
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a Int64.</param>
        /// <returns>The property value as a Int64.</returns>
        public Int64 GetInt64(Int64 defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToInt64(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a unsigned 64-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an integer between 0 and (2^64-1), it is
        /// returned as-is.<br/>
        /// 
        /// If the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// If the value is a boolean, 1 is returned for true and 0 for 
        /// false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as an 
        /// integer between 0 and (2^64-1), the parsed value is returned. 
        /// The string is parsed in a locale-independent way and does not support
        /// the use of commas or other delimiters to group digits together.<br/>
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a UInt64.</param>
        /// <returns>The property value as a UInt64.</returns>
        public UInt64 GetUInt64(UInt64 defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToUInt64(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a single-precision floating-point
        /// number.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is already a single-precision floating-point number, it is
        /// returned as-is.<br/>
        /// 
        /// If the value is a scalar of any other type within the range of values that
        /// a single-precision float can represent, it is converted to its closest
        /// representation as a single-precision float and returned.<br/>
        /// 
        /// If the value is a boolean, 1.0f is returned for true and 0.0f for false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as a
        /// number, the parsed value is returned. The string is parsed in a 
        /// locale-independent way and does not support the use of commas or other 
        /// delimiters to group digits together.<br/>
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a Float.</param>
        /// <returns>The property value as a Float.</returns>
        public float GetFloat(float defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToFloat(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a double-precision floating-point
        /// number.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is already a single- or double- floating-point number, it is
        /// returned as-is.<br/>
        /// 
        /// If the value is an integer, it is converted to the closest representable
        /// double-precision floating-point number.<br/>
        /// 
        /// If the value is a boolean, 1.0 is returned for true and 0.0 for false.<br/>
        /// 
        /// If the value is a string and the entire string can be parsed as a
        /// number, the parsed value is returned. The string is parsed in a 
        /// locale-independent way and does not support the use of commas or other 
        /// delimiters to group digits together.<br/>
        /// In all other cases, the default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a Double.</param>
        /// <returns>The property value as a Double.</returns>
        public double GetDouble(double defaultValue = 0)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToDouble(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an int2.
        /// </summary>
        /// <remarks>
        /// If the value is a 2-dimensional vector, its components will be converted 
        /// to 32-bit signed integers if possible.<br/>
        /// 
        /// If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the int2.<br/>
        /// 
        /// If the value is a scalar that can be converted to a 32-bit signed integer,
        /// the resulting int2 will have this value in both of its components.<br/>
        /// 
        /// If the value is a boolean, (1, 1) is returned for true, while 
        /// (0, 0) is returned for false.<br/>
        /// 
        /// In all other cases, the user-defined default value is returned. 
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a int2.</param>
        /// <returns>The property value as a int2.</returns>
        public int2 GetInt2(int2 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToInt2(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a uint2.
        /// </summary>
        /// <remarks>
        /// If the value is a 2-dimensional vector, its components will be converted 
        /// to 32-bit unsigned integers if possible.<br/>
        /// 
        /// If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the uint2.<br/>
        /// 
        /// If the value is a scalar that can be converted to a 32-bit unsigned integer,
        /// the resulting uint2 will have this value in both of its components.<br/>
        /// 
        /// If the value is a boolean, (1, 1) is returned for true, while 
        /// (0, 0) is returned for false.<br/>
        /// 
        /// In all other cases, the user-defined default value is returned. 
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a uint2.</param>
        /// <returns>The property value as a uint2.</returns>
        public uint2 GetUInt2(uint2 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToUInt2(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a float2.
        /// </summary>
        /// <remarks>
        /// If the value is a 2-dimensional vector, its components will be converted 
        /// to the closest representable single-precision floats, if possible.
        /// 
        /// If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the float2.<br/>
        /// 
        /// If the value is a scalar that can be converted to a single-precision
        /// floating-point number, the resulting float2 will have this value in
        /// both of its components.<br/>
        /// 
        /// If the value is a boolean, (1.0f, 1.0f) is returned for true, while 
        /// (0.0f, 0.0f) is returned for false.<br/>
        /// 
        /// In all other cases, the user-defined default value is returned. 
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a float2.</param>
        /// <returns>The property value as a float2.</returns>
        public float2 GetFloat2(float2 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToFloat2(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a double2.
        /// </summary>
        /// <remarks>
        /// If the value is a 2-dimensional vector, its components will be converted 
        /// to double-precision floating-point numbers.<br/>
        /// 
        /// If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the double2.<br/>
        /// 
        /// If the value is a scalar, the resulting double2 will have this value in
        /// both of its components.<br/>
        /// 
        /// If the value is a boolean, (1.0, 1.0) is returned for true, while 
        /// (0.0, 0.0) is returned for false.<br/>
        /// In all other cases, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a double2.</param>
        /// <returns>The property value as a double2.</returns>
        public double2 GetDouble2(double2 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToDouble2(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an int3.
        /// </summary>
        /// <remarks>
        /// If the value is a 3-dimensional vector, its components will be converted 
        /// to 32-bit signed integers if possible.<br/>
        /// 
        /// If the value is a 4-dimensional vector, it will use the first three
        /// components to construct the int3.<br/>
        /// 
        /// If the value is a 2-dimensional vector, it will become the XY-components of
        /// the int3. The Z component will be set to zero.<br/>
        /// 
        /// If the value is a scalar that can be converted to a 32-bit signed integer,
        /// the resulting int3 will have this value in both of its components.<br/>
        /// 
        /// If the value is a boolean, (1, 1, 1) is returned for true, while 
        /// (0, 0, 0) is returned for false.<br/>
        /// 
        /// In all other cases, the user-defined default value is returned. 
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a int3.</param>
        /// <returns>The property value as a int3.</returns>
        public int3 GetInt3(int3 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToInt3(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an uint3.
        /// </summary>
        /// <remarks>
        /// If the value is a 3-dimensional vector, its components will be converted 
        /// to 32-bit unsigned integers if possible.<br/>
        /// 
        /// If the value is a 4-dimensional vector, it will use the first three
        /// components to construct the uint3.<br/>
        /// 
        /// If the value is a 2-dimensional vector, it will become the XY-components of
        /// the uint3. The Z component will be set to zero.<br/>
        /// 
        /// If the value is a scalar that can be converted to a 32-bit signed integer,
        /// the resulting uint3 will have this value in both of its components.<br/>
        /// 
        /// If the value is a boolean, (1, 1, 1) is returned for true, while 
        /// (0, 0, 0) is returned for false.<br/>
        /// 
        /// In all other cases, the user-defined default value is returned. 
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a uint3.</param>
        /// <returns>The property value as a uint3.</returns>
        public uint3 GetUInt3(uint3 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToUInt3(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a float3.
        /// </summary>
        /// <remarks>
        /// If the value is a 3-dimensional vector, its components will be converted to
        /// the closest representable single-precision floats, if possible.<br/>
        /// 
        /// If the value is a 4-dimensional vector, a float3 containing the first
        /// three components will be returned.<br/>
        /// 
        /// If the value is a 2-dimensional vector, it will become the XY-components of
        /// the float3. The Z-component will be set to zero.<br/>
        /// 
        /// If the value is a scalar that can be converted to a single-precision
        /// floating-point number, the resulting float3 will have this value in
        /// both of its components.<br/>
        /// 
        /// If the value is a boolean, (1.0f, 1.0f, 1.0f) is returned for true, while 
        /// (0.0f, 0.0f, 0.0f) is returned for false.<br/>
        /// 
        /// In all other cases, the user-defined default value is returned. 
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a float3.</param>
        /// <returns>The property value as a float3.</returns>
        public float3 GetFloat3(float3 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToFloat3(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a double3.
        /// </summary>
        /// <remarks>
        /// If the value is a 3-dimensional vector, its components will be converted 
        /// to double-precision floating-point numbers.<br/>
        /// 
        /// If the value is a 4-dimensional vector, a double3 containing the first
        /// three components will be returned.<br/>
        /// 
        /// If the value is a 2-dimensional vector, it will become the XY-components of
        /// the double3. The Z-component will be set to zero.<br/>
        /// 
        /// If the value is a scalar, the resulting double3 will have this value in
        /// both of its components.<br/>
        /// 
        /// If the value is a boolean, (1.0, 1.0, 1.0) is returned for true, while 
        /// (0.0, 0.0, 0.0) is returned for false.<br/>
        /// In all other cases, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a double3.</param>
        /// <returns>The property value as a double3.</returns>
        public double3 GetDouble3(double3 defaultValue)
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToDouble3(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a String.
        /// </summary>
        /// <remarks>
        /// <para>
        /// String values are returned as-is.<br/>
        /// Scalar values are converted to a string with `ToString()`.<br/>
        /// Boolean values are converted to "true" or "false".<br/>
        /// Vector values are returned as strings in the format (X, Y, Z, W)
        /// depending on how many components they have.<br/>
        /// Array values return the default value.<br/>
        /// </para>
        /// </remarks>
        /// <param name="defaultValue">The default value to use if the value 
        /// cannot be converted to a String.</param>
        /// <returns>The property value as a String.</returns>
        public String GetString(String defaultValue = "")
        {
            if (this.isEmpty || this.valueType.isArray)
            {
                return defaultValue;
            }

            return ConvertToString(this, defaultValue);
        }

        /// <summary>
        /// Attempts to retrieve the value as a <see cref="CesiumPropertyArray"/>. If the value
        /// is not an array type, this returns an empty array.
        /// </summary>
        /// <returns>The value as a CesiumPropertyArray.</returns>
        public CesiumPropertyArray GetArray()
        {
            if (valueType.isArray)
            {
                return this.valueImpl as CesiumPropertyArray;
            }

            return new CesiumPropertyArray();
        }

        /// <summary>
        /// Gets the given dictionary of CesiumMetadataValues as a new dictionary of strings, 
        /// mapped by name. This is useful for displaying the values from a property table or 
        /// property texture as strings in a user interface.
        /// </summary>
        /// <remarks>
        /// Array properties cannot be converted to strings, so empty strings will 
        /// be returned for their values.
        /// </remarks>
        /// <param name="values">The dictionary of CesiumMetadataValues mapped by name.</param>
        /// <returns>The dictionary of values converted to strings, mapped by name.</returns>
        public static Dictionary<String, String> GetValuesAsStrings(Dictionary<String, CesiumMetadataValue> values)
        {
            Dictionary<String, String> result = new Dictionary<String, String>(values.Count);
            foreach (KeyValuePair<String, CesiumMetadataValue> pair in values)
            {
                result.Add(pair.Key, pair.Value.GetString());
            }

            return result;
        }
        #endregion

        #region Private conversion methods
        //private static double2 ConvertVecNObjectToDouble2(System.Object value, double2 defaultValue)
        //{
        //    switch (value)
        //    {
        //        case CesiumInt2:
        //            CesiumInt2 Int2Value = (value as CesiumInt2?).Value;
        //            return new double2(
        //                Convert.ToDouble(Int2Value[0]), Convert.ToDouble(Int2Value[1]));
        //        case CesiumUInt2:
        //            CesiumUInt2 uInt2Value = (value as CesiumUInt2?).Value;
        //            return new double2(
        //                Convert.ToDouble(uInt2Value[0]), Convert.ToDouble(uInt2Value[1]));
        //        case float2:
        //            float2 float2Value = (value as float2?).Value;
        //            return new double2(float2Value);
        //        case double2:
        //            return (value as double2?).Value;
        //        case CesiumInt3:
        //            CesiumInt3 Int3Value = (value as CesiumInt3?).Value;
        //            return new double2(
        //                Convert.ToDouble(Int3Value[0]), Convert.ToDouble(Int3Value[1]));
        //        case CesiumUInt3:
        //            CesiumUInt3 uInt3Value = (value as CesiumUInt3?).Value;
        //            return new double2(
        //                Convert.ToDouble(uInt3Value[0]), Convert.ToDouble(uInt3Value[1]));
        //        case float3:
        //            float3 float3Value = (value as float3?).Value;
        //            return new double2(float3Value.x, float3Value.y);
        //        case double3:
        //            return (value as double3?).Value.xy;
        //        case CesiumInt4:
        //            CesiumInt4 Int4Value = (value as CesiumInt4?).Value;
        //            return new double2(
        //                Convert.ToDouble(Int4Value[0]), Convert.ToDouble(Int4Value[1]));
        //        case CesiumUInt4:
        //            CesiumUInt4 uInt4Value = (value as CesiumUInt4?).Value;
        //            return new double2(
        //                Convert.ToDouble(uInt4Value[0]), Convert.ToDouble(uInt4Value[1]));
        //        case float4:
        //            float4 float4Value = (value as float4?).Value;
        //            return new double2(float4Value.x, float4Value.y);
        //        case double4:
        //            return (value as double4?).Value.xy;
        //        default:
        //            return defaultValue;
        //    }
        //}
        #endregion

        #region Internal casts for Reinterop
        internal static bool? GetObjectAsBoolean(System.Object inObject)
        {
            return inObject as bool?;
        }

        internal static SByte? GetObjectAsSByte(System.Object inObject)
        {
            return inObject as SByte?;
        }

        internal static Byte? GetObjectAsByte(System.Object inObject)
        {
            return inObject as Byte?;
        }

        internal static Int16? GetObjectAsInt16(System.Object inObject)
        {
            return inObject as Int16?;
        }

        internal static UInt16? GetObjectAsUInt16(System.Object inObject)
        {
            return inObject as UInt16?;
        }

        internal static Int32? GetObjectAsInt32(System.Object inObject)
        {
            return inObject as Int32?;
        }

        internal static UInt32? GetObjectAsUInt32(System.Object inObject)
        {
            return inObject as UInt32?;
        }

        internal static Int64? GetObjectAsInt64(System.Object inObject)
        {
            return inObject as Int64?;
        }

        internal static UInt64? GetObjectAsUInt64(System.Object inObject)
        {
            return inObject as UInt64?;
        }

        internal static float? GetObjectAsFloat(System.Object inObject)
        {
            return inObject as float?;
        }

        internal static double? GetObjectAsDouble(System.Object inObject)
        {
            return inObject as double?;
        }

        internal static CesiumIntVec2? GetObjectAsCesiumIntVec2(System.Object inObject)
        {
            return inObject as CesiumIntVec2?;
        }

        internal static CesiumIntVec3? GetObjectAsCesiumIntVec3(System.Object inObject)
        {
            return inObject as CesiumIntVec3?;
        }

        internal static CesiumIntVec4? GetObjectAsCesiumIntVec4(System.Object inObject)
        {
            return inObject as CesiumIntVec4?;
        }

        internal static CesiumUIntVec2? GetObjectAsCesiumUIntVec2(System.Object inObject)
        {
            return inObject as CesiumUIntVec2?;
        }

        internal static CesiumUIntVec3? GetObjectAsCesiumUIntVec3(System.Object inObject)
        {
            return inObject as CesiumUIntVec3?;
        }

        internal static CesiumUIntVec4? GetObjectAsCesiumUIntVec4(System.Object inObject)
        {
            return inObject as CesiumUIntVec4?;
        }

        internal static float2? GetObjectAsFloat2(System.Object inObject)
        {
            return inObject as float2?;
        }

        internal static float3? GetObjectAsFloat3(System.Object inObject)
        {
            return inObject as float3?;
        }

        internal static float4? GetObjectAsFloat4(System.Object inObject)
        {
            return inObject as float4?;
        }

        internal static double2? GetObjectAsDouble2(System.Object inObject)
        {
            return inObject as double2?;
        }

        internal static double3? GetObjectAsDouble3(System.Object inObject)
        {
            return inObject as double3?;
        }

        internal static double4? GetObjectAsDouble4(System.Object inObject)
        {
            return inObject as double4?;
        }

        internal static float2x2? GetObjectAsFloat2x2(System.Object inObject)
        {
            return inObject as float2x2?;
        }

        internal static float3x3? GetObjectAsFloat3x3(System.Object inObject)
        {
            return inObject as float3x3?;
        }

        internal static float4x4? GetObjectAsFloat4x4(System.Object inObject)
        {
            return inObject as float4x4?;
        }

        internal static double2x2? GetObjectAsDouble2x2(System.Object inObject)
        {
            return inObject as double2x2?;
        }

        internal static double3x3? GetObjectAsDouble3x3(System.Object inObject)
        {
            return inObject as double3x3?;
        }

        internal static double4x4? GetObjectAsDouble4x4(System.Object inObject)
        {
            return inObject as double4x4?;
        }
        internal static String GetObjectAsString(System.Object inObject)
        {
            return inObject as String;
        }

        #endregion

        #region Internal static partial methods
        internal static partial bool ConvertToBoolean(CesiumMetadataValue value, bool defaultValue);
        internal static partial SByte ConvertToSByte(CesiumMetadataValue value, SByte defaultValue);
        internal static partial Byte ConvertToByte(CesiumMetadataValue value, Byte defaultValue);
        internal static partial Int16 ConvertToInt16(CesiumMetadataValue value, Int16 defaultValue);
        internal static partial UInt16 ConvertToUInt16(CesiumMetadataValue value, UInt16 defaultValue);
        internal static partial Int32 ConvertToInt32(CesiumMetadataValue value, Int32 defaultValue);
        internal static partial UInt32 ConvertToUInt32(CesiumMetadataValue value, UInt32 defaultValue);
        internal static partial Int64 ConvertToInt64(CesiumMetadataValue value, Int64 defaultValue);
        internal static partial UInt64 ConvertToUInt64(CesiumMetadataValue value, UInt64 defaultValue);
        internal static partial float ConvertToFloat(CesiumMetadataValue value, float defaultValue);
        internal static partial double ConvertToDouble(CesiumMetadataValue value, double defaultValue);
        internal static partial int2 ConvertToInt2(CesiumMetadataValue value, int2 defaultValue);
        internal static partial uint2 ConvertToUInt2(CesiumMetadataValue value, uint2 defaultValue);
        internal static partial float2 ConvertToFloat2(CesiumMetadataValue value, float2 defaultValue);
        internal static partial double2 ConvertToDouble2(CesiumMetadataValue value, double2 defaultValue);
        internal static partial int3 ConvertToInt3(CesiumMetadataValue value, int3 defaultValue);
        internal static partial uint3 ConvertToUInt3(CesiumMetadataValue value, uint3 defaultValue);
        internal static partial float3 ConvertToFloat3(CesiumMetadataValue value, float3 defaultValue);
        internal static partial double3 ConvertToDouble3(CesiumMetadataValue value, double3 defaultValue);
        internal static partial String ConvertToString(CesiumMetadataValue value, String defaultValue);

        #endregion
    }
}
