#pragma once

#include <CesiumGltf/AccessorUtility.h>
#include <CesiumGltf/FeatureIdTextureView.h>
#include <CesiumUtility/ReferenceCounted.h>

namespace DotNet::CesiumForUnity {
class CesiumFeatureIdTexture;
} // namespace DotNet::CesiumForUnity

namespace DotNet::UnityEngine {
class Vector2;
class RaycastHit;
} // namespace DotNet::UnityEngine

namespace CesiumGltf {
struct Model;
struct MeshPrimitive;
struct FeatureIdTexture;
} // namespace CesiumGltf

namespace CesiumForUnityNative {
class CesiumFeatureIdTextureImpl
    : public CesiumUtility::ReferenceCountedThreadSafe<
          CesiumFeatureIdTextureImpl> {
public:
  CesiumFeatureIdTextureImpl(
      const DotNet::CesiumForUnity::CesiumFeatureIdTexture& featureIdTexture);
  ~CesiumFeatureIdTextureImpl();

  static DotNet::CesiumForUnity::CesiumFeatureIdTexture CreateTexture(
      const CesiumGltf::Model& model,
      const CesiumGltf::MeshPrimitive& primitive,
      const int64_t featureCount,
      const CesiumGltf::FeatureIdTexture& featureIdTexture);

  std::int64_t GetFeatureIdForUV(
      const DotNet::CesiumForUnity::CesiumFeatureIdTexture& featureIdTexture,
      const DotNet::UnityEngine::Vector2& uv);

  std::int64_t GetFeatureIdForVertex(
      const DotNet::CesiumForUnity::CesiumFeatureIdTexture& featureIdTexture,
      const int64_t vertexIndex);

  std::int64_t GetFeatureIdFromRaycastHit(
      const DotNet::CesiumForUnity::CesiumFeatureIdTexture& featureIdTexture,
      const DotNet::UnityEngine::RaycastHit& hitInfo);

private:
  CesiumGltf::FeatureIdTextureView _featureIdTextureView;
  CesiumGltf::TexCoordAccessorType _texCoordAccessor;
  CesiumGltf::IndexAccessorType _indexAccessor;
  CesiumGltf::PositionAccessorType _positionAccessor;
  int32_t _primitiveMode;
};
} // namespace CesiumForUnityNative
