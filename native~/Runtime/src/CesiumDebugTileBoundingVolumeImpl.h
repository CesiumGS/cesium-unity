#pragma once

namespace Cesium3DTilesSelection {
class Tile;
}

namespace DotNet::CesiumForUnity {
class CesiumDebugTileBoundingVolume;
}

namespace CesiumForUnityNative {

class CesiumDebugTileBoundingVolumeImpl {
public:
  CesiumDebugTileBoundingVolumeImpl(
      const DotNet::CesiumForUnity::CesiumDebugTileBoundingVolume& instance);

  void OnEnable(
      const DotNet::CesiumForUnity::CesiumDebugTileBoundingVolume& instance);
  void OnDrawGizmos(
      const DotNet::CesiumForUnity::CesiumDebugTileBoundingVolume& instance);

private:
  Cesium3DTilesSelection::Tile* _pTile;
};

} // namespace CesiumForUnityNative
