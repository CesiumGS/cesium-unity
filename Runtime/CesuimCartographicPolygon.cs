using Reinterop;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace CesiumForUnity
{
    /// <summary>
    /// A spline-based polygon used to rasterize 2D polygons on top of <see cref="Cesium3DTileset"/>s.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(CesiumGlobeAnchor))]
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumCartographicPolygonImpl", "CesiumCartographicPolygonImpl.h")]
    [AddComponentMenu("Cesium/Cesium Cartographic Polygon")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumCartographicPolygon : MonoBehaviour
    {
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

        internal void ApplySplineChanges()
        {
            CesiumGeoreference georeference = this._globeAnchor.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
            {
                return;
            }

            // TODO: "Apply Spline Changes" button
            IReadOnlyList<Spline> splines = this._splineContainer.Splines;
            if (splines.Count == 0)
            {
                return;
            }

            Spline spline = splines[0];
            BezierKnot[] knots = spline.ToArray();
            List<double2> cartographicPoints = new List<double2>(knots.Length);

            for (int i = 0; i < knots.Length; i++)
            {
                BezierKnot knot = knots[i];

                float3 unityPosition = this.transform.TransformPoint(knot.Position);
                double3 ecefPosition = georeference.TransformUnityPositionToEarthCenteredEarthFixed(unityPosition);
                double3 cartographicPosition = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecefPosition);

                cartographicPoints[i] = cartographicPosition.xz;
            }

            this.UpdatePolygon(cartographicPoints.ToArray());
        }

        private partial void UpdatePolygon(double2[] cartographicPoints);
    }
}
