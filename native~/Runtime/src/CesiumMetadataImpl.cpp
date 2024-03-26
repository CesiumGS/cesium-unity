#include "CesiumMetadataImpl.h"

#include "CesiumFeaturesMetadataUtility.h"

#include <CesiumGltf/AccessorUtility.h>
#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/ExtensionExtMeshFeatures.h>
#include <CesiumGltf/ExtensionModelExtStructuralMetadata.h>
#include <CesiumGltf/PropertyTablePropertyView.h>
#include <CesiumGltf/PropertyTableView.h>

#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/UnityEngine/Transform.h>

using namespace DotNet;
using namespace DotNet::CesiumForUnity;
using namespace CesiumGltf;

namespace CesiumForUnityNative {

namespace {

int64_t getFirstVertexFromTriangle(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    int64_t vertexCount,
    int64_t triangleIndex) {
  CesiumGltf::IndexAccessorType indexAccessor =
      getIndexAccessorView(model, primitive);

  auto indices = std::visit(
      IndicesForFaceFromAccessor{triangleIndex, vertexCount, primitive.mode},
      indexAccessor);

  return indices[0];
}

int64_t getFeatureIdFromVertex(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    const int64_t featureIdAttributeIndex,
    const int64_t vertexIndex) {
  CesiumGltf::FeatureIdAccessorType featureIDAccessor =
      getFeatureIdAccessorView(model, primitive, featureIdAttributeIndex);

  return std::visit(
      CesiumGltf::FeatureIdFromAccessor{vertexIndex},
      featureIDAccessor);
}

} // namespace

CesiumMetadataImpl::CesiumMetadataImpl(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata) {}

CesiumMetadataImpl::~CesiumMetadataImpl() {}

void CesiumMetadataImpl::addMetadata(
    int32_t instanceID,
    const CesiumGltf::Model* pModel,
    const CesiumGltf::MeshPrimitive* pPrimitive) {
  this->_pModels.insert({instanceID, {pModel, pPrimitive}});
}

void CesiumMetadataImpl::removeMetadata(int32_t instanceID) {
  auto find = this->_pModels.find(instanceID);
  if (find != this->_pModels.end()) {
    this->_pModels.erase(find);
  }
}

namespace {}

DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>
CesiumForUnityNative::CesiumMetadataImpl::GetFeatures(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata,
    const DotNet::UnityEngine::Transform& transform,
    int triangleIndex) {
  auto find = this->_pModels.find(transform.GetInstanceID());
  if (find == this->_pModels.end()) {
    return DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(0);
  }
  const Model* pModel = find->second.first;
  const MeshPrimitive* pPrimitive = find->second.second;

  int64_t vertexCount = 0;
  auto positionIt = pPrimitive->attributes.find("POSITION");
  if (positionIt != pPrimitive->attributes.end()) {
    const Accessor* pPositionAccessor =
        Model::getSafe<Accessor>(&pModel->accessors, positionIt->second);
    if (pPositionAccessor) {
      vertexCount = pPositionAccessor->count;
    }
  }

  if (vertexCount == 0) {
    return DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(0);
  }

  const ExtensionModelExtStructuralMetadata* pModelMetadata =
      pModel->getExtension<ExtensionModelExtStructuralMetadata>();
  if (!pModelMetadata || !pModelMetadata->schema) {
    return DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(0);
  }

  const ExtensionExtMeshFeatures* pFeatures =
      pPrimitive->getExtension<ExtensionExtMeshFeatures>();
  if (!pFeatures) {
    return DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(0);
  }

  std::vector<const FeatureId*> featureIdAttributes;
  for (const FeatureId& featureIdSet : pFeatures->featureIds) {
    if (featureIdSet.attribute) {
      featureIdAttributes.push_back(&featureIdSet);
    }
  }

  const int32_t attributesCount =
      static_cast<int32_t>(featureIdAttributes.size());
  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature> features =
      DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(
          attributesCount);

  for (int32_t i = 0; i < attributesCount; i++) {
    DotNet::CesiumForUnity::CesiumFeature feature;
    features.Item(i, feature);

    const CesiumGltf::FeatureId& featureIdSet = *featureIdAttributes[i];
    if (!featureIdSet.propertyTable || *featureIdSet.propertyTable < 0 ||
        *featureIdSet.propertyTable >=
            static_cast<int64_t>(pModelMetadata->propertyTables.size())) {
      continue;
    }

    const auto propertyTable = pModelMetadata->propertyTables[i];

    int64_t vertexIndex = getFirstVertexFromTriangle(
        *pModel,
        *pPrimitive,
        vertexCount,
        triangleIndex);

    if (propertyTable.name) {
      feature.featureTableName(*propertyTable.name);
    }

    auto classIt =
        pModelMetadata->schema->classes.find(propertyTable.classProperty);
    if (classIt != pModelMetadata->schema->classes.end() &&
        classIt->second.name) {
      feature.className(*classIt->second.name);
    }

    int64_t featureID = getFeatureIdFromVertex(
        *pModel,
        *pPrimitive,
        *featureIdSet.attribute,
        vertexIndex);
    if (featureID < 0) {
      feature.properties(DotNet::System::Array1<DotNet::System::String>(0));
      continue;
    }

    CesiumGltf::PropertyTableView propertyTableView(*pModel, propertyTable);
    feature.properties(DotNet::System::Array1<DotNet::System::String>(
        propertyTable.properties.size()));
    int propertyIndex = 0;
    auto& values = feature.NativeImplementation().values;
    propertyTableView.forEachProperty(
        [featureID, &propertyIndex, feature, &values](
            const std::string& propertyName,
            auto property) {
          // The 3D Tiles Next implementation of this class did not account for
          // noData values, so using getRaw() most accurately preserves
          // backwards compatibility.
          auto rawValue = property.getRaw(featureID);
          CesiumMetadataValue value = CesiumMetadataValue();
          using ValueType = decltype(rawValue);

          if constexpr (
              IsMetadataBoolean<ValueType>::value ||
              IsMetadataScalar<ValueType>::value) {
            value.SetObjectValue(rawValue);
          }

          if constexpr (IsMetadataString<ValueType>::value) {
            std::string stringValue = std::string(rawValue);
            value.SetObjectValue(System::String(stringValue));
          }

          if constexpr (IsMetadataArray<ValueType>::value) {
            CesiumPropertyArray array =
                CesiumFeaturesMetadataUtility::makePropertyArray(rawValue);
            value.SetObjectValue(array);
          }

          feature.properties().Item(propertyIndex++, propertyName);
          CesiumFeatureImpl::PropertyInfo propertyInfo{
              property.arrayCount(),
              property.normalized()};
          values.insert({propertyName, {propertyInfo, value}});
        });
  }
  return features;
}

} // namespace CesiumForUnityNative
