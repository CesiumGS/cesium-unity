
using Reinterop;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::Cesium3DTileImpl", "Cesium3DTileImpl.h", staticOnly: true)]
    public partial class Cesium3DTile
    {
        internal CesiumGeoreference _georeference;
        internal IntPtr _pTile;

        /// <summary>
        /// Gets the axis-aligned bounding box of this tile. If this tile came from a <see cref="CesiumTileExcluder"/>,
        /// the bounding box is expressed in the local coordinates of the excluder's game object.
        /// </summary>
        public Bounds bounds
        {
            get => Cesium3DTile.getBounds(this._pTile, this._georeference.ecefToLocalMatrix);
        }

        private static partial Bounds getBounds(IntPtr pTile, double4x4 ecefToLocalMatrix);
    }
}
