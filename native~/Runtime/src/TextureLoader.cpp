#include "TextureLoader.h"

#include <CesiumGltf/Model.h>
#include <CesiumGltf/Sampler.h>
#include <CesiumUtility/Tracing.h>

#include <DotNet/Unity/Collections/LowLevel/Unsafe/NativeArrayUnsafeUtility.h>
#include <DotNet/Unity/Collections/NativeArray1.h>
#include <DotNet/UnityEngine/FilterMode.h>
#include <DotNet/UnityEngine/Texture.h>
#include <DotNet/UnityEngine/Texture2D.h>
#include <DotNet/UnityEngine/TextureFormat.h>
#include <DotNet/UnityEngine/TextureWrapMode.h>

#include <cstring>

using namespace CesiumGltf;
using namespace DotNet;

namespace CesiumForUnityNative {

UnityEngine::Texture
TextureLoader::loadTexture(const CesiumGltf::ImageCesium& image) {
  CESIUM_TRACE("TextureLoader::loadTexture");
  bool useMipMaps = !image.mipPositions.empty();

  UnityEngine::TextureFormat textureFormat;

  switch (image.compressedPixelFormat) {
  case GpuCompressedPixelFormat::ETC1_RGB:
    textureFormat = UnityEngine::TextureFormat::ETC_RGB4;
    break;
  case GpuCompressedPixelFormat::ETC2_RGBA:
    textureFormat = UnityEngine::TextureFormat::ETC2_RGBA8;
    break;
  case GpuCompressedPixelFormat::BC1_RGB:
    textureFormat = UnityEngine::TextureFormat::DXT1;
    break;
  case GpuCompressedPixelFormat::BC3_RGBA:
    textureFormat = UnityEngine::TextureFormat::DXT5;
    break;
  case GpuCompressedPixelFormat::BC4_R:
    textureFormat = UnityEngine::TextureFormat::BC4;
    break;
  case GpuCompressedPixelFormat::BC5_RG:
    textureFormat = UnityEngine::TextureFormat::BC5;
    break;
  case GpuCompressedPixelFormat::BC7_RGBA:
    textureFormat = UnityEngine::TextureFormat::BC7;
    break;
  case GpuCompressedPixelFormat::ASTC_4x4_RGBA:
    textureFormat = UnityEngine::TextureFormat::ASTC_4x4;
    break;
  case GpuCompressedPixelFormat::PVRTC1_4_RGB:
    textureFormat = UnityEngine::TextureFormat::PVRTC_RGB4;
    break;
  case GpuCompressedPixelFormat::PVRTC1_4_RGBA:
    textureFormat = UnityEngine::TextureFormat::PVRTC_RGBA4;
    break;
  case GpuCompressedPixelFormat::ETC2_EAC_R11:
    textureFormat = UnityEngine::TextureFormat::EAC_R;
    break;
  case GpuCompressedPixelFormat::ETC2_EAC_RG11:
    textureFormat = UnityEngine::TextureFormat::EAC_RG;
    break;
  case GpuCompressedPixelFormat::PVRTC2_4_RGB:
  case GpuCompressedPixelFormat::PVRTC2_4_RGBA:
  default:
    textureFormat = UnityEngine::TextureFormat::RGBA32;
    break;
  }

  UnityEngine::Texture2D
      result(image.width, image.height, textureFormat, useMipMaps, false);
  result.hideFlags(UnityEngine::HideFlags::HideAndDontSave);

  Unity::Collections::NativeArray1<std::uint8_t> textureData =
      result.GetRawTextureData<std::uint8_t>();
  std::uint8_t* pixels = static_cast<std::uint8_t*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(textureData));

  size_t textureLength = textureData.Length();
  assert(textureLength >= image.pixelData.size());

  if (!useMipMaps) {
    // No mipmaps, copy the whole thing and then let Unity generate mipmaps on a
    // worker thread.
    std::memcpy(pixels, image.pixelData.data(), image.pixelData.size());
    result.Apply(false, true);
  } else {
    // Copy the mipmaps explicitly.
    std::uint8_t* pWritePosition = pixels;
    const std::byte* pReadBuffer = image.pixelData.data();

    for (const ImageCesiumMipPosition& mip : image.mipPositions) {
      size_t start = mip.byteOffset;
      size_t end = mip.byteOffset + mip.byteSize;
      if (start >= textureLength || end > textureLength)
        continue; // invalid mip spec, ignore it

      std::memcpy(pWritePosition, pReadBuffer + start, mip.byteSize);
      pWritePosition += mip.byteSize;
    }

    result.Apply(false, true);
  }

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
  UnityEngine::Texture unityTexture = loadTexture(imageCesium);

  const Sampler* pSampler = Model::getSafe(&model.samplers, texture.sampler);
  if (pSampler) {
    switch (pSampler->wrapS) {
    case CesiumGltf::Sampler::WrapS::MIRRORED_REPEAT:
      unityTexture.wrapModeU(UnityEngine::TextureWrapMode::Mirror);
      break;
    case CesiumGltf::Sampler::WrapS::REPEAT:
      unityTexture.wrapModeU(UnityEngine::TextureWrapMode::Repeat);
      break;
    // case CesiumGltf::Sampler::WrapS::CLAMP_TO_EDGE:
    default:
      unityTexture.wrapModeU(UnityEngine::TextureWrapMode::Clamp);
    }

    switch (pSampler->wrapT) {
    case CesiumGltf::Sampler::WrapT::MIRRORED_REPEAT:
      unityTexture.wrapModeV(UnityEngine::TextureWrapMode::Mirror);
      break;
    case CesiumGltf::Sampler::WrapT::REPEAT:
      unityTexture.wrapModeV(UnityEngine::TextureWrapMode::Repeat);
      break;
    // case CesiumGltf::Sampler::WrapT::CLAMP_TO_EDGE:
    default:
      unityTexture.wrapModeV(UnityEngine::TextureWrapMode::Clamp);
    }

    if (!pSampler->minFilter) {
      if (pSampler->magFilter &&
          *pSampler->magFilter == Sampler::MagFilter::NEAREST) {
        unityTexture.filterMode(UnityEngine::FilterMode::Point);
      } else {
        unityTexture.filterMode(UnityEngine::FilterMode::Bilinear);
      }
    } else {
      switch (*pSampler->minFilter) {
      case Sampler::MinFilter::NEAREST:
      case Sampler::MinFilter::NEAREST_MIPMAP_NEAREST:
        unityTexture.filterMode(UnityEngine::FilterMode::Point);
        break;
      case Sampler::MinFilter::LINEAR:
      case Sampler::MinFilter::LINEAR_MIPMAP_NEAREST:
        unityTexture.filterMode(UnityEngine::FilterMode::Bilinear);
        break;
      // case Sampler::MinFilter::LINEAR_MIPMAP_LINEAR:
      // case Sampler::MinFilter::NEAREST_MIPMAP_LINEAR:
      default:
        unityTexture.filterMode(UnityEngine::FilterMode::Trilinear);
      }
    }

    // Use anisotropic filtering if we have mipmaps.
    switch (pSampler->minFilter.value_or(
        CesiumGltf::Sampler::MinFilter::LINEAR_MIPMAP_LINEAR)) {
    case CesiumGltf::Sampler::MinFilter::LINEAR_MIPMAP_LINEAR:
    case CesiumGltf::Sampler::MinFilter::LINEAR_MIPMAP_NEAREST:
    case CesiumGltf::Sampler::MinFilter::NEAREST_MIPMAP_LINEAR:
    case CesiumGltf::Sampler::MinFilter::NEAREST_MIPMAP_NEAREST:
      unityTexture.anisoLevel(16);
    }
  }

  return unityTexture;
}

} // namespace CesiumForUnityNative
