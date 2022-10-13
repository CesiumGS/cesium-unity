#pragma once

#include <CesiumIonClient/Assets.h>

#include <DotNet/CesiumForUnity/IonAssetsColumn.h>

#include <DotNet/System/Collections/Generic/IList1.h>
#include <DotNet/System/String.h>

#include <DotNet/UnityEditor/IMGUI/Controls/TreeViewItem.h>
#include <DotNet/UnityEngine/Rect.h>

#include <memory>
#include <map>

namespace DotNet::CesiumForUnity {
class IonAssetsTreeView;
}

namespace CesiumForUnityNative {

class IonAssetTreeViewItem
    : public DotNet::UnityEditor::IMGUI::Controls::TreeViewItem {
public:
  IonAssetTreeViewItem(const CesiumIonClient::Asset& asset);
  ~IonAssetTreeViewItem();

  const int64_t assetId;
  const DotNet::System::String assetName;
  const DotNet::System::String assetType;
  const DotNet::System::String assetDateAdded;

private:
  std::shared_ptr<CesiumIonClient::Asset> _pAsset;
  const static int _depth = 0; // All items are at the same depth in the tree.
};

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

  void CellGUI(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      DotNet::UnityEngine::Rect cellRect,
      DotNet::UnityEditor::IMGUI::Controls::TreeViewItem item,
      DotNet::CesiumForUnity::IonAssetsColumn column);

  void Refresh(const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);

private:
};
} // namespace CesiumForUnityNative
