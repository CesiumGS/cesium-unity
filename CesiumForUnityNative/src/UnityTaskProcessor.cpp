#include "UnityTaskProcessor.h"

#include <DotNet/System/Action.h>
#include <DotNet/System/Threading/Tasks/Task.h>

using namespace DotNet;

namespace CesiumForUnityNative {

void UnityTaskProcessor::startTask(std::function<void()> f) {
  System::Threading::Tasks::Task::Run(f);
}

} // namespace CesiumForUnityNative
