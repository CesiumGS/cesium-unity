#include "TextureLoader.h"

#include <CesiumGltf/Model.h>

#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/Texture2D.h>
#include <DotNet/UnityEngine/TextureFormat.h>

using namespace CesiumGltf;
using namespace DotNet;

namespace CesiumForUnity {

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
  UnityEngine::Texture2D result(
      imageCesium.width,
      imageCesium.height,
      UnityEngine::TextureFormat::RGBA32,
      false,
      false);

  result.LoadRawTextureData(
      const_cast<void*>(static_cast<const void*>(imageCesium.pixelData.data())),
      imageCesium.pixelData.size());

  result.Apply(true, true);

  return result;
}

} // namespace CesiumForUnity
