#pragma once

#include <CesiumGeospatial/SimplePlanarEllipsoidCurve.h>

#include <DotNet/Unity/Mathematics/double3.h>

#include <memory>
#include <optional>

namespace DotNet::CesiumForUnity {
class CesiumSimplePlanarEllipsoidCurve;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double3;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumSimplePlanarEllipsoidCurveImpl {
public:
  CesiumSimplePlanarEllipsoidCurveImpl(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path);
  ~CesiumSimplePlanarEllipsoidCurveImpl();

  bool CreateFromEarthCenteredEarthFixedCoordinates(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
      const DotNet::Unity::Mathematics::double3 sourceEcef,
      const DotNet::Unity::Mathematics::double3 destinationEcef);

  bool CreateFromLongitudeLatitudeHeight(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
      const DotNet::Unity::Mathematics::double3 sourceLlh,
      const DotNet::Unity::Mathematics::double3 destinationLlh);

  DotNet::Unity::Mathematics::double3 GetPosition(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
      double percentage,
      double additionalHeight) const;

private:
  std::unique_ptr<CesiumGeospatial::SimplePlanarEllipsoidCurve> _curve;
};

} // namespace CesiumForUnityNative
