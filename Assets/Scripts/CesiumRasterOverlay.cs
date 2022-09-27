using UnityEngine;

namespace CesiumForUnity
{
    [ExecuteInEditMode]
    public abstract class CesiumRasterOverlay : MonoBehaviour
    {
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

        protected abstract void AddToTileset(Cesium3DTileset tileset);
        protected abstract void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
