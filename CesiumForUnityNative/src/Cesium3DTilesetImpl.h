#pragma once

#include <Cesium3DTilesSelection/ViewUpdateResult.h>

#include <memory>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
}

namespace Cesium3DTilesSelection {
class Tileset;
}

namespace CesiumForUnity {

class Cesium3DTilesetImpl {
public:
  Cesium3DTilesetImpl(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  ~Cesium3DTilesetImpl();
  void
  JustBeforeDelete(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void Start(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void Update(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  void updateLastViewUpdateResultState(
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
      const Cesium3DTilesSelection::ViewUpdateResult& currentResult);

  std::unique_ptr<Cesium3DTilesSelection::Tileset> _pTileset;
  Cesium3DTilesSelection::ViewUpdateResult _lastUpdateResult;
};

} // namespace CesiumForUnity
