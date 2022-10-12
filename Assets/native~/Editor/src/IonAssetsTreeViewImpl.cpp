#pragma once

#include "IonAssetsTreeViewImpl.h"

#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/CesiumForUnity/IonAssetsTreeView.h>
#include <DotNet/System/Collections/Generic/List1.h>

using namespace DotNet;
using namespace DotNet::System::Collections::Generic;
using namespace DotNet::UnityEditor::IMGUI::Controls;

namespace CesiumForUnityNative {
/*
IonAssetTreeViewItem::IonAssetTreeViewItem(const CesiumIonClient::Asset& asset)
    : super(
          asset.id,
          IonAssetTreeViewItem::_depth,
          System::String(asset.name)),
      assetId(asset.id)
      assetName(asset.name),
      assetType(asset.type),
      assetDateAdded(asset.dateAdded),
      _pAsset(std::make_shared<CesiumIonClient::Asset>(asset)) {}

IonAssetTreeViewItem::~IonAssetTreeViewItem() {}*/

IonAssetsTreeViewImpl::IonAssetsTreeViewImpl(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {}

IonAssetsTreeViewImpl::~IonAssetsTreeViewImpl() {}

void IonAssetsTreeViewImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::IonAssetsTreeView& treeView) {}

IList1<TreeViewItem> IonAssetsTreeViewImpl::BuildRows(
    const CesiumForUnity::IonAssetsTreeView& treeView,
    TreeViewItem root) {
  CesiumForUnity::CesiumIonSession session =
      CesiumForUnity::CesiumIonSession::currentSession();
  CesiumIonSessionImpl& sessionImpl = session.NativeImplementation();

  const CesiumIonClient::Assets& assets = sessionImpl.getAssets();

  size_t numAssets = assets.items.size();
  List1<TreeViewItem> rows(numAssets);
  for (int i = 0; i < numAssets; i++) {
    const CesiumIonClient::Asset& asset = assets.items[i];
    //IonAssetTreeViewItem assetTreeViewItem = new IonAssetTreeViewItem(asset);

    //rows[i] = assetTreeViewItem;
    //root.addChild(assetTreeViewItem);
  }

  return rows;
}

} // namespace CesiumForUnityNative
