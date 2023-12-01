#pragma once

#include "CesiumFeatureIdTextureImpl.h"

#include <DotNet/CesiumForUnity/CesiumFeatureIdTexture.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdTextureStatus.h>
#include <DotNet/UnityEngine/RaycastHit.h>
#include <DotNet/UnityEngine/Vector3.h>

using namespace DotNet::CesiumForUnity;

namespace CesiumForUnityNative {
/*static*/ DotNet::CesiumForUnity::CesiumFeatureIdTexture
CesiumFeatureIdTextureImpl::CreateTexture(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    const CesiumGltf::FeatureIdTexture& featureIdTexture) {
  CesiumFeatureIdTexture texture;
  CesiumFeatureIdTextureImpl& textureImpl = texture.NativeImplementation();
  textureImpl._featureIdTextureView =
      CesiumGltf::FeatureIdTextureView(model, featureIdTexture);

  auto& texCoordAccessor = textureImpl._texCoordAccessor;
  texCoordAccessor = CesiumGltf::GetTexCoordAccessorView(
      model,
      primitive,
      featureIdTexture.texCoord);

  switch (textureImpl._featureIdTextureView.status()) {
  case CesiumGltf::FeatureIdTextureViewStatus::Valid:
    texture.status(CesiumFeatureIdTextureStatus::Valid);
    break;
  case CesiumGltf::FeatureIdTextureViewStatus::ErrorInvalidChannels:
    texture.status(CesiumFeatureIdTextureStatus::ErrorInvalidTextureAccess);
    break;
  default:
    // Error with the texture or image. The status is already set by the C#
    // constructor.
    break;
  }

  return texture;
}

std::int64_t CesiumFeatureIdTextureImpl::GetFeatureIdForUV(
    const CesiumFeatureIdTexture& featureIdTexture,
    const glm::dvec2& uv) {
  return featureIdTexture.NativeImplementation()
      ._featureIdTextureView.getFeatureID(uv[0], uv[1]);
}

std::int64_t CesiumFeatureIdTextureImpl::GetFeatureIdForVertex(
    const CesiumFeatureIdTexture& featureIdTexture,
    const int64_t vertexIndex) {
  const std::optional<glm::dvec2> texCoords = std::visit(
      CesiumGltf::TexCoordFromAccessor{vertexIndex},
      featureIdTexture.NativeImplementation()._texCoordAccessor);
  if (!texCoords) {
    return -1;
  }

  return GetFeatureIdForUV(featureIdTexture, *texCoords);
}

std::int64_t CesiumFeatureIdTextureImpl::GetFeatureIdFromRaycastHit(
    const CesiumFeatureIdTexture& featureIdTexture,
    const DotNet::UnityEngine::RaycastHit& hitInfo) {/*
  DotNet::UnityEngine::Vector3 barycentricCoords =
      hitInfo.barycentricCoordinate();*/
  // TODO
  return -1;
}

} // namespace CesiumForUnityNative
