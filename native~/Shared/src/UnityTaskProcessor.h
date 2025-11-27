#pragma once

#include <CesiumAsync/ITaskProcessor.h>

#ifdef __EMSCRIPTEN__
#include <memory>
#endif

namespace CesiumForUnityNative {

class UnityThreadPool;

class UnityTaskProcessor : public CesiumAsync::ITaskProcessor {
public:
  UnityTaskProcessor();

  virtual ~UnityTaskProcessor() override;

  virtual void startTask(std::function<void()> f) override;

private:
#ifdef __EMSCRIPTEN__
  // Unity Wasm can't use C# System/Threads, so we need to implement our own
  // thread pool.
  std::unique_ptr<UnityThreadPool> _pThreadPool;
#endif
};

} // namespace CesiumForUnityNative
