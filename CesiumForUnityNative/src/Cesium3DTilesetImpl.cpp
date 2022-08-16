#include "Cesium3DTilesetImpl.h"

#include "CameraManager.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/GameObject.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnity {

Cesium3DTilesetImpl::Cesium3DTilesetImpl(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset)
    : _pTileset(), _lastUpdateResult() {}

Cesium3DTilesetImpl::~Cesium3DTilesetImpl() {}

void Cesium3DTilesetImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {}

void Cesium3DTilesetImpl::Start(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {}

void Cesium3DTilesetImpl::Update(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (!this->_pTileset) {
    this->LoadTileset(tileset);
  }

  std::vector<ViewState> viewStates =
      CameraManager::getAllCameras(tileset.gameObject());

  const ViewUpdateResult& updateResult =
      this->_pTileset->updateView(viewStates);
  this->updateLastViewUpdateResultState(tileset, updateResult);

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

void Cesium3DTilesetImpl::OnValidate(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->DestroyTileset(tileset);
}

void Cesium3DTilesetImpl::RecreateTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->DestroyTileset(tileset);
}

void Cesium3DTilesetImpl::updateLastViewUpdateResultState(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
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
        tileset.gameObject().name().ToStlString(),
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

void Cesium3DTilesetImpl::DestroyTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->_pTileset.reset();
}

void Cesium3DTilesetImpl::LoadTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  TilesetOptions options{};

  this->_lastUpdateResult = ViewUpdateResult();
  this->_pTileset = std::make_unique<Tileset>(
      createTilesetExternals(tileset.gameObject()),
      tileset.ionAssetID(),
      tileset.ionAccessToken().ToStlString(),
      options);
}

} // namespace CesiumForUnity
