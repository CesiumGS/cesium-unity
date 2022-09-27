#pragma once

#include <CesiumGltf/Model.h>
#include <CesiumGltf/ExtensionMeshPrimitiveExtFeatureMetadata.h>

namespace DotNet::CesiumForUnity {
class CesiumMetadata;
}

namespace CesiumForUnityNative {
class CesiumMetadataImpl {
public:
  ~CesiumMetadataImpl();
  CesiumMetadataImpl(
      const DotNet::CesiumForUnity::CesiumMetadata& georeference){};
  void JustBeforeDelete(
      const DotNet::CesiumForUnity::CesiumMetadata& metadata);
  void loadMetadataPrimitive(const CesiumGltf::Model& model, const CesiumGltf::MeshPrimitive& primitive, const CesiumGltf::ExtensionMeshPrimitiveExtFeatureMetadata& metadata);

private:
};
} // namespace CesiumForUnityNative
