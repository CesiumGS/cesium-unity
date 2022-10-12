#pragma once

#include <CesiumIonClient/Assets.h>

#include <DotNet/System/Collections/Generic/IList1.h>
#include <DotNet/System/String.h>

#include <DotNet/UnityEditor/IMGUI/Controls/TreeViewItem.h>

#include <memory>
#include <optional>
#include <vector>

namespace DotNet::CesiumForUnity {
class IonAssetsTreeView;
}

namespace CesiumForUnityNative {
/*
class IonAssetTreeViewItem
    : public DotNet::UnityEditor::IMGUI::Controls::TreeViewItem {
public:
  IonAssetTreeViewItem(const CesiumIonClient::Asset& asset);
  ~IonAssetTreeViewItem();

  const int64_t assetId;
  const std::string assetName;
  const std::string assetType;
  const std::string assetDateAdded;

private:
  std::shared_ptr<CesiumIonClient::Asset> _pAsset;
  const static int _depth = 0; // All items are at the same depth in the tree.
};*/

class IonAssetsTreeViewImpl {
public:
  IonAssetsTreeViewImpl(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);
  ~IonAssetsTreeViewImpl();

  void
  JustBeforeDelete(const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);

  DotNet::System::Collections::Generic::IList1<
      DotNet::UnityEditor::IMGUI::Controls::TreeViewItem>
  BuildRows(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      DotNet::UnityEditor::IMGUI::Controls::TreeViewItem root);

private:
};
} // namespace CesiumForUnityNative
