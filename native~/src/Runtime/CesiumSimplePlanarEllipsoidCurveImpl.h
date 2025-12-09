#pragma once

#include "CesiumImpl.h"

#include <CesiumGeospatial/SimplePlanarEllipsoidCurve.h>

#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/Unity/Mathematics/double3.h>

#include <optional>

namespace DotNet::CesiumForUnity {
class CesiumSimplePlanarEllipsoidCurve;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double3;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumSimplePlanarEllipsoidCurveImpl
    : public CesiumImpl<CesiumSimplePlanarEllipsoidCurveImpl> {
public:
  CesiumSimplePlanarEllipsoidCurveImpl(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path);
  ~CesiumSimplePlanarEllipsoidCurveImpl();

  bool CreateFromCenteredFixed(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
      const DotNet::CesiumForUnity::CesiumEllipsoid& ellipsoid,
      const DotNet::Unity::Mathematics::double3 sourceEcef,
      const DotNet::Unity::Mathematics::double3 destinationEcef);

  bool CreateFromLongitudeLatitudeHeight(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
      const DotNet::CesiumForUnity::CesiumEllipsoid& ellipsoid,
      const DotNet::Unity::Mathematics::double3 sourceLlh,
      const DotNet::Unity::Mathematics::double3 destinationLlh);

  DotNet::Unity::Mathematics::double3 GetPosition(
      const DotNet::CesiumForUnity::CesiumSimplePlanarEllipsoidCurve& path,
      double percentage,
      double additionalHeight) const;

private:
  std::optional<CesiumGeospatial::SimplePlanarEllipsoidCurve> _curve;
};

} // namespace CesiumForUnityNative
