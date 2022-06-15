#pragma once

#include <Cesium3DTilesSelection/ViewState.h>

#include <vector>

namespace UnityEngine {
struct GameObject;
}

namespace CesiumForUnity {

class CameraManager {
public:
  static std::vector<Cesium3DTilesSelection::ViewState>
  getAllCameras(UnityEngine::GameObject& context);
};

} // namespace CesiumForUnity
