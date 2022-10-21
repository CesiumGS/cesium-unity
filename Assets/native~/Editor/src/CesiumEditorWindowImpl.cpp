#pragma once

#include "CesiumEditorWindowImpl.h"

#include "CesiumIonSessionImpl.h"
#include "SelectIonTokenWindowImpl.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/SharedFuture.h>
#include <CesiumIonClient/Connection.h>
#include <CesiumIonClient/Response.h>
#include <CesiumIonClient/Token.h>

#include <DotNet/CesiumForUnity/CesiumEditorWindow.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEditor/Selection.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Object.h>

#include <optional>

using namespace DotNet;

namespace CesiumForUnityNative {

CesiumEditorWindowImpl::CesiumEditorWindowImpl(
    const DotNet::CesiumForUnity::CesiumEditorWindow& window) {}

CesiumEditorWindowImpl::~CesiumEditorWindowImpl() {}

void CesiumEditorWindowImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::CesiumEditorWindow& window) {}

void CesiumEditorWindowImpl::AddAssetFromIon(
    const DotNet::CesiumForUnity::CesiumEditorWindow& window,
    DotNet::System::String name,
    int64_t tilesetID,
    int64_t overlayID) {
  const std::optional<CesiumIonClient::Connection>& connection =
      CesiumIonSessionImpl::ion().getConnection();
  if (!connection) {
    UnityEngine::Debug::LogWarning(
        System::String("Cannot add an ion asset without an active connection"));
    return;
  }

  if (this->_itemsBeingAdded.find(name.ToStlString()) !=
      this->_itemsBeingAdded.end()) {
    // Add is already in progress.
    return;
  }

  this->_itemsBeingAdded.insert(name.ToStlString());

  std::vector<int64_t> assetIDs{tilesetID};
  if (overlayID > 0) {
    assetIDs.push_back(overlayID);
  }

  SelectIonTokenWindowImpl::SelectAndAuthorizeToken(assetIDs)
      .thenInMainThread(
          [connection, tilesetID](
              const std::optional<CesiumIonClient::Token>& /*maybeToken*/) {
            // If token selection was canceled, or if an error occurred while
            // selecting the token, ignore it and create the tileset anyway.
            // It's already been logged if necessary, and we can let the user
            // sort out the problem using the resulting Troubleshooting panel.
            return connection->asset(tilesetID);
          })
      .thenInMainThread(
          [connection, tilesetID, overlayID](
              CesiumIonClient::Response<CesiumIonClient::Asset>&& response) {
            if (!response.value.has_value()) {
              return connection->getAsyncSystem().createResolvedFuture<int64_t>(
                  std::move(int64_t(tilesetID)));
            }

            if (overlayID >= 0) {
              return connection->asset(overlayID).thenInMainThread(
                  [overlayID](
                      CesiumIonClient::Response<CesiumIonClient::Asset>&&
                          overlayResponse) {
                    return overlayResponse.value.has_value()
                               ? int64_t(-1)
                               : int64_t(overlayID);
                  });
            } else {
              return connection->getAsyncSystem().createResolvedFuture<int64_t>(
                  -1);
            }
          })
      .thenInMainThread(
          [this, name, tilesetID, overlayID](int64_t missingAsset) {
            if (missingAsset != -1) {
              // showAssetDepotConfirmWindow(itemName, missingAsset);
            } else {
              CesiumForUnity::Cesium3DTileset tileset =
                  this->findFirstTilesetWithAssetID(tilesetID);

              if (tileset == nullptr) {
                UnityEngine::GameObject tilesetGameObject(name);
                tileset = tilesetGameObject
                              .AddComponent<CesiumForUnity::Cesium3DTileset>();
              }
              tileset.ionAssetID(tilesetID);

              CesiumIonSessionImpl::ion().getAssets();

              if (overlayID > 0) {
                UnityEngine::GameObject gameObject = tileset.gameObject();
                // TODO: Need to fix this when we support multiple overlays
                CesiumForUnity::CesiumIonRasterOverlay overlay =
                    gameObject
                        .GetComponent<CesiumForUnity::CesiumIonRasterOverlay>();
                if (overlay != nullptr) {
                  UnityEngine::Object::DestroyImmediate(overlay);
                }

                overlay =
                    gameObject
                        .AddComponent<CesiumForUnity::CesiumIonRasterOverlay>();

                overlay.ionAssetID(overlayID);
              }

              tileset.RecreateTileset();

              UnityEditor::Selection::activeGameObject(tileset.gameObject());
            }

            this->_itemsBeingAdded.erase(name.ToStlString());
          });
}

CesiumForUnity::Cesium3DTileset
CesiumEditorWindowImpl::findFirstTilesetWithAssetID(int64_t assetID) {
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

} // namespace CesiumForUnityNative
