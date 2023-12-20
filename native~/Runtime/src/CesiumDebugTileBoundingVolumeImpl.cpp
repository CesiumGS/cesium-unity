#include "CesiumDebugTileBoundingVolumeImpl.h"

#include "UnityPrepareRendererResources.h"
#include "UnityTransforms.h"

#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumDebugTileBoundingVolume.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Color.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Gizmos.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector3.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumDebugTileBoundingVolumeImpl::CesiumDebugTileBoundingVolumeImpl(
    const CesiumForUnity::CesiumDebugTileBoundingVolume& instance)
    : _pTile(nullptr) {}

void CesiumDebugTileBoundingVolumeImpl::OnEnable(
    const CesiumForUnity::CesiumDebugTileBoundingVolume& instance) {
  this->_pTile = nullptr;

  UnityEngine::GameObject gameObject = instance.gameObject();

  CesiumForUnity::Cesium3DTileset tileset =
      gameObject.GetComponentInParent<CesiumForUnity::Cesium3DTileset>();
  if (tileset == nullptr)
    return;

  Tileset* pTileset = tileset.NativeImplementation().getTileset();
  if (pTileset == nullptr)
    return;

  const Tile* pFoundTile = nullptr;

  // Found the native Tile that corresponds to this game object.
  pTileset->forEachLoadedTile([this, gameObject](Tile& tile) {
    if (tile.getState() != TileLoadState::Done)
      return;

    const TileRenderContent* pRenderContent =
        tile.getContent().getRenderContent();
    if (pRenderContent == nullptr)
      return;

    const CesiumGltfGameObject* pCesiumGameObject =
        static_cast<const CesiumGltfGameObject*>(
            pRenderContent->getRenderResources());
    if (pCesiumGameObject == nullptr ||
        *pCesiumGameObject->pGameObject != gameObject)
      return;

    this->_pTile = &tile;
  });
}

void CesiumDebugTileBoundingVolumeImpl::OnDrawGizmos(
    const CesiumForUnity::CesiumDebugTileBoundingVolume& instance) {
  if (this->_pTile == nullptr)
    return;

  CesiumForUnity::CesiumGeoreference georeference =
      instance.gameObject()
          .GetComponentInParent<CesiumForUnity::CesiumGeoreference>();
  if (georeference == nullptr)
    return;

  const CesiumGeospatial::LocalHorizontalCoordinateSystem& coordinateSystem =
      georeference.NativeImplementation().getCoordinateSystem(georeference);

  CesiumGeometry::OrientedBoundingBox obb =
      getOrientedBoundingBoxFromBoundingVolume(
          this->_pTile->getBoundingVolume());

  UnityEngine::Gizmos::color(
      UnityEngine::Color::Construct(1.0f, 0.0f, 0.0f, 1.0f));
  glm::dmat4 localToWorld = UnityTransforms::fromUnity(
      instance.transform().parent().localToWorldMatrix());
  const glm::dmat3& halfAxes = obb.getHalfAxes();
  glm::dmat4 obbTransform = glm::dmat4(
      glm::dvec4(halfAxes[0], 0.0),
      glm::dvec4(halfAxes[1], 0.0),
      glm::dvec4(halfAxes[2], 0.0),
      glm::dvec4(obb.getCenter(), 1.0));
  UnityEngine::Gizmos::matrix(UnityTransforms::toUnity(
      localToWorld * coordinateSystem.getEcefToLocalTransformation() *
      obbTransform));
  UnityEngine::Gizmos::DrawWireCube(
      UnityEngine::Vector3::Construct(0.0f, 0.0f, 0.0f),
      UnityEngine::Vector3::Construct(2.0f, 2.0f, 2.0f));

  obb = obb.transform(coordinateSystem.getEcefToLocalTransformation());
  CesiumGeometry::AxisAlignedBox aabb = obb.toAxisAligned();
  UnityEngine::Gizmos::color(
      UnityEngine::Color::Construct(0.0f, 1.0f, 0.0f, 1.0f));
  UnityEngine::Gizmos::matrix(UnityTransforms::toUnity(localToWorld));
  UnityEngine::Gizmos::DrawWireCube(
      UnityTransforms::toUnity(aabb.center),
      UnityTransforms::toUnity(
          glm::dvec3(aabb.lengthX, aabb.lengthY, aabb.lengthZ)));
}

} // namespace CesiumForUnityNative
