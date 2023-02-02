#pragma once

#include <CesiumIonClient/Assets.h>

#include <DotNet/CesiumForUnity/IonAssetsColumn.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEditor/IMGUI/Controls/TreeViewItem.h>
#include <DotNet/UnityEngine/Rect.h>

#include <memory>

namespace DotNet::CesiumForUnity {
class IonAssetsTreeView;
}

namespace CesiumForUnityNative {

class IonAssetsTreeViewImpl {
public:
  IonAssetsTreeViewImpl(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);
  ~IonAssetsTreeViewImpl();

  int GetAssetsCount(const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);

  void CellGUI(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      DotNet::UnityEngine::Rect cellRect,
      int assetIndex,
      DotNet::CesiumForUnity::IonAssetsColumn column);

  DotNet::System::String GetAssetName(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);
  DotNet::System::String GetAssetType(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);
  int64_t GetAssetID(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);
  DotNet::System::String GetAssetDescription(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);
  DotNet::System::String GetAssetAttribution(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);

  void Refresh(const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);

  void AddAssetToLevel(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);

  void AddOverlayToTerrain(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);

private:
  void applyFilter(const DotNet::System::String searchString);
  void applySorting(
      const DotNet::CesiumForUnity::IonAssetsColumn column,
      const bool sortAscending);

  std::vector<std::shared_ptr<CesiumIonClient::Asset>> _assets;
};
} // namespace CesiumForUnityNative
