#include "CesiumMaterialVariantsUtility.h"

#include "TextureLoader.h"
#include "TilesetMaterialProperties.h"
#include "UnityPrepareRendererResources.h"

#include <CesiumGltf/ExtensionMeshPrimitiveKhrMaterialsVariants.h>
#include <CesiumGltf/ExtensionModelKhrMaterialsVariants.h>
#include <CesiumGltf/Material.h>
#include <CesiumGltf/Model.h>
#include <CesiumGltf/MeshPrimitive.h>
#include <CesiumUtility/Tracing.h>

#include <unordered_map>

#include <DotNet/CesiumForUnity/CesiumMaterialVariants.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Collections/Generic/Dictionary2.h>
#include <DotNet/System/String.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/HideFlags.h>
#include <DotNet/UnityEngine/Material.h>
#include <DotNet/UnityEngine/Object.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/Vector4.h>
#include <DotNet/UnityEngine/Rendering/CullMode.h>

using namespace DotNet;
using namespace DotNet::System::Collections::Generic;

namespace CesiumForUnityNative {

DotNet::CesiumForUnity::CesiumMaterialVariants
CesiumMaterialVariantsUtility::addMaterialVariants(
    const DotNet::UnityEngine::GameObject& primitiveGameObject,
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    const CesiumPrimitiveInfo& primitiveInfo,
    const DotNet::UnityEngine::Material& defaultMaterial,
    const TilesetMaterialProperties& materialProperties) noexcept {

  // Get the model-level extension (contains variant names)
  const CesiumGltf::ExtensionModelKhrMaterialsVariants* pModelVariants =
      model.getExtension<CesiumGltf::ExtensionModelKhrMaterialsVariants>();
  if (!pModelVariants || pModelVariants->variants.empty()) {
    return nullptr;
  }

  // Get the primitive-level extension (contains variant-to-material mappings)
  const CesiumGltf::ExtensionMeshPrimitiveKhrMaterialsVariants* pPrimitiveVariants =
      primitive.getExtension<CesiumGltf::ExtensionMeshPrimitiveKhrMaterialsVariants>();
  if (!pPrimitiveVariants || pPrimitiveVariants->mappings.empty()) {
    return nullptr;
  }

  // Build the variant-to-material map from the primitive extension
  std::unordered_map<int32_t, int32_t> variantMaterialMap;
  for (const auto& mapping : pPrimitiveVariants->mappings) {
    int32_t materialIndex = mapping.material;
    for (int64_t variantIndex : mapping.variants) {
      if (variantIndex >= 0 && variantIndex <= INT32_MAX) {
        variantMaterialMap[static_cast<int32_t>(variantIndex)] = materialIndex;
      }
    }
  }

  if (variantMaterialMap.empty()) {
    return nullptr;
  }

  // Create the component
  CesiumForUnity::CesiumMaterialVariants variantsComponent =
      primitiveGameObject.AddComponent<CesiumForUnity::CesiumMaterialVariants>();

  if (variantsComponent == nullptr) {
    return nullptr;
  }

  // Set variant names from model extension
  const auto& variants = pModelVariants->variants;
  System::Array1<System::String> variantNames(static_cast<std::int32_t>(variants.size()));
  for (size_t i = 0; i < variants.size(); i++) {
    variantNames.Item(static_cast<std::int32_t>(i), System::String(variants[i].name));
  }
  variantsComponent.variantNames(variantNames);

  variantsComponent.defaultMaterial(defaultMaterial);

  // Load the base material for creating variant materials
  UnityEngine::Material baseMaterial =
      UnityEngine::Resources::Load<UnityEngine::Material>(
          primitiveInfo.isUnlit
              ? System::String("CesiumUnlitTilesetMaterial")
              : System::String("CesiumDefaultTilesetMaterial"));

  if (baseMaterial == nullptr) {
    UnityEngine::Debug::LogWarning(static_cast<System::Object>(System::String(
        "CesiumMaterialVariants: Failed to load base material. Material variants will not be available.")));
    return variantsComponent;
  }

  // Create materials for each variant
  auto variantMaterialsDict = Dictionary2<int32_t, UnityEngine::Material>();

  for (const auto& [variantIndex, materialIndex] : variantMaterialMap) {
    const CesiumGltf::Material* pVariantMaterial =
        CesiumGltf::Model::getSafe(&model.materials, materialIndex);

    if (pVariantMaterial && baseMaterial != nullptr) {
      UnityEngine::Material variantMaterial =
          UnityEngine::Object::Instantiate(baseMaterial);

      if (variantMaterial == nullptr) {
        continue;
      }

      variantMaterial.hideFlags(UnityEngine::HideFlags::HideAndDontSave);

      setGltfMaterialParameterValues(
          model,
          primitiveInfo,
          *pVariantMaterial,
          variantMaterial,
          materialProperties);

      variantMaterialsDict.Add(variantIndex, variantMaterial);
    }
  }

  variantsComponent.variantMaterials(variantMaterialsDict);

  return variantsComponent;
}

} // namespace CesiumForUnityNative
