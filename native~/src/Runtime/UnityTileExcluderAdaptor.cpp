#include "UnityTileExcluderAdaptor.h"

#include "UnityTransforms.h"

#include <CesiumGeometry/Transforms.h>

#include <DotNet/CesiumForUnity/Cesium3DTile.h>
#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
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
      _tilesetTransform(tileset.transform()),
      _isValid(true) {}

void UnityTileExcluderAdaptor::startNewFrame() noexcept {
  // If any of the Unity objects we need have been destroyed, disable this
  // excluder.
  if (this->_tile == nullptr || this->_georeference == nullptr ||
      this->_excluder == nullptr || this->_excluderTransform == nullptr ||
      this->_tilesetTransform == nullptr) {
    this->_isValid = false;
    return;
  }

  this->_isValid = true;

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
  if (!this->_isValid) {
    return false;
  }

  const CesiumGeospatial::Ellipsoid ellipsoid =
      _georeference.ellipsoid().NativeImplementation().GetEllipsoid();

  this->_tile._pTile(const_cast<Cesium3DTilesSelection::Tile*>(&tile));
  // it's ok for us to pass by pointer since it will only be valid for this call
  // anyways
  this->_tile._pTileEllipsoid(
      const_cast<CesiumGeospatial::Ellipsoid*>(&ellipsoid));
  return this->_excluder.ShouldExclude(this->_tile);
}

} // namespace CesiumForUnityNative
