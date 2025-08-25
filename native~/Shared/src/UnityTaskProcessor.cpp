#include "UnityTaskProcessor.h"

#include <DotNet/System/Action.h>
//#include <DotNet/System/Threading/Tasks/Task.h>
#include <thread>

using namespace DotNet;

namespace CesiumForUnityNative {

void UnityTaskProcessor::startTask(std::function<void()> f) {
  // TODO: System::Threading does not work with WASM builds, and std::thread(f) will
  // be slower because it will create a new thread each time. A thread pool would be better.
  std::thread(f).detach();
  //System::Threading::Tasks::Task::Run(f);
}

} // namespace CesiumForUnityNative
