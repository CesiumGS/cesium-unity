#pragma once

#include "UnityPrepareRendererResources.h"

namespace DotNet::CesiumForUnity {
class CesiumMaterialVariants;
} // namespace DotNet::CesiumForUnity

namespace DotNet::UnityEngine {
class GameObject;
class Material;
} // namespace DotNet::UnityEngine

namespace CesiumGltf {
struct Model;
struct MeshPrimitive;
} // namespace CesiumGltf

namespace CesiumForUnityNative {

class CesiumMaterialVariantsUtility {
public:
  /**
   * @brief Adds a CesiumMaterialVariants component to a primitive GameObject
   * if the primitive has the KHR_materials_variants extension.
   *
   * This function handles all extension parsing internally, following the same
   * pattern as CesiumFeaturesMetadataUtility.
   *
   * @param primitiveGameObject The primitive GameObject to add the component to.
   * @param model The glTF model containing the materials and variant definitions.
   * @param primitive The glTF primitive (checked for KHR_materials_variants extension).
   * @param primitiveInfo Information about the primitive, including UV mappings.
   * @param defaultMaterial The default Unity material for this primitive.
   * @param opaqueMaterial The tileset's base opaque material to use for variant materials.
   * @param materialProperties Material properties for creating variant materials.
   * @return The CesiumMaterialVariants component, or null if no variants exist.
   */
  static DotNet::CesiumForUnity::CesiumMaterialVariants addMaterialVariants(
      const DotNet::UnityEngine::GameObject& primitiveGameObject,
      const CesiumGltf::Model& model,
      const CesiumGltf::MeshPrimitive& primitive,
      const CesiumPrimitiveInfo& primitiveInfo,
      const DotNet::UnityEngine::Material& defaultMaterial,
      const DotNet::UnityEngine::Material& opaqueMaterial,
      const TilesetMaterialProperties& materialProperties) noexcept;
};

} // namespace CesiumForUnityNative
