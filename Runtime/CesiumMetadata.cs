using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Provides access to the metadata attached to features in a <see cref="Cesium3DTileset"/>.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataImpl", "CesiumMetadataImpl.h")]
    public partial class CesiumMetadata : MonoBehaviour
    {
        internal partial void loadMetadata(Transform transform, int triangleIndex, MetadataProperty[] properties);

        private partial int getNumberOfProperties(Transform transform);

        /// <summary>
        /// Gets the properties corresponding to a particular triangle in a tile.
        /// </summary>
        /// <param name="transform">The <code>Transform</code> of the tile from which to obtain properties.</param>
        /// <param name="triangleIndex">The index of the tile's triangle for which to obtain properties.</param>
        /// <returns>An array of properties associated with the triangle.</returns>
        /// <remarks>
        /// The information to pass to this function can be obtained using the <code>Physics.Raycast</code>
        /// function.
        /// </remarks>
        public MetadataProperty[] GetProperties(Transform transform, int triangleIndex)
        {
            int numberOfProperties = getNumberOfProperties(transform);
            MetadataProperty[] properties = new MetadataProperty[numberOfProperties];
            for(int i = 0; i < numberOfProperties; i++){
                properties[i] = new MetadataProperty();
            }
            loadMetadata(transform, triangleIndex, properties);
            return properties;
        }
    }
}