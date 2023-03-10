using System;
using UnityEngine;

namespace CesiumForUnity
{
    public class MeshPool : ObjectPool<Mesh>
    {
        public MeshPool(int capacity,
            int preallocationCount,
            Func<Mesh> objectConstructor,
            Action<Mesh> objectDeconstructor = null,
            Action<Mesh> onRent = null,
            Action<Mesh> onReturn = null) : base(
                capacity,
                preallocationCount,
                objectConstructor,
                objectDeconstructor,
                onRent,
                onReturn)
        { }
    }

    public class CesiumObjectPool : MonoBehaviour
    {
        [SerializeField]
        private int meshPoolCount = 200;

        public static CesiumObjectPool Instance = null;

        private MeshPool meshPool = null;
        public MeshPool MeshPool => meshPool;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Duplicate singleton CesiumObjectPool.cs");
                return;
            }

            Instance = this;

            meshPool = new MeshPool(
                meshPoolCount,
                meshPoolCount,
                () => new Mesh(),
                (mesh) => GameObject.Destroy(mesh),
                (mesh) => mesh.Clear());
        }
    }
}
