using UnityEngine;
using UnityEngine.Pool;

namespace CesiumForUnity
{
    public static class CesiumObjectPool
    {
        public static ObjectPool<Mesh> MeshPool => meshPool;

        private const int capacity = 200;
        private static ObjectPool<Mesh> meshPool = new ObjectPool<Mesh>(
                () => new Mesh(),
                (mesh) => mesh.Clear(),
                null,
                (mesh) => GameObject.Destroy(mesh),
                collectionCheck: false,
                capacity);
    }
}
