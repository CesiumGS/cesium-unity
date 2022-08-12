#include "CameraManager.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumUtility/Math.h>

#include <Oxidize/UnityEditor/SceneView.h>
#include <Oxidize/UnityEngine/Camera.h>
#include <Oxidize/UnityEngine/GameObject.h>
#include <Oxidize/UnityEngine/Transform.h>
#include <Oxidize/UnityEngine/Vector3.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumGeospatial;
using namespace CesiumUtility;
using namespace Oxidize::UnityEditor;
using namespace Oxidize::UnityEngine;

namespace CesiumForUnity {

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

  SceneView lastActiveEditorView = SceneView::lastActiveSceneView();
  if (lastActiveEditorView != nullptr) {
    Camera editorCamera = lastActiveEditorView.camera();
    if (camera != nullptr) {
      result.emplace_back(unityCameraToViewState(editorCamera));
    }
  }

  return result;
}

} // namespace CesiumForUnity
