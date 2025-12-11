using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents the EXT_mesh_features of a glTF primitive in a <see cref="Cesium3DTileset"/>. 
    /// It holds views of the feature ID sets associated with this primitive.
    /// </summary>
    /// <remarks>
    /// This component is automatically added to primitive game objects if they
    /// contain the extension.
    /// </remarks>
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

        #region Helper functions

        // Cache a list of indices to prevent the allocation of a new array every time
        // GetFirstVertexFromTriangle is called.
        private static List<int> _indices;

        private static int GetFirstVertexFromTriangle(MeshFilter meshFilter, int triangleIndex)
        {
            if (meshFilter == null || meshFilter.mesh == null)
            {
                return -1;
            }

            if (CesiumPrimitiveFeatures._indices == null)
            {
                CesiumPrimitiveFeatures._indices = new List<int>();
            }

            meshFilter.mesh.GetTriangles(CesiumPrimitiveFeatures._indices, 0);
            int targetVertex = triangleIndex * 3;
            return targetVertex < CesiumPrimitiveFeatures._indices.Count ? CesiumPrimitiveFeatures._indices[targetVertex] : -1;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Given a successful raycast hit, finds the index of the first vertex
        /// from the hit triangle.
        /// </summary>        
        /// <param name="hitInfo">The raycast hit.</param>
        /// <returns>The index of the first vertex on the triangle, or -1 if the hit was unsuccessful.</returns>
        public static int GetFirstVertexFromHitTriangle(RaycastHit hitInfo)
        {
            MeshFilter meshFilter = hitInfo.transform.GetComponent<MeshFilter>();
            return GetFirstVertexFromTriangle(meshFilter, hitInfo.triangleIndex);
        }

        /// <summary>
        /// Gets all the feature ID sets of the given type. If the primitive has no sets
        /// of that type, the returned array will be empty.
        /// </summary>
        /// <param name="type"> The desired feature ID set type.</param>
        /// <returns>An array of feature ID sets of the specified type, if any exist on the CesiumPrimitiveFeatures.</returns>
        public CesiumFeatureIdSet[] GetFeatureIdSetsOfType(CesiumFeatureIdSetType type)
        {
            return Array.FindAll(this.featureIdSets, set => (set.type == type));
        }

        /// <summary>
        /// Gets the feature ID associated with the given triangle, specified by index.
        /// </summary>
        /// <remarks>
        /// A primitive may have multiple feature ID sets, so this allows a feature ID
        /// set to be specified by index.This value should index into the array of
        /// CesiumFeatureIdSets in the CesiumPrimitiveFeatures. If the specified
        /// feature ID set index is invalid, this returns -1.
        /// </remarks>
        /// <param name="triangleIndex">The index of the target triangle.</param>
        /// <param name="featureIdSetIndex">The index of the target feature ID set.</param>
        /// <returns>The feature ID, or -1 if the specified triangle or feature ID set is invalid.</returns>

        public Int64 GetFeatureIdFromTriangle(int triangleIndex, Int64 featureIdSetIndex = 0)
        {
            MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || featureIdSetIndex < 0 || featureIdSetIndex >= this.featureIdSets.Length)
            {
                return -1;
            }

            CesiumFeatureIdSet featureIdSet = this.featureIdSets[featureIdSetIndex];

            return featureIdSet.GetFeatureIdForVertex(
                GetFirstVertexFromTriangle(meshFilter, triangleIndex));
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
        public Int64 GetFeatureIdFromRaycastHit(RaycastHit hitInfo, Int64 featureIdSetIndex = 0)
        {
            if (hitInfo.transform.GetComponent<CesiumPrimitiveFeatures>() != this ||
                featureIdSetIndex < 0 || featureIdSetIndex >= this.featureIdSets.Length)
            {
                return -1;
            }

            CesiumFeatureIdSet featureIdSet = this.featureIdSets[featureIdSetIndex];

            return featureIdSet.GetFeatureIdFromRaycastHit(hitInfo);
        }

        #endregion 
    }
}
