#pragma once

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>

namespace CesiumForUnityNative {
struct CreateModelOptions {
  /**
   * Whether to ignore the KHR_materials_unlit extension in the model. If this
   * is true and the extension is present, then flat normals will be generated
   * for the model as it loads.
   */
  bool ignoreKHRMaterialUnlit = false;

  CreateModelOptions() = default;
  CreateModelOptions(
      const DotNet::CesiumForUnity::Cesium3DTileset& tilesetComponent)
      : ignoreKHRMaterialUnlit(tilesetComponent.ignoreKHRMaterialsUnlit()) {}

  CreateModelOptions operator=(const CreateModelOptions& other) {
    this->ignoreKHRMaterialUnlit = other.ignoreKHRMaterialUnlit;

    return *this;
  }
};
} // namespace CesiumForUnityNative