using UnityEngine;
using Cesium3DTilesSelection;

namespace CesiumForUnity
{

    public class Cesium3DTileset : MonoBehaviour
    {
        static Cesium3DTileset()
        {
            Tileset.RegisterAllTileContentTypes();
        }

        void Start()
        {
            using (TilesetOptions options = new TilesetOptions())
            {
                this._tileset = new Tileset(
                    UnityTilesetExternals.Instance,
                    1,
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJjZmUzNjE3MC0wZmUwLTQzODItODMwZC01ZjE1Yzg1N2Y1MDIiLCJpZCI6MjU4LCJpYXQiOjE1MTczNTg0ODF9.Yv10hy_E1N0Ccc4y23fMlNkBtxiFc852wAfUSwmVUaA",
                    options);
            }
        }

        void OnDestroy()
        {
            if (this._tileset != null)
            {
                this._tileset.Dispose();
                this._tileset = null;
            }
        }

        void Update()
        {
            if (this._tileset == null) return;

            // TODO: Maybe add a "Cesium Camera" behavior that can be added to arbitrary cameras, and handle its
            // OnPreCull event to accumulate cameras to use for tile selection. But probably fall back on the
            // main camera when no cameras have been explicitly tagged.
            Camera camera = Camera.main;

            double verticalFOV = camera.fieldOfView * Mathf.PI / 180.0;

            var viewStates = new ViewState[] {
                new ViewState(){
                    positionX = 7378137.0,
                    positionY = 0.0,
                    positionZ = 0.0,
                    directionX = -1.0,
                    directionY = 0.0,
                    directionZ = 0.0,
                    upX = 0.0,
                    upY = 0.0,
                    upZ = 1.0,
                    viewportWidth = camera.pixelWidth,
                    viewportHeight = camera.pixelHeight,
                    horizontalFieldOfView = verticalFOV * camera.aspect,
                    verticalFieldOfView = verticalFOV
                }
            };

            ViewUpdateResult updateResult = this._tileset.UpdateView(viewStates);
            this.updateLastViewUpdateResultState(updateResult);
        }

        private void updateLastViewUpdateResultState(ViewUpdateResult currentResult)
        {
            ViewUpdateResult? previousResult = this._lastUpdateResult;
            if (
                previousResult == null ||
                currentResult.tilesToRenderThisFrame.Length != previousResult.tilesToRenderThisFrame.Length ||
                currentResult.tilesLoadingLowPriority != previousResult.tilesLoadingLowPriority ||
                currentResult.tilesLoadingMediumPriority != previousResult.tilesLoadingMediumPriority ||
                currentResult.tilesLoadingHighPriority != previousResult.tilesLoadingHighPriority ||
                currentResult.tilesVisited != previousResult.tilesVisited ||
                currentResult.culledTilesVisited != previousResult.culledTilesVisited ||
                currentResult.tilesCulled != previousResult.tilesCulled ||
                currentResult.maxDepthVisited != previousResult.maxDepthVisited)
            {
                this.printViewUpdateStats(currentResult);
            }

            this._lastUpdateResult = currentResult;
        }

        private void printViewUpdateStats(ViewUpdateResult currentResult)
        {
            Debug.LogFormat("{0}: Visited {1}, Culled Visited {2}, Rendered {3}, Culled {4}, Max Depth Visited {5}, Loading-Low {6}, Loading-Medium {7}, Loading-High {8}",
                this.gameObject.name,
                currentResult.tilesVisited,
                currentResult.culledTilesVisited,
                currentResult.tilesToRenderThisFrame.Length,
                currentResult.tilesCulled,
                currentResult.maxDepthVisited,
                currentResult.tilesLoadingLowPriority,
                currentResult.tilesLoadingMediumPriority,
                currentResult.tilesLoadingHighPriority);
        }

        private Tileset? _tileset;
        private ViewUpdateResult? _lastUpdateResult;
    }

}
