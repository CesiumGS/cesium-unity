using UnityEngine;
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
    //[ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataValueImpl", "CesiumMetadataValueImpl.h", staticOnly: true)]
    public partial class CesiumMetadataValue
    {
        internal System.Object value { get; set; }

        /// <summary>
        /// The type of the metadata value as defined in the 
        /// EXT_structural_metadata extension. Some of these types are not 
        /// accessible from Unity, but can be converted to a close-fitting type.
        /// </summary>
        public CesiumMetadataValueType valueType
        {
            get
            {
                return CesiumMetadataValueType.GetValueType(this.value);
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
            get { return this.value == null; }
        }

        public CesiumMetadataValue() : this(null)
        { }

        public CesiumMetadataValue(System.Object value)
        {
            this.value = value;
        }

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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                    return (this.value as Boolean?).Value;
                case CesiumMetadataType.Scalar:
                    return Convert.ToBoolean(this.value);
                case CesiumMetadataType.String:
                    String str = this.value as String;
                    if (
                        str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        str.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                        str.Equals("1", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (
                        str.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                        str.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                        str.Equals("0", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    return defaultValue;
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToSByte(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToSByte(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range-number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToByte(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToByte(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range-number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToInt16(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToInt16(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range-number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToUInt16(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToUInt16(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range-number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToInt32(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToInt32(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range-number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToUInt32(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToUInt32(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range-number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToInt64(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToInt64(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToUInt64(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                case CesiumMetadataType.Scalar:
                    // We need to explicitly truncate floating-point values. Otherwise,
                    // Convert will round to the nearest number.
                    System.Object value = this.value;
                    switch (valueType.componentType)
                    {
                        case CesiumMetadataComponentType.Float32:
                            value = Math.Truncate((value as float?).Value);
                            break;
                        case CesiumMetadataComponentType.Float64:
                            value = Math.Truncate((value as double?).Value);
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        return Convert.ToUInt64(value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an out-of-range-number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Scalar:
                    if (valueType.componentType == CesiumMetadataComponentType.Float64)
                    {
                        double value = (this.value as double?).Value;
                        if (value < Single.MinValue || value > Single.MaxValue)
                        {
                            return defaultValue;
                        }
                    }
                    // [[fallthrough]]
                    goto case CesiumMetadataType.String;
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                    try
                    {
                        return Convert.ToSingle(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string
                        // or an out-of-range number.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            if (this.isEmpty)
            {
                return defaultValue;
            }

            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            switch (valueType.type)
            {
                case CesiumMetadataType.Boolean:
                case CesiumMetadataType.String:
                case CesiumMetadataType.Scalar:
                    try
                    {
                        return Convert.ToDouble(this.value);
                    }
                    catch
                    {
                        // The above may throw if trying to convert an invalid string.
                        return defaultValue;
                    }
                default:
                    return defaultValue;
            }
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
            CesiumMetadataValueType valueType = this.valueType;
            if (valueType.isArray)
            {
                return defaultValue;
            }

            if (valueType.type == CesiumMetadataType.String)
            {
                return value as String;
            }

            return defaultValue;
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
                return this.value as CesiumPropertyArray;
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
    }
}
