#include "CesiumGlobeFlightPathImpl.h"

#include <DotNet/CesiumForUnity/CesiumGlobeFlightPath.h>
#include <glm/vec3.hpp>

using namespace CesiumGeospatial;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumGlobeFlightPathImpl::CesiumGlobeFlightPathImpl(
    const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path)
    : _flightPath(nullptr) {}

CesiumGlobeFlightPathImpl::~CesiumGlobeFlightPathImpl() {}

bool CesiumGlobeFlightPathImpl::CreateFromEarthCenteredEarthFixedCoordinates(
    const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path,
    const DotNet::Unity::Mathematics::double3 sourceEcef,
    const DotNet::Unity::Mathematics::double3 destinationEcef) {
  std::optional<GlobeFlightPath> flightPath =
      GlobeFlightPath::fromEarthCenteredEarthFixedCoordinates(
          glm::dvec3(sourceEcef.x, sourceEcef.y, sourceEcef.z),
          glm::dvec3(destinationEcef.x, destinationEcef.y, destinationEcef.z));
  if (!flightPath.has_value()) {
    this->_flightPath = nullptr;
    return false;
  }

  this->_flightPath = std::make_unique<GlobeFlightPath>(*flightPath);
  return this->_flightPath != nullptr;
}

bool CesiumGlobeFlightPathImpl::CreateFromLongitudeLatitudeHeight(
    const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path,
    const DotNet::Unity::Mathematics::double3 sourceLlh,
    const DotNet::Unity::Mathematics::double3 destinationLlh) {
  std::optional<GlobeFlightPath> flightPath =
      GlobeFlightPath::fromLongitudeLatitudeHeight(
          Cartographic(sourceLlh.x, sourceLlh.y, sourceLlh.z),
          Cartographic(destinationLlh.x, destinationLlh.y, destinationLlh.z));

  if (!flightPath.has_value()) {
    this->_flightPath = nullptr;
    return false;
  }

  this->_flightPath = std::make_unique<GlobeFlightPath>(*flightPath);
  return this->_flightPath != nullptr;
}

DotNet::Unity::Mathematics::double3 CesiumGlobeFlightPathImpl::GetPosition(
    const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path,
    double percentage,
    double additionalHeight) const {
  assert(this->_flightPath != nullptr);

  glm::dvec3 result =
      this->_flightPath->getPosition(percentage, additionalHeight);

  return DotNet::Unity::Mathematics::double3::Construct(
      result.x,
      result.y,
      result.z);
}

double CesiumGlobeFlightPathImpl::GetLength(
    const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path) const {
  assert(this->_flightPath != nullptr);
  return this->_flightPath->getLength();
}

} // namespace CesiumForUnityNative
