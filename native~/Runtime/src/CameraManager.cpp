#include "CameraManager.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumGeoreferenceImpl.h"
#include "UnityTransforms.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/GlobeTransforms.h>
#include <CesiumUtility/Math.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/System/Collections/Generic/List1.h>
#include <DotNet/UnityEngine/Camera.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector3.h>
#include <glm/trigonometric.hpp>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/EditorApplication.h>
#include <DotNet/UnityEditor/SceneView.h>
#endif

using namespace Cesium3DTilesSelection;
using namespace CesiumGeospatial;
using namespace CesiumUtility;
using namespace DotNet::UnityEngine;
using namespace DotNet::CesiumForUnity;
using namespace DotNet;

#if UNITY_EDITOR
using namespace DotNet::UnityEditor;
#endif

namespace CesiumForUnityNative {

namespace {

ViewState unityCameraToViewState(
    const CesiumGeoreference& georeference,
    const LocalHorizontalCoordinateSystem* pCoordinateSystem,
    const glm::dmat4& unityWorldToTileset,
    const Camera& camera) {
  Transform transform = camera.transform();

  Vector3 cameraPositionUnity = transform.position();
  glm::dvec3 cameraPosition = glm::dvec3(
      unityWorldToTileset * glm::dvec4(
                                cameraPositionUnity.x,
                                cameraPositionUnity.y,
                                cameraPositionUnity.z,
                                1.0));

  Vector3 cameraDirectionUnity = transform.forward();
  glm::dvec3 cameraDirection = glm::dvec3(
      unityWorldToTileset * glm::dvec4(
                                cameraDirectionUnity.x,
                                cameraDirectionUnity.y,
                                cameraDirectionUnity.z,
                                0.0));

  Vector3 cameraUpUnity = transform.up();
  glm::dvec3 cameraUp = glm::dvec3(
      unityWorldToTileset *
      glm::dvec4(cameraUpUnity.x, cameraUpUnity.y, cameraUpUnity.z, 0.0));

  if (pCoordinateSystem) {
    cameraPosition = pCoordinateSystem->localPositionToEcef(cameraPosition);
    cameraDirection = pCoordinateSystem->localDirectionToEcef(cameraDirection);
    cameraUp = pCoordinateSystem->localDirectionToEcef(cameraUp);
  }

  double verticalFOV = Math::degreesToRadians(camera.fieldOfView());
  double horizontalFOV =
      2 * glm::atan(camera.aspect() * glm::tan(verticalFOV * 0.5));

  const CesiumGeospatial::Ellipsoid& ellipsoid =
      georeference != nullptr
          ? georeference.ellipsoid().NativeImplementation().GetEllipsoid()
          : CesiumGeospatial::Ellipsoid::WGS84;

  return ViewState(
      cameraPosition,
      glm::normalize(cameraDirection),
      glm::normalize(cameraUp),
      glm::dvec2(camera.pixelWidth(), camera.pixelHeight()),
      horizontalFOV,
      verticalFOV,
      ellipsoid);
}

} // namespace

namespace {

void addMainCamera(
    std::vector<ViewState>& result,
    const CesiumGeoreference& georeferenceComponent,
    const CesiumGeospatial::LocalHorizontalCoordinateSystem* pCoordinateSystem,
    const glm::dmat4& unityWorldToTileset) {
  Camera camera = Camera::main();
  if (camera != nullptr) {
    result.emplace_back(unityCameraToViewState(
        georeferenceComponent,
        pCoordinateSystem,
        unityWorldToTileset,
        camera));
  }
}

void addActiveSceneCameraInEditor(
    std::vector<ViewState>& result,
    const CesiumGeoreference& georeferenceComponent,
    const CesiumGeospatial::LocalHorizontalCoordinateSystem* pCoordinateSystem,
    const glm::dmat4& unityWorldToTileset) {
#if UNITY_EDITOR
  if (!EditorApplication::isPlaying()) {
    SceneView lastActiveEditorView = SceneView::lastActiveSceneView();
    if (lastActiveEditorView != nullptr) {
      Camera editorCamera = lastActiveEditorView.camera();
      // check for invalid scale
      if (0.0 == unityWorldToTileset[0].x ||
          0.0 == unityWorldToTileset[1].y ||
          0.0 == unityWorldToTileset[2].z)
        return;

      if (editorCamera != nullptr) {
        result.emplace_back(unityCameraToViewState(
          georeferenceComponent,
          pCoordinateSystem,
          unityWorldToTileset,
          editorCamera));
      }
    }
  }
#endif
}

} // namespace

/*static*/ std::vector<Cesium3DTilesSelection::ViewState>
CameraManager::getAllCameras(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
    const CesiumForUnityNative::Cesium3DTilesetImpl& impl) {
  const LocalHorizontalCoordinateSystem* pCoordinateSystem = nullptr;

  glm::dmat4 unityWorldToTileset =
      UnityTransforms::fromUnity(tileset.transform().worldToLocalMatrix());

  // check for invalid scale
  if (0.0 == unityWorldToTileset[0].x ||
      0.0 == unityWorldToTileset[1].y ||
      0.0 == unityWorldToTileset[2].z)
    return {};


  CesiumGeoreference georeferenceComponent =
      tileset.gameObject().GetComponentInParent<CesiumGeoreference>();
  if (georeferenceComponent != nullptr) {
    CesiumGeoreferenceImpl& georeference =
        georeferenceComponent.NativeImplementation();
    pCoordinateSystem =
        &georeference.getCoordinateSystem(georeferenceComponent);
  }

  std::vector<ViewState> result;

  const CesiumCameraManager& cameraManager = impl.getCameraManager();

  if (cameraManager == nullptr || cameraManager.useMainCamera()) {
    addMainCamera(
        result,
        georeferenceComponent,
        pCoordinateSystem,
        unityWorldToTileset);
  }

  if (cameraManager == nullptr || cameraManager.useSceneViewCameraInEditor()) {
    addActiveSceneCameraInEditor(
        result,
        georeferenceComponent,
        pCoordinateSystem,
        unityWorldToTileset);
  }

  if (cameraManager != nullptr) {
    System::Collections::Generic::List1<Camera> cameras =
        cameraManager.additionalCameras();
    for (int32_t i = 0, len = cameras.Count(); i < len; ++i) {
      Camera camera = cameras[i];
      if (camera == nullptr)
        continue;

      result.emplace_back(unityCameraToViewState(
          georeferenceComponent,
          pCoordinateSystem,
          unityWorldToTileset,
          camera));
    }
  }

  return result;
}

} // namespace CesiumForUnityNative
