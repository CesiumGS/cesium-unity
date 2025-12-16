#include "Cesium3DTileImpl.h"

#include "UnityTransforms.h"

#include <Cesium3DTilesSelection/Tile.h>
#include <CesiumGeospatial/Ellipsoid.h>

#include <DotNet/Unity/Mathematics/double4x4.h>
#include <DotNet/UnityEngine/Bounds.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumGeometry;
using namespace CesiumGeospatial;

namespace CesiumForUnityNative {

DotNet::UnityEngine::Bounds Cesium3DTileImpl::getBounds(
    void* pTileVoid,
    void* pTileEllipsoidVoid,
    const DotNet::Unity::Mathematics::double4x4& ecefToLocalMatrix) {
  const Tile* pTile = static_cast<const Tile*>(pTileVoid);
  const Ellipsoid* pTileEllipsoid =
      static_cast<const Ellipsoid*>(pTileEllipsoidVoid);
  const BoundingVolume& bv = pTile->getBoundingVolume();
  OrientedBoundingBox obb =
      getOrientedBoundingBoxFromBoundingVolume(bv, *pTileEllipsoid);
  obb = obb.transform(UnityTransforms::fromUnity(ecefToLocalMatrix));
  AxisAlignedBox aabb = obb.toAxisAligned();
  return DotNet::UnityEngine::Bounds::Construct(
      UnityTransforms::toUnity(aabb.center),
      DotNet::UnityEngine::Vector3{
          float(aabb.lengthX),
          float(aabb.lengthY),
          float(aabb.lengthZ)});
}

} // namespace CesiumForUnityNative
