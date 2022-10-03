using System;
using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataImpl", "CesiumMetadataImpl.h")]
    public partial class CesiumMetadata : MonoBehaviour
    {
        [SerializeField]
        [Header("test")]
        [Tooltip("test 2")]
        private double _test = 234;

        public double test 
        {
            get => this._test;
            set
            {
                this._test = value;
            }
        }

        public partial void loadMetadata(Transform transform, int triangleIndex);
    }
}