#pragma once

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/System/String.h>

namespace CesiumForUnityNative {

class CesiumEditorUtility {

public:
  static DotNet::CesiumForUnity::Cesium3DTileset
  FindFirstTilesetWithAssetID(int64_t assetID);

  static DotNet::CesiumForUnity::Cesium3DTileset
  FindFirstTileset();

  static DotNet::CesiumForUnity::Cesium3DTileset
  CreateTileset(DotNet::System::String name, int64_t assetID);

  static DotNet::CesiumForUnity::CesiumIonRasterOverlay AddBaseOverlayToTileset(
      DotNet::CesiumForUnity::Cesium3DTileset tileset,
      int64_t assetID);
};

} // namespace CesiumForUnityNative
