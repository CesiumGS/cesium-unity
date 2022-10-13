#pragma once

#include "IonAssetsTreeViewImpl.h"

#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/IonAssetsTreeView.h>

#include <DotNet/System/Collections/Generic/List1.h>

#include <DotNet/UnityEngine/GUI.h>

using namespace DotNet;
using namespace DotNet::System::Collections::Generic;
using namespace DotNet::UnityEditor::IMGUI::Controls;

namespace CesiumForUnityNative {

System::String assetTypeToString(const std::string& assetType) {
  static std::map<std::string, std::string> lookup = {
      {"3DTILES", "3D Tiles"},
      {"GLTF", "glTF"},
      {"IMAGERY", "Imagery"},
      {"TERRAIN", "Terrain"},
      {"CZML", "CZML"},
      {"KML", "KML"},
      {"GEOJSON", "GeoJSON"}};
  auto it = lookup.find(assetType);
  if (it != lookup.end()) {
    return System::String(it->second);
  }
  return System::String("(Unknown)");
}

System::String formatDate(const std::string& assetDate) {
  // TODO
  /* FDateTime dateTime;
  bool success = FDateTime::ParseIso8601(*unrealDateString, dateTime);
  if (!success) {
    UE_LOG(
        LogCesiumEditor,
        Warning,
        TEXT("Could not parse date %s"),
        assetDate.c_str());
    return UTF8_TO_TCHAR(assetDate.c_str());
  }
  return dateTime.ToString(TEXT("%Y-%m-%d"));*/
  return System::String(assetDate);
}

IonAssetTreeViewItem::IonAssetTreeViewItem(const CesiumIonClient::Asset& asset)
    : TreeViewItem(
          asset.id,
          IonAssetTreeViewItem::_depth,
          System::String(asset.name)),
      assetId(asset.id),
      assetName(System::String(asset.name)),
      assetType(assetTypeToString(asset.type)),
      assetDateAdded(formatDate(asset.dateAdded)),
      _pAsset(std::make_shared<CesiumIonClient::Asset>(asset)) {}

IonAssetTreeViewItem::~IonAssetTreeViewItem() {}

IonAssetsTreeViewImpl::IonAssetsTreeViewImpl(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {}

IonAssetsTreeViewImpl::~IonAssetsTreeViewImpl() {}

void IonAssetsTreeViewImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {}

CesiumIonSessionImpl& getCurrentSessionImpl() {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonSession::currentSession();
  CesiumIonSessionImpl& sessionImpl = session.NativeImplementation();

  return sessionImpl;
}

IList1<TreeViewItem> IonAssetsTreeViewImpl::BuildRows(
    const CesiumForUnity::IonAssetsTreeView& treeView,
    DotNet::UnityEditor::IMGUI::Controls::TreeViewItem root) {

  CesiumIonSessionImpl& session = getCurrentSessionImpl();
  const CesiumIonClient::Assets& assets = session.getAssets();

  size_t numAssets = assets.items.size();
  IList1<TreeViewItem> rows = List1<TreeViewItem>(numAssets);
  for (int i = 0; i < numAssets; i++) {
    const CesiumIonClient::Asset& asset = assets.items[i];
    IonAssetTreeViewItem assetTreeViewItem(asset);

    rows.Insert(i, assetTreeViewItem);
    root.AddChild(assetTreeViewItem);
  }

  return rows;
}

void IonAssetsTreeViewImpl::CellGUI(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
    DotNet::UnityEngine::Rect cellRect,
    DotNet::UnityEditor::IMGUI::Controls::TreeViewItem item,
    DotNet::CesiumForUnity::IonAssetsColumn columnIndex) {
  IonAssetTreeViewItem* pIonAssetItem = static_cast<IonAssetTreeViewItem*>(&item);

  if (columnIndex == CesiumForUnity::IonAssetsColumn::Name) {
    UnityEngine::GUI::Label(cellRect, pIonAssetItem->assetName);
  } else if (columnIndex == CesiumForUnity::IonAssetsColumn::Type) {
    UnityEngine::GUI::Label(cellRect, pIonAssetItem->assetType);
  } else if (columnIndex == CesiumForUnity::IonAssetsColumn::DateAdded) {
    UnityEngine::GUI::Label(cellRect, pIonAssetItem->assetDateAdded);
  }
}

void IonAssetsTreeViewImpl::Refresh(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {
  CesiumIonSessionImpl& session = getCurrentSessionImpl();
  session.refreshAssets();
  treeView.Reload();
}
} // namespace CesiumForUnityNative
