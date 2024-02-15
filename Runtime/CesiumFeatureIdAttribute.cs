using Reinterop;
using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Reports the status of a CesiumFeatureIdAttribute. If the feature ID
    /// attribute cannot be accessed, this briefly indicates why.
    /// </summary>
    public enum CesiumFeatureIdAttributeStatus
    {
        /// <summary>
        /// The feature ID attribute is valid.
        /// </summary>
        Valid,
        /// <summary>
        /// The feature ID attribute does not exist in the glTF primitive.
        /// </summary>
        ErrorInvalidAttribute,
        /// <summary>
        /// The feature ID attribute uses an invalid accessor in the glTF.
        /// </summary>
        ErrorInvalidAccessor
    }

    /// <summary>
    /// Represents a feature ID attribute from a glTF primitive. Provides 
    /// access to per-vertex feature IDs which can be used with the 
    /// corresponding <see cref="CesiumPropertyTable"/> to access per-vertex
    /// metadata.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumFeatureIdAttributeImpl", "CesiumFeatureIdAttributeImpl.h")]
    public partial class CesiumFeatureIdAttribute : CesiumFeatureIdSet
    {
        /// <summary>
        /// The status of this feature ID attribute.
        /// </summary>
        public CesiumFeatureIdAttributeStatus status
        {
            get; internal set;
        }

        internal CesiumFeatureIdAttribute()
        {
            this.type = CesiumFeatureIdSetType.Attribute;
            this.status = CesiumFeatureIdAttributeStatus.ErrorInvalidAttribute;
            this.CreateImplementation();
        }

        /// <inheritdoc />
        public override partial Int64 GetFeatureIdForVertex(Int64 vertexIndex);

        /// <inheritdoc />
        public override Int64 GetFeatureIdFromRaycastHit(RaycastHit hitInfo)
        {
            int vertex = CesiumPrimitiveFeatures.GetFirstVertexFromHitTriangle(hitInfo);
            return this.GetFeatureIdForVertex(vertex);
        }
    }
}
