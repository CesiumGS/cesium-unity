#pragma once

#include <cstdint>

namespace CesiumGltf {
struct Model;
struct Texture;
} // namespace CesiumGltf

namespace CesiumImage {
struct ImageAsset;
} // namespace CesiumImage

namespace DotNet::UnityEngine {
class Texture;
}

namespace CesiumForUnityNative {

class TextureLoader {
public:
  static ::DotNet::UnityEngine::Texture
  loadTexture(const CesiumImage::ImageAsset& image, bool sRGB);

  static ::DotNet::UnityEngine::Texture loadTexture(
      const CesiumGltf::Model& model,
      std::int32_t textureIndex,
      bool sRGB);

  static ::DotNet::UnityEngine::Texture loadTexture(
      const CesiumGltf::Model& model,
      const CesiumGltf::Texture& texture,
      bool sRGB);
};

} // namespace CesiumForUnityNative
