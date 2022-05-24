
using Cesium3DTilesSelection;
using CesiumGltf;
using glm;
using UnityEngine;

namespace CesiumForUnity
{
    internal class UnityPrepareRendererResources : PrepareRendererResources
    {
        public override object? prepareInLoadThread(Model model, dmat4 transform)
        {
            Debug.Log("prepareInLoadThread");
            return null;
        }

        public override object? prepareInMainThread(Tile tile, object? loadThreadData)
        {
            Debug.Log("prepareInMainThread");
            return null;
        }
        public override void free(Tile tile, object? loadThreadData, object? mainThreadData)
        {
            Debug.Log("free");
        }
    }
}
