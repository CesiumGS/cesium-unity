#include "TextureLoader.h"

#include <CesiumGltf/Model.h>

#include <DotNet/Unity/Collections/LowLevel/Unsafe/NativeArrayUnsafeUtility.h>
#include <DotNet/Unity/Collections/NativeArray1.h>
#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/Texture2D.h>
#include <DotNet/UnityEngine/TextureFormat.h>

#include <cstring>

using namespace CesiumGltf;
using namespace DotNet;

namespace CesiumForUnityNative {

UnityEngine::Texture
TextureLoader::loadTexture(const CesiumGltf::ImageCesium& image) {
  UnityEngine::Texture2D result(
      image.width,
      image.height,
      UnityEngine::TextureFormat::RGBA32,
      true,
      false);

  Unity::Collections::NativeArray1<std::uint8_t> textureData =
      result.GetRawTextureData<std::uint8_t>();
  std::uint8_t* pixels = static_cast<std::uint8_t*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(textureData));

  assert(textureData.Length() >= image.pixelData.size());

  std::memcpy(pixels, image.pixelData.data(), image.pixelData.size());

  result.Apply(true, true);

  return result;
}

UnityEngine::Texture TextureLoader::loadTexture(
    const CesiumGltf::Model& model,
    std::int32_t textureIndex) {
  const Texture* pTexture = Model::getSafe(&model.textures, textureIndex);
  if (pTexture) {
    return TextureLoader::loadTexture(model, *pTexture);
  } else {
    return UnityEngine::Texture(nullptr);
  }
}

UnityEngine::Texture TextureLoader::loadTexture(
    const CesiumGltf::Model& model,
    const CesiumGltf::Texture& texture) {
  const Image* pImage = Model::getSafe(&model.images, texture.source);
  if (!pImage) {
    return UnityEngine::Texture(nullptr);
  }

  const ImageCesium& imageCesium = pImage->cesium;
  return loadTexture(imageCesium);
}

} // namespace CesiumForUnityNative
