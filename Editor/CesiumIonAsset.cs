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
            Null
        }

        private AssetType _type = AssetType.Null;
        private Cesium3DTileset _tileset;
        private CesiumRasterOverlay _overlay;

        public CesiumIonAsset()
        {
            this._type = AssetType.Null;
        }

        public CesiumIonAsset(Cesium3DTileset tileset)
        {
            this._type = AssetType.Tileset;
            this._tileset = tileset;
        }

        public CesiumIonAsset(CesiumRasterOverlay overlay)
        {
            this._type = AssetType.Overlay;
            this._overlay = overlay;
        }

        public Cesium3DTileset tileset
        {
            get => this._type == AssetType.Tileset ? this._tileset : null;
        }

        public CesiumRasterOverlay overlay
        {
            get => this._type == AssetType.Overlay ? this._overlay : null;
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

                if (this._type == AssetType.Overlay)
                {
                    return "Raster Overlay";
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

            return false;
        }
    }
}