using Reinterop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Configures where the CesiumGeoJsonDocumentRasterOverlay should load its
    /// GeoJSON data from.
    /// </summary>
    public enum CesiumGeoJsonDocumentRasterOverlaySource
    {
        /// <summary>
        /// The raster overlay will load a GeoJSON document from a URL.
        /// </summary>
        FromUrl = 0,

        /// <summary>
        /// The raster overlay will load a GeoJSON document from Cesium ion.
        /// </summary>
        FromCesiumIon = 1,

        /// <summary>
        /// The raster overlay will use a GeoJSON document that has been parsed
        /// and styled in code using <see cref="CesiumGeoJsonDocumentRasterOverlay.SetGeoJsonDocument"/>.
        /// </summary>
        FromDocument = 2
    }

    /// <summary>
    /// A raster overlay that rasterizes a GeoJSON document and drapes it over the
    /// tileset. This allows stylized vector data to be displayed on terrain and
    /// other 3D Tiles.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumGeoJsonDocumentRasterOverlayImpl",
        "CesiumGeoJsonDocumentRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium GeoJSON Document Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumGeoJsonDocumentRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private CesiumGeoJsonDocumentRasterOverlaySource _source =
            CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon;

        /// <summary>
        /// The source from which to load the GeoJSON document.
        /// </summary>
        public CesiumGeoJsonDocumentRasterOverlaySource source
        {
            get => this._source;
            set
            {
                this._source = value;
                this.Refresh();
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
                this.Refresh();
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
                this.Refresh();
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
                this.Refresh();
            }
        }

        [SerializeField]
        private CesiumIonServer _ionServer = null;

        /// <summary>
        /// The Cesium ion server from which this raster overlay is loaded.
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
                {
                    this._ionServer = CesiumIonServer.serverForNewObjects;
                }
                return this._ionServer;
            }
            set
            {
                if (value == null) value = CesiumIonServer.serverForNewObjects;
                this._ionServer = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Range(0, 8)]
        private int _mipLevels = 0;

        /// <summary>
        /// The number of mip levels to generate for each tile of this raster overlay.
        /// </summary>
        /// <remarks>
        /// Additional mip levels can improve the visual quality of tiles farther from
        /// the camera at the cost of additional rasterization time to create each mip
        /// level.
        /// </remarks>
        public int mipLevels
        {
            get => this._mipLevels;
            set
            {
                this._mipLevels = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private CesiumVectorStyle _defaultStyle = CesiumVectorStyle.Default;

        /// <summary>
        /// The default style to use for this raster overlay.
        /// </summary>
        /// <remarks>
        /// If no style is set on a GeoJSON object or any of its parents, this style
        /// will be used instead.
        /// </remarks>
        public CesiumVectorStyle defaultStyle
        {
            get => this._defaultStyle;
            set
            {
                this._defaultStyle = value;
                this.Refresh();
            }
        }

        private CesiumGeoJsonDocument _document = null;

        /// <summary>
        /// Gets or sets the GeoJSON document used by this overlay.
        /// </summary>
        /// <remarks>
        /// Setting this property automatically changes <see cref="source"/> to
        /// <see cref="CesiumGeoJsonDocumentRasterOverlaySource.FromDocument"/>.
        /// You can style individual features in the document before setting it
        /// by using <see cref="CesiumGeoJsonObject.SetStyle"/>.
        /// </remarks>
        public CesiumGeoJsonDocument document
        {
            get => this._document;
            set
            {
                this._document = value;
                this._source = CesiumGeoJsonDocumentRasterOverlaySource.FromDocument;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);

        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);

#if UNITY_EDITOR
        private void Reset()
        {
            this._defaultStyle = CesiumVectorStyle.Default;
        }
#endif
    }
}
