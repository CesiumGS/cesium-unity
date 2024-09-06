#pragma once

#include "CesiumImpl.h"

#include <CesiumGltf/AccessorUtility.h>
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

class CesiumMetadataImpl : public CesiumImpl<CesiumMetadataImpl> {
public:
  CesiumMetadataImpl(const DotNet::CesiumForUnity::CesiumMetadata& metadata);
  ~CesiumMetadataImpl();

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
};
} // namespace CesiumForUnityNative
