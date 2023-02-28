using Reinterop;
using UnityEngine;
using System;

namespace CesiumForUnity
{
    /// <summary>
    /// Identifies the type of a property. 
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
    /// Allows access to a particular feature of <see cref="CesiumMetadata"/>.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumFeatureImpl", "CesiumFeatureImpl.h")]
    public partial class CesiumFeature
    {
        /// <summary>
        /// The name of the class that this CesiumFeature conforms to.
        /// </summary>
        public string className { get; internal set; }

        /// <summary>
        /// The name of the feature table containing the values of this CesiumFeature's properties.
        /// </summary>
        public string featureTableName { get; internal set; }

        /// <summary>
        /// The names of the properties that exist in this CesiumFeature.
        /// </summary>
        public string[] properties { get; internal set; }

        /// <summary>
        /// Gets the value of the specified property as a signed byte. Returns the default 
        /// value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial sbyte GetInt8(string property, sbyte defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a byte. Returns the default value
        /// if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial byte GetUInt8(string property, byte defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a signed 16-bit integer. Returns
        /// the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Int16 GetInt16(string property, Int16 defaultValue);

        /// <summary>
        /// Gets the value of the specified property as an unsigned 16-bit integer. Returns
        /// the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt16 GetUInt16(string property, UInt16 defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a signed 32-bit integer. Returns
        /// the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Int32 GetInt32(string property, Int32 defaultValue);

        /// <summary>
        /// Gets the value of the specified property as an unsigned 32-bit integer.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt32 GetUInt32(string property, UInt32 defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a signed 64-bit integer. Returns
        /// the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Int64 GetInt64(string property, Int64 defaultValue);

        /// <summary>
        /// Gets the value of the specified property as an unsigned 64-bit integer. Returns
        /// the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt64 GetUInt64(string property, UInt64 defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a 32-bit floating-point number.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial float GetFloat32(string property, float defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a 64-bit floating-point number.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial double GetFloat64(string property, double defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a boolean value. Returns the default
        /// value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Boolean GetBoolean(string property, Boolean defaultValue);

        /// <summary>
        /// Gets the value of the specified property as a string. Returns the default value if
        /// the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial String GetString(string property, String defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as a signed byte. 
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial sbyte GetComponentInt8(string property, int index, sbyte defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as a byte.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial byte GetComponentUInt8(string property, int index, byte defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as a signed 16-bit integer.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Int16 GetComponentInt16(string property, int index, Int16 defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as an unsigned 16-bit integer.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt16 GetComponentUInt16(string property, int index, UInt16 defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as a signed 32-bit integer.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Int32 GetComponentInt32(string property, int index, Int32 defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as an unsigned 32-bit integer.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt32 GetComponentUInt32(string property, int index, UInt32 defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as a signed 64-bit integer.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Int64 GetComponentInt64(string property, int index, Int64 defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as an unsigned 64-bit integer.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial UInt64 GetComponentUInt64(string property, int index, UInt64 defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as a 32-bit floating-point number.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial float GetComponentFloat32(string property, int index, float defaultValue);

        /// <summary>
        /// Gets the value of the specified property property from an array as a 64-bit 
        /// floating-point number. Returns the default value if the property value cannot 
        /// be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial double GetComponentFloat64(string property, int index, double defaultValue);

        /// <summary>
        /// Gets the value of the specified property property from an array as a boolean value.
        /// Returns the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial Boolean GetComponentBoolean(string property, int index, Boolean defaultValue);

        /// <summary>
        /// Gets the value of the specified property from an array as a string. Returns 
        /// the default value if the property value cannot be converted to that type.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="index">The index of the component.</param>
        /// <param name="defaultValue">The default value.</param>
        public partial String GetComponentString(string property, int index, String defaultValue);

        /// <summary>
        /// Gets the number of components in the array.
        /// <param name="property">The name of the property.</param>
        /// </summary>
        public partial int GetComponentCount(string property);

        /// <summary>
        /// Gets the type of component array.
        /// <param name="property">The name of the property.</param>
        /// </summary>
        public partial MetadataType GetComponentType(string property);

        /// <summary>
        /// Gets the type of the specified property.
        /// <param name="property">The name of the property.</param>
        /// </summary>
        public partial MetadataType GetMetadataType(string property);

        /// <summary>
        /// Determines if the specified property's value is normalized.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        public partial bool IsNormalized(string property);
    }
}
