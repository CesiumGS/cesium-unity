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

    /// <summary>
    /// The CesiumObjectPool is a singleton that has member ObjectPools of different types.
    /// This class offers an easy way to Rent and Return common resources.
    /// </summary>
    public class CesiumObjectPool : MonoBehaviour
    {
        public static CesiumObjectPool Instance = null;

        [Tooltip("The amount of meshes you wish to pre-allocate and pool")]
        [SerializeField]
        private int meshPoolCount = 200;

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
