using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Provides access to the metadata attached to features in a <see cref="Cesium3DTileset"/>.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataImpl", "CesiumMetadataImpl.h")]
    [AddComponentMenu("Cesium/Cesium Metadata")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumMetadata : MonoBehaviour
    {
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
