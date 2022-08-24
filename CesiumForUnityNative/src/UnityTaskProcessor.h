#pragma once

#include <CesiumAsync/ITaskProcessor.h>

namespace CesiumForUnityNative {

class UnityTaskProcessor : public CesiumAsync::ITaskProcessor {
public:
  virtual void startTask(std::function<void()> f) override;
};

} // namespace CesiumForUnityNative
