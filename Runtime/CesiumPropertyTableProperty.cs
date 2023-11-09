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
    /// A property has a specific type, such as int64 scalar or string, and values of 
    /// that type that can be accessed with primitive feature IDs from EXT_mesh_features.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumPropertyTablePropertyImpl", "CesiumPropertyTablePropertyImpl.h")]
    public partial class CesiumPropertyTableProperty
    {
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
        /// property has  an integer component type.
        /// </summary>
        public bool isNormalized { get; internal set; }

        public partial T GetAs<T>(T value);
    }
}
