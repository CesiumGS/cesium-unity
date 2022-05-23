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
            print("START!!!");
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
                viewportWidth = 1000.0,
                viewportHeight = 1000.0,
                horizontalFieldOfView = 1.0,
                verticalFieldOfView = 1.0
            }
        };

            ViewUpdateResult updateResult = this._tileset.UpdateView(viewStates);
        }

        private Tileset? _tileset;
    }

}
