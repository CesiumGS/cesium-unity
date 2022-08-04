#pragma once

#include <cstdint>

namespace CesiumGltf {
struct Model;
struct Texture;
} // namespace CesiumGltf

namespace Oxidize::UnityEngine {
class Texture;
}

namespace CesiumForUnity {

class TextureLoader {
public:
  static ::Oxidize::UnityEngine::Texture
  loadTexture(const CesiumGltf::Model& model, std::int32_t textureIndex);

  static ::Oxidize::UnityEngine::Texture loadTexture(
      const CesiumGltf::Model& model,
      const CesiumGltf::Texture& texture);
};

} // namespace CesiumForUnity
