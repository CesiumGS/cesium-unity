#pragma once

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/GameObject.h>

#include <memory>
#include <unordered_set>

namespace DotNet::CesiumForUnity {
class CesiumEditorWindow;
}

namespace CesiumForUnityNative {

class CesiumEditorWindowImpl {

public:
  CesiumEditorWindowImpl(
      const DotNet::CesiumForUnity::CesiumEditorWindow& window);
  ~CesiumEditorWindowImpl();

  void AddAssetFromIon(
      const DotNet::CesiumForUnity::CesiumEditorWindow& window,
      DotNet::System::String name,
      DotNet::System::String tilesetName,
      int64_t tilesetID,
      DotNet::System::String overlayName,
      int64_t overlayID);

private:
  std::unordered_set<std::string> _itemsBeingAdded;
};
} // namespace CesiumForUnityNative
