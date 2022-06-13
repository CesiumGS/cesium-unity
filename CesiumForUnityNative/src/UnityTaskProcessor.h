#pragma once

#include <CesiumAsync/ITaskProcessor.h>

namespace CesiumForUnity {

class UnityTaskProcessor : public CesiumAsync::ITaskProcessor {
public:
  virtual void startTask(std::function<void()> f) override;
};

} // namespace CesiumForUnity
