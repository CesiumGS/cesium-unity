using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if SUPPORTS_SPLINES
using UnityEngine.Splines;
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// Configures where a <see cref="CesiumCartographicPolygon"/> gets the vertices
    /// that define its shape.
    /// </summary>
    public enum CesiumCartographicPolygonSource
    {
        /// <summary>
        /// The polygon's shape is defined manually by editing the spline directly
        /// in the Editor (or at runtime).
        /// </summary>
        Manual = 0,

        /// <summary>
        /// The polygon's shape is loaded from a GeoJSON document hosted on Cesium ion.
        /// </summary>
        FromDocument = 1,

        /// <summary>
        /// The polygon's shape is loaded from a GeoJSON document that has been parsed
        /// and assigned in code using <see cref="CesiumCartographicPolygon.document"/>.
        /// </summary>
        FromCesiumIon = 2,

        /// <summary>
        /// The polygon's shape is loaded from a GeoJSON document hosted at a URL.
        /// </summary>
        FromUrl = 3
    }

    /// <summary>
    /// A spline-based polygon used to rasterize 2D polygons on top of <see cref="Cesium3DTileset"/>s.
    /// Cartographic polygons are only supported for Unity 2022.2 or later.
    /// </summary>
    /// <remarks>
    /// The polygon's shape is defined by a spline that is georeferenced using a
    /// <see cref="CesiumGlobeAnchor"/>. The spline can either be edited manually, or its
    /// knots can be generated automatically from a GeoJSON document loaded from Cesium ion,
    /// a URL, or a document that has been parsed and assigned in code.
    /// </remarks>
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
        internal static readonly List<double2> emptyList = new List<double2>();

#if SUPPORTS_SPLINES
        private SplineContainer _splineContainer;
        private CesiumGlobeAnchor _globeAnchor;
#endif

        [SerializeField]
        private CesiumCartographicPolygonSource _source = CesiumCartographicPolygonSource.Manual;

        /// <summary>
        /// The source from which this polygon's shape is derived.
        /// </summary>
        /// <remarks>
        /// Setting this to <see cref="CesiumCartographicPolygonSource.FromCesiumIon"/>,
        /// <see cref="CesiumCartographicPolygonSource.FromDocument"/>, or
        /// <see cref="CesiumCartographicPolygonSource.FromUrl"/> will reload the polygon's
        /// spline from the configured GeoJSON source. Setting it to
        /// <see cref="CesiumCartographicPolygonSource.Manual"/> leaves the current spline
        /// as-is, allowing it to be edited by hand.
        /// </remarks>
        public CesiumCartographicPolygonSource source
        {
            get => this._source;
            set
            {
                this._source = value;
                this.LoadFromSource();
            }
        }

        [SerializeField]
        private string _url = "";

        /// <summary>
        /// The URL from which to load the GeoJSON document.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumCartographicPolygonSource.FromUrl"/>.
        /// </remarks>
        public string url
        {
            get => this._url;
            set
            {
                this._url = value;
                if (this._source == CesiumCartographicPolygonSource.FromUrl)
                {
                    this.LoadFromSource();
                }
            }
        }

        [SerializeField]
        private long _ionAssetID = 0;

        /// <summary>
        /// The ID of the Cesium ion asset to use.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumCartographicPolygonSource.FromCesiumIon"/>.
        /// </remarks>
        public long ionAssetID
        {
            get => this._ionAssetID;
            set
            {
                this._ionAssetID = value;
                if (this._source == CesiumCartographicPolygonSource.FromCesiumIon)
                {
                    this.LoadFromSource();
                }
            }
        }

        [SerializeField]
        private string _ionAccessToken = "";

        /// <summary>
        /// The access token to use to access the Cesium ion resource.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumCartographicPolygonSource.FromCesiumIon"/>.
        /// If empty, the default token from the ion server will be used.
        /// </remarks>
        public string ionAccessToken
        {
            get => this._ionAccessToken;
            set
            {
                this._ionAccessToken = value;
                if (this._source == CesiumCartographicPolygonSource.FromCesiumIon)
                {
                    this.LoadFromSource();
                }
            }
        }

        [SerializeField]
        private CesiumIonServer _ionServer = null;

        /// <summary>
        /// The Cesium ion server from which this polygon's GeoJSON is loaded.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumCartographicPolygonSource.FromCesiumIon"/>.
        /// </remarks>
        public CesiumIonServer ionServer
        {
            get
            {
                if (this._ionServer == null)
                    this._ionServer = CesiumIonServer.serverForNewObjects;
                return this._ionServer;
            }
            set
            {
                if (value == null) value = CesiumIonServer.serverForNewObjects;
                this._ionServer = value;
                if (this._source == CesiumCartographicPolygonSource.FromCesiumIon)
                {
                    this.LoadFromSource();
                }
            }
        }

        private CesiumGeoJsonDocument _document = null;

        /// <summary>
        /// Gets or sets the GeoJSON document used by this polygon.
        /// </summary>
        /// <remarks>
        /// Setting this property automatically changes <see cref="source"/> to
        /// <see cref="CesiumCartographicPolygonSource.FromDocument"/>.
        /// </remarks>
        public CesiumGeoJsonDocument document
        {
            get => this._document;
            set
            {
                this._document = value;
                this._source = CesiumCartographicPolygonSource.FromDocument;
                this.ApplyDocument(value);
            }
        }

        // Set to true while this component is writing to the spline itself, so that
        // the resulting Spline.Changed event isn't misinterpreted as a manual edit.
        // Exposed as internal so the editor can skip its own spline-modified handling
        // while a reload from the configured source is in progress.
        internal bool _isUpdatingSplineInternally = false;

#if SUPPORTS_SPLINES
        // Exposed for the editor so it can verify a modified spline belongs to this
        // polygon before reacting to the global AfterSplineWasModified callback.
        internal bool IsSplineOwned(Spline spline)
        {
            if (this._splineContainer == null)
            {
                return false;
            }

            IReadOnlyList<Spline> splines = this._splineContainer.Splines;
            for (int i = 0; i < splines.Count; i++)
            {
                if (splines[i] == spline)
                {
                    return true;
                }
            }

            return false;
        }
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

            if (this._source != CesiumCartographicPolygonSource.Manual)
            {
                this.LoadFromSource();
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

        /// <summary>
        /// Reloads the polygon's spline from the configured <see cref="source"/>
        /// and rebuilds any <see cref="CesiumPolygonRasterOverlay"/> that references
        /// this polygon, so the cutout in the tileset matches the latest data.
        /// </summary>
        /// <remarks>
        /// This has no effect on the spline when <see cref="source"/> is set to
        /// <see cref="CesiumCartographicPolygonSource.Manual"/>, but dependent
        /// raster overlays are still rebuilt so that manual edits to the spline
        /// are reflected in the cutout.
        /// </remarks>
        public void Refresh()
        {
            LoadFromSource();

            CesiumPolygonRasterOverlay[] overlays = FindObjectsByType<CesiumPolygonRasterOverlay>(FindObjectsSortMode.None);
            for (int i = 0; i < overlays.Length; i++)
            {
                CesiumPolygonRasterOverlay overlay = overlays[i];
                if (overlay == null || overlay.polygons == null)
                    continue;
                if (overlay.polygons.Contains(this))
                {
                    overlay.Refresh();
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Writes this polygon's current spline to a GeoJSON file at the given path.
        /// </summary>
        /// <param name="filePath">Absolute or relative path to the output .geojson file.</param>
        /// <returns>True on success, false on any validation or I/O failure.</returns>
        public bool SaveAsGeoJson(string filePath)
        {
            return CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJson(this, filePath);
        }
#endif

        private async void LoadFromSource()
        {
#if SUPPORTS_SPLINES
            CesiumGeoJsonDocument loaded = null;
            switch (this._source)
            {
                case CesiumCartographicPolygonSource.Manual:
                    return;
                case CesiumCartographicPolygonSource.FromDocument:
                    loaded = this._document;
                    break;
                case CesiumCartographicPolygonSource.FromUrl:
                    if (!string.IsNullOrEmpty(this._url))
                        loaded = await CesiumGeoJsonDocument.LoadFromUrlAsync(this._url);
                    break;
                case CesiumCartographicPolygonSource.FromCesiumIon:
                    if (this._ionAssetID > 0)
                        loaded = await CesiumGeoJsonDocument.LoadFromCesiumIonAsync(
                            this._ionAssetID, this._ionAccessToken, this.ionServer);
                    break;
            }

            if (loaded == null && this._source != CesiumCartographicPolygonSource.Manual)
            {
                Debug.LogWarning("CesiumCartographicPolygon: failed to load the GeoJSON. The spline was left unchanged.");
            }

            this.ApplyDocument(loaded);
#endif
        }

#if SUPPORTS_SPLINES
        private static List<double2> GetPolygonRingPoints(CesiumGeoJsonDocument geoJsonDocument)
        {
            if (geoJsonDocument == null)
                return null;

            CesiumGeoJsonObject obj = geoJsonDocument.GetRootObject();
            if (obj == null)
                return null;

            CesiumGeoJsonObjectType objType = obj.GetObjectType();

            CesiumGeoJsonObject geometry = obj;
            if (objType == CesiumGeoJsonObjectType.Feature)
            {
                CesiumGeoJsonFeature feature = obj.GetObjectAsFeature();
                geometry = feature?.GetGeometry();
            }
            else if (objType == CesiumGeoJsonObjectType.FeatureCollection)
            {
                CesiumGeoJsonFeature[] features = obj.GetObjectAsFeatureCollection();
                if (features == null || features.Length == 0)
                    return null;
                geometry = features[0].GetGeometry();
            }

            if (geometry == null)
                return null;

            CesiumGeoJsonLineString[] rings = null;

            CesiumGeoJsonObjectType geometryType = geometry.GetObjectType();
            if (geometryType == CesiumGeoJsonObjectType.Polygon)
            {
                rings = geometry.GetObjectAsPolygon()?.rings;
            }
            else if (geometryType == CesiumGeoJsonObjectType.MultiPolygon)
            {
                CesiumGeoJsonPolygon[] polys = geometry.GetObjectAsMultiPolygon();
                rings = polys?[0]?.rings;
            }

            if (rings == null || rings.Length == 0 || rings[0] == null || rings[0].points.Length < 3)
                return null;

            List<double2> result = new List<double2>(rings[0].points.Length);
            foreach (double3 p in rings[0].points)
                result.Add(new double2(p.x, p.y));
            return result;
        }

        private void ApplyDocument(CesiumGeoJsonDocument geoJsonDocument)
        {
            List<double2> points = GetPolygonRingPoints(geoJsonDocument);
            if (points == null || points.Count < 3)
            {
                return;
            }

            CesiumGeoreference georeference = this._globeAnchor.GetComponentInParent<CesiumGeoreference>();
            if (georeference == null)
            {
                Debug.LogError("CesiumCartographicPolygon could not find a CesiumGeoreference in its " +
                    "parent hierarchy, so the GeoJSON source could not be applied.");
                return;
            }

            // Remove the last point if it duplicates the first (GeoJson Spec.)
            if (points.Count > 3)
            {
                double2 first = points[0];
                double2 last = points[points.Count - 1];
                if (math.abs(first.x - last.x) < 1e-9 && math.abs(first.y - last.y) < 1e-9)
                {
                    points.RemoveAt(points.Count - 1);
                }
            }

            Matrix4x4 worldToLocal = this.transform.worldToLocalMatrix;

            BezierKnot[] knots = new BezierKnot[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                double3 cartographic = new double3(points[i].x, points[i].y, 0.0);
                double3 ecef = georeference.ellipsoid.LongitudeLatitudeHeightToCenteredFixed(cartographic);
                double3 unityPosition = georeference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);
                Vector3 localPosition = worldToLocal.MultiplyPoint3x4(
                    new Vector3((float)unityPosition.x, (float)unityPosition.y, (float)unityPosition.z));
                knots[i] = new BezierKnot(new float3(localPosition.x, localPosition.y, localPosition.z));
            }

            Spline newSpline = new Spline();
            newSpline.Knots = knots;
            newSpline.Closed = true;
            newSpline.SetTangentMode(TangentMode.Linear);

            this._isUpdatingSplineInternally = true;
            try
            {
                IReadOnlyList<Spline> splines = this._splineContainer.Splines;
                for (int i = splines.Count - 1; i >= 0; i--)
                {
                    this._splineContainer.RemoveSpline(splines[i]);
                }

                this._splineContainer.AddSpline(newSpline);
            }
            finally
            {
                this._isUpdatingSplineInternally = false;
            }
        }
#endif

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
                double3 cartographicPosition = georeference.ellipsoid.CenteredFixedToLongitudeLatitudeHeight(ecefPosition);

                cartographicPoints.Add(cartographicPosition.xy);
            }

            return cartographicPoints;
#else
            return emptyList;
#endif
        }
    }
}
