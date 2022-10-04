using System;
using System.Collections.Generic;
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

        private partial int getNumberOfProperties();

        private partial string getKey(int index);

        private partial string getValueAsString(int index);

        public IEnumerable<string> Keys(){
            for(int i = 0; i < getNumberOfProperties(); i++){
                yield return getKey(i);
            }
        }

        public IEnumerable<string> ValuesAsString(){
            for(int i = 0; i < getNumberOfProperties(); i++){
                yield return getValueAsString(i);
            }
        }
    }
}