#include "Cesium3DTilesetImpl.h"

#include "CameraManager.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/IonRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeospatial/GlobeTransforms.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/Cesium3DTilesetLoadFailureDetails.h>
#include <DotNet/CesiumForUnity/Cesium3DTilesetLoadType.h>
#include <DotNet/CesiumForUnity/CesiumDataSource.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/System/Action.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Camera.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Time.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector3.h>

#include <variant>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/CallbackFunction.h>
#include <DotNet/UnityEditor/EditorApplication.h>
#include <DotNet/UnityEditor/SceneView.h>
#endif

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

Cesium3DTilesetImpl::Cesium3DTilesetImpl(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset)
    : _pTileset(),
      _lastUpdateResult(),
#if UNITY_EDITOR
      _updateInEditorCallback(nullptr),
#endif
      _creditSystem(nullptr),
      _destroyTilesetOnNextUpdate(false) {
}

Cesium3DTilesetImpl::~Cesium3DTilesetImpl() {}

void Cesium3DTilesetImpl::Start(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {}

void Cesium3DTilesetImpl::Update(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  assert(tileset.enabled());

  // If "Suspend Update" is true, return early.
  if (tileset.suspendUpdate()) {
    return;
  }

  if (this->_destroyTilesetOnNextUpdate) {
    this->_destroyTilesetOnNextUpdate = false;
    this->DestroyTileset(tileset);
  }

#if UNITY_EDITOR
  // If "Update In Editor" is false, return early.
  if (UnityEngine::Application::isEditor() &&
      !UnityEditor::EditorApplication::isPlaying() &&
      !tileset.updateInEditor()) {
    return;
  }
#endif

  if (!this->_pTileset) {
    this->LoadTileset(tileset);
    if (!this->_pTileset)
      return;
  }

  std::vector<ViewState> viewStates =
      CameraManager::getAllCameras(tileset.gameObject());

  const ViewUpdateResult& updateResult = this->_pTileset->updateView(
      viewStates,
      DotNet::UnityEngine::Time::deltaTime());
  this->updateLastViewUpdateResultState(tileset, updateResult);

  for (auto pTile : updateResult.tilesFadingOut) {
    if (pTile->getState() != TileLoadState::Done) {
      continue;
    }

    const Cesium3DTilesSelection::TileContent& content = pTile->getContent();
    const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
        content.getRenderContent();
    if (pRenderContent) {
      CesiumGltfGameObject* pCesiumGameObject =
          static_cast<CesiumGltfGameObject*>(
              pRenderContent->getRenderResources());
      if (pCesiumGameObject && pCesiumGameObject->pGameObject) {
        pCesiumGameObject->pGameObject->SetActive(false);
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
      CesiumGltfGameObject* pCesiumGameObject =
          static_cast<CesiumGltfGameObject*>(
              pRenderContent->getRenderResources());
      if (pCesiumGameObject && pCesiumGameObject->pGameObject) {
        pCesiumGameObject->pGameObject->SetActive(true);
      }
    }
  }
}

void Cesium3DTilesetImpl::OnValidate(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  // Check if "Suspend Update" was the modified value.
  if (tileset.suspendUpdate() != tileset.previousSuspendUpdate()) {
    // If so, don't destroy the tileset.
    tileset.previousSuspendUpdate(tileset.suspendUpdate());
  } else {
    // Otherwise, destroy the tileset so it can be recreated with new settings.
    // Unity does not allow us to destroy GameObjects and MonoBehaviours in this
    // callback, so instead it is marked to happen later.
    this->_destroyTilesetOnNextUpdate = true;
  }
}

void Cesium3DTilesetImpl::OnEnable(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
#if UNITY_EDITOR
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
#endif
}

void Cesium3DTilesetImpl::OnDisable(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
#if UNITY_EDITOR
  if (this->_updateInEditorCallback != nullptr) {
    UnityEditor::EditorApplication::update(
        UnityEditor::EditorApplication::update() -
        this->_updateInEditorCallback);
    this->_updateInEditorCallback = nullptr;
  }
#endif

  this->_creditSystem = nullptr;

  this->DestroyTileset(tileset);
}

void Cesium3DTilesetImpl::RecreateTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->DestroyTileset(tileset);
}

namespace {

struct CalculateECEFCameraPosition {
  const CesiumGeospatial::Ellipsoid& ellipsoid;

  glm::dvec3 operator()(const CesiumGeometry::BoundingSphere& sphere) {
    const glm::dvec3& center = sphere.getCenter();
    glm::dmat4 enuToEcef =
        glm::dmat4(CesiumGeospatial::GlobeTransforms::eastNorthUpToFixedFrame(
            center,
            ellipsoid));
    glm::dvec3 offset = sphere.getRadius() * glm::normalize(
                                                 glm::dvec3(enuToEcef[0]) +
                                                 glm::dvec3(enuToEcef[1]) +
                                                 glm::dvec3(enuToEcef[2]));
    glm::dvec3 position = center + offset;
    return position;
  }

  glm::dvec3
  operator()(const CesiumGeometry::OrientedBoundingBox& orientedBoundingBox) {
    const glm::dvec3& center = orientedBoundingBox.getCenter();
    glm::dmat4 enuToEcef =
        glm::dmat4(CesiumGeospatial::GlobeTransforms::eastNorthUpToFixedFrame(
            center,
            ellipsoid));
    const glm::dmat3& halfAxes = orientedBoundingBox.getHalfAxes();
    glm::dvec3 offset =
        glm::length(halfAxes[0] + halfAxes[1] + halfAxes[2]) *
        glm::normalize(
            glm::dvec3(enuToEcef[0]) + glm::dvec3(enuToEcef[1]) +
            glm::dvec3(enuToEcef[2]));
    glm::dvec3 position = center + offset;
    return position;
  }

  glm::dvec3
  operator()(const CesiumGeospatial::BoundingRegion& boundingRegion) {
    return (*this)(boundingRegion.getBoundingBox());
  }

  glm::dvec3
  operator()(const CesiumGeospatial::BoundingRegionWithLooseFittingHeights&
                 boundingRegionWithLooseFittingHeights) {
    return (*this)(boundingRegionWithLooseFittingHeights.getBoundingRegion()
                       .getBoundingBox());
  }

  glm::dvec3 operator()(const CesiumGeospatial::S2CellBoundingVolume& s2) {
    return (*this)(s2.computeBoundingRegion());
  }
};
} // namespace

void Cesium3DTilesetImpl::FocusTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {

#if UNITY_EDITOR
  UnityEditor::SceneView lastActiveEditorView =
      UnityEditor::SceneView::lastActiveSceneView();
  if (!this->_pTileset || !this->_pTileset->getRootTile() ||
      lastActiveEditorView == nullptr) {
    return;
  }

  UnityEngine::Camera editorCamera = lastActiveEditorView.camera();
  if (editorCamera == nullptr) {
    return;
  }

  DotNet::CesiumForUnity::CesiumGeoreference georeferenceComponent =
      tileset.gameObject()
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumGeoreference>();

  const CesiumGeospatial::LocalHorizontalCoordinateSystem& georeferenceCrs =
      georeferenceComponent.NativeImplementation().getCoordinateSystem(
          georeferenceComponent);
  const glm::dmat4& ecefToUnityWorld =
      georeferenceCrs.getEcefToLocalTransformation();

  const BoundingVolume& boundingVolume =
      this->_pTileset->getRootTile()->getBoundingVolume();
  glm::dvec3 ecefCameraPosition = std::visit(
      CalculateECEFCameraPosition{CesiumGeospatial::Ellipsoid::WGS84},
      boundingVolume);
  glm::dvec3 unityCameraPosition =
      glm::dvec3(ecefToUnityWorld * glm::dvec4(ecefCameraPosition, 1.0));

  glm::dvec3 ecefCenter =
      Cesium3DTilesSelection::getBoundingVolumeCenter(boundingVolume);
  glm::dvec3 unityCenter =
      glm::dvec3(ecefToUnityWorld * glm::dvec4(ecefCenter, 1.0));
  glm::dvec3 unityCameraFront =
      glm::normalize(unityCenter - unityCameraPosition);
  glm::dvec3 unityCameraRight =
      glm::normalize(glm::cross(glm::dvec3(0.0, 0.0, 1.0), unityCameraFront));
  glm::dvec3 unityCameraUp =
      glm::normalize(glm::cross(unityCameraFront, unityCameraRight));

  UnityEngine::Vector3 unityCameraPositionf;
  unityCameraPositionf.x = static_cast<float>(unityCameraPosition.x);
  unityCameraPositionf.y = static_cast<float>(unityCameraPosition.y);
  unityCameraPositionf.z = static_cast<float>(unityCameraPosition.z);

  UnityEngine::Vector3 unityCameraFrontf;
  unityCameraFrontf.x = static_cast<float>(unityCameraFront.x);
  unityCameraFrontf.y = static_cast<float>(unityCameraFront.y);
  unityCameraFrontf.z = static_cast<float>(unityCameraFront.z);

  lastActiveEditorView.pivot(unityCameraPositionf);
  lastActiveEditorView.rotation(UnityEngine::Quaternion::LookRotation(
      unityCameraFrontf,
      UnityEngine::Vector3::up()));
#endif
}

Tileset* Cesium3DTilesetImpl::getTileset() { return this->_pTileset.get(); }

const Tileset* Cesium3DTilesetImpl::getTileset() const {
  return this->_pTileset.get();
}
const DotNet::CesiumForUnity::CesiumCreditSystem&
Cesium3DTilesetImpl::getCreditSystem() const {
  return this->_creditSystem;
}

void Cesium3DTilesetImpl::setCreditSystem(
    const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem) {
  this->_creditSystem = creditSystem;
}

void Cesium3DTilesetImpl::updateLastViewUpdateResultState(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
    const Cesium3DTilesSelection::ViewUpdateResult& currentResult) {
  if (!tileset.logSelectionStats())
    return;

  const ViewUpdateResult& previousResult = this->_lastUpdateResult;
  if (currentResult.tilesToRenderThisFrame.size() !=
          previousResult.tilesToRenderThisFrame.size() ||
      currentResult.workerThreadTileLoadQueueLength !=
          previousResult.workerThreadTileLoadQueueLength ||
      currentResult.mainThreadTileLoadQueueLength !=
          previousResult.mainThreadTileLoadQueueLength ||
      currentResult.tilesVisited != previousResult.tilesVisited ||
      currentResult.culledTilesVisited != previousResult.culledTilesVisited ||
      currentResult.tilesCulled != previousResult.tilesCulled ||
      currentResult.maxDepthVisited != previousResult.maxDepthVisited) {
    SPDLOG_LOGGER_INFO(
        this->_pTileset->getExternals().pLogger,
        "{0}: Visited {1}, Culled Visited {2}, Rendered {3}, Culled {4}, Max "
        "Depth Visited {5}, Loading-Worker {6}, Loading-Main {7} "
        "Total Tiles Resident {8}, Frame {9}",
        tileset.gameObject().name().ToStlString(),
        currentResult.tilesVisited,
        currentResult.culledTilesVisited,
        currentResult.tilesToRenderThisFrame.size(),
        currentResult.tilesCulled,
        currentResult.maxDepthVisited,
        currentResult.workerThreadTileLoadQueueLength,
        currentResult.mainThreadTileLoadQueueLength,
        this->_pTileset->getNumberOfTilesLoaded(),
        currentResult.frameNumber);
  }

  this->_lastUpdateResult = currentResult;
}

void Cesium3DTilesetImpl::DestroyTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  // Remove any existing raster overlays
  System::Array1<CesiumForUnity::CesiumRasterOverlay> overlays =
      tileset.gameObject().GetComponents<CesiumForUnity::CesiumRasterOverlay>();
  for (int32_t i = 0, len = overlays.Length(); i < len; ++i) {
    CesiumForUnity::CesiumRasterOverlay overlay = overlays[i];
    overlay.RemoveFromTileset();
  }

  this->_pTileset.reset();
}

void Cesium3DTilesetImpl::LoadTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  TilesetOptions options{};
  options.maximumScreenSpaceError = tileset.maximumScreenSpaceError();
  options.preloadAncestors = tileset.preloadAncestors();
  options.preloadSiblings = tileset.preloadSiblings();
  options.forbidHoles = tileset.forbidHoles();
  options.maximumSimultaneousTileLoads = tileset.maximumSimultaneousTileLoads();
  options.maximumCachedBytes = tileset.maximumCachedBytes();
  options.loadingDescendantLimit = tileset.loadingDescendantLimit();
  options.enableFrustumCulling = tileset.enableFrustumCulling();
  options.enableFogCulling = tileset.enableFogCulling();
  options.enforceCulledScreenSpaceError =
      tileset.enforceCulledScreenSpaceError();
  options.culledScreenSpaceError = tileset.culledScreenSpaceError();
  // options.enableLodTransitionPeriod = tileset.useLodTransitions();
  // options.lodTransitionLength = tileset.lodTransitionLength();
  options.showCreditsOnScreen = tileset.showCreditsOnScreen();
  options.loadErrorCallback =
      [tileset](const TilesetLoadFailureDetails& details) {
        int typeValue = (int)details.type;
        CesiumForUnity::Cesium3DTilesetLoadFailureDetails unityDetails(
            tileset,
            CesiumForUnity::Cesium3DTilesetLoadType(typeValue),
            details.statusCode,
            System::String(details.message));

        CesiumForUnity::Cesium3DTileset::BroadcastCesium3DTilesetLoadFailure(
            unityDetails);
      };

  // Generous per-frame time limits for loading / unloading on main thread.
  options.mainThreadLoadingTimeLimit = 5.0;
  options.tileCacheUnloadTimeLimit = 5.0;

  TilesetContentOptions contentOptions{};
  contentOptions.generateMissingNormalsSmooth = true;
  // .. = tileset.generateSmoothNormals();

  options.contentOptions = contentOptions;

  this->_lastUpdateResult = ViewUpdateResult();

  if (tileset.tilesetSource() ==
      CesiumForUnity::CesiumDataSource::FromCesiumIon) {
    System::String ionAccessToken = tileset.ionAccessToken();
    if (System::String::IsNullOrEmpty(ionAccessToken)) {
      ionAccessToken =
          CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessToken();
    }

    this->_pTileset = std::make_unique<Tileset>(
        createTilesetExternals(tileset),
        tileset.ionAssetID(),
        ionAccessToken.ToStlString(),
        options);
  } else {
    this->_pTileset = std::make_unique<Tileset>(
        createTilesetExternals(tileset),
        tileset.url().ToStlString(),
        options);
  }

  // Add any overlay components
  System::Array1<CesiumForUnity::CesiumRasterOverlay> overlays =
      tileset.gameObject().GetComponents<CesiumForUnity::CesiumRasterOverlay>();
  for (int32_t i = 0, len = overlays.Length(); i < len; ++i) {
    CesiumForUnity::CesiumRasterOverlay overlay = overlays[i];
    overlay.AddToTileset();
  }
}

} // namespace CesiumForUnityNative
