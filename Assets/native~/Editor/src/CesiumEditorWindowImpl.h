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

  void
  JustBeforeDelete(const DotNet::CesiumForUnity::CesiumEditorWindow& window);

  void AddAssetFromIon(
      const DotNet::CesiumForUnity::CesiumEditorWindow& window,
      DotNet::System::String name,
      int64_t tilesetID,
      int64_t overlayID);

private:
  std::unordered_set<std::string> _itemsBeingAdded;
};
} // namespace CesiumForUnityNative
