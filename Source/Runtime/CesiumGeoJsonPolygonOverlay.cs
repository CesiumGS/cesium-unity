using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A polygon component that derives its vertices from a GeoJSON document for use
    /// with <see cref="CesiumPolygonRasterOverlay"/>. Supports loading GeoJSON from a
    /// pre-parsed document, a URL, or a Cesium ion asset.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Cesium/Cesium GeoJSON Polygon Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public class CesiumGeoJsonPolygonOverlay : CesiumCartographicPolygonBase
    {
        [SerializeField]
        private CesiumGeoJsonDocumentRasterOverlaySource _source =
            CesiumGeoJsonDocumentRasterOverlaySource.FromDocument;

        /// <summary>
        /// The source from which to load the GeoJSON document.
        /// </summary>
        public CesiumGeoJsonDocumentRasterOverlaySource source
        {
            get => this._source;
            set
            {
                this._source = value;
                this.LoadDocument();
            }
        }

        [SerializeField]
        private string _url = "";

        /// <summary>
        /// The URL from which to load the GeoJSON document.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumGeoJsonDocumentRasterOverlaySource.FromUrl"/>.
        /// </remarks>
        public string url
        {
            get => this._url;
            set
            {
                this._url = value;
                this.LoadDocument();
            }
        }

        [SerializeField]
        private long _ionAssetID = 0;

        /// <summary>
        /// The ID of the Cesium ion asset to use.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon"/>.
        /// </remarks>
        public long ionAssetID
        {
            get => this._ionAssetID;
            set
            {
                this._ionAssetID = value;
                this.LoadDocument();
            }
        }

        [SerializeField]
        private string _ionAccessToken = "";

        /// <summary>
        /// The access token to use to access the Cesium ion resource.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon"/>.
        /// If empty, the default token from the ion server will be used.
        /// </remarks>
        public string ionAccessToken
        {
            get => this._ionAccessToken;
            set
            {
                this._ionAccessToken = value;
                this.LoadDocument();
            }
        }

        [SerializeField]
        private CesiumIonServer _ionServer = null;

        /// <summary>
        /// The Cesium ion server from which this overlay's GeoJSON is loaded.
        /// </summary>
        /// <remarks>
        /// Only used when <see cref="source"/> is set to
        /// <see cref="CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon"/>.
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
                this.LoadDocument();
            }
        }

        private CesiumGeoJsonDocument _document = null;

        /// <summary>
        /// Gets or sets the GeoJSON document used by this overlay.
        /// </summary>
        /// <remarks>
        /// Setting this property automatically changes <see cref="source"/> to
        /// <see cref="CesiumGeoJsonDocumentRasterOverlaySource.FromDocument"/>.
        /// </remarks>
        public CesiumGeoJsonDocument document
        {
            get => this._document;
            set
            {
                this._document = value;
                this._source = CesiumGeoJsonDocumentRasterOverlaySource.FromDocument;
                this._cachedDocument = value;
                this.NotifyOverlays();
            }
        }

        private CesiumGeoJsonDocument _cachedDocument = null;

        private void OnEnable()
        {
            this.LoadDocument();
        }

        private void OnValidate()
        {
            this.LoadDocument();
        }

        /// <summary>
        /// Reloads the GeoJSON document from the configured source and notifies any
        /// referencing <see cref="CesiumPolygonRasterOverlay"/> components to refresh.
        /// </summary>
        public void Refresh()
        {
            this.LoadDocument();
        }

        private async void LoadDocument()
        {
            CesiumGeoJsonDocument loaded = null;
            switch (this._source)
            {
                case CesiumGeoJsonDocumentRasterOverlaySource.FromDocument:
                    loaded = this._document;
                    break;
                case CesiumGeoJsonDocumentRasterOverlaySource.FromUrl:
                    if (!string.IsNullOrEmpty(this._url))
                        loaded = await CesiumGeoJsonDocument.LoadFromUrlAsync(this._url);
                    break;
                case CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon:
                    if (this._ionAssetID > 0)
                        loaded = await CesiumGeoJsonDocument.LoadFromCesiumIonAsync(
                            this._ionAssetID, this._ionAccessToken, this.ionServer);
                    break;
            }
            this._cachedDocument = loaded;
            this.NotifyOverlays();
        }

        private void NotifyOverlays()
        {
            foreach (CesiumPolygonRasterOverlay o in FindObjectsOfType<CesiumPolygonRasterOverlay>())
            {
                if (o.polygons != null && o.polygons.Contains(this))
                    o.Refresh();
            }
        }

        /// <inheritdoc/>
        internal override List<double2> GetCartographicPoints(Matrix4x4 worldToTileset)
        {
            if (this._cachedDocument == null)
                return emptyList;

            CesiumGeoJsonObject obj = this._cachedDocument.GetRootObject();
            CesiumGeoJsonFeature[] features = obj?.GetObjectAsFeatureCollection();
            CesiumGeoJsonObject geometry = features?[0].GetGeometry();
            CesiumGeoJsonPolygon[] poly = geometry?.GetObjectAsMultiPolygon();
            CesiumGeoJsonLineString[] rings = poly?[0].rings;

            if (rings == null || rings[0] == null || rings[0].points.Length < 3)
                return emptyList;

            List<double2> result = new List<double2>(rings[0].points.Length);
            foreach (double3 p in rings[0].points)
                result.Add(new double2(p.x, p.y));
            return result;
        }
    }
}
