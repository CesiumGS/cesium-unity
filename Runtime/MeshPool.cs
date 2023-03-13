using System;
using UnityEngine;
using UnityEngine.Pool;

namespace CesiumForUnity
{
    public static class CesiumObjectPool
    {
        public static MeshPool MeshPool => meshPool;

        private const int capacity = 200;
        private static MeshPool meshPool = new MeshPool(
                () => new Mesh(),
                (mesh) => mesh.Clear(),
                null,
                (mesh) => GameObject.Destroy(mesh),
                collectionCheck: false,
                capacity);
    }

    public class MeshPool : ObjectPool<Mesh>
    {
        public MeshPool(
            Func<Mesh> createFunc,
            Action<Mesh> actionOnGet = null,
            Action<Mesh> actionOnRelease = null,
            Action<Mesh> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10000) : base(
                createFunc,
                actionOnGet,
                actionOnRelease,
                actionOnDestroy,
                collectionCheck,
                defaultCapacity,
                maxSize
                )
        {
            for (int i = 0; i < defaultCapacity; i++)
            {
                Release(Get());
            }
        }
    }
}
