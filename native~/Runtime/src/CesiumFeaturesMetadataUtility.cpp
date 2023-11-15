#include "CesiumFeaturesMetadataUtility.h"

#include "CesiumFeatureIdAttributeImpl.h"

#include <CesiumGltf/ExtensionExtMeshFeatures.h>
#include <CesiumGltf/MeshPrimitive.h>
#include <CesiumGltf/Model.h>

#include <DotNet/CesiumForUnity/CesiumFeatureIdAttribute.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdSet.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdSetType.h>
#include <DotNet/CesiumForUnity/CesiumPrimitiveFeatures.h>
#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>

using namespace DotNet;

namespace CesiumForUnityNative {

DotNet::CesiumForUnity::CesiumPrimitiveFeatures
CesiumFeaturesMetadataUtility::AddPrimitiveFeatures(
    const DotNet::UnityEngine::GameObject& primitiveGameObject,
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    const CesiumGltf::ExtensionExtMeshFeatures& extension) {
  CesiumForUnity::CesiumPrimitiveFeatures& primitiveFeatures =
      primitiveGameObject
          .AddComponent<CesiumForUnity::CesiumPrimitiveFeatures>();

  auto& featureIds = extension.featureIds;
  primitiveFeatures.featureIdSets(
      System::Array1<CesiumForUnity::CesiumFeatureIdSet>(featureIds.size()));

  auto& featureIdSets = primitiveFeatures.featureIdSets();

  for (size_t i = 0; i < featureIds.size(); i++) {
    const CesiumGltf::FeatureId& featureIdSet = featureIds[i];
    if (featureIdSet.attribute) {
      featureIdSets[i] = CesiumFeatureIdAttributeImpl::Create(
          model,
          primitive,
          *featureIdSet.attribute);
    } else if (featureIdSet.texture) {
      // TODO
    } else {
      // Create implicit feature ID set (or an invalid one if featureCount = 0).
      featureIdSets[i] =
          CesiumForUnity::CesiumFeatureIdSet(featureIdSet.featureCount);
    }
  }

  return primitiveFeatures;
}

} // namespace CesiumForUnityNative
