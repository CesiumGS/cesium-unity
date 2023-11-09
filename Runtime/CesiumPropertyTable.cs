using Reinterop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    ///  Reports the status of a CesiumPropertyTable. If the property table 
    ///  cannot be accessed, this briefly indicates why.
    /// </summary>
    public enum CesiumPropertyTableStatus
    {
        /// <summary>
        /// The property table is valid.
        /// </summary>
        Valid = 0,
        /// <summary>
        /// The property table instance was not initialized from an actual glTF
        /// property table.
        /// </summary>
        ErrorInvalidPropertyTable,
        /// <summary>
        /// The property table's class could be found in the schema of the metadata
        /// extension.
        /// </summary>
        ErrorInvalidPropertyTableClass
    }

    /// <summary>
    /// Represents a glTF property table in the EXT_structural_metadata extension.
    /// A property table is a collection of properties for the features in a mesh.
    /// It knows how to look up the metadata values associated with a given 
    /// feature ID.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumPropertyTableImpl", "CesiumPropertyTableImpl.h")]
    public partial class CesiumPropertyTable
    {
        private CesiumPropertyTableStatus _status =
            CesiumPropertyTableStatus.ErrorInvalidPropertyTable;

        /// <summary>
        /// The status of the property table. If an error occurred while parsing 
        /// the property table from the glTF extension, this briefly conveys why.
        /// </summary>
        public CesiumPropertyTableStatus status
        {
            get => this._status;
            internal set => this._status = value;
        }

        /// <summary>
        /// The name of the property table. If no name was specified in the glTF
        /// extension, this is an empty string.
        /// </summary>
        public string name { get; internal set; }

        /// <summary>
        /// The number of values each property in the table is expected to have.
        /// If an error occurred while parsing the property table, this returns zero.
        /// </summary>
        public Int64 count { get; internal set; }

        /// <summary>
        /// The properties of the property table, mapped by property name.
        /// </summary>
        public Dictionary<string, CesiumPropertyTableProperty> properties { get; internal set; }

        // public Dictionary<string, ____> GetMetadataValuesForFeature() { }
    }
}
