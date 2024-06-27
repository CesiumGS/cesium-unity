#include "CesiumSimplePlanarEllipsoidCurveImpl.h"

#include <DotNet/CesiumForUnity/CesiumSimplePlanarEllipsoidCurve.h>
#include <glm/vec3.hpp>

using namespace CesiumGeospatial;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumSimplePlanarEllipsoidCurveImpl::CesiumSimplePlanarEllipsoidCurveImpl(
    const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path)
    : _curve() {}

CesiumSimplePlanarEllipsoidCurveImpl::~CesiumSimplePlanarEllipsoidCurveImpl() {}

bool CesiumSimplePlanarEllipsoidCurveImpl::
    CreateFromCenteredFixed(
        const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
        const DotNet::CesiumForUnity::CesiumEllipsoid& ellipsoid,
        const DotNet::Unity::Mathematics::double3 sourceEcef,
        const DotNet::Unity::Mathematics::double3 destinationEcef) {
  this->_curve =
      SimplePlanarEllipsoidCurve::fromEarthCenteredEarthFixedCoordinates(
          ellipsoid.NativeImplementation().GetEllipsoid(),
          glm::dvec3(sourceEcef.x, sourceEcef.y, sourceEcef.z),
          glm::dvec3(destinationEcef.x, destinationEcef.y, destinationEcef.z));

  return this->_curve.has_value();
}

bool CesiumSimplePlanarEllipsoidCurveImpl::CreateFromLongitudeLatitudeHeight(
    const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
    const DotNet::CesiumForUnity::CesiumEllipsoid& ellipsoid,
    const DotNet::Unity::Mathematics::double3 sourceLlh,
    const DotNet::Unity::Mathematics::double3 destinationLlh) {
  this->_curve = SimplePlanarEllipsoidCurve::fromLongitudeLatitudeHeight(
      ellipsoid.NativeImplementation().GetEllipsoid(),
      Cartographic(sourceLlh.x, sourceLlh.y, sourceLlh.z),
      Cartographic(destinationLlh.x, destinationLlh.y, destinationLlh.z));

  return this->_curve.has_value();
}

DotNet::Unity::Mathematics::double3
CesiumSimplePlanarEllipsoidCurveImpl::GetPosition(
    const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
    double percentage,
    double additionalHeight) const {
  assert(this->_curve.has_value());

  glm::dvec3 result = this->_curve->getPosition(percentage, additionalHeight);

  return DotNet::Unity::Mathematics::double3::Construct(
      result.x,
      result.y,
      result.z);
}

} // namespace CesiumForUnityNative
