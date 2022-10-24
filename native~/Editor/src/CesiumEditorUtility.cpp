#include "CesiumEditorUtility.h"

#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Object.h>

using namespace DotNet;

namespace CesiumForUnityNative {

/*static*/ CesiumForUnity::Cesium3DTileset
CesiumEditorUtility::FindFirstTilesetWithAssetID(int64_t assetID) {
  System::Array1 tilesets =
      UnityEngine::Object::FindObjectsOfType<CesiumForUnity::Cesium3DTileset>();
  for (int32_t i = 0; i < tilesets.Length(); i++) {
    const CesiumForUnity::Cesium3DTileset tileset = tilesets[i];
    if (tileset != nullptr && tileset.ionAssetID() == assetID) {
      return tileset;
    }
  }

  return nullptr;
}

/*static*/ CesiumForUnity::Cesium3DTileset
CesiumEditorUtility::FindFirstTileset() {
  System::Array1 tilesets =
      UnityEngine::Object::FindObjectsOfType<CesiumForUnity::Cesium3DTileset>();
  for (int32_t i = 0; i < tilesets.Length(); i++) {
    const CesiumForUnity::Cesium3DTileset tileset = tilesets[i];
    if (tileset != nullptr) {
      return tileset;
    }
  }

  return nullptr;
}

/*static*/ CesiumForUnity::Cesium3DTileset CesiumEditorUtility::CreateTileset(
    DotNet::System::String name,
    int64_t assetID) {
  UnityEngine::GameObject gameObject(name);
  CesiumForUnity::Cesium3DTileset tileset =
      gameObject.AddComponent<CesiumForUnity::Cesium3DTileset>();
  tileset.ionAssetID(assetID);

  return tileset;
}

/*static*/ CesiumForUnity::CesiumIonRasterOverlay
CesiumEditorUtility::AddBaseOverlayToTileset(
    DotNet::CesiumForUnity::Cesium3DTileset tileset,
    int64_t assetID) {
  UnityEngine::GameObject gameObject = tileset.gameObject();
  CesiumForUnity::CesiumIonRasterOverlay overlay =
      gameObject.GetComponent<CesiumForUnity::CesiumIonRasterOverlay>();
  if (overlay != nullptr) {
    UnityEngine::Object::DestroyImmediate(overlay);
    overlay = gameObject.AddComponent<CesiumForUnity::CesiumIonRasterOverlay>();
  }
  overlay.ionAssetID(assetID);

  return overlay;
}

} // namespace CesiumForUnityNative
