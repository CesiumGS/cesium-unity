#pragma once

#include <Cesium3DTilesSelection/TilesetExternals.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>

namespace Cesium3DTilesSelection {
class TilesetExternals;
} // namespace Cesium3DTilesSelection

namespace CesiumUtility {
class CreditSystem;
}

namespace CesiumAsync {
class AsyncSystem;
class IAssetAccessor;
class ITaskProcessor;
} // namespace CesiumAsync

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
}

namespace CesiumForUnityNative {

const std::shared_ptr<CesiumAsync::IAssetAccessor>& getAssetAccessor();
const std::shared_ptr<CesiumAsync::ITaskProcessor>& getTaskProcessor();
CesiumAsync::AsyncSystem getAsyncSystem();

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const DotNet::CesiumForUnity::Cesium3DTileset& tileset);

} // namespace CesiumForUnityNative
