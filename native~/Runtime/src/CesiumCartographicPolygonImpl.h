#pragma once

#include <CesiumGeospatial/CartographicPolygon.h>

#include <DotNet/System/Array1.h>

namespace DotNet::CesiumForUnity {
class CesiumCartographicPolygon;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double2;
}

namespace CesiumForUnityNative {

class CesiumCartographicPolygonImpl {
public:
  CesiumCartographicPolygonImpl(
      const DotNet::CesiumForUnity::CesiumCartographicPolygon& polygon);
  ~CesiumCartographicPolygonImpl();

  void UpdatePolygon(
      const DotNet::CesiumForUnity::CesiumCartographicPolygon& polygon,
      const DotNet::System::Array1<DotNet::Unity::Mathematics::double2>&
          cartographicPoints);

private:
  CesiumGeospatial::CartographicPolygon _polygon;
};

} // namespace CesiumForUnityNative
