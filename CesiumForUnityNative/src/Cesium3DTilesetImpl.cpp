#include "Cesium3DTilesetImpl.h"

#include "CameraManager.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/NativeCoroutine.h>
#include <DotNet/System/Collections/IEnumerator.h>
#include <DotNet/System/Func2.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Coroutine.h>
#include <DotNet/UnityEngine/GameObject.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnity {

Cesium3DTilesetImpl::Cesium3DTilesetImpl(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset)
    : _pTileset(), _lastUpdateResult() {}

Cesium3DTilesetImpl::~Cesium3DTilesetImpl() {}

void Cesium3DTilesetImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->DestroyTileset(tileset);
}

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
  if (!tileset.logSelectionStats())
    return;

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
  // Create a coroutine to wait for the tileset's async operations to complete
  // and then destroy it.
  std::shared_ptr<Tileset> pTileset = std::move(this->_pTileset);
  if (!pTileset)
    return;

  tileset.StartCoroutine(
      DotNet::CesiumForUnity::NativeCoroutine(
          System::Func2<System::Object, System::Object>(
              [pTileset, firstTime = true](
                  const System::Object& terminateCoroutine) mutable {
                // Do nothing on the first invocation so that the stack can
                // unwind.
                if (firstTime) {
                  firstTime = false;
                  return System::Object(nullptr);
                }

                if (!pTileset->canBeDestroyedWithoutBlocking()) {
                  // Tileset can't be destroyed yet, keep going.

                  // But first, mark all tiles inactive so that they disappear
                  // immediately.
                  pTileset->forEachLoadedTile([](Tile& tile) {
                    if (tile.getState() != Tile::LoadState::Done) {
                      return;
                    }

                    UnityEngine::GameObject* pTileGO =
                        static_cast<UnityEngine::GameObject*>(
                            tile.getRendererResources());
                    if (pTileGO) {
                      pTileGO->SetActive(false);
                    }
                  });

                  return System::Object(nullptr);
                }

                // It is now safe to destroy the tileset and terminate the
                // coroutine.
                pTileset.reset();
                return terminateCoroutine;
              }))
          .GetEnumerator());
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
