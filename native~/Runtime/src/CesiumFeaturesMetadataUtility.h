#pragma once

namespace DotNet::CesiumForUnity {
class CesiumPrimitiveFeatures;
}

namespace DotNet::UnityEngine {
class GameObject;
}

namespace CesiumGltf {
struct Model;
struct MeshPrimitive;
struct ExtensionExtMeshFeatures;
} // namespace CesiumGltf

namespace CesiumForUnityNative {
class CesiumFeaturesMetadataUtility {
public:
  static DotNet::CesiumForUnity::CesiumPrimitiveFeatures AddPrimitiveFeatures(
      const DotNet::UnityEngine::GameObject& primitiveGameObject,
      const CesiumGltf::Model& model,
      const CesiumGltf::MeshPrimitive& primitive,
      const CesiumGltf::ExtensionExtMeshFeatures& extension);
};
} // namespace CesiumForUnityNative
