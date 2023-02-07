#include "CameraManager.h"

#include "UnityTransforms.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/Transforms.h>
#include <CesiumUtility/Math.h>

#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumOrbitalFrustum.h>
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

#if UNITY_EDITOR
using namespace DotNet::UnityEditor;
#endif

namespace CesiumForUnityNative {

namespace {

ViewState unityCameraToViewState(
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

  return ViewState::create(
      cameraPosition,
      cameraDirection,
      cameraUp,
      glm::dvec2(camera.pixelWidth(), camera.pixelHeight()),
      horizontalFOV,
      verticalFOV);
}

ViewState frustumToViewState(const CesiumOrbitalFrustum& globeFrustum) {

  double verticalFOV = Math::degreesToRadians(globeFrustum.VerticalFov());
  double aspectRatio = globeFrustum.AspectRatio();

  double horizontalFOV =
      2 * glm::atan(aspectRatio * glm::tan(verticalFOV * 0.5));

  auto cartogrpahicDegrees = globeFrustum.CartographicPosition();
  glm::dvec3 position =
      Ellipsoid::WGS84.cartographicToCartesian(Cartographic::fromDegrees(
          cartogrpahicDegrees.x,
          cartogrpahicDegrees.y,
          cartogrpahicDegrees.z));
  glm::dvec3 forward = -glm::normalize(position);
  glm::dvec3 right = glm::cross({0, 0, 1}, -forward);
  glm::dvec3 up = glm::cross(right, forward);

  double pixelWidth = globeFrustum.ViewPortPixelWidth();
  double pixelHeight = (int)ceil(pixelWidth / aspectRatio);

  return ViewState::create(
      position,
      forward,
      up,
      glm::dvec2(pixelWidth, pixelHeight),
      horizontalFOV,
      verticalFOV);
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
    pCoordinateSystem = &georeference.getCoordinateSystem();
  }

  std::vector<ViewState> result;
  CesiumOrbitalFrustum frustum = CesiumOrbitalFrustum::Instance();
  if (frustum != nullptr) {
    result.emplace_back(frustumToViewState(frustum));
  } else {
    Camera camera = Camera::main();
    if (camera != nullptr) {
      result.emplace_back(unityCameraToViewState(
          pCoordinateSystem,
          unityWorldToTileset,
          camera));
    }
  }

#if UNITY_EDITOR
  if (!EditorApplication::isPlaying()) {
    SceneView lastActiveEditorView = SceneView::lastActiveSceneView();
    if (lastActiveEditorView != nullptr) {
      Camera editorCamera = lastActiveEditorView.camera();
      if (editorCamera != nullptr) {
        result.emplace_back(unityCameraToViewState(
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
