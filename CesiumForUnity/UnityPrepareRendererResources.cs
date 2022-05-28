
using Cesium3DTilesSelection;
using CesiumGltf;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace CesiumForUnity
{
    internal class UnityPrepareRendererResources : PrepareRendererResources
    {
        public override object? prepareInLoadThread(Model model, CesiumUtility.Matrix4 transform)
        {
            Debug.Log("prepareInLoadThread");
            ScenePrimitive[] primitives = model.GetPrimitivesInScene(-1);

            for (int i = 0; i < primitives.Length; ++i)
            {
                Debug.Log("Primitive! " + primitives[i].transform);
            }
            //var a = new NativeArray<float>(5, Allocator.Persistent);
            //unsafe
            //{
            //    void* p = NativeArrayUnsafeUtility.GetUnsafePtr(a);
            //}
            //model.GetAllPrimitives()
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
