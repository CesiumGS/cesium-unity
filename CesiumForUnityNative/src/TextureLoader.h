#pragma once

#include <cstdint>

namespace CesiumGltf {
struct Model;
struct Texture;
} // namespace CesiumGltf

namespace DotNet::UnityEngine {
class Texture;
}

namespace CesiumForUnity {

class TextureLoader {
public:
  static ::DotNet::UnityEngine::Texture
  loadTexture(const CesiumGltf::Model& model, std::int32_t textureIndex);

  static ::DotNet::UnityEngine::Texture loadTexture(
      const CesiumGltf::Model& model,
      const CesiumGltf::Texture& texture);
};

} // namespace CesiumForUnity
