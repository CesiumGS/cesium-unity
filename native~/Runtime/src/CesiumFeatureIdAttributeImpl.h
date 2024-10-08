#pragma once

#include "CesiumImpl.h"

#include <CesiumGltf/AccessorUtility.h>

namespace DotNet::CesiumForUnity {
class CesiumFeatureIdAttribute;
} // namespace DotNet::CesiumForUnity

namespace CesiumGltf {
struct Model;
struct MeshPrimitive;
} // namespace CesiumGltf

namespace CesiumForUnityNative {
class CesiumFeatureIdAttributeImpl
    : public CesiumImpl<CesiumFeatureIdAttributeImpl> {
public:
  CesiumFeatureIdAttributeImpl(
      const DotNet::CesiumForUnity::CesiumFeatureIdAttribute&
          featureIdAttribute);
  ~CesiumFeatureIdAttributeImpl();

  static DotNet::CesiumForUnity::CesiumFeatureIdAttribute CreateAttribute(
      const CesiumGltf::Model& model,
      const CesiumGltf::MeshPrimitive& primitive,
      const int64_t featureCount,
      const int32_t attributeSetIndex);

  std::int64_t GetFeatureIdForVertex(
      const DotNet::CesiumForUnity::CesiumFeatureIdAttribute&
          featureIdAttribute,
      const int64_t vertexIndex);

private:
  CesiumGltf::FeatureIdAccessorType _accessor;
};
} // namespace CesiumForUnityNative
