#pragma once

#include <Cesium3DTilesSelection/TilesetExternals.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>

#include <memory>

namespace CesiumAsync {
class AsyncSystem;
class IAssetAccessor;
class ITaskProcessor;
} // namespace CesiumAsync

namespace CesiumForUnityNative {

const std::shared_ptr<CesiumAsync::IAssetAccessor>& getAssetAccessor();
const std::shared_ptr<CesiumAsync::ITaskProcessor>& getTaskProcessor();
CesiumAsync::AsyncSystem& getAsyncSystem();

} // namespace CesiumForUnityNative
