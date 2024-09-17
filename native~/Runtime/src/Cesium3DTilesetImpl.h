#pragma once

#include "CesiumImpl.h"

#include <Cesium3DTilesSelection/ViewUpdateResult.h>

#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/System/Action.h>
#include <DotNet/System/Array1.h>

#include <memory>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/CallbackFunction.h>
#endif

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumCreditSystem;
class CesiumRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class Tileset;
}

namespace CesiumForUnityNative {

class Cesium3DTilesetImpl : public CesiumImpl<Cesium3DTilesetImpl> {
public:
  Cesium3DTilesetImpl(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  ~Cesium3DTilesetImpl();

  void SetShowCreditsOnScreen(
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
      bool value);
  void Start(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void Update(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void OnValidate(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void OnEnable(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void OnDisable(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

  void RecreateTileset(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void FocusTileset(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void UpdateOverlayMaterialKeys(
      const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

  float
  ComputeLoadProgress(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

  Cesium3DTilesSelection::Tileset* getTileset();
  const Cesium3DTilesSelection::Tileset* getTileset() const;

  /**
   * Gets the Unity credit system for this tileset to pass credits to.
   */
  const DotNet::CesiumForUnity::CesiumCreditSystem& getCreditSystem() const;
  /**
   * Sets the Unity credit system for this tileset to pass credits to.
   * This is done when a new credit system is created in Unity.
   *
   * This is necessary to keep track of the Unity credit system's lifetime. If
   * the credit system in Unity is destroyed, then the native one should update
   * accordingly.
   */
  void setCreditSystem(
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);

private:
  void updateOverlayMaterialKeys(
      const DotNet::System::Array1<DotNet::CesiumForUnity::CesiumRasterOverlay>&
          overlays);
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
  DotNet::CesiumForUnity::CesiumCreditSystem _creditSystem;
  bool _destroyTilesetOnNextUpdate;
  int32_t _lastOpaqueMaterialHash;
};

} // namespace CesiumForUnityNative
