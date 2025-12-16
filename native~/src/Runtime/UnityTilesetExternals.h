#pragma once

#include "UnityExternals.h"

#include <Cesium3DTilesSelection/TilesetExternals.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>

#include <memory>

namespace Cesium3DTilesSelection {
class TilesetExternals;
} // namespace Cesium3DTilesSelection

namespace CesiumUtility {
class CreditSystem;
} // namespace CesiumUtility

namespace CesiumAsync {
class AsyncSystem;
class IAssetAccessor;
class ITaskProcessor;
} // namespace CesiumAsync

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
}

namespace CesiumForUnityNative {

// Gets the credit system on the input Cesium3DTileset. If it does not exist,
// this will create a new default credit system for the tileset.
std::shared_ptr<CesiumUtility::CreditSystem>
getOrCreateCreditSystem(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

} // namespace CesiumForUnityNative
