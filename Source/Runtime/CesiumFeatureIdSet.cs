using System;
using System.Collections.Generic;
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
    public class CesiumFeatureIdSet : IDisposable
    {
        #region Getters

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

        #endregion

        #region Constructors

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

        #endregion

        #region Public methods
        /// <summary>
        /// Gets the feature ID associated with a given vertex. The feature ID can be
        /// used with a <see cref="CesiumPropertyTable"/> to retrieve the corresponding
        /// metadata.
        /// </summary>
        /// <remarks>
        /// This returns -1 if the given vertex is out-of-bounds, or if the feature ID
        /// set is invalid.
        /// </remarks>
        /// <param name="vertexIndex">The index of the target vertex.</param>
        /// <returns>The feature ID associated with the given vertex, or -1 for an
        /// invalid feature ID set or invalid input.</returns>
        public virtual Int64 GetFeatureIdForVertex(Int64 vertexIndex)
        {
            if (this.type != CesiumFeatureIdSetType.Implicit)
            {
                // Other methods should be handled by CesiumFeatureIdAttribute and
                // CesiumFeatureIdTexture respectively.
                return -1;
            }

            if (vertexIndex < 0 || vertexIndex >= this.featureCount)
            {
                return -1;
            }

            return vertexIndex;
        }

        /// <summary>
        /// Gets the feature ID from the feature ID set using the given raycast hit.
        /// This returns a more accurate value for feature ID textures, since they 
        /// define feature IDs per-texel instead of per-vertex. The feature ID can
        /// be used with a <see cref="CesiumPropertyTable"/> to retrieve the 
        /// corresponding metadata.
        /// </summary>
        /// <remarks>
        /// This can still retrieve the feature IDs for non-texture feature ID sets.
        /// For attribute or implicit feature IDs, the first feature ID associated
        /// with the first vertex of the intersected face is returned.<br/><br/>
        /// 
        /// This returns -1 if the feature ID set is invalid.
        /// </remarks>
        /// <param name="hitInfo">The raycast hit info.</param>
        /// <returns>The feature ID associated with the given vertex, or -1 for an
        /// invalid feature ID set.</returns>
        public virtual Int64 GetFeatureIdFromRaycastHit(RaycastHit hitInfo)
        {
            if (this.type != CesiumFeatureIdSetType.Implicit)
            {
                // Other methods should be handled by CesiumFeatureIdAttribute and
                // CesiumFeatureIdTexture respectively.
                return -1;
            }


            return CesiumPrimitiveFeatures.GetFirstVertexFromHitTriangle(hitInfo);
        }

        #endregion

        // Cache the list of indices to prevent the allocation of a new array every time
        // GetFirstVertexFromHitTriangle is called.
        List<int> _indices;

        public virtual void Dispose() { }
    }
}
