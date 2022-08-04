#pragma once

namespace Oxidize::CesiumForUnity {
class Cesium3DTileset;
}

namespace CesiumForUnity {

class Cesium3DTilesetImpl {
public:
  Cesium3DTilesetImpl(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);
  void JustBeforeDelete(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);
  void Start(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);
  void Update(const Oxidize::CesiumForUnity::Cesium3DTileset& tileset);
};

}
