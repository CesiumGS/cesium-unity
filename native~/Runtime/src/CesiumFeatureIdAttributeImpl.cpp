#pragma once

#include "CesiumFeatureIdAttributeImpl.h"

#include <DotNet/CesiumForUnity/CesiumFeatureIdAttribute.h>
#include <DotNet/CesiumForUnity/CesiumFeatureIdAttributeStatus.h>

using namespace DotNet::CesiumForUnity;

namespace CesiumForUnityNative {
/*static*/ DotNet::CesiumForUnity::CesiumFeatureIdAttribute
CesiumFeatureIdAttributeImpl::CreateAttribute(
    const CesiumGltf::Model& model,
    const CesiumGltf::MeshPrimitive& primitive,
    const int32_t attributeSetIndex) {
  CesiumFeatureIdAttribute attribute;
  auto& accessor = attribute.NativeImplementation()._accessor;
  accessor =
      CesiumGltf::getFeatureIdAccessorView(model, primitive, attributeSetIndex);

  auto accessorStatus = std::visit(CesiumGltf::StatusFromAccessor{}, accessor);
  attribute.status(
      accessorStatus == CesiumGltf::AccessorViewStatus::Valid
          ? CesiumFeatureIdAttributeStatus::Valid
          : CesiumFeatureIdAttributeStatus::ErrorInvalidAccessor);

  return attribute;
}

std::int64_t CesiumFeatureIdAttributeImpl::GetFeatureIdForVertex(
    const CesiumFeatureIdAttribute& featureIdAttribute,
    const int64_t vertexIndex) {
  return std::visit(CesiumGltf::FeatureIdFromAccessor{vertexIndex}, _accessor);
}
} // namespace CesiumForUnityNative
