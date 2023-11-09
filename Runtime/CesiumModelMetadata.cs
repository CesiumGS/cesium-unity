using Reinterop;
using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents the root EXT_structural_extension of a glTF model in a 
    /// <see cref="Cesium3DTileset"/>. This component is automatically added 
    /// to tile game objects if their models contain the root extension.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumModelMetadataImpl", "CesiumModelMetadataImpl.h")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    [AddComponentMenu("")]
    public partial class CesiumModelMetadata : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="CesiumPropertyTable"/>s available on this model metadata.
        /// </summary>
        public CesiumPropertyTable[] propertyTables
        {
            get; internal set;
        }
    }
}
