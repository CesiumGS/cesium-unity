#include "CesiumMetadataImpl.h"

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/ExtensionMeshPrimitiveExtFeatureMetadata.h>
#include <CesiumGltf/ExtensionModelExtFeatureMetadata.h>
#include <CesiumGltf/MetadataFeatureTableView.h>
#include <CesiumGltf/MetadataPropertyView.h>

#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/UnityEngine/Transform.h>

using namespace CesiumForUnityNative;
using namespace CesiumGltf;

namespace {

int64_t getVertexIndexFromTriangleIndex(
    const CesiumGltf::Model* pModel,
    const CesiumGltf::MeshPrimitive* pPrimitive,
    int64_t triangleIndex) {
  const Accessor& indicesAccessor =
      pModel->getSafe(pModel->accessors, pPrimitive->indices);

  AccessorType indicesView;

  switch (indicesAccessor.componentType) {
  case Accessor::ComponentType::UNSIGNED_BYTE:
    indicesView =
        AccessorView<AccessorTypes::SCALAR<uint8_t>>(*pModel, indicesAccessor);
    break;
  case Accessor::ComponentType::UNSIGNED_SHORT:
    indicesView =
        AccessorView<AccessorTypes::SCALAR<uint16_t>>(*pModel, indicesAccessor);
    break;
  case Accessor::ComponentType::UNSIGNED_INT:
    indicesView =
        AccessorView<AccessorTypes::SCALAR<uint32_t>>(*pModel, indicesAccessor);
    break;
  }

  return std::visit(
      [index = triangleIndex * 3](auto&& value) {
        if (index >= 0 && index < value.size()) {
          return static_cast<int64_t>(value[index].value[0]);
        } else {
          return static_cast<int64_t>(-1);
        }
      },
      indicesView);
}

namespace {

struct FeatureIDFromAccessor {
  int64_t operator()(std::monostate) { return -1; }

  int64_t operator()(
      const CesiumGltf::AccessorView<AccessorTypes::SCALAR<float>>& value) {
    if (vertexIdx >= 0 && vertexIdx < value.size()) {
      return static_cast<int64_t>(glm::round(value[vertexIdx].value[0]));
    } else {
      return static_cast<int64_t>(-1);
    }
  }

  template <typename T>
  int64_t operator()(const CesiumGltf::AccessorView<T>& value) {
    if (vertexIdx >= 0 && vertexIdx < value.size()) {
      return static_cast<int64_t>(value[vertexIdx].value[0]);
    } else {
      return static_cast<int64_t>(-1);
    }
  }

  int64_t vertexIdx;
};

} // namespace

int64_t getFeatureIdFromVertexIndex(
    const CesiumGltf::Model* pModel,
    const CesiumGltf::MeshPrimitive* pPrimitive,
    const std::optional<std::string>& attribute,
    int64_t vertexIndex) {
  if (attribute) {
    auto featureAttribute = pPrimitive->attributes.find(attribute.value());
    if (featureAttribute == pPrimitive->attributes.end()) {
      return -1;
    }
    const CesiumGltf::Accessor* accessor =
        pModel->getSafe<CesiumGltf::Accessor>(
            &pModel->accessors,
            featureAttribute->second);
    if (!accessor) {
      return -1;
    }
    if (accessor->type != CesiumGltf::Accessor::Type::SCALAR) {
      return -1;
    }
    AccessorType featureIDAccessor;
    switch (accessor->componentType) {
    case CesiumGltf::Accessor::ComponentType::BYTE:
      featureIDAccessor =
          CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int8_t>>(
              *pModel,
              *accessor);
      break;
    case CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE:
      featureIDAccessor =
          CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint8_t>>(
              *pModel,
              *accessor);
      break;
    case CesiumGltf::Accessor::ComponentType::SHORT:
      featureIDAccessor =
          CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int16_t>>(
              *pModel,
              *accessor);
      break;
    case CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT:
      featureIDAccessor =
          CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint16_t>>(
              *pModel,
              *accessor);
      break;
    case CesiumGltf::Accessor::ComponentType::FLOAT:
      featureIDAccessor =
          CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<float>>(
              *pModel,
              *accessor);
      break;
    default:
      return 0;
    }

    return std::visit(FeatureIDFromAccessor{vertexIndex}, featureIDAccessor);
  }
  return -1;
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

DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>
CesiumForUnityNative::CesiumMetadataImpl::GetFeatures(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata,
    const DotNet::UnityEngine::Transform& transform,
    int triangleIndex) {
  auto find = this->_pModels.find(transform.GetInstanceID());
  if (find != this->_pModels.end()) {

    const Model* pModel = find->second.first;
    const MeshPrimitive* pPrimitive = find->second.second;

    int64_t vertexIndex =
        getVertexIndexFromTriangleIndex(pModel, pPrimitive, triangleIndex);
    const ExtensionModelExtFeatureMetadata* pModelMetadata =
        pModel->getExtension<ExtensionModelExtFeatureMetadata>();
    const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
        pPrimitive->getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();
    DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature> features =
        DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(
            pMetadata->featureIdAttributes.size());
    for (int i = 0; i < pMetadata->featureIdAttributes.size(); i++) {
      const CesiumGltf::FeatureIDAttribute& featIDAttr =
          pMetadata->featureIdAttributes[i];
      auto find = pModelMetadata->featureTables.find(featIDAttr.featureTable);
      if (find != pModelMetadata->featureTables.end()) {
        DotNet::CesiumForUnity::CesiumFeature feature =
            DotNet::CesiumForUnity::CesiumFeature();
        features.Item(i, feature);
        const std::string& featureTableName = find->first;
        feature.featureTableName(featureTableName);
        int numProperties = find->second.properties.size();
        int64_t featureID = getFeatureIdFromVertexIndex(
            pModel,
            pPrimitive,
            featIDAttr.featureIds.attribute,
            vertexIndex);
        if (find->second.classProperty && pModelMetadata->schema.has_value()) {
          auto classIt =
              pModelMetadata->schema->classes.find(*find->second.classProperty);
          if (classIt != pModelMetadata->schema->classes.end() &&
              classIt->second.name.has_value()) {
            feature.className(*classIt->second.name);
          }
        }
        auto find = pModelMetadata->featureTables.find(featureTableName);
        if (find != pModelMetadata->featureTables.end()) {
          const CesiumGltf::FeatureTable& featureTable = find->second;
          CesiumGltf::MetadataFeatureTableView featureTableView{
              pModel,
              &featureTable};
          feature.properties(DotNet::System::Array1<DotNet::System::String>(
              featureTable.properties.size()));
          auto size = feature.properties().Length();
          auto& nativeProperties = feature.NativeImplementation().properties;
          int index = 0;
          featureTableView.forEachProperty(
              [featureID, &index, feature, &nativeProperties](
                  const std::string& propertyName,
                  auto propertyType) {
                ValueType propertyValue = std::visit(
                    [featureID](auto&& value) {
                      if (featureID >= 0 && featureID < value.size()) {
                        return static_cast<ValueType>(value.get(featureID));
                      } else {
                        return static_cast<ValueType>(0);
                      }
                    },
                    static_cast<CesiumForUnityNative::PropertyType>(
                        propertyType));
                feature.properties().Item(index++, propertyName);
                nativeProperties.insert(
                    {propertyName, {propertyType, propertyValue}});
              });
        }
      }
    }
    return features;
  }
  return DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature>(0);
}
