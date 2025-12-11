using Reinterop;
using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Reports the status of a CesiumFeatureIdTexture. If the feature ID
    /// texture cannot be accessed, this briefly indicates why.
    /// </summary>
    public enum CesiumFeatureIdTextureStatus
    {
        /// <summary>
        /// The feature ID texture is valid.
        /// </summary>
        Valid,
        /// <summary>
        /// The feature ID texture cannot be found in the glTF, or the texture
        /// itself has errors.
        /// </summary>
        ErrorInvalidTexture,
        /// <summary>
        /// The feature ID texture is being read in an invalid way -- for 
        /// example, trying to read nonexistent image channels.
        /// </summary>
        ErrorInvalidTextureAccess
    }

    /// <summary>
    /// Represents a feature ID texture from a glTF primitive. Provides 
    /// access to per-texel feature IDs which can be used with the 
    /// corresponding <see cref="CesiumPropertyTable"/> to access per-texel
    /// metadata.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumFeatureIdTextureImpl", "CesiumFeatureIdTextureImpl.h")]
    public partial class CesiumFeatureIdTexture : CesiumFeatureIdSet
    {
        /// <summary>
        /// The status of this feature ID texture.
        /// </summary>
        public CesiumFeatureIdTextureStatus status
        {
            get; internal set;
        }

        internal CesiumFeatureIdTexture()
        {
            this.type = CesiumFeatureIdSetType.Texture;
            this.status = CesiumFeatureIdTextureStatus.ErrorInvalidTexture;
            this.CreateImplementation();
        }

        /// <summary>
        /// Gets the feature ID corresponding to the pixel specified by the UV texture
        /// coordinates. The feature ID can be used with a <see cref="CesiumPropertyTable"/>
        /// to retrieve the per-texel metadata.
        /// </summary>
        /// <param name="uv">The texture coordinates.</param>
        /// <returns>The feature ID at the given texture coordinates, or -1 if the feature ID texture is invalid.</returns>
        public partial Int64 GetFeatureIdForUV(Vector2 uv);

        /// <inheritdoc />
        public override partial Int64 GetFeatureIdForVertex(Int64 vertexIndex);

        /// <inheritdoc />
        public override partial Int64 GetFeatureIdFromRaycastHit(RaycastHit hitInfo);
    }
}
