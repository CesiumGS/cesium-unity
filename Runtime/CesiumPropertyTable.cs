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
    /// It knows how to look up the metadata values associated with a given feature ID.
    /// </summary>
    public class CesiumPropertyTable
    {
        /// <summary>
        /// The status of the property table. If an error occurred while parsing 
        /// the property table from the glTF extension, this briefly conveys why.
        /// </summary>
        public CesiumPropertyTableStatus status
        {
            get; internal set;
        }

        /// <summary>
        /// The name of the property table. If no name was specified in the glTF
        /// extension, this is an empty string.
        /// </summary>
        public string name
        {
            get; internal set;
        }

        /// <summary>
        /// The number of values each property in the table is expected to have.
        /// If an error occurred while parsing the property table, this returns zero.
        /// </summary>
        public Int64 count
        {
            get; internal set;
        }

        /// <summary>
        /// The properties of the property table, mapped by property id.
        /// </summary>
        public Dictionary<String, CesiumPropertyTableProperty> properties
        {
            get; internal set;
        }

        internal CesiumPropertyTable()
        {
            this.status = CesiumPropertyTableStatus.ErrorInvalidPropertyTable;
            this.count = 0;
        }

        public Dictionary<String, String> GetMetadataValuesForFeatureAsStrings(Int64 featureId)
        {
            Dictionary<String, String> result = new Dictionary<String, String>();
            foreach (KeyValuePair<String, CesiumPropertyTableProperty> property in this.properties)
            {
                result.Add(property.Key, property.Value.GetString(featureId));
            }
            return result;
        }
    }
}
