#pragma once

#include <cstdint>

namespace CesiumGltf {
struct Model;
struct Texture;
struct ImageCesium;
} // namespace CesiumGltf

namespace DotNet::UnityEngine {
class Texture;
}

namespace CesiumForUnityNative {

class TextureLoader {
public:
  static ::DotNet::UnityEngine::Texture
  loadTexture(const CesiumGltf::ImageCesium& image);

  static ::DotNet::UnityEngine::Texture
  loadTexture(const CesiumGltf::Model& model, std::int32_t textureIndex);

  static ::DotNet::UnityEngine::Texture loadTexture(
      const CesiumGltf::Model& model,
      const CesiumGltf::Texture& texture);
};

} // namespace CesiumForUnityNative
