#pragma once

#include <memory>

namespace CesiumAsync {
class AsyncSystem;
class IAssetAccessor;
class ITaskProcessor;
} // namespace CesiumAsync

namespace CesiumForUnityNative {

const std::shared_ptr<CesiumAsync::IAssetAccessor>& getAssetAccessor();
const std::shared_ptr<CesiumAsync::ITaskProcessor>& getTaskProcessor();
CesiumAsync::AsyncSystem getAsyncSystem();

} // namespace CesiumForUnityNative
