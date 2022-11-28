using System;
using Reinterop;

namespace CesiumForUnity
{
    /// <summary>
    /// Identifies the type of a <see cref="MetadataProperty"/>.
    /// </summary>
    public enum MetadataType
    {
        None,
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,
        Boolean,
        String,
        Array
    }

    /// <summary>
    /// Allows access to a particular property on a particular feature of <see cref="CesiumMetadata"/>.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::MetadataPropertyImpl", "MetadataPropertyImpl.h")]
    public partial class MetadataProperty
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <returns></returns>
        public partial string GetPropertyName();

        /// <summary>
        /// Determines if the property value is normalized.
        /// </summary>
        public partial bool IsNormalized();

        /// <summary>
        /// Gets the number of components if the property is an array. Otherwise, returns 1.
        /// </summary>
        /// <returns></returns>
        public partial int GetComponentCount();

        internal partial void GetComponent(MetadataProperty property, int index);

        /// <summary>
        /// Gets the type of each component if this property is an array.
        /// </summary>
        public partial MetadataType GetComponentType();

        /// <summary>
        /// Gets the type of this property.
        /// </summary>
        public partial MetadataType GetMetadataType();

        /// <summary>
        /// Gets the value of this property as a signed byte, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial sbyte GetInt8(sbyte defaultValue);

        /// <summary>
        /// Gets the value of this property as a byte, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial byte GetUInt8(byte defaultValue);

        /// <summary>
        /// Gets the value of this property as a signed, 16-bit integer, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial Int16 GetInt16(Int16 defaultValue);

        /// <summary>
        /// Gets the value of this property as an unsigned, 16-bit integer, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt16 GetUInt16(UInt16 defaultValue);

        /// <summary>
        /// Gets the value of this property as a signed, 32-bit integer, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial Int32 GetInt32(Int32 defaultValue);

        /// <summary>
        /// Gets the value of this property as an unsigned, 32-bit integer, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt32 GetUInt32(UInt32 defaultValue);

        /// <summary>
        /// Gets the value of this property as a signed, 64-bit integer, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial Int64 GetInt64(Int64 defaultValue);

        /// <summary>
        /// Gets the value of this property as an unsigned, 64-bit integer, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt64 GetUInt64(UInt64 defaultValue);

        /// <summary>
        /// Gets the value of this property as a 32-bit floating-point number, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial float GetFloat32(float defaultValue);

        /// <summary>
        /// Gets the value of this property as a 64-bit floating-point number, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial double GetFloat64(double defaultValue);

        /// <summary>
        /// Gets the value of this property as a boolean value, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial Boolean GetBoolean(Boolean defaultValue);

        /// <summary>
        /// Gets the value of this property as a string, or a default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public partial String GetString(String defaultValue);
    }
}
