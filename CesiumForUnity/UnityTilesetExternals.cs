using Cesium3DTilesSelection;

namespace CesiumForUnity
{

    internal class UnityTilesetExternals : TilesetExternals
    {
        public static UnityTilesetExternals Instance
        {
            get
            {
                if (UnityTilesetExternals._instance == null)
                {
                    UnityTilesetExternals._instance = new UnityTilesetExternals();
                }
                return UnityTilesetExternals._instance;
            }
        }

        private UnityTilesetExternals() : base(
            new UnityAsyncSystem(),
            new UnityAssetAccessor(),
            new UnityLogger(),
            new UnityPrepareRendererResources())
        {

        }

        private static UnityTilesetExternals? _instance;
    }

}
