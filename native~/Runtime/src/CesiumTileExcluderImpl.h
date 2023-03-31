#pragma once

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumTileExcluder;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class CesiumTileExcluderImpl {
public:
  static void AddToTileset(
      const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  static void RemoveFromTileset(
      const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
};

} // namespace CesiumForUnityNative
