using System;
using System.Collections.Generic;
using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataImpl", "CesiumMetadataImpl.h")]
    public partial class CesiumMetadata : MonoBehaviour
    {
        public partial void loadMetadata(Transform transform, int triangleIndex, MetadataProperty[] properties);

        private partial int getNumberOfProperties(Transform transform);

        public MetadataProperty[] GetProperties(Transform transform, int triangleIndex)
        {
            int numberOfProperties = getNumberOfProperties(transform);
            MetadataProperty[] properties = new MetadataProperty[numberOfProperties];
            for(int i = 0; i < numberOfProperties; i++){
                properties[i] = new MetadataProperty();
            }
            loadMetadata(transform, triangleIndex, properties);
            return properties;
        }
    }
}