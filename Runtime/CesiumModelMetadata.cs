using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents the root `EXT_structural_metadata` extension of a glTF model in a 
    /// <see cref="Cesium3DTileset"/>. It holds views of property tables
    /// available on the glTF.
    /// </summary>
    /// <remarks>
    /// This component is automatically added 
    /// to tile game objects if their models contain the root extension.
    /// </remarks>
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    [AddComponentMenu("")]
    public class CesiumModelMetadata : MonoBehaviour
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
