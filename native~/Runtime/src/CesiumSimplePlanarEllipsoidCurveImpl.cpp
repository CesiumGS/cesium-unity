#include "CesiumSimplePlanarEllipsoidCurveImpl.h"

#include <DotNet/CesiumForUnity/CesiumSimplePlanarEllipsoidCurve.h>
#include <glm/vec3.hpp>

using namespace CesiumGeospatial;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumSimplePlanarEllipsoidCurveImpl::CesiumSimplePlanarEllipsoidCurveImpl(
    const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path)
    : _curve(nullptr) {}

CesiumSimplePlanarEllipsoidCurveImpl::~CesiumSimplePlanarEllipsoidCurveImpl() {}

bool CesiumSimplePlanarEllipsoidCurveImpl::
    CreateFromEarthCenteredEarthFixedCoordinates(
        const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve&
            path,
        const DotNet::Unity::Mathematics::double3 sourceEcef,
        const DotNet::Unity::Mathematics::double3 destinationEcef) {
  std::optional<SimplePlanarEllipsoidCurve> flightPath =
      SimplePlanarEllipsoidCurve::fromEarthCenteredEarthFixedCoordinates(
          glm::dvec3(sourceEcef.x, sourceEcef.y, sourceEcef.z),
          glm::dvec3(destinationEcef.x, destinationEcef.y, destinationEcef.z));
  if (!flightPath.has_value()) {
    this->_curve = nullptr;
    return false;
  }

  this->_curve = std::make_unique<SimplePlanarEllipsoidCurve>(*flightPath);
  return this->_curve != nullptr;
}

bool CesiumSimplePlanarEllipsoidCurveImpl::CreateFromLongitudeLatitudeHeight(
    const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
    const DotNet::Unity::Mathematics::double3 sourceLlh,
    const DotNet::Unity::Mathematics::double3 destinationLlh) {
  std::optional<SimplePlanarEllipsoidCurve> flightPath =
      SimplePlanarEllipsoidCurve::fromLongitudeLatitudeHeight(
          Cartographic(sourceLlh.x, sourceLlh.y, sourceLlh.z),
          Cartographic(destinationLlh.x, destinationLlh.y, destinationLlh.z));

  if (!flightPath.has_value()) {
    this->_curve = nullptr;
    return false;
  }

  this->_curve = std::make_unique<SimplePlanarEllipsoidCurve>(*flightPath);
  return this->_curve != nullptr;
}

DotNet::Unity::Mathematics::double3
CesiumSimplePlanarEllipsoidCurveImpl::GetPosition(
    const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
    double percentage,
    double additionalHeight) const {
  assert(this->_curve != nullptr);

  glm::dvec3 result =
      this->_curve->getPosition(percentage, additionalHeight);

  return DotNet::Unity::Mathematics::double3::Construct(
      result.x,
      result.y,
      result.z);
}

} // namespace CesiumForUnityNative
