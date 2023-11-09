#pragma once

#include <DotNet/CesiumForUnity/CesiumFeature.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/String.h>

#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumModelMetadata;
class CesiumPropertyTable;
} // namespace DotNet::CesiumForUnity

namespace DotNet::UnityEngine {
class GameObject;
class Transform;
} // namespace DotNet::UnityEngine

namespace CesiumGltf {
struct Model;
struct ExtensionModelExtStructuralMetadata;
} // namespace CesiumGltf

namespace CesiumForUnityNative {

class CesiumModelMetadataImpl {
public:
  ~CesiumModelMetadataImpl(){};
  CesiumModelMetadataImpl(
      const DotNet::CesiumForUnity::CesiumModelMetadata& metadata){};
  void JustBeforeDelete(
      const DotNet::CesiumForUnity::CesiumModelMetadata& metadata){};

  void initializeMetadata(
      const CesiumGltf::Model* pModel,
      const CesiumGltf::ExtensionModelExtStructuralMetadata* pExtension);
};
} // namespace CesiumForUnityNative
