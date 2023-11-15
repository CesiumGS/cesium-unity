using Reinterop;
using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// The type of a feature ID set.
    /// </summary>
    public enum CesiumFeatureIdSetType
    {
        None,
        Attribute,
        Texture,
        Implicit
    }

    /// <summary>
    /// Represents a feature ID set from a glTF primitive. A feature ID can be defined
    /// as a per-vertex attribute, as a feature ID texture, or implicitly via vertex ID.
    /// These can be used with the corresponding <see cref="CesiumPropertyTable"/> to 
    /// access per-feature metadata.
    /// </summary>
    public class CesiumFeatureIdSet
    {
        /// <summary>
        /// The type of this feature ID set.
        /// </summary>
        public CesiumFeatureIdSetType type
        {
            get; protected set;
        }

        /// <summary>
        /// The label assigned to this feature ID set. If no label was present in
        /// the glTF feature ID set, this returns an empty string.
        /// </summary>
        public String label
        {
            get; internal set;
        }

        /// <summary>
        /// The number of features this primitive has.
        /// </summary>
        public Int64 featureCount
        {
            get; internal set;
        }

        /// <summary>
        /// The null feature ID, i.e., the value that indicates no feature is
        /// associated with the owner.In other words, if a vertex or texel returns
        /// this value, then it is not associated with any feature.
        /// </summary>
        /// <remarks>
        /// If this value was not defined in the glTF feature ID set, this returns -1.
        /// </remarks>
        public Int64 nullFeatureId
        {
            get; internal set;
        }

        /// <summary>
        /// The index of the property table corresponding to this feature ID set.
        /// The index can be used to fetch the appropriate <see cref="CesiumPropertyTable"/>
        /// from the <see cref="CesiumModelMetadata"/>. 
        /// <remarks>
        /// If the feature ID set does not specify a property table, this returns -1.
        /// </remarks>
        /// </summary>
        public Int64 propertyTableIndex
        {
            get; internal set;
        }

        internal CesiumFeatureIdSet() : this(0)
        { }

        internal CesiumFeatureIdSet(Int64 featureCount)
        {
            this.type = featureCount > 0 ? CesiumFeatureIdSetType.Implicit : CesiumFeatureIdSetType.None;
            this.featureCount = featureCount;
            this.label = "";
            this.nullFeatureId = -1;
            this.propertyTableIndex = -1;
        }

        /// <summary>
        /// Gets the feature ID associated with a given vertex. The feature ID can be
        /// used with a CesiumPropertyTable to retrieve the corresponding metadata.
        /// </summary>
        /// <remarks>
        /// This returns -1 if the given vertex is out-of-bounds, or if the feature ID
        /// set is invalid.
        /// </remarks>
        /// <param name="vertexIndex">The index of the target vertex.</param>
        /// <returns>The feature ID associated with the given vertex, or -1 for an invalid
        /// feature ID set or invalid input.</returns>
        public virtual Int64 GetFeatureIDForVertex(Int64 vertexIndex)
        {
            if (this.type == CesiumFeatureIdSetType.Implicit)
            {
                return vertexIndex;
            }

            // Other methods should be handled by CesiumFeatureIdAttribute and CesiumFeatureIdTexture respectively.
            return -1;
        }
    }
}
