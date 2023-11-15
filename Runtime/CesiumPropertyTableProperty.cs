using Reinterop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Reports the status of a CesiumPropertyTableProperty. If the property
    /// table property cannot be accessed, this briefly indicates why.
    /// </summary>
    public enum CesiumPropertyTablePropertyStatus
    {
        /// <summary>
        /// The property table property is valid.
        /// </summary>
        Valid = 0,
        /// <summary>
        /// The property table property is empty but has a specified default value.
        /// </summary>
        EmptyPropertyWithDefault,
        /// <summary>
        /// The property table property does not exist in the glTF, or the property
        /// definition itself contains errors.
        /// </summary>
        ErrorInvalidProperty,
        /// <summary>
        /// The data associated with the property table property is malformed and
        /// cannot be retrieved.
        /// </summary>
        ErrorInvalidPropertyData
    }

    /// <summary>
    /// Represents a glTF property table property in EXT_structural_metadata.
    /// A property has a specific type, such as int64 scalar or string, and 
    /// values of that type that can be accessed with primitive feature IDs 
    /// from EXT_mesh_features.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumPropertyTablePropertyImpl", "CesiumPropertyTablePropertyImpl.h")]
    public partial class CesiumPropertyTableProperty
    {
        #region Fields
        private CesiumPropertyTablePropertyStatus _status =
            CesiumPropertyTablePropertyStatus.ErrorInvalidProperty;

        /// <summary>
        /// The status of the property table property. If this property table 
        /// property is invalid in any way, this will briefly indicate why.
        /// </summary>
        public CesiumPropertyTablePropertyStatus status
        {
            get => this._status;
            internal set => this._status = value;
        }

        /// <summary>
        /// The type of the metadata value as defined in the 
        /// EXT_structural_metadata extension. Some of these types are not 
        /// accessible from Unity, but can be converted to an accessible type.
        /// </summary>
        public CesiumMetadataValueType valueType { get; internal set; }

        /// <summary>
        /// The number of values in the property.
        /// </summary>
        public Int64 size { get; internal set; }

        /// <summary>
        /// The number of elements in an array of this property. Only 
        /// applicable when the property is a fixed-length array type.
        /// </summary>
        public Int64 arraySize { get; internal set; }

        /// <summary>
        /// Whether this property is normalized. Only applicable when this 
        /// property has an integer component type.
        /// </summary>
        public bool isNormalized { get; internal set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Attempts to retrieve the value for the given feature as a boolean.
        /// </summary>
        /// <remarks>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset 
        /// before it is further converted. If the raw value is equal to the 
        /// property's "no data" value, then the property's default value will
        /// be converted if possible. If the property-defined default value 
        /// cannot be converted, or does not exist, then the user-defined 
        /// default value is returned.
        /// 
        /// Property values are converted as follows:
        /// 
        /// - If the value is a boolean, it is returned as-is.
        /// 
        /// - If the value is a scalar, zero is converted to false, while any 
        /// other value is converted to true.
        /// 
        /// - If the value is a string, "0", "false", and "no" (case-insensitive)
        /// are converted to false, while "1", "true", and "yes" are converted to
        /// true. All other strings, including strings that can be converted to 
        /// numbers, will return the user-defined default value.
        /// 
        /// All other types return the user-defined default value. If the feature ID is
        /// out-of-range, or if the property table property is somehow invalid, the
        /// user-defined default value is returned.
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Boolean.</returns>
        public partial Boolean GetBoolean(Int64 featureID, bool defaultValue);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a signed
        /// 8-bit integer.
        /// </summary>
        /// <remarks>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// 
        /// Property values are converted as follows:
        /// 
        /// - If the value is an integer between -128 and 127, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between -128 and 127, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.
        /// 
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Boolean.</returns>
        public partial SByte GetSByte(Int64 featureID, SByte defaultValue);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an unsigned
        /// 8-bit integer.
        /// </summary>
        /// <remarks>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// 
        /// Property values are converted as follows:
        /// 
        /// - If the value is an integer between 0 and 255, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between 0 and 255, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.
        /// 
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Boolean.</returns>
        public partial Byte GetByte(Int64 featureID, Byte defaultValue);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a signed
        /// 16-bit integer.
        /// </summary>
        /// <remarks>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// 
        /// Property values are converted as follows:
        /// 
        /// - If the value is an integer between -32768 and 32767, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between -32768 and 32767, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.
        /// 
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Boolean.</returns>
        public partial Int16 GetInt16(Int64 featureID, Int16 defaultValue);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an unsigned
        /// 16-bit integer.
        /// </summary>
        /// <remarks>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// 
        /// Property values are converted as follows:
        /// 
        /// - If the value is an integer between 0 and 65535, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between 0 and 65535, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.
        /// 
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Boolean.</returns>
        public partial UInt16 GetUInt16(Int64 featureID, UInt16 defaultValue);
        #endregion
    }
}
