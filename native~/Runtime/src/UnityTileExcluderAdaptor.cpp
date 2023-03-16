#include "UnityTileExcluderAdaptor.h"

#include <DotNet/CesiumForUnity/Cesium3DTile.h>
#include <DotNet/CesiumForUnity/CesiumTileExcluder.h>

namespace CesiumForUnityNative {

UnityTileExcluderAdaptor::UnityTileExcluderAdaptor(
    const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference)
    : _excluder(excluder), _tile(excluder._tile()) {
  this->_tile._georeference(georeference);
}

bool UnityTileExcluderAdaptor::shouldExclude(
    const Cesium3DTilesSelection::Tile& tile) const noexcept {
  this->_tile._pTile(const_cast<Cesium3DTilesSelection::Tile*>(&tile));
  return this->_excluder.ShouldExclude(this->_tile);
}

} // namespace CesiumForUnityNative
