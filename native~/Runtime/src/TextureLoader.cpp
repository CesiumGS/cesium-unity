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

namespace {
UnityEngine::TextureFormat
getCompressedPixelFormat(const CesiumGltf::ImageCesium& image) {
  switch (image.compressedPixelFormat) {
  case GpuCompressedPixelFormat::ETC1_RGB:
    return UnityEngine::TextureFormat::ETC_RGB4;
  case GpuCompressedPixelFormat::ETC2_RGBA:
    return UnityEngine::TextureFormat::ETC2_RGBA8;
  case GpuCompressedPixelFormat::BC1_RGB:
    return UnityEngine::TextureFormat::DXT1;
  case GpuCompressedPixelFormat::BC3_RGBA:
    return UnityEngine::TextureFormat::DXT5;
  case GpuCompressedPixelFormat::BC4_R:
    return UnityEngine::TextureFormat::BC4;
  case GpuCompressedPixelFormat::BC5_RG:
    return UnityEngine::TextureFormat::BC5;
  case GpuCompressedPixelFormat::BC7_RGBA:
    return UnityEngine::TextureFormat::BC7;
  case GpuCompressedPixelFormat::ASTC_4x4_RGBA:
    return UnityEngine::TextureFormat::ASTC_4x4;
  case GpuCompressedPixelFormat::PVRTC1_4_RGB:
    return UnityEngine::TextureFormat::PVRTC_RGB4;
  case GpuCompressedPixelFormat::PVRTC1_4_RGBA:
    return UnityEngine::TextureFormat::PVRTC_RGBA4;
  case GpuCompressedPixelFormat::ETC2_EAC_R11:
    return UnityEngine::TextureFormat::EAC_R;
  case GpuCompressedPixelFormat::ETC2_EAC_RG11:
    return UnityEngine::TextureFormat::EAC_RG;
  case GpuCompressedPixelFormat::PVRTC2_4_RGB:
  case GpuCompressedPixelFormat::PVRTC2_4_RGBA:
  default:
    return UnityEngine::TextureFormat::RGBA32;
  }
}

UnityEngine::TextureFormat
getUncompressedPixelFormat(const CesiumGltf::ImageCesium& image) {
  switch (image.channels) {
  case 1:
    return UnityEngine::TextureFormat::R8;
  case 2:
    return UnityEngine::TextureFormat::RG16;
  case 3:
    return UnityEngine::TextureFormat::RGB24;
  case 4:
  default:
    return UnityEngine::TextureFormat::RGBA32;
  }
}

} // namespace

UnityEngine::Texture
TextureLoader::loadTexture(const CesiumGltf::ImageCesium& image) {
  CESIUM_TRACE("TextureLoader::loadTexture");
  std::int32_t mipCount =
      image.mipPositions.empty() ? 1 : std::int32_t(image.mipPositions.size());

  UnityEngine::TextureFormat textureFormat;
  if (image.compressedPixelFormat != GpuCompressedPixelFormat::NONE) {
    textureFormat = getCompressedPixelFormat(image);
  } else {
    textureFormat = getUncompressedPixelFormat(image);
  }

  UnityEngine::Texture2D
      result(image.width, image.height, textureFormat, mipCount, false);
  result.hideFlags(UnityEngine::HideFlags::HideAndDontSave);

  Unity::Collections::NativeArray1<std::uint8_t> textureData =
      result.GetRawTextureData<std::uint8_t>();
  std::uint8_t* pixels = static_cast<std::uint8_t*>(
      Unity::Collections::LowLevel::Unsafe::NativeArrayUnsafeUtility::
          GetUnsafeBufferPointerWithoutChecks(textureData));

  size_t textureLength = textureData.Length();
  assert(textureLength >= image.pixelData.size());

  if (image.mipPositions.empty()) {
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
