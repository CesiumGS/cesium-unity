using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// The abstract base class for <see cref="Cesium3DTileset"/> tile excluders. By creating a class derived
    /// from `CesiumTileExcluder`, then adding it to a game object containing a `Cesium3DTileset` (or one of
    /// its parents), you can implement custom rules for excluding tiles in the `Cesium3DTileset` from loading
    /// and rendering.
    /// </summary>
    [ExecuteInEditMode]
    public abstract partial class CesiumTileExcluder : MonoBehaviour
    {
        /// <summary>
        /// Determines whether the given tile should be excluded from loading and rendering. If a tile is
        /// excluded, all of its children and other descendants in the bounding volume hierarchy will be
        /// excluded as well.
        /// </summary>
        /// <param name="tile">The tile to check. This instance is only valid for the duration of this call. Saving
        /// it and using it later will result in undefined behavior, including crashes.</param>
        /// <returns>True if the tile should be excluded, false if the tile should be loaded and rendered.</returns>
        public abstract bool ShouldExclude(Cesium3DTile tile);
    }
}
