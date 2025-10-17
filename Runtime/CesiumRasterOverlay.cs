﻿using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A pyramid of 2D images - sometimes terabytes or more in size - that can be draped over
    /// a <see cref="Cesium3DTileset"/>.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class CesiumRasterOverlay : MonoBehaviour
    {
        /// <summary>
        /// Encapsulates a method that receives details of a raster overlay load failure.
        /// </summary>
        /// <param name="details">The details of the load failure.</param>
        public delegate void RasterOverlayLoadFailureDelegate(
            CesiumRasterOverlayLoadFailureDetails details);

        /// <summary>
        /// An event that is raised when the raster overlay encounters an error that prevents it from loading.
        /// </summary>
        public static event
            RasterOverlayLoadFailureDelegate OnCesiumRasterOverlayLoadFailure;

        internal static void BroadcastCesiumRasterOverlayLoadFailure(
            CesiumRasterOverlayLoadFailureDetails details)
        {
            if (OnCesiumRasterOverlayLoadFailure != null)
            {
                OnCesiumRasterOverlayLoadFailure(details);
            }
        }

        [SerializeField]
        private string _materialKey = "0";

        /// <summary>
        /// The key to use to match this overlay to the corresponding parameters in the tileset's material.
        /// </summary>
        /// <remarks>
        /// In the tileset's materials, an overlay requires parameters for its texture, texture coordinate index,
        /// and translation and scale. Overlays must specify a string key to match with the correct parameters.
        /// The format of these parameters is as follows.
        /// <list type="bullet">
        /// <item><b>Overlay Texture</b>: _overlayTexture_KEY</item>
        /// <item><b>Overlay Texture Coordinate Index</b>: _overlayTextureCoordinateIndex_KEY</item>
        /// <item><b>Overlay Translation and Scale</b>: _overlayTranslationScale_KEY</item>
        /// </list>
        /// Material keys are useful for specifying the order of the raster overlays, or distinguishing them for
        /// overlay-specific effects.
        /// </remarks>
        public string materialKey
        {
            get => this._materialKey;
            set
            {
                this._materialKey = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private bool _showCreditsOnScreen = false;

        /// <summary>
        /// Whether or not to force this raster overlay's credits to be shown on the main screen. If false, the
        /// credits are usually only shown on a "Data Attribution" popup.
        /// </summary>
        public bool showCreditsOnScreen
        {
            get => this._showCreditsOnScreen;
            set
            {
                this._showCreditsOnScreen = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private float _maximumScreenSpaceError = 2.0f;

        /// <summary>
        /// The maximum number of pixels of error when rendering this overlay.
        /// This is used to select an appropriate level-of-detail.
        /// </summary>
        /// <remarks>
        /// When this property has its default value, 2.0, it means that raster overlay
        /// images will be sized so that, when zoomed in closest, a single pixel in
        /// the raster overlay maps to approximately 2x2 pixels on the screen.
        /// </remarks>
        public float maximumScreenSpaceError
        {
            get => this._maximumScreenSpaceError;
            set
            {
                this._maximumScreenSpaceError = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private int _maximumTextureSize = 2048;

        /// <summary>
        /// The maximum texel size of raster overlay textures, in either direction.
        /// </summary>
        /// <remarks>
        /// Images created by this overlay will be no more than this number of texels
        /// in either direction.This may result in reduced raster overlay detail in
        /// some cases.
        /// </remarks>
        public int maximumTextureSize
        {
            get => this._maximumTextureSize;
            set
            {
                this._maximumTextureSize = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private int _maximumSimultaneousTileLoads = 20;

        /// <summary>
        /// The maximum number of overlay tiles that may simultaneously be in
        /// the process of loading.
        /// </summary>
        public int maximumSimultaneousTileLoads
        {
            get => this._maximumSimultaneousTileLoads;
            set
            {
                this._maximumSimultaneousTileLoads = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private long _subTileCacheBytes = 16 * 1024 * 1024;

        /// <summary>
        /// The maximum number of bytes to use to cache sub-tiles in memory.
        /// </summary>
        /// <remarks>
        /// This is used by provider types that have an underlying tiling
        /// scheme that may not align with the tiling scheme of the geometry tiles on
        /// which the raster overlay tiles are draped. Because a single sub-tile may
        /// overlap multiple geometry tiles, it is useful to cache loaded sub-tiles
        /// in memory in case they're needed again soon. This property controls the
        /// maximum size of that cache.
        /// </remarks>
        public long subTileCacheBytes
        {
            get => this._subTileCacheBytes;
            set
            {
                this._subTileCacheBytes = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Adds this raster overlay to the <see cref="Cesium3DTileset"/> on the same game object.
        /// </summary>
        /// <remarks>
        /// If the overlay is already added or if the game object does not have a <see cref="Cesium3DTileset"/>,
        /// this method does nothing.
        /// </remarks>
        public void AddToTileset()
        {
            if (!isActiveAndEnabled)
                return;
            
            Cesium3DTileset tileset = this.gameObject.GetComponent<Cesium3DTileset>();
            if (tileset == null)
                return;

            // In case the material key changed, register the new key with the tileset.
            tileset.UpdateOverlayMaterialKeys();

            this.AddToTileset(tileset);
        }

        /// <summary>
        /// Removes this raster overlay from the <see cref="Cesium3DTileset"/> on the same game object.
        /// </summary>
        /// <remarks>
        /// If the overlay is not yet added or if the game object does not have a <see cref="Cesium3DTileset"/>,
        /// this method does nothing.
        /// </remarks>
        public void RemoveFromTileset()
        {
            Cesium3DTileset tileset = this.gameObject.GetComponent<Cesium3DTileset>();
            if (tileset == null)
                return;

            this.RemoveFromTileset(tileset);
        }

        /// <summary>
        /// Refreshes this overlay by calling <see cref="RemoveFromTileset"/> followed by
        /// <see cref="AddToTileset"/>. If the game object does not have a <see cref="Cesium3DTileset"/>,
        /// this method does nothing.
        /// </summary>
        public void Refresh()
        {
            this.RemoveFromTileset();
            this.AddToTileset();
        }

        private void OnEnable()
        {
            this.AddToTileset();
        }

        private void OnDisable()
        {
            this.RemoveFromTileset();
        }

        private void OnValidate()
        {
            this.Refresh();
        }

        /// <summary>
        /// When implemented in a derived class, adds the raster overlay to the given tileset.
        /// </summary>
        /// <param name="tileset">The tileset.</param>
        protected abstract void AddToTileset(Cesium3DTileset tileset);

        /// <summary>
        /// When implemented in a derived class, removes the raster overlay from the given tileset.
        /// </summary>
        /// <param name="tileset">The tileset.</param>
        protected abstract void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
