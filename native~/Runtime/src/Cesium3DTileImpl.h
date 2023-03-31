#pragma once

namespace DotNet::Unity::Mathematics {
struct double4x4;
}

namespace DotNet::UnityEngine {
struct Bounds;
}

namespace CesiumForUnityNative {

class Cesium3DTileImpl {
public:
  static DotNet::UnityEngine::Bounds getBounds(
      void* pTileVoid,
      const DotNet::Unity::Mathematics::double4x4& ecefToLocalMatrix);
};

} // namespace CesiumForUnityNative
