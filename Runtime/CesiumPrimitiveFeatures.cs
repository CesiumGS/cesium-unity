using Reinterop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents the EXT_mesh_features of a glTF primitive in a <see cref="Cesium3DTileset"/>. 
    /// It holds views of the feature ID  sets associated with this primitive.
    /// 
    /// This component is automatically added to primitive game objects if they
    /// contain the extension.
    /// </summary>
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    [AddComponentMenu("")]
    public partial class CesiumPrimitiveFeatures : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="CesiumFeatureIdSet"/>s available on this primitive.
        /// </summary>
        public CesiumFeatureIdSet[] featureIdSets
        {
            get; internal set;
        }

        /// <summary>
        /// Gets the feature ID from the given raycast hit, assuming it has hit a 
        /// glTF primitive component containing this CesiumPrimitiveFeatures.
        /// </summary>
        /// <remarks>
        /// A primitive may have multiple feature ID sets, so this allows a feature ID
        /// set to be specified by index. This value should index into the array of
        /// CesiumFeatureIdSets in the CesiumPrimitiveFeatures. If the specified
        /// feature ID set index is invalid, this returns -1.
        /// </remarks>
        /// <param name="hitInfo">The raycast hit info.</param>
        /// <param name="featureIdSetIndex">The index of the target feature ID set.</param>
        /// <returns>The feature ID, or -1 if the specified feature ID set is invalid.</returns>
        public static Int64 GetFeatureIdFromRaycastHit(
            RaycastHit hitInfo,
            Int64 featureIdSetIndex = 0)
        {
            CesiumPrimitiveFeatures primitiveFeatures = hitInfo.transform.GetComponent<CesiumPrimitiveFeatures>();
            if (primitiveFeatures == null || 
                featureIdSetIndex < 0 || featureIdSetIndex >= primitiveFeatures.featureIdSets.Length)
            {
                return -1;
            }

            CesiumFeatureIdSet featureIdSet = primitiveFeatures.featureIdSets[featureIdSetIndex];

            return featureIdSet.GetFeatureIdFromRaycastHit(hitInfo);
        }
    }
}
