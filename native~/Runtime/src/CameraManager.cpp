#include "CameraManager.h"

#include "CesiumGeoreferenceImpl.h"
#include "UnityTransforms.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/GlobeTransforms.h>
#include <CesiumUtility/Math.h>

#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumCamera.h>
#include <DotNet/UnityEngine/Camera.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector3.h>
#include <DotNet/System/Collections/Generic/List1.h>

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
    Camera& camera) {
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

  return ViewState::create(
      cameraPosition,
      glm::normalize(cameraDirection),
      glm::normalize(cameraUp),
      glm::dvec2(camera.pixelWidth(), camera.pixelHeight()),
      horizontalFOV,
      verticalFOV,
      ellipsoid);
}

} // namespace

std::vector<ViewState> CameraManager::getAllCameras(const GameObject& context) {
  const LocalHorizontalCoordinateSystem* pCoordinateSystem = nullptr;

  glm::dmat4 unityWorldToTileset =
      UnityTransforms::fromUnity(context.transform().worldToLocalMatrix());

  CesiumGeoreference georeferenceComponent =
      context.GetComponentInParent<CesiumGeoreference>();
  if (georeferenceComponent != nullptr) {
    CesiumGeoreferenceImpl& georeference =
        georeferenceComponent.NativeImplementation();
    pCoordinateSystem =
        &georeference.getCoordinateSystem(georeferenceComponent);
  }

  std::vector<ViewState> result;
  

  Camera camera = Camera::main();
  
  System::Collections::Generic::List1<Camera> cameraList = CesiumCamera::cameraList();

  if (cameraList.Count()>0) {
   camera = cameraList[0];
  }

  if (camera != nullptr) {
    result.emplace_back(unityCameraToViewState(
        georeferenceComponent,
        pCoordinateSystem,
        unityWorldToTileset,
        camera));
  }

#if UNITY_EDITOR
  if (!EditorApplication::isPlaying()) {
    SceneView lastActiveEditorView = SceneView::lastActiveSceneView();
    if (lastActiveEditorView != nullptr) {
      Camera editorCamera = lastActiveEditorView.camera();
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

  return result;
}

} // namespace CesiumForUnityNative
