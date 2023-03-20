#pragma once

#include <Cesium3DTilesSelection/ITileExcluder.h>

#include <DotNet/CesiumForUnity/Cesium3DTile.h>
#include <DotNet/CesiumForUnity/CesiumTileExcluder.h>

namespace DotNet::CesiumForUnity {
class CesiumGeoreference;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class UnityTileExcluderAdaptor : public Cesium3DTilesSelection::ITileExcluder {
public:
  UnityTileExcluderAdaptor(
      const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);

  bool isValid() const noexcept;

  virtual bool shouldExclude(
      const Cesium3DTilesSelection::Tile& tile) const noexcept override;

private:
  DotNet::CesiumForUnity::CesiumTileExcluder _excluder;
  DotNet::CesiumForUnity::Cesium3DTile _tile;
  bool _isValid;
};

} // namespace CesiumForUnityNative
