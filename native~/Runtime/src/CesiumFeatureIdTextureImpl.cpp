#include "CesiumFeatureIdTextureImpl.h"

#include <CesiumGeometry/IntersectionTests.h>
#include <CesiumGltf/AccessorUtility.h>

#include <DotNet/CesiumForUnity/CesiumFeatureIdTexture.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdTextureStatus.h>
#include <DotNet/CesiumForUnity/CesiumPrimitiveFeatures.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/RaycastHit.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector2.h>
#include <DotNet/UnityEngine/Vector3.h>

using namespace DotNet::CesiumForUnity;

namespace CesiumForUnityNative {
/*static*/ DotNet::CesiumForUnity::CesiumFeatureIdTexture
CesiumFeatureIdTextureImpl::CreateTexture(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    const int64_t featureCount,
    const CesiumGltf::FeatureIdTexture& featureIdTexture) {
  CesiumGltf::TextureViewOptions options;
  options.applyKhrTextureTransformExtension = true;

  CesiumFeatureIdTexture texture;
  CesiumFeatureIdTextureImpl& textureImpl = texture.NativeImplementation();
  textureImpl._featureIdTextureView =
      CesiumGltf::FeatureIdTextureView(model, featureIdTexture, options);

  switch (textureImpl._featureIdTextureView.status()) {
  case CesiumGltf::FeatureIdTextureViewStatus::Valid:
    texture.status(CesiumFeatureIdTextureStatus::Valid);
    texture.featureCount(featureCount);
    break;
  case CesiumGltf::FeatureIdTextureViewStatus::ErrorInvalidChannels:
    texture.status(CesiumFeatureIdTextureStatus::ErrorInvalidTextureAccess);
    texture.featureCount(0);
    return texture;
  default:
    // Error with the texture or image. The status is already set by the C#
    // constructor.
    texture.featureCount(0);
    return texture;
  }

  textureImpl._texCoordAccessor = CesiumGltf::getTexCoordAccessorView(
      model,
      primitive,
      textureImpl._featureIdTextureView.getTexCoordSetIndex());
  textureImpl._indexAccessor =
      CesiumGltf::getIndexAccessorView(model, primitive);
  textureImpl._positionAccessor =
      CesiumGltf::getPositionAccessorView(model, primitive);
  textureImpl._primitiveMode = primitive.mode;

  return texture;
}

std::int64_t CesiumFeatureIdTextureImpl::GetFeatureIdForUV(
    const CesiumFeatureIdTexture& featureIdTexture,
    const DotNet::UnityEngine::Vector2& uv) {
  return this->_featureIdTextureView.getFeatureID(uv.x, uv.y);
}

std::int64_t CesiumFeatureIdTextureImpl::GetFeatureIdForVertex(
    const CesiumFeatureIdTexture& featureIdTexture,
    const int64_t vertexIndex) {
  const std::optional<glm::dvec2> maybeTexCoords = std::visit(
      CesiumGltf::TexCoordFromAccessor{vertexIndex},
      this->_texCoordAccessor);
  if (!maybeTexCoords) {
    return -1;
  }

  auto texCoords = *maybeTexCoords;
  return this->_featureIdTextureView.getFeatureID(texCoords[0], texCoords[1]);
}

std::int64_t CesiumFeatureIdTextureImpl::GetFeatureIdFromRaycastHit(
    const CesiumFeatureIdTexture& featureIdTexture,
    const DotNet::UnityEngine::RaycastHit& hitInfo) {
  if (hitInfo.transform().GetComponent<CesiumPrimitiveFeatures>() == nullptr) {
    return -1;
  }

  if (this->_positionAccessor.status() !=
      CesiumGltf::AccessorViewStatus::Valid) {
    return -1;
  }

  int64_t vertexCount =
      std::visit(CesiumGltf::CountFromAccessor{}, this->_texCoordAccessor);

  std::array<int64_t, 3> vertexIndices = std::visit(
      CesiumGltf::IndicesForFaceFromAccessor{
          hitInfo.triangleIndex(),
          vertexCount,
          this->_primitiveMode},
      this->_indexAccessor);

  std::array<glm::dvec2, 3> uvs;
  std::array<glm::vec3, 3> positions;

  for (size_t i = 0; i < positions.size(); i++) {
    int64_t index = vertexIndices[i];
    if (index < 0 || index >= vertexCount) {
      return -1;
    }

    auto maybeTexCoord = std::visit(
        CesiumGltf::TexCoordFromAccessor{index},
        this->_texCoordAccessor);
    if (!maybeTexCoord) {
      return -1;
    }
    uvs[i] = *maybeTexCoord;

    CesiumGltf::AccessorTypes::VEC3<float> position =
        this->_positionAccessor[index];
    positions[i] =
        glm::vec3(position.value[0], position.value[1], position.value[2]);
  }

  // The barycentric coordinates in RaycastHit don't align with the positions
  // in the glTF accessor, so we manually compute barycentric coordinates here.
  DotNet::UnityEngine::Vector3 worldPosition = hitInfo.point();
  DotNet::UnityEngine::Matrix4x4 worldToLocal =
      hitInfo.transform().worldToLocalMatrix();
  DotNet::UnityEngine::Vector3 localPosition =
      worldToLocal.MultiplyPoint3x4(worldPosition);

  glm::dvec3 barycentricCoordinates;
  bool foundIntersection = CesiumGeometry::IntersectionTests::pointInTriangle(
      glm::dvec3(localPosition.x, localPosition.y, localPosition.z),
      glm::dvec3(positions[0]),
      glm::dvec3(positions[1]),
      glm::dvec3(positions[2]),
      barycentricCoordinates);
  if (!foundIntersection) {
    return -1;
  }

  glm::dvec2 UV = (barycentricCoordinates[0] * uvs[0]) +
                  (barycentricCoordinates[1] * uvs[1]) +
                  (barycentricCoordinates[2] * uvs[2]);

  return this->_featureIdTextureView.getFeatureID(UV.x, UV.y);
}

} // namespace CesiumForUnityNative
