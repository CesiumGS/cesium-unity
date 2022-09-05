#include "Cesium3DTilesetImpl.h"

#include "CameraManager.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/IonRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/NativeCoroutine.h>
#include <DotNet/System/Collections/IEnumerator.h>
#include <DotNet/System/Func2.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEditor/CallbackFunction.h>
#include <DotNet/UnityEditor/EditorApplication.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Coroutine.h>
#include <DotNet/UnityEngine/GameObject.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

Cesium3DTilesetImpl::Cesium3DTilesetImpl(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset)
    : _pTileset(),
      _lastUpdateResult(),
      _updateInEditorCallback(nullptr),
      _destroyTilesetOnNextUpdate(false) {}

Cesium3DTilesetImpl::~Cesium3DTilesetImpl() {}

void Cesium3DTilesetImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->OnDisable(tileset);
}

void Cesium3DTilesetImpl::Start(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {}

void Cesium3DTilesetImpl::Update(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  assert(tileset.enabled());

  if (this->_destroyTilesetOnNextUpdate) {
    this->_destroyTilesetOnNextUpdate = false;
    this->DestroyTileset(tileset);
  }

  if (!this->_pTileset) {
    this->LoadTileset(tileset);
    if (!this->_pTileset)
      return;
  }

  std::vector<ViewState> viewStates =
      CameraManager::getAllCameras(tileset.gameObject());

  const ViewUpdateResult& updateResult =
      this->_pTileset->updateView(viewStates);
  this->updateLastViewUpdateResultState(tileset, updateResult);

  for (auto pTile : updateResult.tilesFadingOut) {
    if (pTile->getState() != TileLoadState::Done) {
      continue;
    }

    const Cesium3DTilesSelection::TileContent& content = pTile->getContent();
    const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
        content.getRenderContent();
    if (pRenderContent) {
      UnityEngine::GameObject* pTileGO = static_cast<UnityEngine::GameObject*>(
          pRenderContent->getRenderResources());
      if (pTileGO) {
        pTileGO->SetActive(false);
      }
    }
  }

  for (auto pTile : updateResult.tilesToRenderThisFrame) {
    if (pTile->getState() != TileLoadState::Done) {
      continue;
    }

    const Cesium3DTilesSelection::TileContent& content = pTile->getContent();
    const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
        content.getRenderContent();
    if (pRenderContent) {
      UnityEngine::GameObject* pTileGO = static_cast<UnityEngine::GameObject*>(
          pRenderContent->getRenderResources());
      if (pTileGO) {
        pTileGO->SetActive(true);
      }
    }
  }
}

void Cesium3DTilesetImpl::OnValidate(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  // Unity does not allow us to destroy GameObjects and MonoBehaviours in this
  // callback. So instead mark it to happen later.
  this->_destroyTilesetOnNextUpdate = true;
}

void Cesium3DTilesetImpl::OnEnable(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  // In the Editor, Update will only be called when something
  // changes. We need to call it continuously to allow tiles to
  // load.
  if (UnityEngine::Application::isEditor() &&
      !UnityEditor::EditorApplication::isPlaying()) {
    this->_updateInEditorCallback = UnityEditor::CallbackFunction(
        [this, tileset]() { this->Update(tileset); });
    UnityEditor::EditorApplication::update(
        UnityEditor::EditorApplication::update() +
        this->_updateInEditorCallback);
  }
}

void Cesium3DTilesetImpl::OnDisable(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_updateInEditorCallback != nullptr) {
    UnityEditor::EditorApplication::update(
        UnityEditor::EditorApplication::update() -
        this->_updateInEditorCallback);
    this->_updateInEditorCallback = nullptr;
  }
  this->DestroyTileset(tileset);
}

void Cesium3DTilesetImpl::RecreateTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->DestroyTileset(tileset);
}

Tileset* Cesium3DTilesetImpl::getTileset() { return this->_pTileset.get(); }

const Tileset* Cesium3DTilesetImpl::getTileset() const {
  return this->_pTileset.get();
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
  // Remove any existing raster overlays
  // TODO: support more than one
  CesiumForUnity::CesiumRasterOverlay overlay =
      tileset.gameObject().GetComponent<CesiumForUnity::CesiumRasterOverlay>();
  if (overlay != nullptr) {
    overlay.RemoveFromTileset();
  }

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

  // Add any overlay components
  // TODO: support more than one
  CesiumForUnity::CesiumRasterOverlay overlay =
      tileset.gameObject().GetComponent<CesiumForUnity::CesiumRasterOverlay>();
  if (overlay != nullptr) {
    overlay.AddToTileset();
  }
}

} // namespace CesiumForUnityNative
