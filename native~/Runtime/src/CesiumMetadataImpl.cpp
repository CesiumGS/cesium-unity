#include "CesiumMetadataImpl.h"

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/Class.h>
#include <CesiumGltf/ExtensionMeshPrimitiveExtFeatureMetadata.h>
#include <CesiumGltf/ExtensionModelExtFeatureMetadata.h>
#include <CesiumGltf/MetadataFeatureTableView.h>
#include <CesiumGltf/MetadataPropertyView.h>

#include <DotNet/CesiumForUnity/FeatureReference.h>
#include <DotNet/CesiumForUnity/CesiumMetadata.h>
#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/UnityEngine/Transform.h>

using namespace CesiumForUnityNative;
using namespace CesiumGltf;

namespace{

  int64_t getVertexIndexFromTriangleIndex(const CesiumGltf::Model* pModel, const CesiumGltf::MeshPrimitive* pPrimitive, int64_t triangleIndex){
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

  int64_t getFeatureIdFromVertexIndex(const CesiumGltf::Model* pModel, const CesiumGltf::MeshPrimitive* pPrimitive,
          const std::optional<std::string>& attribute, int64_t vertexIndex){
      if (attribute){
        auto featureAttribute =
            pPrimitive->attributes.find(attribute.value());
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
          return 0;
        }
        return std::visit(
            [vertexIndex](auto&& value) {
              if (vertexIndex >= 0 && vertexIndex < value.size()) {
                return static_cast<int64_t>(value[vertexIndex].value[0]);
              } else {
                return static_cast<int64_t>(-1);
              }
            },
            featureIDAccessor);
    }
    return -1;
  }

  void getProperties(const CesiumGltf::Model* pModel, const ExtensionModelExtFeatureMetadata* pModelMetadata,
          const std::string& featureTable, int64_t featureID, DotNet::System::Array1<DotNet::CesiumForUnity::MetadataProperty> properties){
      auto find = pModelMetadata->featureTables.find(featureTable);
      if (find != pModelMetadata->featureTables.end()) {
      const CesiumGltf::FeatureTable& featureTable = find->second;
      CesiumGltf::MetadataFeatureTableView featureTableView{
          pModel,
          &featureTable};
      int propertiesIndex = 0;
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
                static_cast<CesiumForUnityNative::PropertyType>(propertyType));
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

int CesiumForUnityNative::CesiumMetadataImpl::getNumberOfFeatures(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata,
    const DotNet::UnityEngine::Transform& transform) {
  auto find = this->_pModels.find(transform.GetInstanceID());
  if (find != this->_pModels.end()) {
    const CesiumGltf::MeshPrimitive* pPrimitive = find->second.second;
    const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
        pPrimitive->getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();
    return pMetadata->featureIdAttributes.size();
  }
  return 0;
}

void CesiumForUnityNative::CesiumMetadataImpl::getFeatureReferences(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform,
      int triangleIndex,
      DotNet::System::Array1<DotNet::CesiumForUnity::FeatureReference> references) {
  auto find = this->_pModels.find(transform.GetInstanceID());
  if (find != this->_pModels.end()) {

    const Model* pModel = find->second.first;
    const MeshPrimitive* pPrimitive = find->second.second;

    int64_t vertexIndex = getVertexIndexFromTriangleIndex(pModel, pPrimitive, triangleIndex);
    const ExtensionModelExtFeatureMetadata* pModelMetadata =
        pModel->getExtension<ExtensionModelExtFeatureMetadata>();
    const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
        pPrimitive->getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();

    int referenceIndex = 0;
    for(auto it = pMetadata->featureIdAttributes.begin(); it != pMetadata->featureIdAttributes.end(); it++, referenceIndex++){
      DotNet::CesiumForUnity::FeatureReference reference = references[referenceIndex];
      auto find = pModelMetadata->featureTables.find(it->featureTable);
      if (find != pModelMetadata->featureTables.end()) {
          reference.featureTable(find->first);
          reference.numProperties(find->second.properties.size());
          reference.featureID(getFeatureIdFromVertexIndex(pModel, pPrimitive, it->featureIds.attribute, vertexIndex));
          if(find->second.classProperty && pModelMetadata->schema.has_value())
          {
              auto classIt = pModelMetadata->schema->classes.find(*find->second.classProperty);
              if(classIt != pModelMetadata->schema->classes.end() && classIt->second.name.has_value()){
                reference.className(*classIt->second.name);
              }
          }
      }
    }
  }
}

void CesiumForUnityNative::CesiumMetadataImpl::getProperties(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform,
      DotNet::CesiumForUnity::FeatureReference reference,
      DotNet::System::Array1<DotNet::CesiumForUnity::MetadataProperty>
          properties){
  auto find = this->_pModels.find(transform.GetInstanceID());
  if (find != this->_pModels.end()) {
    const Model* pModel = find->second.first;
    const ExtensionModelExtFeatureMetadata* pModelMetadata =
        pModel->getExtension<ExtensionModelExtFeatureMetadata>();
    ::getProperties(pModel, pModelMetadata, reference.featureTable().ToStlString(), reference.featureID(), properties);
  }
}