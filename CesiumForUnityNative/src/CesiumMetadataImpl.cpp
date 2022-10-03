#include "CesiumMetadataImpl.h"

#include <CesiumGltf/MetadataFeatureTableView.h>
#include <CesiumGltf/MetadataPropertyView.h>
#include <CesiumGltf/AccessorView.h>

#include <DotNet/CesiumForUnity/CesiumMetadata.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Mesh.h>
#include <DotNet/System/Array1.h>

using namespace CesiumForUnityNative;
using namespace CesiumGltf;

namespace {

template <typename T>
void doSomething(const CesiumGltf::MetadataPropertyView<T>& value) {

  auto type = CesiumGltf::TypeToPropertyType<T>::value;
  ElementType elem = value.get(0);
}
} // namespace

CesiumMetadataImpl::~CesiumMetadataImpl() {}

void CesiumMetadataImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata) {}

void CesiumMetadataImpl::loadMetadata(
    const CesiumGltf::Model& model,
    const CesiumGltf::ExtensionModelExtFeatureMetadata& modelMetadata) {
  for (auto kvp : modelMetadata.featureTables) {
    const std::string& featureTableName = kvp.first;
    const CesiumGltf::FeatureTable& featureTable = kvp.second;

    CesiumGltf::MetadataFeatureTableView featureTableView{
        &model,
        &featureTable};

    std::unordered_map<std::string, PropertyType>& myFeatureTable =
        this->_featureTables[featureTableName];

    featureTableView.forEachProperty(
        [&myFeatureTable](const std::string& propertyName, auto propertyValue) {
          std::pair<std::string, PropertyType> p = {propertyName, propertyValue};
          myFeatureTable.insert(p);
        });
  }
  model.forEachPrimitiveInScene(
      -1,
      [this](const Model& gltf,
         const Node& node,
         const Mesh& mesh,
         const MeshPrimitive& primitive,
         const glm::dmat4& transform) {
        const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
            primitive.getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();

        VertexIDAccessorType vertexIDAccessor;

        if (pMetadata) {
          const CesiumGltf::Accessor& indicesAccessor =
              gltf.getSafe(gltf.accessors, primitive.indices);
          switch (indicesAccessor.componentType) {
          case CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE:
            vertexIDAccessor = CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint8_t>>(gltf, indicesAccessor);
            break;
          case CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT:
            vertexIDAccessor = CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint16_t>>(gltf, indicesAccessor);
            break;
          case CesiumGltf::Accessor::ComponentType::UNSIGNED_INT:
            vertexIDAccessor = CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint32_t>>(gltf, indicesAccessor);
            break;
            default:
            break;
          }

          auto& primitiveInfo = this->_featureIDs.emplace_back(std::pair<VertexIDAccessorType, std::vector<std::pair<std::string, FeatureIDAccessorType>>>());
          primitiveInfo.first = vertexIDAccessor;
          auto& pairs = primitiveInfo.second;

          for(const CesiumGltf::FeatureIDAttribute& attribute : pMetadata->featureIdAttributes){
            if(attribute.featureIds.attribute){
              auto featureID = primitive.attributes.find(attribute.featureIds.attribute.value());
              if(featureID == primitive.attributes.end()){
                continue;
              }

              const CesiumGltf::Accessor* accessor = gltf.getSafe<CesiumGltf::Accessor>(&gltf.accessors, featureID->second);
              if(!accessor){
                continue;
              }

             FeatureIDAccessorType featureIDAccessor;
             switch(accessor->componentType){
              case CesiumGltf::Accessor::ComponentType::BYTE:
              featureIDAccessor = CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int8_t>>(gltf, *accessor);
              break;
              case CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE:
                featureIDAccessor = CesiumGltf::AccessorView<
                    CesiumGltf::AccessorTypes::SCALAR<uint8_t>>(gltf, *accessor);
                break;
              case CesiumGltf::Accessor::ComponentType::SHORT:
              featureIDAccessor = CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int16_t>>(gltf, *accessor);
              break;
              case CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT:
                featureIDAccessor = CesiumGltf::AccessorView<
                    CesiumGltf::AccessorTypes::SCALAR<uint16_t>>(gltf, *accessor);
                break;
              case CesiumGltf::Accessor::ComponentType::FLOAT:
                featureIDAccessor = CesiumGltf::AccessorView<
                    CesiumGltf::AccessorTypes::SCALAR<float>>(gltf, *accessor);
                break;
              default:
                break;
             }  

              std::string featureTableName = attribute.featureTable.c_str();
              std::pair<std::string, FeatureIDAccessorType> p = {featureTableName, featureIDAccessor}; 
              pairs.emplace_back(p);
            }
          }
        }
      });
}


    void CesiumMetadataImpl::loadMetadata(const DotNet::CesiumForUnity::CesiumMetadata& metadata, const DotNet::UnityEngine::Transform& transform, int triangleIndex){

      this->_currentMetadataValues.clear();

      DotNet::CesiumForUnity::CesiumMetadata metadataScript = transform.gameObject().GetComponentInParent<DotNet::CesiumForUnity::CesiumMetadata>();

      if(metadataScript != nullptr){
        this->_primitiveIndex = transform.GetSiblingIndex();
        if (_primitiveIndex < _featureIDs.size()){

          const std::pair<VertexIDAccessorType, std::vector<std::pair<std::string, FeatureIDAccessorType>>>& primitiveInfo = _featureIDs[_primitiveIndex];
          const VertexIDAccessorType& indicesAccessor = primitiveInfo.first;
          this->_vertexIndex = std::visit([triangleIndex](auto value){return getValueAtIndex(value, triangleIndex);}, indicesAccessor);
          const auto& pairs = primitiveInfo.second;
          for(const auto& pair : pairs){
            const std::string& featureTableName = pair.first;
            int64_t featureID = std::visit([this](auto value){ return getValueAtIndex(value, this->_vertexIndex);}, pair.second);

            auto find = this->_featureTables.find(featureTableName);
            if(find != this->_featureTables.end()){
              const std::unordered_map<std::string, PropertyType>& featureTable = find->second;
              for(auto kvp : featureTable){
                const std::string& propertyName = kvp.first;
                const PropertyType& propertyType = kvp.second;

                ValueType value = std::visit([featureID](auto value){ return getValueType(value, featureID);}, propertyType);
                std::pair<std::string, ValueType> p = {propertyName, value};
                _currentMetadataValues.emplace_back(p);
              }
            }
          }
        }
      }
    }