#include <CesiumForUnity/CameraManager.h>

// #include <CesiumGeospatial/Ellipsoid.h>
// #include <CesiumGeospatial/Transforms.h>
// #include <CesiumUtility/Math.h>

#include <TestOxidize/UnityEditor/SceneView.h>
#include <TestOxidize/UnityEngine/Camera.h>
#include <TestOxidize/UnityEngine/GameObject.h>
#include <TestOxidize/UnityEngine/Transform.h>
#include <TestOxidize/UnityEngine/Vector3.h>

// using namespace Cesium3DTilesSelection;
// using namespace CesiumGeospatial;
// using namespace CesiumUtility;
using namespace TestOxidize::UnityEditor;
using namespace TestOxidize::UnityEngine;

namespace CesiumForUnity {

namespace {

void /*ViewState*/ unityCameraToViewState(const Camera& camera) {
  Transform transform = camera.transform();

  // glm::dvec3 origin = Ellipsoid::WGS84.cartographicToCartesian(
  //     Cartographic::fromDegrees(144.96133, -37.81510, 2250.0));
  // glm::dmat4 enuToFixed = Transforms::eastNorthUpToFixedFrame(origin);
  // glm::dmat4 swapYandZ(
  //     glm::dvec4(1.0, 0.0, 0.0, 0.0),
  //     glm::dvec4(0.0, 0.0, 1.0, 0.0),
  //     glm::dvec4(0.0, 1.0, 0.0, 0.0),
  //     glm::dvec4(0.0, 0.0, 0.0, 1.0));
  // glm::dmat4 unityToEcef = enuToFixed * swapYandZ;

  Vector3 cameraPositionUnity = transform.position();
  // glm::dvec3 cameraPositionEcef = glm::dvec3(
  //     unityToEcef * glm::dvec4(
  //                       cameraPositionUnity.x,
  //                       cameraPositionUnity.y,
  //                       cameraPositionUnity.z,
  //                       1.0));

  // Vector3 cameraDirectionUnity = transform.GetForward();
  // glm::dvec3 cameraDirectionEcef = glm::dvec3(
  //     unityToEcef * glm::dvec4(
  //                       cameraDirectionUnity.x,
  //                       cameraDirectionUnity.y,
  //                       cameraDirectionUnity.z,
  //                       0.0));

  Vector3 cameraUpUnity = transform.up();
  // float x = cameraUpUnity.x();
  // float y = cameraUpUnity.y();
  // float z = cameraUpUnity.z();
  // glm::dvec3 cameraUpEcef = glm::dvec3(
  //     unityToEcef *
  //     glm::dvec4(cameraUpUnity.x, cameraUpUnity.y, cameraUpUnity.z, 0.0));

  double verticalFOV = /*Math::degreesToRadians(*/camera.fieldOfView()/*)*/;

  int32_t pixelWidth = camera.pixelWidth();
  int32_t pixelHeight = camera.pixelHeight();
  float aspect = camera.aspect();
  // return ViewState::create(
  //     cameraPositionEcef,
  //     cameraDirectionEcef,
  //     cameraUpEcef,
  //     glm::dvec2(camera.pixelWidth(), camera.pixelHeight()),
  //     verticalFOV * camera.aspect(),
  //     verticalFOV);
}

} // namespace

void /*std::vector<ViewState>*/
CameraManager::getAllCameras(const GameObject& context) {
  // std::vector<ViewState> result;

  Camera camera = Camera::main();
  if (camera != nullptr) {
    /*result.emplace_back(*/ unityCameraToViewState(camera) /*)*/;
  }

  SceneView lastActiveEditorView = SceneView::lastActiveSceneView();
  if (lastActiveEditorView != nullptr) {
    Camera editorCamera = lastActiveEditorView.camera();
    if (camera != nullptr) {
      /*result.emplace_back(*/ unityCameraToViewState(editorCamera) /*)*/;
    }
  }

  // return result;
}

} // namespace CesiumForUnity
