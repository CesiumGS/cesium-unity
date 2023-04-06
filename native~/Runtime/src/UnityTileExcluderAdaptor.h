#pragma once

#include <Cesium3DTilesSelection/ITileExcluder.h>

#include <DotNet/CesiumForUnity/Cesium3DTile.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumTileExcluder.h>
#include <DotNet/UnityEngine/Transform.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class UnityTileExcluderAdaptor : public Cesium3DTilesSelection::ITileExcluder {
public:
  UnityTileExcluderAdaptor(
      const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);

  virtual void startNewFrame() noexcept override;
  virtual bool shouldExclude(
      const Cesium3DTilesSelection::Tile& tile) const noexcept override;

  const DotNet::CesiumForUnity::CesiumTileExcluder& getExcluder() noexcept {
    return this->_excluder;
  }

private:
  DotNet::CesiumForUnity::Cesium3DTile _tile;
  DotNet::CesiumForUnity::CesiumGeoreference _georeference;
  DotNet::CesiumForUnity::CesiumTileExcluder _excluder;
  DotNet::UnityEngine::Transform _excluderTransform;
  DotNet::UnityEngine::Transform _tilesetTransform;
  bool _isValid;
};

} // namespace CesiumForUnityNative
