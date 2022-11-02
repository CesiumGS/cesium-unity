using System;
using UnityEngine;

namespace CesiumForUnity
{
    [ExecuteInEditMode]
    public abstract class CesiumRasterOverlay : MonoBehaviour
    {
        public delegate void RasterOverlayLoadFailureDelegate(
            CesiumRasterOverlayLoadFailureDetails details);
        public static event
            RasterOverlayLoadFailureDelegate? OnCesiumRasterOverlayLoadFailure;

        public static void BroadcastCesiumRasterOverlayLoadFailure(
            CesiumRasterOverlayLoadFailureDetails details)
        {
            if (OnCesiumRasterOverlayLoadFailure != null)
            {
                OnCesiumRasterOverlayLoadFailure(details);

            }
        }

        [SerializeField]
        private bool _showCreditsOnScreen = false;

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

        public long subTileCacheBytes
        {
            get => this._subTileCacheBytes;
            set
            {
                this._subTileCacheBytes = value;
                this.Refresh();
            }
        }

        public void AddToTileset()
        {
            Cesium3DTileset? tileset = this.gameObject.GetComponent<Cesium3DTileset>();
            if (tileset == null)
                return;

            this.AddToTileset(tileset);
        }

        public void RemoveFromTileset()
        {
            Cesium3DTileset? tileset = this.gameObject.GetComponent<Cesium3DTileset>();
            if (tileset == null)
                return;

            this.RemoveFromTileset(tileset);
        }

        public void Refresh()
        {
            this.RemoveFromTileset();
            if (this.enabled)
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

        protected abstract void
            AddToTileset(Cesium3DTileset tileset);
        protected abstract void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
