#include "CesiumMetadataImpl.h"

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
    const std::string& attribute,
    int64_t vertexIndex) {
  auto featureIdAttribute = primitive.attributes.find(attribute);
  if (featureIdAttribute == primitive.attributes.end()) {
    return -1;
  }

  CesiumGltf::FeatureIdAccessorType featureIDAccessor =
      getFeatureIdAccessorView(model, primitive, featureIdAttribute->second);

  return std::visit(
      CesiumGltf::FeatureIdFromAccessor{vertexIndex},
      featureIDAccessor);
}

} // namespace

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
    const CesiumGltf::FeatureId& featureIdSet = *featureIdAttributes[i];
    if (!featureIdSet.propertyTable || *featureIdSet.propertyTable < 0 ||
        *featureIdSet.propertyTable >=
            static_cast<int64_t>(pModelMetadata->propertyTables.size())) {
      continue;
    }

    const auto propertyTable = pModelMetadata->propertyTables[i];
    DotNet::CesiumForUnity::CesiumFeature feature;
    features.Item(i, feature);

    int64_t vertexIndex = getFirstVertexFromTriangle(
        *pModel,
        *pPrimitive,
        vertexCount,
        triangleIndex);

    const std::string featureIdAttributeName =
        "_FEATURE_ID_" + *featureIdSet.attribute;
    int64_t featureID = getFeatureIdFromVertex(
        *pModel,
        *pPrimitive,
        featureIdAttributeName,
        vertexIndex);

    const std::string& featureTableName = propertyTable.name.value_or("");
    feature.featureTableName(featureTableName);

    auto classIt =
        pModelMetadata->schema->classes.find(propertyTable.classProperty);
    if (classIt != pModelMetadata->schema->classes.end() &&
        classIt->second.name.has_value()) {
      feature.className(*classIt->second.name);
    }

    CesiumGltf::PropertyTableView propertyTableView(*pModel, propertyTable);
    feature.properties(DotNet::System::Array1<DotNet::System::String>(
        propertyTable.properties.size()));
    auto size = feature.properties().Length();
    auto& values = feature.NativeImplementation().values;
    int index = 0;
    propertyTableView.forEachProperty([featureID, &index, feature, &values](
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
        value.SetObjectValue(System::String(std::string(rawValue)));
      }

      feature.properties().Item(index++, propertyName);
      CesiumFeatureImpl::PropertyInfo propertyInfo{
          property.arrayCount(),
          property.normalized()};
      values.insert({propertyName, {propertyInfo, value}});
    });
  }
  return features;
}

} // namespace CesiumForUnityNative
