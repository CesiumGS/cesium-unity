#include "Cesium3DTileset.h"

#include "Bindings.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumUtility/Math.h>

#include <memory>

using namespace Cesium3DTilesSelection;
using namespace CesiumForUnity;
using namespace CesiumGeospatial;
using namespace CesiumUtility;
using namespace UnityEngine;

CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONTENTS_DEFINITION
CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONSTRUCTOR_DEFINITION

void Cesium3DTileset::Start() {
  TilesetOptions options{};
  options.enableFrustumCulling = false;

  this->_lastUpdateResult = ViewUpdateResult();
  this->_pTileset = std::make_unique<Tileset>(
      createTilesetExternals(this->GetGameObject()),
      1,
      "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9."
      "eyJqdGkiOiJjZmUzNjE3MC0wZmUwLTQzODItODMwZC01ZjE1Yzg1N2Y1MDIiLCJpZCI6MjU4"
      "LCJpYXQiOjE1MTczNTg0ODF9.Yv10hy_E1N0Ccc4y23fMlNkBtxiFc852wAfUSwmVUaA",
      options);
}

void Cesium3DTileset::Update() {
  if (!this->_pTileset) {
    return;
  }

  Camera camera = Camera::GetMain();

  double verticalFOV = Math::degreesToRadians(camera.GetFieldOfView());

  std::vector<ViewState> viewStates;
  viewStates.emplace_back(ViewState::create(
      Ellipsoid::WGS84.cartographicToCartesian(
          Cartographic::fromDegrees(-105.25737, 39.736401, 2250.0)),
      glm::dvec3(-1.0, 0.0, 0.0),
      glm::dvec3(0.0, 0.0, 1.0),
      glm::dvec2(camera.GetPixelWidth(), camera.GetPixelHeight()),
      verticalFOV * camera.GetAspect(),
      verticalFOV));

  const ViewUpdateResult& updateResult =
      this->_pTileset->updateView(viewStates);
  this->updateLastViewUpdateResultState(updateResult);
}

void Cesium3DTileset::updateLastViewUpdateResultState(
    const Cesium3DTilesSelection::ViewUpdateResult& currentResult) {
  const ViewUpdateResult& previousResult = this->_lastUpdateResult;
  if (currentResult.tilesToRenderThisFrame.size() !=
          previousResult.tilesToRenderThisFrame.size() ||
      currentResult.tilesLoadingLowPriority !=
          previousResult.tilesLoadingLowPriority ||
      currentResult.tilesLoadingMediumPriority !=
          previousResult.tilesLoadingMediumPriority ||
      currentResult.tilesLoadingHighPriority !=
          previousResult.tilesLoadingHighPriority ||
      currentResult.tilesVisited != previousResult.tilesVisited ||
      currentResult.culledTilesVisited != previousResult.culledTilesVisited ||
      currentResult.tilesCulled != previousResult.tilesCulled ||
      currentResult.maxDepthVisited != previousResult.maxDepthVisited) {
    SPDLOG_LOGGER_INFO(
        this->_pTileset->getExternals().pLogger,
        "{0}: Visited {1}, Culled Visited {2}, Rendered {3}, Culled {4}, Max "
        "Depth Visited {5}, Loading-Low {6}, Loading-Medium {7}, Loading-High "
        "{8}",
        "TODO", // this->GetName().,
        currentResult.tilesVisited,
        currentResult.culledTilesVisited,
        currentResult.tilesToRenderThisFrame.size(),
        currentResult.tilesCulled,
        currentResult.maxDepthVisited,
        currentResult.tilesLoadingLowPriority,
        currentResult.tilesLoadingMediumPriority,
        currentResult.tilesLoadingHighPriority);
  }

  this->_lastUpdateResult = currentResult;
}
