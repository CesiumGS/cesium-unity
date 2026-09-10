#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /*
     * This class functions like a std::variant in C++. It only contains
     * either a Cesium3DTileset or a CesiumRasterOverlay.
     */
    public class CesiumIonAsset
    {
        private enum AssetType
        {
            Tileset,
            Overlay,
            GeoJsonOverlay,
            CartographicPolygon,
            Null
        }

        private AssetType _type = AssetType.Null;
        private Cesium3DTileset _tileset;
        private CesiumIonRasterOverlay _overlay;
        private CesiumGeoJsonDocumentRasterOverlay _geoJsonOverlay;
        private CesiumCartographicPolygon _cartographicPolygon;

        public CesiumIonAsset()
        {
            this._type = AssetType.Null;
        }

        public CesiumIonAsset(Cesium3DTileset tileset)
        {
            this._type = AssetType.Tileset;
            this._tileset = tileset;
        }

        public CesiumIonAsset(CesiumIonRasterOverlay overlay)
        {
            this._type = AssetType.Overlay;
            this._overlay = overlay;
        }

        public CesiumIonAsset(CesiumGeoJsonDocumentRasterOverlay overlay)
        {
            this._type = AssetType.GeoJsonOverlay;
            this._geoJsonOverlay = overlay;
        }

        public CesiumIonAsset(CesiumCartographicPolygon polygon)
        {
            this._type = AssetType.CartographicPolygon;
            this._cartographicPolygon = polygon;
        }

        public Cesium3DTileset tileset
        {
            get => this._type == AssetType.Tileset ? this._tileset : null;
        }

        public CesiumIonRasterOverlay overlay
        {
            get => this._type == AssetType.Overlay ? this._overlay : null;
        }

        public CesiumGeoJsonDocumentRasterOverlay geoJsonOverlay
        {
            get => this._type == AssetType.GeoJsonOverlay ? this._geoJsonOverlay : null;
        }

        public CesiumCartographicPolygon cartographicPolygon
        {
            get => this._type == AssetType.CartographicPolygon ? this._cartographicPolygon : null;
        }

        public string objectName
        {
            get
            {
                if (this._type == AssetType.Tileset && this._tileset != null)
                {
                    return this._tileset.gameObject.name;
                }

                if (this._type == AssetType.Overlay && this._overlay != null)
                {
                    return this._overlay.gameObject.name;
                }

                if (this._type == AssetType.GeoJsonOverlay && this._geoJsonOverlay != null)
                {
                    return this._geoJsonOverlay.gameObject.name;
                }

                if (this._type == AssetType.CartographicPolygon && this._cartographicPolygon != null)
                {
                    return this._cartographicPolygon.gameObject.name;
                }

                return "";
            }
        }

        public string type
        {
            get
            {
                if (this._type == AssetType.Tileset)
                {
                    return "Tileset";
                }

                if (this._type == AssetType.Overlay || this._type == AssetType.GeoJsonOverlay)
                {
                    return "Raster Overlay";
                }

                if (this._type == AssetType.CartographicPolygon)
                {
                    return "Cartographic Polygon";
                }

                return "";
            }
        }

        public string componentType
        {
            get
            {
                if (this._type == AssetType.Tileset && this._tileset != null)
                {
                    return this._tileset.GetType().Name;
                }

                if (this._type == AssetType.Overlay && this._overlay != null)
                {
                    return this._overlay.GetType().Name;
                }

                if (this._type == AssetType.GeoJsonOverlay && this._geoJsonOverlay != null)
                {
                    return this._geoJsonOverlay.GetType().Name;
                }

                if (this._type == AssetType.CartographicPolygon && this._cartographicPolygon != null)
                {
                    return this._cartographicPolygon.GetType().Name;
                }

                return "";
            }
        }

        public string ionAccessToken
        {
            get
            {
                if (this._type == AssetType.Tileset && this._tileset != null)
                {
                    return this._tileset.tilesetSource == CesiumDataSource.FromCesiumIon
                        ? this._tileset.ionAccessToken : "";
                }

                if (this._type == AssetType.Overlay)
                {
                    CesiumIonRasterOverlay ionOverlay = this._overlay as CesiumIonRasterOverlay;
                    return ionOverlay != null ? ionOverlay.ionAccessToken : "";
                }

                if (this._type == AssetType.GeoJsonOverlay && this._geoJsonOverlay != null)
                {
                    return this._geoJsonOverlay.source == CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon
                        ? this._geoJsonOverlay.ionAccessToken : "";
                }

                if (this._type == AssetType.CartographicPolygon && this._cartographicPolygon != null)
                {
                    return this._cartographicPolygon.source == CesiumCartographicPolygonSource.FromCesiumIon
                        ? this._cartographicPolygon.ionAccessToken : "";
                }

                return "";
            }
            set
            {
                if (this._type == AssetType.Tileset && this._tileset != null)
                {
                    this._tileset.ionAccessToken = value;
                }

                if (this._type == AssetType.Overlay)
                {
                    CesiumIonRasterOverlay ionOverlay = this._overlay as CesiumIonRasterOverlay;
                    if (ionOverlay != null)
                    {
                        ionOverlay.ionAccessToken = value;
                    }
                }

                if (this._type == AssetType.GeoJsonOverlay && this._geoJsonOverlay != null)
                {
                    this._geoJsonOverlay.ionAccessToken = value;
                }

                if (this._type == AssetType.CartographicPolygon && this._cartographicPolygon != null)
                {
                    this._cartographicPolygon.ionAccessToken = value;
                }
            }
        }

        public long ionAssetID
        {
            get
            {
                if (this._type == AssetType.Tileset && this._tileset != null)
                {
                    return this._tileset.tilesetSource == CesiumDataSource.FromCesiumIon
                        ? this._tileset.ionAssetID : 0;
                }

                if (this._type == AssetType.Overlay && this._overlay != null)
                {
                    CesiumIonRasterOverlay ionOverlay = this._overlay as CesiumIonRasterOverlay;
                    return ionOverlay != null ? ionOverlay.ionAssetID : 0;
                }

                if (this._type == AssetType.GeoJsonOverlay && this._geoJsonOverlay != null)
                {
                    return this._geoJsonOverlay.source == CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon
                        ? this._geoJsonOverlay.ionAssetID : 0;
                }

                if (this._type == AssetType.CartographicPolygon && this._cartographicPolygon != null)
                {
                    return this._cartographicPolygon.source == CesiumCartographicPolygonSource.FromCesiumIon
                        ? this._cartographicPolygon.ionAssetID : 0;
                }

                return 0;
            }
        }

        public bool IsNull()
        {
            if(this._type == AssetType.Tileset)
            {
                return this._tileset == null;
            }

            if(this._type == AssetType.Overlay)
            {
                return this._overlay == null;
            }

            if(this._type == AssetType.GeoJsonOverlay)
            {
                return this._geoJsonOverlay == null;
            }

            if (this._type == AssetType.CartographicPolygon)
            {
                return this._cartographicPolygon == null;
            }

            return true;
        }

        public bool IsUsingCesiumIon()
        {
            if (this._type == AssetType.Tileset && this._tileset != null)
            {
                return this._tileset.tilesetSource == CesiumDataSource.FromCesiumIon;
            }

            if (this._type == AssetType.Overlay && this._overlay != null)
            {
                CesiumIonRasterOverlay ionOverlay = this._overlay as CesiumIonRasterOverlay;
                return ionOverlay != null;
            }

            if (this._type == AssetType.GeoJsonOverlay && this._geoJsonOverlay != null)
            {
                return this._geoJsonOverlay.source == CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon;
            }

            if (this._type == AssetType.CartographicPolygon && this._cartographicPolygon != null)
            {
                return this._cartographicPolygon.source == CesiumCartographicPolygonSource.FromCesiumIon;
            }

            return false;
        }
    }
}
#endif
