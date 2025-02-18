#pragma once

#include <Cesium3DTilesSelection/ViewState.h>

#include <vector>

namespace DotNet::UnityEngine {
class GameObject;
}

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumCameraGroup;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class Cesium3DTilesetImpl;

class CameraManager {
public:
  static std::vector<Cesium3DTilesSelection::ViewState> getAllCameras(
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
      const CesiumForUnityNative::Cesium3DTilesetImpl& impl);
  static std::vector<Cesium3DTilesSelection::ViewState> getAllCameras(
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
      const DotNet::CesiumForUnity::CesiumCameraGroup& group,
      const CesiumForUnityNative::Cesium3DTilesetImpl& impl);
};

} // namespace CesiumForUnityNative
