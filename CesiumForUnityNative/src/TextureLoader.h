#pragma once

#include "Bindings.h"

namespace CesiumGltf {
struct Model;
struct Texture;
} // namespace CesiumGltf

namespace CesiumForUnity {

class TextureLoader {
public:
  static UnityEngine::Texture loadTexture(
      const CesiumGltf::Model& model,
      int32_t textureIndex);

  static UnityEngine::Texture loadTexture(
      const CesiumGltf::Model& model,
      const CesiumGltf::Texture& texture);
};

} // namespace CesiumForUnity
