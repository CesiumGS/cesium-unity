#pragma once

#include <Cesium3DTilesSelection/ViewUpdateResult.h>

#include <memory>

namespace Oxidize::CesiumForUnity {
class Cesium3DTileset;
}

namespace Cesium3DTilesSelection {
class Tileset;
}

namespace CesiumForUnity {

class Cesium3DTilesetImpl {
public:
  Cesium3DTilesetImpl(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);
  void
  JustBeforeDelete(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);
  void Start(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);
  void Update(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);

private:
  void updateLastViewUpdateResultState(
      const Cesium3DTilesSelection::ViewUpdateResult& currentResult);

  std::unique_ptr<Cesium3DTilesSelection::Tileset> _pTileset;
  Cesium3DTilesSelection::ViewUpdateResult _lastUpdateResult;
};

} // namespace CesiumForUnity
