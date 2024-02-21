using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if SUPPORTS_SPLINES
using UnityEngine.Splines;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// A spline-based polygon used to rasterize 2D polygons on top of <see cref="Cesium3DTileset"/>s.
    /// Cartographic polygons are only supported for Unity 2022.2 or later.
    /// </summary>
    [ExecuteInEditMode]
#if SUPPORTS_SPLINES
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(CesiumGlobeAnchor))]
    [AddComponentMenu("Cesium/Cesium Cartographic Polygon")]
#else
    [AddComponentMenu("")]
#endif
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumCartographicPolygon : MonoBehaviour
    {
#if SUPPORTS_SPLINES
        private SplineContainer _splineContainer;
        private CesiumGlobeAnchor _globeAnchor;
#endif

        void OnEnable()
        {
#if SUPPORTS_SPLINES
            this._splineContainer = this.GetComponent<SplineContainer>();
            this._globeAnchor = this.GetComponent<CesiumGlobeAnchor>();

            // If this component is created before the Splines package is added, the
            // "RequireComponent" attributes won't automatically apply. This extra check
            // should ensure the required components exist.
            if (this._splineContainer == null)
            {
                this._splineContainer = this.gameObject.AddComponent<SplineContainer>();
#if UNITY_EDITOR
                this.Reset();
#endif
            }
            if (this._globeAnchor == null)
            {
                this._globeAnchor = this.gameObject.AddComponent<CesiumGlobeAnchor>();
            }

#elif UNITY_2022_2_OR_NEWER
            Debug.LogError("CesiumCartographicPolygon requires the Splines package, which is currently not installed " +
                "in the project. Install the Splines package using the Package Manager.");
#else
            Debug.LogError("CesiumCartographicPolygon requires the Splines package, which is not available " +
                "in this version of Unity.");
#endif
        }

#if SUPPORTS_SPLINES && UNITY_EDITOR
        void Reset()
        {
            IReadOnlyList<Spline> splines = this._splineContainer.Splines;
            for (int i = splines.Count - 1; i >= 0; i--)
            {
                this._splineContainer.RemoveSpline(splines[i]);
            }

            Spline defaultSpline = new Spline();

            BezierKnot[] knots = new BezierKnot[] {
                new BezierKnot(new float3(-100.0f, 0f, -100.0f)),
                new BezierKnot(new float3(100.0f, 0f, -100.0f)),
                new BezierKnot(new float3(100.0f, 0f, 100.0f)),
                new BezierKnot(new float3(-100.0f, 0f, 100.0f)),
            };

            defaultSpline.Knots = knots;
            defaultSpline.Closed = true;
            defaultSpline.SetTangentMode(TangentMode.Linear);

            this._splineContainer.AddSpline(defaultSpline);
        }
#endif

        static List<double2> emptyList = new List<double2>();

        internal List<double2> GetCartographicPoints(Matrix4x4 worldToTileset)
        {
#if SUPPORTS_SPLINES
            CesiumGeoreference georeference = this._globeAnchor.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
            {
                return emptyList;
            }

            IReadOnlyList<Spline> splines = this._splineContainer.Splines;
            if (splines.Count == 0)
            {
                return emptyList;
            }

            if (splines.Count > 1)
            {
                Debug.LogWarning("CesiumCartographicPolygon has multiple splines in its Spline component, " +
                    "but can only support one at a time. Only the first spline will be rasterized.");
            }

            Spline spline = splines[0];
            if (!spline.Closed)
            {
                Debug.LogError("Spline must be closed to be used as a cartographic polygon.");
                return emptyList;
            }

            BezierKnot[] knots = spline.ToArray();
            List<double2> cartographicPoints = new List<double2>(knots.Length);

            float4x4 localToWorld = this.transform.localToWorldMatrix;

            for (int i = 0; i < knots.Length; i++)
            {
                if (spline.GetTangentMode(i) != TangentMode.Linear)
                {
                    Debug.LogError("CesiumCartographicPolygon only supports linear splines.");
                    return emptyList;
                }

                BezierKnot knot = knots[i];

                // The spline points should be located in the tileset *exactly where they
                // appear to be*. The way we do that is by getting their world position, and
                // then transforming that world position to a Cesium3DTileset local position.
                // That way if the tileset is transformed relative to the globe, the polygon
                // will still affect the tileset where the user thinks it should.

                float3 worldPosition = knot.Transform(localToWorld).Position;
                float3 unityPosition = worldToTileset.MultiplyPoint3x4(worldPosition);
                double3 ecefPosition = georeference.TransformUnityPositionToEarthCenteredEarthFixed(unityPosition);
                double3 cartographicPosition = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecefPosition);

                cartographicPoints.Add(cartographicPosition.xy);
            }

            return cartographicPoints;
#else
            return emptyList;
#endif
        }
    }
}
