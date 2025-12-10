using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    internal class CesiumObjectPools
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

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            Dispose();
        }
#endif
    }
}
