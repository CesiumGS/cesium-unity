#pragma once

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/Model.h>

#include <DotNet/CesiumForUnity/MetadataProperty.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/GameObject.h>

#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumMetadata;
class FeatureReference;
} // namespace DotNet::CesiumForUnity

namespace DotNet::UnityEngine {
class Transform;
}

namespace CesiumForUnityNative {

using AccessorType = std::variant<
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int8_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint8_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<int16_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint16_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<uint32_t>>,
    CesiumGltf::AccessorView<CesiumGltf::AccessorTypes::SCALAR<float>>>;

class CesiumMetadataImpl {
public:
  ~CesiumMetadataImpl(){};
  CesiumMetadataImpl(const DotNet::CesiumForUnity::CesiumMetadata& metadata){};
  void
  JustBeforeDelete(const DotNet::CesiumForUnity::CesiumMetadata& metadata){};
  void addMetadata(
      int32_t instanceID,
      const CesiumGltf::Model* pModel,
      const CesiumGltf::MeshPrimitive* pPrimitive);

  void removeMetadata(int32_t instanceID);

  int getNumberOfFeatures(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform);

  void getFeatureReferences(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform,
      int triangleIndex,
      DotNet::System::Array1<DotNet::CesiumForUnity::FeatureReference>
          attributes);

  void getProperties(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform,
      DotNet::CesiumForUnity::FeatureReference attribute,
      DotNet::System::Array1<DotNet::CesiumForUnity::MetadataProperty>
          properties);

private:
  std::unordered_map<
      int32_t,
      std::pair<const CesiumGltf::Model*, const CesiumGltf::MeshPrimitive*>>
      _pModels;

  using FeatureTable = std::unordered_map<std::string, PropertyType>;

  using FeatureIDAttribute = std::pair<std::string, AccessorType>;
};
} // namespace CesiumForUnityNative
