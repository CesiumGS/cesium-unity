#pragma once

#include <Cesium3DTilesSelection/ViewState.h>

#include <vector>

namespace DotNet::UnityEngine {
class GameObject;
}

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
}

namespace CesiumForUnityNative {

class Cesium3DTilesetImpl;

class CameraManager {
public:
  static std::vector<Cesium3DTilesSelection::ViewState> getAllCameras(
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
      const CesiumForUnityNative::Cesium3DTilesetImpl& impl);
};

} // namespace CesiumForUnityNative
