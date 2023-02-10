#pragma once

#include <CesiumGltf/AccessorView.h>
#include <CesiumGltf/Model.h>

#include <DotNet/CesiumForUnity/CesiumFeature.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/String.h>

#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumMetadata;
class CesiumFeature;
} // namespace DotNet::CesiumForUnity

namespace DotNet::UnityEngine {
class GameObject;
class Transform;
} // namespace DotNet::UnityEngine

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

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumFeature> GetFeatures(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata,
      const DotNet::UnityEngine::Transform& transform,
      int triangleIndex);

private:
  std::unordered_map<
      int32_t,
      std::pair<const CesiumGltf::Model*, const CesiumGltf::MeshPrimitive*>>
      _pModels;

  using FeatureTable = std::unordered_map<std::string, PropertyType>;

  using FeatureIDAttribute = std::pair<std::string, AccessorType>;
};
} // namespace CesiumForUnityNative
