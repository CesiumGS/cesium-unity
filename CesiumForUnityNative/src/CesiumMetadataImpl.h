#pragma once

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

private:
};
} // namespace CesiumForUnityNative
