#include "CesiumTileExcluderImpl.h"

#include "UnityTileExcluderAdaptor.h"

#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumTileExcluder.h>
#include <DotNet/System/Object.h>
#include <DotNet/UnityEngine/Debug.h>

using namespace Cesium3DTilesSelection;

namespace CesiumForUnityNative {

namespace {

Tileset*
getNativeTileset(const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (tileset == nullptr)
    return nullptr;

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  return tilesetImpl.getTileset();
}

auto findExistingExcluder(
    std::vector<std::shared_ptr<ITileExcluder>>& excluders,
    const DotNet::CesiumForUnity::CesiumTileExcluder& excluder) {
  return std::find_if(
      excluders.begin(),
      excluders.end(),
      [&excluder](const std::shared_ptr<ITileExcluder>& pCandidate) {
        UnityTileExcluderAdaptor* pUnity =
            dynamic_cast<UnityTileExcluderAdaptor*>(pCandidate.get());
        if (pUnity == nullptr)
          return false;
        return pUnity->getExcluder() == excluder;
      });
}

} // namespace

void CesiumTileExcluderImpl::AddToTileset(
    const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  Tileset* pTileset = getNativeTileset(tileset);

  std::vector<std::shared_ptr<ITileExcluder>>& excluders =
      pTileset->getOptions().excluders;

  auto it = findExistingExcluder(excluders, excluder);
  if (it != excluders.end())
    return; // already added

  // Excluder doesn't exist yet, add it.
  DotNet::CesiumForUnity::CesiumGeoreference excluderGeoreference =
      excluder.gameObject()
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumGeoreference>();
  DotNet::CesiumForUnity::CesiumGeoreference tilesetGeoreference =
      tileset.gameObject()
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumGeoreference>();

  if (excluderGeoreference != tilesetGeoreference) {
    DotNet::UnityEngine::Debug::Log(DotNet::System::String(
        "Cesium3DTileset on " + tileset.name().ToStlString() +
        " and CesiumTileExcluder on " + excluder.name().ToStlString() +
        " have different CesiumGeoreferences. The excluder will not be "
        "used."));
    return;
  }

  auto pAdaptor = std::make_shared<UnityTileExcluderAdaptor>(
      excluder,
      tileset,
      tilesetGeoreference);
  excluders.push_back(std::move(pAdaptor));
}

void CesiumTileExcluderImpl::RemoveFromTileset(
    const DotNet::CesiumForUnity::CesiumTileExcluder& excluder,
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  Tileset* pTileset = getNativeTileset(tileset);
  if (pTileset == nullptr)
    return;

  std::vector<std::shared_ptr<ITileExcluder>>& excluders =
      pTileset->getOptions().excluders;

  auto it = findExistingExcluder(excluders, excluder);
  if (it != excluders.end()) {
    excluders.erase(it);
  }
}

} // namespace CesiumForUnityNative
