#pragma once

#include "IonAssetsTreeViewImpl.h"

#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/IonAssetDetails.h>
#include <DotNet/CesiumForUnity/IonAssetsTreeView.h>

#include <DotNet/UnityEngine/GUI.h>

using namespace DotNet;
using namespace DotNet::UnityEditor::IMGUI::Controls;

namespace CesiumForUnityNative {

IonAssetsTreeViewImpl::IonAssetsTreeViewImpl(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView)
    : _pAssets() {}

IonAssetsTreeViewImpl::~IonAssetsTreeViewImpl() {}

void IonAssetsTreeViewImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {}

int IonAssetsTreeViewImpl::GetAssetsCount(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {
  return this->_pAssets->items.size();
}

System::String IonAssetsTreeViewImpl::GetAssetName(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  const CesiumIonClient::Asset& asset = this->_pAssets->items[index];
  return System::String(asset.name);
}

int IonAssetsTreeViewImpl::GetAssetID(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  const CesiumIonClient::Asset& asset = this->_pAssets->items[index];
  return asset.id;
}

System::String IonAssetsTreeViewImpl::GetAssetType(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  const CesiumIonClient::Asset& asset = this->_pAssets->items[index];
  return System::String(asset.type);
}

System::String IonAssetsTreeViewImpl::GetAssetDescription(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  const CesiumIonClient::Asset& asset = this->_pAssets->items[index];
  return System::String(asset.description);
}

System::String IonAssetsTreeViewImpl::GetAssetAttribution(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    int index) {
  const CesiumIonClient::Asset& asset = this->_pAssets->items[index];
  return System::String(asset.attribution);
}

void IonAssetsTreeViewImpl::CellGUI(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    DotNet::UnityEngine::Rect cellRect,
    int assetIndex,
    DotNet::CesiumForUnity::IonAssetsColumn columnIndex) {

  const CesiumIonClient::Asset& asset = this->_pAssets->items[assetIndex];

  if (columnIndex == CesiumForUnity::IonAssetsColumn::Name) {
    System::String name = System::String(asset.name);
    UnityEngine::GUI::Label(cellRect, name);
  } else if (columnIndex == CesiumForUnity::IonAssetsColumn::Type) {
    System::String type = System::String(asset.type);
    UnityEngine::GUI::Label(
        cellRect,
        CesiumForUnity::IonAssetDetails::FormatType(type));
  } else if (columnIndex == CesiumForUnity::IonAssetsColumn::DateAdded) {
    System::String date = System::String(asset.dateAdded);
    UnityEngine::GUI::Label(
        cellRect,
        CesiumForUnity::IonAssetDetails::FormatDate(date));
  }
}

void IonAssetsTreeViewImpl::Refresh(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonSession::Ion();
  CesiumIonSessionImpl& sessionImpl = session.NativeImplementation();
  sessionImpl.refreshAssets();

  const CesiumIonClient::Assets& assets = sessionImpl.getAssets();
  this->_pAssets = std::make_shared<CesiumIonClient::Assets>(assets);

  treeView.Reload();
}
} // namespace CesiumForUnityNative
