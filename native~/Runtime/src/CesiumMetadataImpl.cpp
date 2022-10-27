#include "CesiumMetadataImpl.h"

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/ExtensionMeshPrimitiveExtFeatureMetadata.h>
#include <CesiumGltf/ExtensionModelExtFeatureMetadata.h>
#include <CesiumGltf/MetadataFeatureTableView.h>
#include <CesiumGltf/MetadataPropertyView.h>

#include <DotNet/CesiumForUnity/CesiumMetadata.h>
#include <DotNet/CesiumForUnity/MetadataProperty.h>
#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/UnityEngine/Transform.h>

using namespace CesiumForUnityNative;
using namespace CesiumGltf;

void CesiumMetadataImpl::loadMetadata(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata,
    const DotNet::UnityEngine::Transform& transform,
    int triangleIndex,
    DotNet::System::Array1<DotNet::CesiumForUnity::MetadataProperty>
        properties) {
  auto find = this->_pModels.find(&transform);
  if (find != this->_pModels.end()) {

    const Model* pModel = find->second.first;
    const MeshPrimitive* pPrimitive = find->second.second;

    const Accessor& indicesAccessor =
        pModel->getSafe(pModel->accessors, pPrimitive->indices);

    AccessorType indicesView;

    switch (indicesAccessor.componentType) {
    case Accessor::ComponentType::UNSIGNED_BYTE:
      indicesView = AccessorView<AccessorTypes::SCALAR<uint8_t>>(
          *pModel,
          indicesAccessor);
      break;
    case Accessor::ComponentType::UNSIGNED_SHORT:
      indicesView = AccessorView<AccessorTypes::SCALAR<uint16_t>>(
          *pModel,
          indicesAccessor);
      break;
    case Accessor::ComponentType::UNSIGNED_INT:
      indicesView = AccessorView<AccessorTypes::SCALAR<uint32_t>>(
          *pModel,
          indicesAccessor);
      break;
    }

    int64_t vertexIndex = std::visit(
        [index = triangleIndex * 3](auto&& value) {
          if (index >= 0 && index < value.size()) {
            return static_cast<int64_t>(value[index].value[0]);
          } else {
            return static_cast<int64_t>(-1);
          }
        },
        indicesView);

    const ExtensionModelExtFeatureMetadata* pModelMetadata =
        pModel->getExtension<ExtensionModelExtFeatureMetadata>();
    const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
        pPrimitive->getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();

    int propertiesIndex = 0;
    for (const CesiumGltf::FeatureIDAttribute& attribute :
         pMetadata->featureIdAttributes) {
      if (attribute.featureIds.attribute) {
        auto featureAttribute =
            pPrimitive->attributes.find(attribute.featureIds.attribute.value());
        if (featureAttribute == pPrimitive->attributes.end()) {
          continue;
        }
        const CesiumGltf::Accessor* accessor =
            pModel->getSafe<CesiumGltf::Accessor>(
                &pModel->accessors,
                featureAttribute->second);
        if (!accessor) {
          continue;
        }
        if (accessor->type != CesiumGltf::Accessor::Type::SCALAR) {
          continue;
        }

        AccessorType featureIDAccessor;
        switch (accessor->componentType) {
        case CesiumGltf::Accessor::ComponentType::BYTE:
          featureIDAccessor = CesiumGltf::AccessorView<
              CesiumGltf::AccessorTypes::SCALAR<int8_t>>(*pModel, *accessor);
          break;
        case CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE:
          featureIDAccessor = CesiumGltf::AccessorView<
              CesiumGltf::AccessorTypes::SCALAR<uint8_t>>(*pModel, *accessor);
          break;
        case CesiumGltf::Accessor::ComponentType::SHORT:
          featureIDAccessor = CesiumGltf::AccessorView<
              CesiumGltf::AccessorTypes::SCALAR<int16_t>>(*pModel, *accessor);
          break;
        case CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT:
          featureIDAccessor = CesiumGltf::AccessorView<
              CesiumGltf::AccessorTypes::SCALAR<uint16_t>>(*pModel, *accessor);
          break;
        case CesiumGltf::Accessor::ComponentType::FLOAT:
          featureIDAccessor = CesiumGltf::AccessorView<
              CesiumGltf::AccessorTypes::SCALAR<float>>(*pModel, *accessor);
          break;
        default:
          continue;
        }

        int64_t featureID = std::visit(
            [vertexIndex](auto&& value) {
              if (vertexIndex >= 0 && vertexIndex < value.size()) {
                return static_cast<int64_t>(value[vertexIndex].value[0]);
              } else {
                return static_cast<int64_t>(-1);
              }
            },
            featureIDAccessor);

        auto find = pModelMetadata->featureTables.find(attribute.featureTable);
        if (find != pModelMetadata->featureTables.end()) {
          const CesiumGltf::FeatureTable& featureTable = find->second;
          CesiumGltf::MetadataFeatureTableView featureTableView{
              pModel,
              &featureTable};
          featureTableView.forEachProperty(
              [featureID, &propertiesIndex, &properties](
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
                    static_cast<PropertyType>(propertyType));
                DotNet::CesiumForUnity::MetadataProperty property =
                    properties[propertiesIndex++];
                property.NativeImplementation().SetProperty(
                    propertyName,
                    propertyType,
                    propertyValue);
              });
        }
      }
    }
  }
}

void CesiumMetadataImpl::loadMetadata(
    const DotNet::UnityEngine::Transform* pTransform,
    const CesiumGltf::Model* pModel,
    const CesiumGltf::MeshPrimitive* pPrimitive) {
  this->_pModels.insert({pTransform, {pModel, pPrimitive}});
}

int CesiumMetadataImpl::getNumberOfProperties(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata,
    const DotNet::UnityEngine::Transform& transform) {

  int totalNumberOfProperties = 0;

  auto find = this->_pModels.find(&transform);
  if (find != this->_pModels.end()) {
    const CesiumGltf::Model* pModel = find->second.first;
    const CesiumGltf::MeshPrimitive* pPrimitive = find->second.second;
    const ExtensionModelExtFeatureMetadata* pModelMetadata =
        pModel->getExtension<ExtensionModelExtFeatureMetadata>();
    const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
        pPrimitive->getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();
    for (const CesiumGltf::FeatureIDAttribute& attribute :
         pMetadata->featureIdAttributes) {
      auto find = pModelMetadata->featureTables.find(attribute.featureTable);
      if (find != pModelMetadata->featureTables.end()) {
        totalNumberOfProperties += find->second.properties.size();
      }
    }
  }
  return totalNumberOfProperties;
}