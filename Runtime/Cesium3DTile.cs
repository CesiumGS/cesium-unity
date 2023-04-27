
using Reinterop;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents a tile in a <see cref="Cesium3DTileset"/> and allows information
    /// about the tile to be queried from the underlying C++ tile representation.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::Cesium3DTileImpl", "Cesium3DTileImpl.h", staticOnly: true)]
    public partial class Cesium3DTile
    {
        internal double4x4 _transform;
        internal IntPtr _pTile;

        internal Cesium3DTile()
        {}

        /// <summary>
        /// Gets the axis-aligned bounding box of this tile. If this tile came from a <see cref="CesiumTileExcluder"/>,
        /// the bounding box is expressed in the local coordinates of the excluder's game object.
        /// </summary>
        public Bounds bounds
        {
            get
            {
                return Cesium3DTile.getBounds(this._pTile, this._transform);
            }
        }

        private static partial Bounds getBounds(IntPtr pTile, double4x4 ecefToLocalMatrix);
    }
}
