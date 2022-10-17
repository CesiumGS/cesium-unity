#pragma once

#include <CesiumIonClient/Assets.h>

#include <DotNet/CesiumForUnity/IonAssetsColumn.h>

#include <DotNet/System/String.h>

#include <DotNet/UnityEditor/IMGUI/Controls/TreeViewItem.h>
#include <DotNet/UnityEngine/Rect.h>

#include <memory>
#include <vector>

namespace DotNet::CesiumForUnity {
class IonAssetsTreeView;
}

namespace CesiumForUnityNative {

class IonAssetsTreeViewImpl {
public:
  IonAssetsTreeViewImpl(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);
  ~IonAssetsTreeViewImpl();

  void
  JustBeforeDelete(const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);

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
  int GetAssetID(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);
  DotNet::System::String GetAssetDescription(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);
  DotNet::System::String GetAssetAttribution(
      const DotNet::CesiumForUnity::IonAssetsTreeView& treeView,
      int index);

  void Refresh(const DotNet::CesiumForUnity::IonAssetsTreeView& treeView);

private:
  std::shared_ptr<CesiumIonClient::Assets> _pAssets;
  const static DotNet::System::String dateAddedFormat;
};
} // namespace CesiumForUnityNative
