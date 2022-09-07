#include "CameraManager.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumUtility/Math.h>

#include <DotNet/UnityEngine/Camera.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector3.h>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/SceneView.h>
#endif

using namespace Cesium3DTilesSelection;
using namespace CesiumGeospatial;
using namespace CesiumUtility;
using namespace DotNet::UnityEngine;

#if UNITY_EDITOR
using namespace DotNet::UnityEditor;
#endif

namespace CesiumForUnityNative {

namespace {

ViewState unityCameraToViewState(Camera& camera) {
  Transform transform = camera.transform();

  glm::dvec3 origin = Ellipsoid::WGS84.cartographicToCartesian(
      Cartographic::fromDegrees(144.96133, -37.81510, 2250.0));
  glm::dmat4 enuToFixed = Transforms::eastNorthUpToFixedFrame(origin);
  glm::dmat4 swapYandZ(
      glm::dvec4(1.0, 0.0, 0.0, 0.0),
      glm::dvec4(0.0, 0.0, 1.0, 0.0),
      glm::dvec4(0.0, 1.0, 0.0, 0.0),
      glm::dvec4(0.0, 0.0, 0.0, 1.0));
  glm::dmat4 unityToEcef = enuToFixed * swapYandZ;

  Vector3 cameraPositionUnity = transform.position();
  glm::dvec3 cameraPositionEcef = glm::dvec3(
      unityToEcef * glm::dvec4(
                        cameraPositionUnity.x,
                        cameraPositionUnity.y,
                        cameraPositionUnity.z,
                        1.0));

  Vector3 cameraDirectionUnity = transform.forward();
  glm::dvec3 cameraDirectionEcef = glm::dvec3(
      unityToEcef * glm::dvec4(
                        cameraDirectionUnity.x,
                        cameraDirectionUnity.y,
                        cameraDirectionUnity.z,
                        0.0));

  Vector3 cameraUpUnity = transform.up();
  glm::dvec3 cameraUpEcef = glm::dvec3(
      unityToEcef *
      glm::dvec4(cameraUpUnity.x, cameraUpUnity.y, cameraUpUnity.z, 0.0));

  double verticalFOV = Math::degreesToRadians(camera.fieldOfView());

  return ViewState::create(
      cameraPositionEcef,
      cameraDirectionEcef,
      cameraUpEcef,
      glm::dvec2(camera.pixelWidth(), camera.pixelHeight()),
      verticalFOV * camera.aspect(),
      verticalFOV);
}

} // namespace

std::vector<ViewState> CameraManager::getAllCameras(const GameObject& context) {
  std::vector<ViewState> result;

  Camera camera = Camera::main();
  if (camera != nullptr) {
    result.emplace_back(unityCameraToViewState(camera));
  }

#if UNITY_EDITOR
  SceneView lastActiveEditorView = SceneView::lastActiveSceneView();
  if (lastActiveEditorView != nullptr) {
    Camera editorCamera = lastActiveEditorView.camera();
    if (camera != nullptr) {
      result.emplace_back(unityCameraToViewState(editorCamera));
    }
  }
#endif

  return result;
}

} // namespace CesiumForUnityNative
