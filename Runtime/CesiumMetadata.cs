using Reinterop;
using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Provides access to the metadata attached to features in a <see cref="Cesium3DTileset"/>.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataImpl", "CesiumMetadataImpl.h")]
    [AddComponentMenu("")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    [Obsolete("Retrieve metadata using the CesiumModelMetadata component attached to a tile instead.")]
    public partial class CesiumMetadata : MonoBehaviour
    {
        private void OnEnable()
        {
            Debug.LogWarning("CesiumMetadata component is deprecated. Retrieve metadata using the CesiumModelMetadata component of a tile object.");
        }

        /// <summary>
        /// Gets the features corresponding to a particular triangle in a tile.
        /// </summary>
        /// <param name="transform">The <code>Transform</code> of the tile from which to obtain properties.</param>
        /// <param name="triangleIndex">The index of the tile's triangle for which to obtain properties.</param>
        /// <returns>An array of features associated with the triangle.</returns>
        /// <remarks>
        /// The information to pass to this function can be obtained using the <code>Physics.Raycast</code>
        /// function.
        /// </remarks>
        public partial CesiumFeature[] GetFeatures(Transform transform, int triangleIndex);
    }
}
