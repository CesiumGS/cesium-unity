#include "UnityTaskProcessor.h"

#include <DotNet/System/Action.h>
#include <DotNet/System/Threading/Tasks/Task.h>

using namespace DotNet;

namespace CesiumForUnity {

namespace {

class TaskRunner /*: public System::Action<>*/ {
public:
  TaskRunner(std::function<void()>&& f) : _f(std::move(f)) {}

  virtual void operator()() /*override*/ {
    try {
      this->_f();
      delete this;
    } catch (...) {
      delete this;
      throw;
    }
  }

private:
  std::function<void()> _f;
};

} // namespace

void UnityTaskProcessor::startTask(std::function<void()> f) {
  System::Threading::Tasks::Task::Run(f);
}

} // namespace CesiumForUnity
