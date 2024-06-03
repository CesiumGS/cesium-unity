#include "CesiumFeaturesMetadataUtility.h"

#include "CesiumFeatureIdAttributeImpl.h"
#include "CesiumFeatureIdTextureImpl.h"
#include "CesiumPropertyTablePropertyImpl.h"

#include <CesiumGltf/ExtensionExtMeshFeatures.h>
#include <CesiumGltf/ExtensionModelExtStructuralMetadata.h>
#include <CesiumGltf/MeshPrimitive.h>
#include <CesiumGltf/Model.h>
#include <CesiumGltf/PropertyTableView.h>

#include <DotNet/CesiumForUnity/CesiumFeatureIdAttribute.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdSet.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdTexture.h>
#include <DotNet/CesiumForUnity/CesiumModelMetadata.h>
#include <DotNet/CesiumForUnity/CesiumPrimitiveFeatures.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTable.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTableProperty.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTableStatus.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Collections/Generic/Dictionary2.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/GameObject.h>

using namespace DotNet;
using namespace DotNet::System::Collections::Generic;

namespace CesiumForUnityNative {

DotNet::CesiumForUnity::CesiumPrimitiveFeatures
CesiumFeaturesMetadataUtility::addPrimitiveFeatures(
    const DotNet::UnityEngine::GameObject& primitiveGameObject,
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    const CesiumGltf::ExtensionExtMeshFeatures& extension) noexcept {
  CesiumForUnity::CesiumPrimitiveFeatures primitiveFeatures =
      primitiveGameObject
          .AddComponent<CesiumForUnity::CesiumPrimitiveFeatures>();

  const auto& gltfFeatureIds = extension.featureIds;
  primitiveFeatures.featureIdSets(
      System::Array1<CesiumForUnity::CesiumFeatureIdSet>(
          gltfFeatureIds.size()));

  auto featureIdSets = primitiveFeatures.featureIdSets();

  for (size_t i = 0; i < gltfFeatureIds.size(); i++) {
    const CesiumGltf::FeatureId& gltfFeatureId = gltfFeatureIds[i];
    if (gltfFeatureId.attribute) {
      featureIdSets.Item(
          i,
          CesiumFeatureIdAttributeImpl::CreateAttribute(
              model,
              primitive,
              gltfFeatureId.featureCount,
              *gltfFeatureId.attribute));
    } else if (gltfFeatureId.texture) {
      featureIdSets.Item(
          i,
          CesiumFeatureIdTextureImpl::CreateTexture(
              model,
              primitive,
              gltfFeatureId.featureCount,
              *gltfFeatureId.texture));
    } else {
      // Create implicit feature ID set (or an invalid one if featureCount = 0).
      featureIdSets.Item(
          i,
          CesiumForUnity::CesiumFeatureIdSet(gltfFeatureId.featureCount));
    }

    CesiumForUnity::CesiumFeatureIdSet featureIdSet = featureIdSets[i];
    featureIdSet.label(System::String(gltfFeatureId.label.value_or("")));
    featureIdSet.nullFeatureId(gltfFeatureId.nullFeatureId.value_or(-1));
    featureIdSet.propertyTableIndex(gltfFeatureId.propertyTable);
  }

  return primitiveFeatures;
}

DotNet::CesiumForUnity::CesiumModelMetadata
CesiumFeaturesMetadataUtility::addModelMetadata(
    const DotNet::UnityEngine::GameObject& modelGameObject,
    const CesiumGltf::Model& model,
    const CesiumGltf::ExtensionModelExtStructuralMetadata& extension) noexcept {
  CesiumForUnity::CesiumModelMetadata modelMetadata =
      modelGameObject.AddComponent<CesiumForUnity::CesiumModelMetadata>();

  const auto& gltfPropertyTables = extension.propertyTables;
  modelMetadata.propertyTables(
      System::Array1<CesiumForUnity::CesiumPropertyTable>(
          gltfPropertyTables.size()));

  auto propertyTables = modelMetadata.propertyTables();

  for (size_t i = 0; i < gltfPropertyTables.size(); i++) {
    const CesiumGltf::PropertyTable& gltfPropertyTable = gltfPropertyTables[i];

    CesiumForUnity::CesiumPropertyTable propertyTable =
        CesiumForUnity::CesiumPropertyTable();
    propertyTable.name(System::String(gltfPropertyTable.name.value_or("")));
    propertyTable.count(gltfPropertyTable.count);

    CesiumGltf::PropertyTableView view(model, gltfPropertyTable);
    if (view.status() == CesiumGltf::PropertyTableViewStatus::Valid) {
      propertyTable.status(CesiumForUnity::CesiumPropertyTableStatus::Valid);
      propertyTable.properties(Dictionary2<
                               System::String,
                               CesiumForUnity::CesiumPropertyTableProperty>(
          gltfPropertyTable.properties.size()));

      view.forEachProperty([properties = propertyTable.properties()](
                               const std::string& propertyId,
                               auto propertyValue) mutable {
        properties.Add(
            System::String(propertyId),
            CesiumFeaturesMetadataUtility::makePropertyTableProperty(
                propertyValue));
      });
    } else {
      propertyTable.status(CesiumForUnity::CesiumPropertyTableStatus::
                               ErrorInvalidPropertyTableClass);
    }

    propertyTables.Item(i, propertyTable);
  }

  return modelMetadata;
}

} // namespace CesiumForUnityNative
