#pragma once

#include <CesiumGltf/PropertyTypeTraits.h>

#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>

namespace DotNet::CesiumForUnity {
class CesiumPrimitiveFeatures;
class CesiumModelMetadata;
} // namespace DotNet::CesiumForUnity

namespace DotNet::UnityEngine {
class GameObject;
}

namespace CesiumGltf {
struct Model;
struct MeshPrimitive;
struct ExtensionExtMeshFeatures;
struct ExtensionModelExtStructuralMetadata;
} // namespace CesiumGltf

namespace CesiumForUnityNative {
class CesiumFeaturesMetadataUtility {
public:
  template <typename T>
  static DotNet::CesiumForUnity::CesiumMetadataValueType
  TypeToMetadataValueType() noexcept;

  static DotNet::CesiumForUnity::CesiumPrimitiveFeatures AddPrimitiveFeatures(
      const DotNet::UnityEngine::GameObject& primitiveGameObject,
      const CesiumGltf::Model& model,
      const CesiumGltf::MeshPrimitive& primitive,
      const CesiumGltf::ExtensionExtMeshFeatures& extension);

  static DotNet::CesiumForUnity::CesiumModelMetadata AddModelMetadata(
      const DotNet::UnityEngine::GameObject& modelGameObject,
      const CesiumGltf::Model& model,
      const CesiumGltf::ExtensionModelExtStructuralMetadata& extension);
};

template <typename T>
DotNet::CesiumForUnity::CesiumMetadataValueType
CesiumFeaturesMetadataUtility::TypeToMetadataValueType() noexcept {
  DotNet::CesiumForUnity::CesiumMetadataType type;
  DotNet::CesiumForUnity::CesiumMetadataComponentType componentType;
  bool isArray;

  if constexpr (CesiumGltf::IsMetadataArray<T>::value) {
    using ArrayType = typename CesiumGltf::MetadataArrayType<T>::type;
    type = DotNet::CesiumForUnity::CesiumMetadataType(
        CesiumGltf::TypeToPropertyType<ArrayType>::value);
    componentType = DotNet::CesiumForUnity::CesiumMetadataComponentType(
        CesiumGltf::TypeToPropertyType<ArrayType>::component);
    isArray = true;
  } else {
    type = DotNet::CesiumForUnity::CesiumMetadataType(
        CesiumGltf::TypeToPropertyType<T>::value);
    componentType = DotNet::CesiumForUnity::CesiumMetadataComponentType(
        CesiumGltf::TypeToPropertyType<T>::component);
    isArray = false;
  }

  return DotNet::CesiumForUnity::CesiumMetadataValueType::Construct(
      type,
      componentType,
      isArray);
}

} // namespace CesiumForUnityNative
