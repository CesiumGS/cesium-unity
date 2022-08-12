#pragma once

#include <Cesium3DTilesSelection/ViewState.h>

#include <vector>

namespace Oxidize::UnityEngine {
class GameObject;
}

namespace CesiumForUnity {

class CameraManager {
public:
  static std::vector<Cesium3DTilesSelection::ViewState>
  getAllCameras(const Oxidize::UnityEngine::GameObject& context);
};

} // namespace CesiumForUnity
