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

  std::unordered_map<std::string, FeatureTable> featureTables;
  std::vector<std::pair<AccessorType, std::vector<FeatureIDAttribute>>>
      featureIDs;

  const DotNet::UnityEngine::GameObject* pParentGameObject =
      &transform.parent().gameObject();

  auto find = _pModels.find(pParentGameObject);

  if (find != _pModels.end()) {
    const CesiumGltf::Model* pModel = find->second;

    const ExtensionModelExtFeatureMetadata* pModelMetadata =
        pModel->getExtension<ExtensionModelExtFeatureMetadata>();
    if (pModelMetadata != nullptr) {
      for (auto kvp : pModelMetadata->featureTables) {
        const std::string& featureTableName = kvp.first;
        const CesiumGltf::FeatureTable& featureTable = kvp.second;

        CesiumGltf::MetadataFeatureTableView featureTableView{
            pModel,
            &featureTable};

        FeatureTable& myFeatureTable = featureTables[featureTableName];

        featureTableView.forEachProperty([&myFeatureTable](
                                             const std::string& propertyName,
                                             auto propertyValue) {
          std::pair<std::string, PropertyType> p = {
              propertyName,
              propertyValue};
          myFeatureTable.insert(p);
        });
      }

      pModel->forEachPrimitiveInScene(
          -1,
          [this, &featureIDs](
              const Model& gltf,
              const Node& node,
              const Mesh& mesh,
              const MeshPrimitive& primitive,
              const glm::dmat4& transform) {
            const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata =
                primitive
                    .getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();

            AccessorType vertexIDAccessor;

            if (pMetadata) {
              const CesiumGltf::Accessor& indicesAccessor =
                  gltf.getSafe(gltf.accessors, primitive.indices);

              switch (indicesAccessor.componentType) {
              case CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE:
                vertexIDAccessor = CesiumGltf::AccessorView<
                    CesiumGltf::AccessorTypes::SCALAR<uint8_t>>(
                    gltf,
                    indicesAccessor);
                break;
              case CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT:
                vertexIDAccessor = CesiumGltf::AccessorView<
                    CesiumGltf::AccessorTypes::SCALAR<uint16_t>>(
                    gltf,
                    indicesAccessor);
                break;
              case CesiumGltf::Accessor::ComponentType::UNSIGNED_INT:
                vertexIDAccessor = CesiumGltf::AccessorView<
                    CesiumGltf::AccessorTypes::SCALAR<uint32_t>>(
                    gltf,
                    indicesAccessor);
                break;
              default:
                return;
              }

              auto& primitiveInfo = featureIDs.emplace_back(
                  std::pair<AccessorType, std::vector<FeatureIDAttribute>>());
              primitiveInfo.first = vertexIDAccessor;
              auto& pairs = primitiveInfo.second;

              for (const CesiumGltf::FeatureIDAttribute& attribute :
                   pMetadata->featureIdAttributes) {
                if (attribute.featureIds.attribute) {
                  auto featureID = primitive.attributes.find(
                      attribute.featureIds.attribute.value());
                  if (featureID == primitive.attributes.end()) {
                    continue;
                  }

                  const CesiumGltf::Accessor* accessor =
                      gltf.getSafe<CesiumGltf::Accessor>(
                          &gltf.accessors,
                          featureID->second);
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
                        CesiumGltf::AccessorTypes::SCALAR<int8_t>>(
                        gltf,
                        *accessor);
                    break;
                  case CesiumGltf::Accessor::ComponentType::UNSIGNED_BYTE:
                    featureIDAccessor = CesiumGltf::AccessorView<
                        CesiumGltf::AccessorTypes::SCALAR<uint8_t>>(
                        gltf,
                        *accessor);
                    break;
                  case CesiumGltf::Accessor::ComponentType::SHORT:
                    featureIDAccessor = CesiumGltf::AccessorView<
                        CesiumGltf::AccessorTypes::SCALAR<int16_t>>(
                        gltf,
                        *accessor);
                    break;
                  case CesiumGltf::Accessor::ComponentType::UNSIGNED_SHORT:
                    featureIDAccessor = CesiumGltf::AccessorView<
                        CesiumGltf::AccessorTypes::SCALAR<uint16_t>>(
                        gltf,
                        *accessor);
                    break;
                  case CesiumGltf::Accessor::ComponentType::FLOAT:
                    featureIDAccessor = CesiumGltf::AccessorView<
                        CesiumGltf::AccessorTypes::SCALAR<float>>(
                        gltf,
                        *accessor);
                    break;
                  default:
                    continue;
                  }

                  FeatureIDAttribute p = {
                      attribute.featureTable,
                      featureIDAccessor};
                  pairs.emplace_back(p);
                }
              }
            }
          });
    }
  }

  DotNet::CesiumForUnity::CesiumMetadata metadataScript =
      transform.gameObject()
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumMetadata>();

  if (metadataScript != nullptr) {
    int32_t primitiveIndex = transform.GetSiblingIndex();
    if (primitiveIndex < featureIDs.size()) {
      const std::pair<AccessorType, std::vector<FeatureIDAttribute>>&
          primitiveInfo = featureIDs[primitiveIndex];
      const AccessorType& indicesAccessor = primitiveInfo.first;
      int64_t vertexIndex = std::visit(
          [index = triangleIndex * 3](auto&& value) {
            if (index >= 0 && index < value.size()) {
              return static_cast<int64_t>(value[index].value[0]);
            } else {
              return static_cast<int64_t>(-1);
            }
          },
          indicesAccessor);

      int propertyIndex = 0;

      const std::vector<FeatureIDAttribute>& pairs = primitiveInfo.second;
      for (const auto& pair : pairs) {
        const std::string& featureTableName = pair.first;
        int64_t featureID = std::visit(
            [vertexIndex](auto&& value) {
              if (vertexIndex >= 0 && vertexIndex < value.size()) {
                return static_cast<int64_t>(value[vertexIndex].value[0]);
              } else {
                return static_cast<int64_t>(-1);
              }
            },
            pair.second);

        auto find = featureTables.find(featureTableName);
        if (find != featureTables.end()) {
          const FeatureTable& featureTable = find->second;
          for (auto kvp : featureTable) {
            const std::string& propertyName = kvp.first;
            const PropertyType& propertyType = kvp.second;
            ValueType propertyValue = std::visit(
                [featureID](auto&& value) {
                  if (featureID >= 0 && featureID < value.size()) {
                    return static_cast<ValueType>(value.get(featureID));
                  } else {
                    return static_cast<ValueType>(0);
                  }
                },
                propertyType);

            auto prop = properties[propertyIndex++];
            prop.NativeImplementation().SetProperty(
                propertyName,
                propertyType,
                propertyValue);
          }
        }
      }
    }
  }
}

void CesiumMetadataImpl::loadMetadata(
    const DotNet::UnityEngine::GameObject* pGameObject,
    const CesiumGltf::Model* pModel) {
  this->_pModels.insert({pGameObject, pModel});
}

int CesiumMetadataImpl::getNumberOfProperties(
    const DotNet::CesiumForUnity::CesiumMetadata& metadata,
    const DotNet::UnityEngine::Transform& transform) {

  int totalNumberOfProperties = 0;

  const DotNet::UnityEngine::GameObject* pParentGameObject =
      &transform.parent().gameObject();

  auto find = _pModels.find(pParentGameObject);

  if (find != _pModels.end()) {
    const CesiumGltf::Model* pModel = find->second;

    const ExtensionModelExtFeatureMetadata* pModelMetadata =
        pModel->getExtension<ExtensionModelExtFeatureMetadata>();
    if (pModelMetadata != nullptr) {

      int32_t primitiveIndex = transform.GetSiblingIndex();

      int primitiveCount = 0;
      const ExtensionMeshPrimitiveExtFeatureMetadata* pMetadata = nullptr;
      pModel->forEachPrimitiveInScene(
          -1,
          [&primitiveCount, primitiveIndex, &pMetadata](
              const Model& gltf,
              const Node& node,
              const Mesh& mesh,
              const MeshPrimitive& primitive,
              const glm::dmat4& transform) {
            if (primitiveCount == primitiveIndex) {
              pMetadata =
                  primitive
                      .getExtension<ExtensionMeshPrimitiveExtFeatureMetadata>();
            } else {
              ++primitiveCount;
            }
          });

      if (pMetadata != nullptr) {
        for (const CesiumGltf::FeatureIDAttribute& attribute :
             pMetadata->featureIdAttributes) {
          if (attribute.featureIds.attribute) {
            auto find =
                pModelMetadata->featureTables.find(attribute.featureTable);
            if (find != pModelMetadata->featureTables.end()) {
              totalNumberOfProperties += find->second.properties.size();
            }
          }
        }
      }
    }
  }
  return totalNumberOfProperties;
}