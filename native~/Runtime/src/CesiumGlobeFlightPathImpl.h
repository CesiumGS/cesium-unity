#pragma once

#include <CesiumGeospatial/GlobeFlightPath.h>

#include <DotNet/Unity/Mathematics/double3.h>

#include <memory>
#include <optional>

namespace DotNet::CesiumForUnity {
class CesiumGlobeFlightPath;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double3;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumGlobeFlightPathImpl {
public:
  CesiumGlobeFlightPathImpl(
      const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path);
  ~CesiumGlobeFlightPathImpl();

  bool CreateFromEarthCenteredEarthFixedCoordinates(
      const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path,
      const DotNet::Unity::Mathematics::double3 sourceEcef,
      const DotNet::Unity::Mathematics::double3 destinationEcef);

  bool CreateFromLongitudeLatitudeHeight(
      const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path,
      const DotNet::Unity::Mathematics::double3 sourceLlh,
      const DotNet::Unity::Mathematics::double3 destinationLlh);

  DotNet::Unity::Mathematics::double3 GetPosition(
      const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path,
      double percentage,
      double additionalHeight) const;

  double
  GetLength(const DotNet::CesiumForUnity::CesiumGlobeFlightPath& path) const;

private:
  std::unique_ptr<CesiumGeospatial::GlobeFlightPath> _flightPath;
};

} // namespace CesiumForUnityNative
