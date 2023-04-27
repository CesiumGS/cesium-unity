using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public class CesiumObjectPools
    {
        public static CesiumObjectPool<Mesh> MeshPool => _meshPool;

        private static CesiumObjectPool<Mesh> _meshPool;

        public static void Dispose()
        {
            _meshPool.Dispose();
        }

        static CesiumObjectPools()
        {
            _meshPool = new CesiumObjectPool<Mesh>(
                () => new Mesh(),
                (mesh) => mesh.Clear(),
                (mesh) => UnityLifetime.Destroy(mesh));

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            Dispose();
        }
    }
}
