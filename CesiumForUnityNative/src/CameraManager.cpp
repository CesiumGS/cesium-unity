#include "CameraManager.h"

#include "Bindings.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumUtility/Math.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumGeospatial;
using namespace CesiumUtility;
using namespace UnityEditor;
using namespace UnityEngine;

namespace CesiumForUnity {

namespace {

ViewState unityCameraToViewState(Camera& camera) {
  Transform transform = camera.GetTransform();

  glm::dvec3 origin = Ellipsoid::WGS84.cartographicToCartesian(
      Cartographic::fromDegrees(-105.25737, 39.736401, 2250.0));
  glm::dmat4 enuToFixed = Transforms::eastNorthUpToFixedFrame(origin);
  glm::dmat4 swapYandZ(
      glm::dvec4(1.0, 0.0, 0.0, 0.0),
      glm::dvec4(0.0, 0.0, 1.0, 0.0),
      glm::dvec4(0.0, 1.0, 0.0, 0.0),
      glm::dvec4(0.0, 0.0, 0.0, 1.0));
  glm::dmat4 unityToEcef = enuToFixed * swapYandZ;

  Vector3 cameraPositionUnity = transform.GetPosition();
  glm::dvec3 cameraPositionEcef = glm::dvec3(
      unityToEcef * glm::dvec4(
                        cameraPositionUnity.x,
                        cameraPositionUnity.y,
                        cameraPositionUnity.z,
                        1.0));

  Vector3 cameraDirectionUnity = transform.GetForward();
  glm::dvec3 cameraDirectionEcef = glm::dvec3(
      unityToEcef * glm::dvec4(
                        cameraDirectionUnity.x,
                        cameraDirectionUnity.y,
                        cameraDirectionUnity.z,
                        0.0));

  Vector3 cameraUpUnity = transform.GetUp();
  glm::dvec3 cameraUpEcef = glm::dvec3(
      unityToEcef *
      glm::dvec4(cameraUpUnity.x, cameraUpUnity.y, cameraUpUnity.z, 0.0));

  double verticalFOV = Math::degreesToRadians(camera.GetFieldOfView());

  return ViewState::create(
      cameraPositionEcef,
      cameraDirectionEcef,
      cameraUpEcef,
      glm::dvec2(camera.GetPixelWidth(), camera.GetPixelHeight()),
      verticalFOV * camera.GetAspect(),
      verticalFOV);
}

} // namespace

std::vector<ViewState> CameraManager::getAllCameras(GameObject& context) {
  std::vector<ViewState> result;

  Camera camera = Camera::GetMain();
  if (camera != nullptr) {
    result.emplace_back(unityCameraToViewState(camera));
  }

  SceneView lastActiveEditorView = SceneView::GetLastActiveSceneView();
  if (lastActiveEditorView != nullptr) {
    Camera editorCamera = lastActiveEditorView.GetCamera();
    if (camera != nullptr) {
      result.emplace_back(unityCameraToViewState(editorCamera));
    }
  }

  return result;
}

} // namespace CesiumForUnity
