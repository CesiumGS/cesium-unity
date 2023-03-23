#include "UnityTileExcluderAdaptor.h"

#include "UnityTransforms.h"

#include <CesiumGeometry/Transforms.h>

#include <DotNet/CesiumForUnity/Cesium3DTile.h>
#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumTileExcluder.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Vector3.h>

#include <cassert>

namespace CesiumForUnityNative {

UnityTileExcluderAdaptor::UnityTileExcluderAdaptor(
    const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference)
    : _tile(),
      _georeference(georeference),
      _excluder(excluder),
      _excluderTransform(excluder.transform()),
      _tilesetTransform(tileset.transform()) {}

void UnityTileExcluderAdaptor::startNewFrame() noexcept {
  glm::dmat4 matrix =
      UnityTransforms::fromUnity(this->_georeference.ecefToLocalMatrix());

  // The GameObject with the tileset must be a child of the GameObject with the
  // excluder (or they must be the same GameObject). Compose an ECEF -> excluder
  // transformation matrix.
  DotNet::UnityEngine::Transform currentTransform = this->_tilesetTransform;
  while (currentTransform != this->_excluderTransform &&
         currentTransform != nullptr) {
    glm::dmat4 toParent =
        CesiumGeometry::Transforms::createTranslationRotationScaleMatrix(
            UnityTransforms::fromUnity(currentTransform.localPosition()),
            UnityTransforms::fromUnity(currentTransform.localRotation()),
            UnityTransforms::fromUnity(currentTransform.localScale()));
    matrix = toParent * matrix;
    currentTransform = currentTransform.parent();
  }

  this->_tile._transform(UnityTransforms::toUnityMathematics(matrix));
}

bool UnityTileExcluderAdaptor::shouldExclude(
    const Cesium3DTilesSelection::Tile& tile) const noexcept {
  this->_tile._pTile(const_cast<Cesium3DTilesSelection::Tile*>(&tile));
  return this->_excluder.ShouldExclude(this->_tile);
}

} // namespace CesiumForUnityNative
