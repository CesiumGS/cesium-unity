#pragma once

#include <Cesium3DTilesSelection/ViewState.h>

#include <vector>

namespace DotNet::UnityEngine {
class GameObject;
}

namespace CesiumForUnityNative {

class CameraManager {
public:
  static std::vector<Cesium3DTilesSelection::ViewState>
  getAllCameras(const DotNet::UnityEngine::GameObject& context);
};

} // namespace CesiumForUnityNative
