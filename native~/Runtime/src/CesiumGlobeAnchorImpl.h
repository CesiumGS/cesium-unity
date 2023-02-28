#pragma once

namespace DotNet::CesiumForUnity {
class CesiumGlobeAnchor;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double3;
struct double4x4;
struct quaternion;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumGlobeAnchorImpl {
public:
  static void SetNewLocalToGlobeFixedMatrix(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
      const ::DotNet::Unity::Mathematics::double4x4&
          newLocalToGlobeFixedMatrix);

  static void SetNewLocalToGlobeFixedMatrixFromTransform(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor);

  static ::DotNet::Unity::Mathematics::quaternion GetLocalToEastUpNorthRotation(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor);

  static void SetLocalToEastUpNorthRotation(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
      const ::DotNet::Unity::Mathematics::quaternion& value);
};

} // namespace CesiumForUnityNative
