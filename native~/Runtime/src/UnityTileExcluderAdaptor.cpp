#include "UnityTileExcluderAdaptor.h"

#include <DotNet/CesiumForUnity/Cesium3DTile.h>
#include <DotNet/CesiumForUnity/CesiumTileExcluder.h>

#include <cassert>

namespace CesiumForUnityNative {

UnityTileExcluderAdaptor::UnityTileExcluderAdaptor(
    const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference)
    : _excluder(excluder), _tile(excluder._tile()), _isValid(false) {
  if (this->_tile != nullptr) {
    this->_tile._georeference(georeference);
    this->_isValid = true;
  }
}

bool UnityTileExcluderAdaptor::isValid() const noexcept {
  return this->_isValid;
}

bool UnityTileExcluderAdaptor::shouldExclude(
    const Cesium3DTilesSelection::Tile& tile) const noexcept {
  assert(this->_isValid);
  this->_tile._pTile(const_cast<Cesium3DTilesSelection::Tile*>(&tile));
  return this->_excluder.ShouldExclude(this->_tile);
}

} // namespace CesiumForUnityNative
