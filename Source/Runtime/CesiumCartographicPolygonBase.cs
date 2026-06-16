using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Abstract base for components that define a cartographic polygon used by
    /// <see cref="CesiumPolygonRasterOverlay"/>.
    /// </summary>
    [ExecuteInEditMode]
    public abstract partial class CesiumCartographicPolygonBase : MonoBehaviour
    {
        internal static readonly List<double2> emptyList = new List<double2>();

        /// <summary>
        /// Returns the polygon vertices as longitude/latitude pairs (in degrees).
        /// </summary>
        internal abstract List<double2> GetCartographicPoints(Matrix4x4 worldToTileset);
    }
}
