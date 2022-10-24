using System;
using System.Collections.Generic;
using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataImpl", "CesiumMetadataImpl.h")]
    public partial class CesiumMetadata : MonoBehaviour
    {
        public partial void loadMetadata(Transform transform, int triangleIndex);

        private partial int getNumberOfProperties();

        private partial void getProperty(MetadataProperty property, int index);

        public IEnumerable<MetadataProperty> Properties()
        {
            for (int i = 0; i < getNumberOfProperties(); i++)
            {
                MetadataProperty property = new MetadataProperty();
                getProperty(property, i);
                yield return property;
            }
        }
    }
}