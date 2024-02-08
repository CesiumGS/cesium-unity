using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_2022_2_OR_NEWER
using UnityEngine.Splines;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// A spline-based polygon used to rasterize 2D polygons on top of <see cref="Cesium3DTileset"/>s.
    /// </summary>
    [ExecuteInEditMode]
#if UNITY_2022_2_OR_NEWER
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(CesiumGlobeAnchor))]
#endif
    [AddComponentMenu("Cesium/Cesium Cartographic Polygon")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumCartographicPolygon : MonoBehaviour
    {
#if UNITY_2022_2_OR_NEWER
        private SplineContainer _splineContainer;
        private CesiumGlobeAnchor _globeAnchor;

        void OnEnable()
        {
            this._splineContainer = this.GetComponent<SplineContainer>();
            this._globeAnchor = this.GetComponent<CesiumGlobeAnchor>();
        }

#if UNITY_EDITOR
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
#endif

        static List<double2> emptyList = new List<double2>();

        internal List<double2> GetCartographicPoints(Matrix4x4 worldToTileset)
        {
#if UNITY_2022_2_OR_NEWER
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
