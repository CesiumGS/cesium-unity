#include "UnityTaskProcessor.h"

#include "Bindings.h"

using namespace System;
using namespace System::Threading::Tasks;

namespace CesiumForUnity {

namespace {

class TaskRunner : public Action {
public:
  TaskRunner(std::function<void()>&& f) : _f(std::move(f)) {}

  virtual void operator()() override {
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
  Task::Run(*new TaskRunner(std::move(f)));
}

} // namespace CesiumForUnity
