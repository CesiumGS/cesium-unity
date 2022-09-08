#pragma once

#include <Cesium3DTilesSelection/ViewUpdateResult.h>

#include <memory>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/CallbackFunction.h>
#endif

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
}

namespace Cesium3DTilesSelection {
class Tileset;
}

namespace CesiumForUnityNative {

class Cesium3DTilesetImpl {
public:
  Cesium3DTilesetImpl(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  ~Cesium3DTilesetImpl();

  void JustBeforeDelete(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void Start(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void Update(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void OnValidate(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void OnEnable(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void OnDisable(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

  void RecreateTileset(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

  Cesium3DTilesSelection::Tileset* getTileset();
  const Cesium3DTilesSelection::Tileset* getTileset() const;

private:
  void DestroyTileset(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void LoadTileset(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void updateLastViewUpdateResultState(
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
      const Cesium3DTilesSelection::ViewUpdateResult& currentResult);

  std::unique_ptr<Cesium3DTilesSelection::Tileset> _pTileset;
  Cesium3DTilesSelection::ViewUpdateResult _lastUpdateResult;
#if UNITY_EDITOR
  DotNet::UnityEditor::CallbackFunction _updateInEditorCallback;
#endif
  bool _destroyTilesetOnNextUpdate;
};

} // namespace CesiumForUnityNative
