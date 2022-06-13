#pragma once

#include "Bindings.h"

#include <Cesium3DTilesSelection/ViewUpdateResult.h>

#include <memory>

namespace Cesium3DTilesSelection {
class Tileset;
class ViewUpdateResult;
} // namespace Cesium3DTilesSelection

namespace CesiumForUnity {

class Cesium3DTileset : public BaseCesium3DTileset {
public:
  CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONTENTS_DECLARATION
  CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONSTRUCTOR_DECLARATION
  void Start() override;
  void Update() override;

private:
  void updateLastViewUpdateResultState(
      const Cesium3DTilesSelection::ViewUpdateResult& currentResult);

  std::unique_ptr<Cesium3DTilesSelection::Tileset> _pTileset;
  Cesium3DTilesSelection::ViewUpdateResult _lastUpdateResult;
};

} // namespace CesiumForUnity
