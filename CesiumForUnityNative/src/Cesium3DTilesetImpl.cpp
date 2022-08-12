#include "Cesium3DTilesetImpl.h"

#include "CameraManager.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/Tileset.h>

#include <Oxidize/CesiumForUnity/Cesium3DTileset.h>
#include <Oxidize/UnityEngine/GameObject.h>

using namespace Cesium3DTilesSelection;
using namespace Oxidize;

namespace CesiumForUnity {

Cesium3DTilesetImpl::Cesium3DTilesetImpl(
    const Oxidize::CesiumForUnity::Cesium3DTileset& tileset)
    : _pTileset(), _lastUpdateResult() {}

Cesium3DTilesetImpl::~Cesium3DTilesetImpl() {}

void Cesium3DTilesetImpl::JustBeforeDelete(
    const Oxidize::CesiumForUnity::Cesium3DTileset& tileset) {}

void Cesium3DTilesetImpl::Start(
    const Oxidize::CesiumForUnity::Cesium3DTileset& tileset) {
  TilesetOptions options{};

  this->_lastUpdateResult = ViewUpdateResult();
  this->_pTileset = std::make_unique<Tileset>(
      createTilesetExternals(tileset.gameObject()),
      69380,
      "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9."
      "eyJqdGkiOiJjZmUzNjE3MC0wZmUwLTQzODItODMwZC01ZjE1Yzg1N2Y1MDIiLCJpZCI6MjU4"
      "LCJpYXQiOjE1MTczNTg0ODF9.Yv10hy_E1N0Ccc4y23fMlNkBtxiFc852wAfUSwmVUaA",
      options);
}

void Cesium3DTilesetImpl::Update(
    const Oxidize::CesiumForUnity::Cesium3DTileset& tileset) {
  if (!this->_pTileset) {
    return;
  }

  std::vector<ViewState> viewStates =
      CameraManager::getAllCameras(tileset.gameObject());

  const ViewUpdateResult& updateResult =
      this->_pTileset->updateView(viewStates);
  this->updateLastViewUpdateResultState(updateResult);

  for (auto pTile : updateResult.tilesToNoLongerRenderThisFrame) {
    if (pTile->getState() != Tile::LoadState::Done) {
      continue;
    }

    UnityEngine::GameObject* pTileGO =
        static_cast<UnityEngine::GameObject*>(pTile->getRendererResources());
    if (pTileGO) {
      pTileGO->SetActive(false);
    }
  }

  for (auto pTile : updateResult.tilesToRenderThisFrame) {
    if (pTile->getState() != Tile::LoadState::Done) {
      continue;
    }

    UnityEngine::GameObject* pTileGO =
        static_cast<UnityEngine::GameObject*>(pTile->getRendererResources());
    if (pTileGO) {
      pTileGO->SetActive(true);
    }
  }
}

void Cesium3DTilesetImpl::updateLastViewUpdateResultState(
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

} // namespace CesiumForUnity
