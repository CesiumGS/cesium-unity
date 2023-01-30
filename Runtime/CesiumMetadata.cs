using Reinterop;
using UnityEngine;
using System.Collections.Generic;

namespace CesiumForUnity
{
    /// <summary>
    /// Provides access to the metadata attached to features in a <see cref="Cesium3DTileset"/>.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumMetadataImpl", "CesiumMetadataImpl.h")]
    public partial class CesiumMetadata : MonoBehaviour
    {
         /// <summary>
         /// Contains information to identify a feature within a glTF.
         /// </summary>
         internal class FeatureReference {
            public string className {get; set;}
            public long featureID {get; set;}
            public string featureTable {get; set;}
            public int numProperties {get; set;}
        }

        public class Feature {
            public string className;
            public string featureTableName;
            public Dictionary<string, MetadataProperty> properties;
        }

        internal partial int getNumberOfFeatures(Transform transform);
        internal partial void getFeatureReferences(Transform transform, int triangleIndex, FeatureReference[] references);
        internal partial void getProperties(Transform transform, FeatureReference reference, MetadataProperty[] properties);

        /// <summary>
        /// Gets the features corresponding to a particular triangle in a tile.
        /// </summary>
        /// <param name="transform">The <code>Transform</code> of the tile from which to obtain properties.</param>
        /// <param name="triangleIndex">The index of the tile's triangle for which to obtain properties.</param>
        /// <returns>An array of features associated with the triangle.</returns>
        /// <remarks>
        /// The information to pass to this function can be obtained using the <code>Physics.Raycast</code>
        /// function.
        /// </remarks>
        public Feature[] GetFeatures(Transform transform, int triangleIndex){
            int numFeatures = getNumberOfFeatures(transform);
            FeatureReference[] references = new FeatureReference[numFeatures];
            Feature[] features = new Feature[numFeatures];
            for(int i = 0; i < numFeatures; i++){
                references[i] = new FeatureReference();
                features[i] = new Feature();
                features[i].properties = new Dictionary<string, MetadataProperty>();
            }
            getFeatureReferences(transform, triangleIndex, references);
            for(int i = 0; i < numFeatures; i++){
                int numProperties = references[i].numProperties;
                MetadataProperty[] properties = new MetadataProperty[numProperties];
                for(int j = 0; j < numProperties; j++){
                    properties[j] = new MetadataProperty();
                }
                features[i].className = references[i].className;
                features[i].featureTableName = references[i].featureTable;
                getProperties(transform, references[i], properties);
                for(int j = 0; j < numProperties; j++){
                    features[i].properties.Add(properties[j].GetPropertyName(), properties[j]);
                }
            }
            return features;
       }
   }
}
