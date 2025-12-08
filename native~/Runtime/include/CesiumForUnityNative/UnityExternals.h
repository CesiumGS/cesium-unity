#pragma once

#include <Cesium3DTilesSelection/TilesetExternals.h>
#include <CesiumForUnityNative/Library.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>

#include <memory>

namespace CesiumAsync {
class AsyncSystem;
class IAssetAccessor;
class ITaskProcessor;
} // namespace CesiumAsync

namespace CesiumForUnityNative {

CESIUMFORUNITYNATIVERUNTIME_API const
    std::shared_ptr<CesiumAsync::IAssetAccessor>&
    getAssetAccessor();
CESIUMFORUNITYNATIVERUNTIME_API const
    std::shared_ptr<CesiumAsync::ITaskProcessor>&
    getTaskProcessor();
CESIUMFORUNITYNATIVERUNTIME_API CesiumAsync::AsyncSystem& getAsyncSystem();

} // namespace CesiumForUnityNative
