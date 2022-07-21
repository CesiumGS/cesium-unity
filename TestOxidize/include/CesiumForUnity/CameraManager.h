#pragma once

//#include <Cesium3DTilesSelection/ViewState.h>

#include <vector>

namespace TestOxidize::UnityEngine {
struct GameObject;
}

namespace CesiumForUnity {

class CameraManager {
public:
  static void /*std::vector<Cesium3DTilesSelection::ViewState>*/
  getAllCameras(const TestOxidize::UnityEngine::GameObject& context);
};

} // namespace CesiumForUnity
